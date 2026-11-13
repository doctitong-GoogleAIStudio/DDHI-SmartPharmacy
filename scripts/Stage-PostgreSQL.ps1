$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$archivePath = Join-Path $projectRoot '.tools\postgresql-17.12.zip'
$expectedHash = 'a2cd8a774389347b70be7748ac4a37ef43cb6ee539c5fc3e85ae264b15e4a04c'
if (-not (Test-Path -LiteralPath $archivePath)) {
    Invoke-WebRequest 'https://get.enterprisedb.com/postgresql/postgresql-17.12-1-windows-x64-binaries.zip' -OutFile $archivePath
}
if ((Get-FileHash -LiteralPath $archivePath -Algorithm SHA256).Hash.ToLowerInvariant() -ne $expectedHash) { throw 'PostgreSQL archive checksum mismatch.' }
Add-Type -AssemblyName System.IO.Compression.FileSystem
$archive = [IO.Compression.ZipFile]::OpenRead($archivePath)
try {
    $destinationRoot = Join-Path $projectRoot 'release\Server\PostgreSQL'
    foreach ($entry in $archive.Entries) {
        if ($entry.FullName -notmatch '^pgsql/(bin/|lib/|share/|server_license.txt$)' -or $entry.FullName.EndsWith('/')) { continue }
        $relative = $entry.FullName.Substring(6)
        $destination = [IO.Path]::GetFullPath((Join-Path $destinationRoot $relative))
        if (-not $destination.StartsWith($destinationRoot + '\', [StringComparison]::OrdinalIgnoreCase)) { throw 'Unsafe ZIP entry.' }
        [IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($destination)) | Out-Null
        [IO.Compression.ZipFileExtensions]::ExtractToFile($entry, $destination, $true)
    }
} finally { $archive.Dispose() }
