param([Parameter(Mandatory=$true)][string]$ProtectedLauncher,[string]$At='02:00')
# The approved launcher retrieves database and encryption credentials from the hospital secret store,
# sets PG* and DDHI_BACKUP_PASSWORD for this process, then invokes Backup.ps1.
$ErrorActionPreference='Stop'
if (-not (Test-Path -LiteralPath $ProtectedLauncher -PathType Leaf)) { throw 'Hospital-provisioned protected secret-store launcher required.' }
$action=New-ScheduledTaskAction -Execute 'powershell.exe' -Argument ('-NoProfile -NonInteractive -File "'+$ProtectedLauncher+'"')
$trigger=New-ScheduledTaskTrigger -Daily -At $At
$principal=New-ScheduledTaskPrincipal -UserId 'SYSTEM' -LogonType ServiceAccount
Register-ScheduledTask -TaskName 'DDHI Encrypted PostgreSQL Backup' -Action $action -Trigger $trigger -Principal $principal
