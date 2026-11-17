param([ValidateSet('All','Client','Server')][string]$Target='All')
$ErrorActionPreference='Stop'
Set-Location (Split-Path -Parent $PSScriptRoot)
. "$PSScriptRoot\Environment.ps1"
$testRoot=Join-Path $PWD ('work\isetup-'+[Guid]::NewGuid().ToString('N').Substring(0,8))
New-Item -ItemType Directory -Path $testRoot -Force | Out-Null
$verified=@{Client=$false;Server=$false;ServerSupplemental=$false}
$evidencePath=Join-Path $PWD 'release\Tests\InstallerPayloadSmoke.json'
$initialClientInstallerHash=(Get-FileHash release\Installers\DDHI-SmartPharmacy-Setup-x64.exe -Algorithm SHA256).Hash
$initialServerInstallerHash=(Get-FileHash release\Installers\DDHI-SmartPharmacy-Server-Setup-x64.exe -Algorithm SHA256).Hash
if (Test-Path -LiteralPath $evidencePath) {
 $previous=Get-Content -LiteralPath $evidencePath -Raw | ConvertFrom-Json
 $verified.Client=$previous.clientPayloadVerified -and $previous.clientInstallerSha256 -eq (Get-FileHash release\Installers\DDHI-SmartPharmacy-Setup-x64.exe -Algorithm SHA256).Hash
 $verified.Server=$previous.serverPayloadVerified -and $previous.serverInstallerSha256 -eq (Get-FileHash release\Installers\DDHI-SmartPharmacy-Server-Setup-x64.exe -Algorithm SHA256).Hash
 $verified.ServerSupplemental=$previous.serverSupplementalPayloadVerified -and $previous.serverInstallerSha256 -eq (Get-FileHash release\Installers\DDHI-SmartPharmacy-Server-Setup-x64.exe -Algorithm SHA256).Hash
}
$targets=if ($Target -eq 'All') {@('Client','Server')} else {@($Target)}
foreach ($kind in $targets) {
 $name=if ($kind -eq 'Client') {'DDHI-SmartPharmacy-Setup-x64.exe'} else {'DDHI-SmartPharmacy-Server-Setup-x64.exe'}
 $destinationPath=Join-Path $testRoot $kind
 $log=Join-Path $testRoot ($kind+'.log')
 $arguments=@('/VERYSILENT','/SUPPRESSMSGBOXES','/NORESTART','/CURRENTUSER','/EXTRACTONLY=1','/NOICONS',('/DIR="'+$destinationPath+'"'),('/LOG="'+$log+'"'))
 $setup=Start-Process -FilePath (Join-Path $PWD ('release\Installers\'+$name)) -ArgumentList $arguments -WindowStyle Hidden -PassThru -Wait
 if ($setup.ExitCode -ne 0) {throw "$kind installer extraction failed with code $($setup.ExitCode). See $log"}
 $source=Join-Path $PWD ('release\'+$kind)
 foreach ($file in (Get-ChildItem -LiteralPath $source -File -Recurse)) {
  $relative=[System.IO.Path]::GetRelativePath($source,$file.FullName)
  $installed=Join-Path $destinationPath $relative
  if (-not (Test-Path -LiteralPath $installed -PathType Leaf)) {throw "Missing payload: $installed"}
  if ((Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash -ne (Get-FileHash -LiteralPath $installed -Algorithm SHA256).Hash) {throw "Payload hash mismatch: $relative"}
 }
 $verified[$kind]=$true
 if ($kind -eq 'Server') {
  foreach ($extra in @('Database','Documentation')) {
   $extraRoot=Join-Path $PWD ('release\'+$extra)
   foreach ($file in (Get-ChildItem -LiteralPath $extraRoot -File -Recurse)) {
    $relative=[System.IO.Path]::GetRelativePath($extraRoot,$file.FullName)
    $installed=Join-Path (Join-Path $destinationPath $extra) $relative
    if (-not (Test-Path -LiteralPath $installed -PathType Leaf)) {throw "Missing supplemental payload: $installed"}
    if ((Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash -ne (Get-FileHash -LiteralPath $installed -Algorithm SHA256).Hash) {throw "Supplemental payload hash mismatch: $extra\$relative"}
   }
  }
  $verified.ServerSupplemental=$true
 }
}
if ($initialClientInstallerHash -ne (Get-FileHash release\Installers\DDHI-SmartPharmacy-Setup-x64.exe -Algorithm SHA256).Hash -or $initialServerInstallerHash -ne (Get-FileHash release\Installers\DDHI-SmartPharmacy-Server-Setup-x64.exe -Algorithm SHA256).Hash) {throw 'An installer changed during payload verification. Re-run verification after packaging has finished.'}
if (-not $verified.Client -or -not $verified.Server) {throw 'Both current installer payloads must be verified; run with -Target All.'}
if (-not $verified.ServerSupplemental) {throw 'The server database scripts and documentation must also be verified; run with -Target Server or All.'}
@{clientPayloadVerified=$verified.Client;serverPayloadVerified=$verified.Server;serverSupplementalPayloadVerified=$verified.ServerSupplemental;clientInstallerSha256=(Get-FileHash release\Installers\DDHI-SmartPharmacy-Setup-x64.exe -Algorithm SHA256).Hash;serverInstallerSha256=(Get-FileHash release\Installers\DDHI-SmartPharmacy-Server-Setup-x64.exe -Algorithm SHA256).Hash;mode='Extraction-only smoke test; no shortcuts, registry registration or service changes';cleanMachineInstall=$false;upgradeUninstallAccepted=$false;testDirectory=$testRoot} | ConvertTo-Json | Set-Content release\Tests\InstallerPayloadSmoke.json
Get-Content release\Tests\InstallerPayloadSmoke.json
exit 0
