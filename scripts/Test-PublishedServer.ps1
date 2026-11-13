$ErrorActionPreference='Stop'
Set-Location (Split-Path -Parent $PSScriptRoot)
. "$PSScriptRoot\Environment.ps1"
$smokeRoot=Join-Path $PWD ('work\published-server-'+[Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $smokeRoot -Force | Out-Null
$rsa=[System.Security.Cryptography.RSA]::Create(2048)
$request=[System.Security.Cryptography.X509Certificates.CertificateRequest]::new('CN=localhost',$rsa,[System.Security.Cryptography.HashAlgorithmName]::SHA256,[System.Security.Cryptography.RSASignaturePadding]::Pkcs1)
$san=[System.Security.Cryptography.X509Certificates.SubjectAlternativeNameBuilder]::new();$san.AddDnsName('localhost');$san.AddIpAddress([System.Net.IPAddress]::Loopback);$request.CertificateExtensions.Add($san.Build())
$certificate=$request.CreateSelfSigned([DateTimeOffset]::UtcNow.AddMinutes(-5),[DateTimeOffset]::UtcNow.AddDays(1))
$certificatePassword=[Convert]::ToHexString([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(24))
$certificatePath=Join-Path $smokeRoot 'test-only.pfx'
[System.IO.File]::WriteAllBytes($certificatePath,$certificate.Export([System.Security.Cryptography.X509Certificates.X509ContentType]::Pfx,$certificatePassword))
Add-Type -TypeDefinition @'
using System;
using System.Net.Http;
public static class DdhiPinnedSmokeClient {
 public static HttpClient Create(string thumbprint) {
  var handler=new HttpClientHandler();
  handler.ServerCertificateCustomValidationCallback=(request,cert,chain,errors)=>cert!=null&&string.Equals(cert.Thumbprint,thumbprint,StringComparison.OrdinalIgnoreCase);
  return new HttpClient(handler){BaseAddress=new Uri("https://127.0.0.1:58443/"),Timeout=TimeSpan.FromSeconds(10)};
 }
}
'@
$apiProcess=$null;$workerProcess=$null;$client=$null
& '.\.tools\postgres\pgsql\bin\pg_ctl.exe' -D work\pgdata -l logs\postgres.log -o '-p 55432 -h 127.0.0.1 -c shared_buffers=32MB -c max_connections=40' -w start
if ($LASTEXITCODE -ne 0) { throw 'Test PostgreSQL startup failed.' }
try {
 $database='ddhi_test_published_'+[Guid]::NewGuid().ToString('N')
 $env:PGHOST='127.0.0.1';$env:PGPORT='55432';$env:PGUSER='ddhi_test';$env:PGPASSWORD=Get-Content work\pg-password.txt -Raw
 & '.\.tools\postgres\pgsql\bin\createdb.exe' $database
 if ($LASTEXITCODE -ne 0) { throw 'Smoke database creation failed.' }
 $env:ConnectionStrings__Pharmacy='Host=127.0.0.1;Port=55432;Database='+$database+';Username=ddhi_test;Password='+$env:PGPASSWORD
 $env:ASPNETCORE_ENVIRONMENT='Production';$env:DOTNET_ENVIRONMENT='Production';$env:ASPNETCORE_URLS='https://127.0.0.1:58443'
 $env:DDHI__CertificatePath=$certificatePath;$env:DDHI__CertificatePassword=$certificatePassword
 $env:DDHI__KeyDirectory=Join-Path $smokeRoot 'keys';$env:DDHI__LogDirectory=Join-Path $smokeRoot 'logs'
 $env:Kestrel__Certificates__Default__Path=$certificatePath;$env:Kestrel__Certificates__Default__Password=$certificatePassword
 $apiExe=Join-Path $PWD 'release\Server\Api\DDHI.Pharmacy.Api.exe'
 & $apiExe --migrate *> (Join-Path $smokeRoot 'migrate.log')
 if ($LASTEXITCODE -ne 0) { throw "Published migration failed. See $smokeRoot\migrate.log" }
 $env:DDHI_BOOTSTRAP_EMAIL='bootstrap-'+[Guid]::NewGuid().ToString('N')+'@test.invalid'
 $env:DDHI_BOOTSTRAP_PASSWORD='Aa!1'+[Convert]::ToHexString([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(24))
 & $apiExe --bootstrap *> (Join-Path $smokeRoot 'bootstrap.log')
 if ($LASTEXITCODE -ne 0) { throw "Published bootstrap failed. See $smokeRoot\bootstrap.log" }
 $apiProcess=Start-Process -FilePath $apiExe -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $smokeRoot 'api-stdout.log') -RedirectStandardError (Join-Path $smokeRoot 'api-stderr.log')
 $client=[DdhiPinnedSmokeClient]::Create($certificate.Thumbprint)
 $ready=$false
 for ($attempt=0;$attempt -lt 40;$attempt++) {
  try { $response=$client.GetAsync('health').GetAwaiter().GetResult();if ($response.IsSuccessStatusCode) {$ready=$true;break} } catch {}
  Start-Sleep -Milliseconds 500
  if ($apiProcess.HasExited) {break}
 }
 if (-not $ready) { throw "Published HTTPS API did not become ready. See $smokeRoot" }
 $health=$response.Content.ReadAsStringAsync().GetAwaiter().GetResult() | ConvertFrom-Json
 if ($health.status -ne 'ready') {throw 'Database health is not ready.'}
 $unauth=$client.GetAsync('api/inventory').GetAwaiter().GetResult()
 if ([int]$unauth.StatusCode -ne 401) {throw 'Unauthenticated inventory was not blocked.'}
 function Send-SmokePost([string]$Path,$Body) {
  $message=[System.Net.Http.HttpRequestMessage]::new([System.Net.Http.HttpMethod]::Post,$Path)
  $message.Content=[System.Net.Http.StringContent]::new(($Body|ConvertTo-Json -Depth 10),[System.Text.Encoding]::UTF8,'application/json')
  $message.Headers.Add('Idempotency-Key',[Guid]::NewGuid().ToString())
  $response=$client.SendAsync($message).GetAwaiter().GetResult()
  $text=$response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
  if (-not $response.IsSuccessStatusCode) {throw "Published API rejected $Path with status $([int]$response.StatusCode): $text"}
  return ($text|ConvertFrom-Json)
 }
 $token=Send-SmokePost 'auth/login' @{email=$env:DDHI_BOOTSTRAP_EMAIL;password=$env:DDHI_BOOTSTRAP_PASSWORD}
 $client.DefaultRequestHeaders.Authorization=[System.Net.Http.Headers.AuthenticationHeaderValue]::new('Bearer',$token.accessToken)
 $client.DefaultRequestHeaders.Add('X-Workstation','published-server-smoke')
 $me=$client.GetAsync('api/me').GetAwaiter().GetResult()
 if (-not $me.IsSuccessStatusCode) {throw 'Published authentication failed.'}
 $chiefEmail='chief-'+[Guid]::NewGuid().ToString('N')+'@test.invalid'
 $chiefPassword='Aa!1'+[Convert]::ToHexString([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(24))
 $user=Send-SmokePost 'api/users' @{email=$chiefEmail;password=$chiefPassword;role='Chief Pharmacist'}
 $chiefToken=Send-SmokePost 'auth/login' @{email=$chiefEmail;password=$chiefPassword}
 $client.DefaultRequestHeaders.Authorization=[System.Net.Http.Headers.AuthenticationHeaderValue]::new('Bearer',$chiefToken.accessToken)
 $medicine=Send-SmokePost 'api/medicines' @{genericName='Isolated smoke-test item';brandName='';strength='TEST';dosageForm='TEST';route='';manufacturer='';category='TEST ONLY';baseUnit='unit';reorderLevel=1}
 $location=Send-SmokePost 'api/locations' @{name='Isolated smoke location'}
 $supplier=Send-SmokePost 'api/suppliers' @{name='Isolated smoke supplier'}
 $received=Send-SmokePost 'api/receiving' @{medicineId=$medicine.Id;locationId=$location.Id;supplierId=$supplier.Id;reference=[Guid]::NewGuid().ToString();lot='TEST-ONLY';expiration=(Get-Date).AddYears(1).ToString('yyyy-MM-dd');quantity=1;conversionFactor=1;unitCost=0;confirmed=$true}
 $inventory=$client.GetAsync('api/inventory').GetAwaiter().GetResult()
 if (-not $inventory.IsSuccessStatusCode) {throw 'Published inventory query failed.'}
 $audit=$client.GetAsync('api/audit/verify').GetAwaiter().GetResult()
 $auditData=$audit.Content.ReadAsStringAsync().GetAwaiter().GetResult()|ConvertFrom-Json
 if (-not $auditData.valid) {throw 'Published audit chain is invalid.'}
 $workerProcess=Start-Process -FilePath (Join-Path $PWD 'release\Server\Worker\DDHI.Pharmacy.Worker.exe') -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $smokeRoot 'worker-stdout.log') -RedirectStandardError (Join-Path $smokeRoot 'worker-stderr.log')
 Start-Sleep -Seconds 3
 if ($workerProcess.HasExited) {throw 'Published worker exited unexpectedly.'}
 @{productionEnvironment=$true;httpsHealth=$true;testCertificatePinned=$true;anonymousDenied=$true;bootstrapSucceeded=$true;staffAuthentication=$true;catalogAndReceiving=$true;auditChainValid=$true;workerStarted=$true;database=$database;note='Isolated local smoke test; no hospital server or live Cerebro access.'} | ConvertTo-Json | Set-Content release\Tests\PublishedServerSmoke.json
 Get-Content release\Tests\PublishedServerSmoke.json
} finally {
 if ($client) {$client.Dispose()}
 foreach ($process in @($apiProcess,$workerProcess)) {if ($process -and -not $process.HasExited) {$process.Kill($true);$process.WaitForExit()}}
 & '.\.tools\postgres\pgsql\bin\pg_ctl.exe' -D work\pgdata -m fast -w stop
 $certificate.Dispose();$rsa.Dispose()
}
