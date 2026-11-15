[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)][ValidatePattern('^[A-F0-9]{16}$')][string]$InstanceId,
    [Parameter(Mandatory=$true)][string]$ApiExecutable,
    [Parameter(Mandatory=$true)][string]$WorkerExecutable,
    [ValidateRange(1024,65535)][int]$ApiPort=8443,
    [ValidateRange(1024,65535)][int]$DiscoveryPort=8444,
    [string]$InterfaceName,
    [switch]$Disable
)
$ErrorActionPreference='Stop'
try {
    $rules=@(
        @{Name="DDHI-$InstanceId-HTTPS"; Program=$ApiExecutable; Protocol='TCP'; Port=$ApiPort},
        @{Name="DDHI-$InstanceId-Discovery"; Program=$WorkerExecutable; Protocol='UDP'; Port=$DiscoveryPort}
    )
    foreach($rule in $rules) {
        $existing=Get-NetFirewallRule -Name $rule.Name -ErrorAction SilentlyContinue
        if($existing -and $existing.Group -ne "DDHI-SmartPharmacy-$InstanceId") {throw 'A network rule with this name belongs to another application.'}
    }
    if(-not $Disable) {
        if([string]::IsNullOrWhiteSpace($InterfaceName)){throw 'Choose the hospital LAN adapter.'}
        if(-not(Test-Path -LiteralPath $ApiExecutable) -or -not(Test-Path -LiteralPath $WorkerExecutable)){throw 'The server components are missing.'}
        Get-NetAdapter -Name $InterfaceName -ErrorAction Stop | Out-Null
    }
    foreach($rule in $rules) {
        if(Get-NetFirewallRule -Name $rule.Name -ErrorAction SilentlyContinue){Remove-NetFirewallRule -Name $rule.Name}
        if(-not $Disable) {
            New-NetFirewallRule -Name $rule.Name -DisplayName 'DDHI Pharmacy local network' -Group "DDHI-SmartPharmacy-$InstanceId" -Direction Inbound -Action Allow -Protocol $rule.Protocol -LocalPort $rule.Port -Program $rule.Program -InterfaceAlias $InterfaceName -RemoteAddress LocalSubnet -Profile Any | Out-Null
        }
    }
    Write-Output 'DDHI local-network rules updated; other firewall rules were retained.'
    exit 0
} catch {Write-Error $_ -ErrorAction Continue;exit 1}
