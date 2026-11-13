$ErrorActionPreference='Stop'
Set-Location (Split-Path -Parent $PSScriptRoot)
$toolRoot=Join-Path $PWD '.tools'
New-Item -ItemType Directory -Path $toolRoot -Force | Out-Null
$sdkVersion='10.0.404'
if (-not (Test-Path -LiteralPath (Join-Path $toolRoot 'dotnet\dotnet.exe'))) {
 $metadata=Invoke-RestMethod 'https://builds.dotnet.microsoft.com/dotnet/release-metadata/10.0/releases.json'
 $sdk=$metadata.releases | ForEach-Object { $_.sdks } | Where-Object version -eq $sdkVersion | Select-Object -First 1
 if (-not $sdk) {throw 'Pinned SDK not found in official Microsoft release metadata.'}
 $file=$sdk.files | Where-Object { $_.rid -eq 'win-x64' -and $_.url.EndsWith('.zip') } | Select-Object -First 1
 if (-not $file) {throw 'Windows x64 SDK ZIP not found.'}
 $download=Join-Path $toolRoot 'dotnet-sdk.zip'
 Invoke-WebRequest $file.url -OutFile $download
 if ((Get-FileHash -LiteralPath $download -Algorithm SHA512).Hash -ne $file.hash) {throw 'SDK SHA-512 verification failed.'}
 Expand-Archive -LiteralPath $download -DestinationPath (Join-Path $toolRoot 'dotnet')
}
. "$PSScriptRoot\Environment.ps1"
if (-not (Test-Path -LiteralPath (Join-Path $toolRoot 'inno\ISCC.exe'))) {
 $download=Join-Path $toolRoot 'inno-6.7.4.exe'
 Invoke-WebRequest 'https://github.com/jrsoftware/issrc/releases/download/is-6_7_4/innosetup-6.7.4.exe' -OutFile $download
 $signature=Get-AuthenticodeSignature -LiteralPath $download
 if ($signature.Status -ne 'Valid' -or $signature.SignerCertificate.Subject -notmatch 'Pyrsys') {throw 'Inno Setup publisher verification failed.'}
 $setup=Start-Process -FilePath $download -ArgumentList @('/VERYSILENT','/SUPPRESSMSGBOXES','/NORESTART','/CURRENTUSER','/PORTABLE=1',('/DIR="'+(Join-Path $toolRoot 'inno')+'"')) -WindowStyle Hidden -PassThru -Wait
 if ($setup.ExitCode -ne 0) {throw 'Portable installer compiler setup failed.'}
}
python -m pip install --target .tools\python reportlab pillow
if ($LASTEXITCODE -ne 0) {throw 'PDF authoring dependency setup failed.'}
Write-Output 'Project-local SDK, installer compiler and manual-generation libraries are ready. Provision an isolated PostgreSQL test database separately.'
