$projectRoot = Split-Path -Parent $PSScriptRoot
$env:DOTNET_ROOT = Join-Path $projectRoot '.tools\dotnet'
$env:DOTNET_CLI_HOME = Join-Path $projectRoot '.tools\cli'
$env:NUGET_PACKAGES = Join-Path $projectRoot '.tools\nuget'
$env:DOTNET_CLI_TELEMETRY_OPTOUT='1'
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE='1'
$env:APPDATA = Join-Path $projectRoot '.tools\roaming'
$env:PATH = "$env:DOTNET_ROOT;$env:PATH"
New-Item -ItemType Directory -Path $env:APPDATA -Force | Out-Null
$env:NUGET_HTTP_CACHE_PATH = Join-Path $projectRoot '.tools\nuget-http'
$env:NUGET_SCRATCH = Join-Path $projectRoot '.tools\nuget-scratch'
$env:TEMP = Join-Path $projectRoot 'work\tmp'
$env:TMP = $env:TEMP
New-Item -ItemType Directory -Path $env:TEMP,$env:NUGET_SCRATCH,$env:NUGET_HTTP_CACHE_PATH -Force | Out-Null
$env:PYTHONPATH = Join-Path $projectRoot '.tools\python'
$env:PIP_CACHE_DIR = Join-Path $projectRoot '.tools\pip-cache'
