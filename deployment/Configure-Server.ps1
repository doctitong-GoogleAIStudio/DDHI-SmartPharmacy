# Run elevated on the dedicated Windows Server after installing PostgreSQL and obtaining a trusted TLS certificate.
[CmdletBinding()]
param([string]$InstallRoot=(Split-Path -Parent $PSScriptRoot),[string]$ApiHostname,[string]$CertificatePath,[string]$PostgresBin='C:\Program Files\PostgreSQL\17\bin',[switch]$ApplyMigrations)
$ErrorActionPreference='Stop'
if (-not ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) { throw 'Run elevated on the dedicated hospital server.' }
if (-not $ApiHostname) { $ApiHostname=Read-Host 'IT-approved internal API DNS hostname' }
if (-not $CertificatePath) { $CertificatePath=Read-Host 'Trusted server certificate PFX path' }
if (-not (Test-Path -LiteralPath $CertificatePath)) { throw 'Certificate PFX not found.' }
$certificatePassword=Read-Host 'PFX password' -AsSecureString
$databaseCredential=Get-Credential -Message 'Existing PostgreSQL DDHI application account (not superuser)'
$builder=New-Object System.Data.Common.DbConnectionStringBuilder
$builder['Host']='localhost';$builder['Database']='ddhi_pharmacy';$builder['Username']=$databaseCredential.UserName;$builder['Password']=$databaseCredential.GetNetworkCredential().Password
$connection=$builder.ConnectionString
$dataRoot=Join-Path $env:ProgramData 'DDHI-SmartPharmacy'
if (Test-Path -LiteralPath (Join-Path $dataRoot 'server-configured.txt')) { throw 'Existing configuration detected. Back up database and protected configuration and use the documented upgrade procedure.' }
New-Item -ItemType Directory -Path $dataRoot,(Join-Path $dataRoot 'keys'),(Join-Path $dataRoot 'logs') -Force | Out-Null
& icacls.exe $dataRoot /inheritance:r /grant:r 'SYSTEM:(OI)(CI)F' 'BUILTIN\Administrators:(OI)(CI)F' | Out-Null
if ($LASTEXITCODE -ne 0) { throw 'Could not secure configuration directory.' }
Copy-Item -LiteralPath $CertificatePath -Destination (Join-Path $dataRoot 'server.pfx')
$plainCertificatePassword=[System.Net.NetworkCredential]::new('',$certificatePassword).Password
$config=@{ConnectionStrings=@{Pharmacy=$connection};DDHI=@{KeyDirectory=(Join-Path $dataRoot 'keys');LogDirectory=(Join-Path $dataRoot 'logs');CertificatePath=(Join-Path $dataRoot 'server.pfx');CertificatePassword=$plainCertificatePassword};Kestrel=@{Endpoints=@{Https=@{Url="https://0.0.0.0:8443";Certificate=@{Path=(Join-Path $dataRoot 'server.pfx');Password=$plainCertificatePassword}}}}}
$config | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath (Join-Path $dataRoot 'server.json') -Encoding UTF8
if ($ApplyMigrations) {
 Write-Host 'Use a separately authorized schema-owner connection for migration. Runtime role must not own the database.'
 $migrationCredential=Get-Credential -Message 'Authorized DDHI migration/schema-owner account'
 $builder['Username']=$migrationCredential.UserName;$builder['Password']=$migrationCredential.GetNetworkCredential().Password
 $env:ConnectionStrings__Pharmacy=$builder.ConnectionString
 & (Join-Path $InstallRoot 'Api\DDHI.Pharmacy.Api.exe') --migrate
 if ($LASTEXITCODE -ne 0) { throw 'Migration failed. Services have not been created.' }
 Remove-Item Env:\ConnectionStrings__Pharmacy
}
foreach ($service in @(@{Name='DDHIPharmacyApi';Exe='Api\DDHI.Pharmacy.Api.exe'},@{Name='DDHIPharmacyWorker';Exe='Worker\DDHI.Pharmacy.Worker.exe'})) {
 if (Get-Service -Name $service.Name -ErrorAction SilentlyContinue) { throw "Service $($service.Name) already exists. Use upgrade procedure." }
 $binary='"'+(Join-Path $InstallRoot $service.Exe)+'"'
 New-Service -Name $service.Name -BinaryPathName $binary -StartupType Automatic -Description 'DDHI pharmacy central service' | Out-Null
 & sc.exe config $service.Name obj= "NT SERVICE\$($service.Name)" | Out-Null
 if ($LASTEXITCODE -ne 0) { throw 'Service identity configuration failed.' }
 & sc.exe failure $service.Name reset= 86400 actions= restart/10000/restart/30000/restart/60000 | Out-Null
 Set-ItemProperty -LiteralPath "HKLM:\SYSTEM\CurrentControlSet\Services\$($service.Name)" -Name Environment -Value @("DDHI_CONFIG_FILE=$(Join-Path $dataRoot 'server.json')") -Type MultiString
 & icacls.exe $dataRoot /grant "NT SERVICE\$($service.Name):(OI)(CI)RX" | Out-Null
 & icacls.exe (Join-Path $dataRoot 'logs') /grant "NT SERVICE\$($service.Name):(OI)(CI)M" | Out-Null
}
& icacls.exe (Join-Path $dataRoot 'keys') /grant 'NT SERVICE\DDHIPharmacyApi:(OI)(CI)M' | Out-Null
Set-Content -LiteralPath (Join-Path $dataRoot 'server-configured.txt') -Value 'Configured; hospital acceptance pending.'
Write-Host 'Services created but not started. Review least-privilege SQL grants, bootstrap an administrator, configure subnet-scoped firewall rules and TLS trust, then start services. PostgreSQL must listen on loopback only.'
