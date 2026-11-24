$ErrorActionPreference='Stop'
Set-Location (Split-Path -Parent $PSScriptRoot)
. "$PSScriptRoot\Environment.ps1"
function Check { if ($LASTEXITCODE -ne 0) { throw 'Build command failed.' } }
dotnet restore DDHI.SmartPharmacy.sln --configfile "$PWD\NuGet.Config" --disable-parallel -m:1 -p:RestorePackagesWithLockFile=true; Check
dotnet build DDHI.SmartPharmacy.sln -c Release --no-restore -m:1 --disable-build-servers; Check
dotnet test tests\UnitTests\UnitTests.csproj -c Release --no-build --logger "trx;LogFileName=UnitTestsFinal.trx" --results-directory release\Tests; Check
if (-not $env:DDHI_TEST_CONNECTION) { throw 'Set DDHI_TEST_CONNECTION to the isolated test database before the required integration tests.' }
dotnet test tests\IntegrationTests\IntegrationTests.csproj -c Release --no-build --logger "trx;LogFileName=IntegrationTestsFinal.trx" --results-directory release\Tests; Check
dotnet publish src\DDHI.Pharmacy.Desktop -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:DebugType=None -m:1 -o publish\windows-x64; Check
Copy-Item publish\windows-x64\* release\Client -Recurse -Force
foreach ($project in @('Api','Worker','Tools')) { dotnet publish "src\DDHI.Pharmacy.$project" -c Release -r win-x64 --self-contained true -p:DebugType=None -m:1 -o "release\Server\$project"; Check }
dotnet publish src\DDHI.Pharmacy.ServerManager -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:DebugType=None -m:1 -o release\Server\Manager; Check
& scripts\Stage-PostgreSQL.ps1
dotnet test tests\ServerManagerTests -c Release --filter 'FullyQualifiedName!~AuditSessionClientTests' --logger "trx;LogFileName=ServerManagerTests.trx" --results-directory release\Tests -m:1 --disable-build-servers; Check
dotnet test tests\ServerManagerTests -c Release --no-build --filter 'FullyQualifiedName~AuditSessionClientTests' --logger "trx;LogFileName=ClientSessionTests.trx" --results-directory release\Tests; Check
python scripts\Manuals.py; Check
python scripts\Make-LanConnectionGuide.py; Check
python scripts\Make-CompleteManual.py; Check
python scripts\Make-CompleteManual.py --source docs/DRUG_REFERENCE_GUIDE.md --output docs/DDHI-Drug-Reference-Guide.pdf --title "Drug Reference & Interaction Checker"; Check
python scripts\Make-ClinicalTemplates.py; Check
New-Item -ItemType Directory -Path release\Client\Documentation -Force | Out-Null
Copy-Item docs\DDHI-Server-and-Client-Connection-Guide.pdf release\Client\Documentation -Force
Copy-Item docs\MEDICINE_IMPORTER_DISTRIBUTOR.md release\Client\Documentation -Force
Copy-Item docs\AUDIT_TRAIL.md release\Client\Documentation -Force
Copy-Item docs\DDHI-SmartPharmacy-Complete-User-Manual.pdf release\Client\Documentation -Force
Copy-Item docs\DDHI-SmartPharmacy-Complete-User-Manual.pdf release\Documentation -Force
Copy-Item docs\DDHI-SmartPharmacy-Complete-User-Manual.pdf "release\Documentation\Pharmacy User Manual.pdf" -Force
Copy-Item docs\DDHI-Server-and-Client-Connection-Guide.pdf release\Documentation -Force
Copy-Item docs\*.md release\Documentation -Force
Copy-Item docs\DDHI-Drug-Reference-Guide.pdf release\Documentation -Force
Copy-Item docs\DDHI-Drug-Reference-Guide.pdf release\Client\Documentation -Force
Copy-Item docs\DRUG_REFERENCE*.md release\Client\Documentation -Force
New-Item -ItemType Directory -Path release\Documentation\ClinicalTemplates -Force | Out-Null
Copy-Item samples\clinical-reference\* release\Documentation\ClinicalTemplates -Recurse -Force
New-Item -ItemType Directory -Path release\Client\Documentation\ClinicalTemplates -Force | Out-Null
Copy-Item samples\clinical-reference\* release\Client\Documentation\ClinicalTemplates -Recurse -Force
Copy-Item docs\ThirdParty release\Documentation -Recurse -Force
Copy-Item docs\ThirdParty release\Client\Licenses -Recurse -Force
Copy-Item docs\ThirdParty release\Server\Licenses -Recurse -Force
Copy-Item deployment release\Server -Recurse -Force
Copy-Item database\* release\Database -Recurse -Force
Copy-Item assets\branding\* release\Branding -Force
Copy-Item LICENSE.txt,THIRD_PARTY_NOTICES.md release\Client -Force
& .tools\inno\ISCC.exe installer\client.iss; Check
& .tools\inno\ISCC.exe installer\server.iss; Check
& scripts\Test-Installers.ps1
python scripts\Package.py
Check
