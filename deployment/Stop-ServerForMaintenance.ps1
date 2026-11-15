[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$InstallRoot, [switch]$RemoveServices)
$ErrorActionPreference = 'Stop'
try {
    $rootPath = [IO.Path]::GetFullPath($InstallRoot).TrimEnd('\')
    $components = @(
        @{Name='DDHIPharmacyWorker'; Binary='Worker\DDHI.Pharmacy.Worker.exe'},
        @{Name='DDHIPharmacyApi'; Binary='Api\DDHI.Pharmacy.Api.exe'},
        @{Name='DDHIPostgreSQL'; Binary='PostgreSQL\bin\pg_ctl.exe'}
    )
    $ownedServices = @()
    # Validate all names before stopping anything; never take over another installation.
    foreach ($component in $components) {
        $registryPath = 'HKLM:\SYSTEM\CurrentControlSet\Services\' + $component.Name
        if (-not (Test-Path -LiteralPath $registryPath)) { continue }
        $service = Get-Service -Name $component.Name -ErrorAction Stop
        $imagePath = [Environment]::ExpandEnvironmentVariables((Get-ItemProperty -LiteralPath $registryPath -Name ImagePath).ImagePath).Trim()
        $expected = Join-Path $rootPath $component.Binary
        $quoted = '"' + $expected + '"'
        $matchesQuoted = $imagePath.Equals($quoted, [StringComparison]::OrdinalIgnoreCase) -or $imagePath.StartsWith($quoted + ' ', [StringComparison]::OrdinalIgnoreCase)
        $matchesBare = $imagePath.Equals($expected, [StringComparison]::OrdinalIgnoreCase) -or $imagePath.StartsWith($expected + ' ', [StringComparison]::OrdinalIgnoreCase)
        if (-not ($matchesQuoted -or $matchesBare)) { throw ($component.Name + ' belongs to a different installation. Maintenance was refused.') }
        $ownedServices += $service
    }
    foreach ($service in $ownedServices) {
        $service.Refresh()
        if ($service.Status -eq [System.ServiceProcess.ServiceControllerStatus]::StartPending) {
            $service.WaitForStatus([System.ServiceProcess.ServiceControllerStatus]::Running, [TimeSpan]::FromSeconds(60))
        }
        $service.Refresh()
        if ($service.Status -ne [System.ServiceProcess.ServiceControllerStatus]::Stopped) {
            if ($service.Status -ne [System.ServiceProcess.ServiceControllerStatus]::StopPending) { $service.Stop() }
            $service.WaitForStatus([System.ServiceProcess.ServiceControllerStatus]::Stopped, [TimeSpan]::FromSeconds(60))
        }
    }
    if ($RemoveServices) {
        foreach ($service in $ownedServices) {
            & "$env:SystemRoot\System32\sc.exe" delete $service.ServiceName | Out-Null
            if ($LASTEXITCODE -ne 0) { throw ('Could not remove owned service ' + $service.ServiceName) }
        }
    }
    Write-Output 'Owned services stopped. Database and protected configuration files were retained.'
    exit 0
}
catch {
    Write-Error $_ -ErrorAction Continue
    exit 1
}
