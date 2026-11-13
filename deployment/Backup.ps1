param([Parameter(Mandatory=$true)][string]$ToolPath,[Parameter(Mandatory=$true)][string]$PostgresBin,[Parameter(Mandatory=$true)][string]$Destination)
$ErrorActionPreference='Stop'
if (-not $env:DDHI_BACKUP_PASSWORD) { throw 'Supply DDHI_BACKUP_PASSWORD from the hospital secret store in this process, never in this script or task arguments.' }
if (-not (Test-Path -LiteralPath $Destination -PathType Container)) { throw 'Approved off-server backup destination is not available.' }
$file=Join-Path $Destination ('DDHI-'+(Get-Date -Format 'yyyyMMdd-HHmmss')+'.ddhibak')
& $ToolPath backup (Join-Path $PostgresBin 'pg_dump.exe') $file
if ($LASTEXITCODE -ne 0) { throw 'Encrypted backup failed.' }
Write-Output "Backup authenticated: $file"
