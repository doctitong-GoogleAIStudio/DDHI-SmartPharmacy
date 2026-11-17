$ErrorActionPreference='Stop'
Set-Location (Split-Path -Parent $PSScriptRoot)
. "$PSScriptRoot\Environment.ps1"
$env:VSTEST_ARTIFACTS_DIRECTORY=Join-Path $PWD 'work\vstest'
dotnet build tests\IntegrationTests -c Release --no-restore -m:1 --disable-build-servers *> logs\build-tests.log
if ($LASTEXITCODE -ne 0) { Get-Content logs\build-tests.log -Tail 30; throw 'Test build failed.' }
$pgStart=Start-Process -FilePath "$PWD\.tools\postgres\pgsql\bin\pg_ctl.exe" -ArgumentList @('-D',('"'+"$PWD\work\pgdata"+'"'),'-l',('"'+"$PWD\logs\postgres.log"+'"'),'-o','"-p 55432 -h 127.0.0.1 -c shared_buffers=32MB -c max_connections=40"','-w','start') -WindowStyle Hidden -PassThru
$pgStart.WaitForExit()
if ($pgStart.ExitCode -ne 0) { throw 'Isolated database failed to start.' }
try {
$env:PGHOST='127.0.0.1';$env:PGPORT='55432';$env:PGUSER='ddhi_test';$env:PGDATABASE='ddhi_test_workflows';$env:PGPASSWORD=Get-Content work\pg-password.txt -Raw
$env:DDHI_TEST_CONNECTION='Host=127.0.0.1;Port=55432;Database=ddhi_test_workflows;Username=ddhi_test;Password='+$env:PGPASSWORD
$beforeUpgrade= & '.\.tools\postgres\pgsql\bin\psql.exe' -At -f tests\IntegrationTests\upgrade-fingerprint.sql
if($LASTEXITCODE -ne 0){throw 'Cannot read pre-upgrade QA fingerprint.'}
$env:ConnectionStrings__Pharmacy=$env:DDHI_TEST_CONNECTION
try {
 dotnet src\DDHI.Pharmacy.Api\bin\Release\net10.0\DDHI.Pharmacy.Api.dll --migrate *> logs\schema-upgrade-test.log
 if($LASTEXITCODE -ne 0){throw 'QA schema upgrade failed.'}
} finally {Remove-Item Env:\ConnectionStrings__Pharmacy -ErrorAction SilentlyContinue}
$afterUpgrade= & '.\.tools\postgres\pgsql\bin\psql.exe' -At -f tests\IntegrationTests\upgrade-fingerprint.sql
if($LASTEXITCODE -ne 0 -or ($beforeUpgrade -join '|') -ne ($afterUpgrade -join '|')){throw 'Schema upgrade changed existing catalog, account or stock data.'}
@{countsPreserved=$true;legacyFieldsPreserved=$true;isolatedDatabase=$true;fingerprint=$afterUpgrade}|ConvertTo-Json|Set-Content release\Tests\MedicineSchemaUpgrade.json

dotnet test tests\UnitTests -c Release --no-build --logger 'trx;LogFileName=UnitTestsFinal.trx' --results-directory release\Tests *> logs\unit-final.log
if ($LASTEXITCODE -ne 0) { throw 'Unit tests failed.' }
dotnet test tests\IntegrationTests -c Release --no-build --logger 'trx;LogFileName=IntegrationTestsFinal.trx' --results-directory release\Tests *> logs\integration-final.log
if ($LASTEXITCODE -ne 0) { Get-Content logs\integration-final.log -Tail 40; throw 'Integration tests failed.' }
$query='SELECT COUNT(*) FROM "Medicines"; SELECT COUNT(*) FROM "StockMovements"; SELECT COUNT(*) FROM "AuditEvents"; SELECT SUM("Quantity") FROM "InventoryBalances";'
$sourceCounts= & '.\.tools\postgres\pgsql\bin\psql.exe' -At -c $query
if ($LASTEXITCODE -ne 0) { throw 'Source count query failed.' }
$env:DDHI_BACKUP_PASSWORD=[Convert]::ToHexString([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
$backupFile=Join-Path $PWD ('work\verified-backup-'+[Guid]::NewGuid().ToString('N')+'.ddhibak')
dotnet src\DDHI.Pharmacy.Tools\bin\Release\net10.0\DDHI.Pharmacy.Tools.dll backup "$PWD\.tools\postgres\pgsql\bin\pg_dump.exe" $backupFile *> logs\backup-final.log
if ($LASTEXITCODE -ne 0) { throw 'Backup failed.' }
$env:PGDATABASE='ddhi_restore_'+[Guid]::NewGuid().ToString('N')
& '.\.tools\postgres\pgsql\bin\createdb.exe' $env:PGDATABASE
if ($LASTEXITCODE -ne 0) { throw 'Restore database creation failed.' }
dotnet src\DDHI.Pharmacy.Tools\bin\Release\net10.0\DDHI.Pharmacy.Tools.dll restore-test "$PWD\.tools\postgres\pgsql\bin\pg_restore.exe" $backupFile *>> logs\backup-final.log
if ($LASTEXITCODE -ne 0) { throw 'Restore failed.' }
$restoredCounts= & '.\.tools\postgres\pgsql\bin\psql.exe' -At -c $query
if ($LASTEXITCODE -ne 0 -or ($sourceCounts -join '|') -ne ($restoredCounts -join '|')) { throw 'Restored counts differ from source.' }
@{backupAuthenticated=$true;restoreSucceeded=$true;countsMatch=$true;counts=$sourceCounts;restoreDatabase=$env:PGDATABASE} | ConvertTo-Json | Set-Content release\Tests\BackupRestore.json
$env:DDHI_TEST_CONNECTION='Host=127.0.0.1;Port=55432;Database='+$env:PGDATABASE+';Username=ddhi_test;Password='+$env:PGPASSWORD
dotnet test tests\IntegrationTests -c Release --no-build --filter 'FullyQualifiedName~AuditChainValid|FullyQualifiedName~ReadEndpointsWork' --logger 'trx;LogFileName=RestoredDatabaseFinal.trx' --results-directory release\Tests *> logs\restored-final.log
if ($LASTEXITCODE -ne 0) { Get-Content logs\restored-final.log -Tail 40; throw 'Restored database verification failed.' }
Get-Content logs\unit-final.log -Tail 6
Get-Content logs\integration-final.log -Tail 6
Get-Content logs\restored-final.log -Tail 6
Get-Content release\Tests\BackupRestore.json
} finally { & '.\.tools\postgres\pgsql\bin\pg_ctl.exe' -D work\pgdata -m fast -w stop }
