#requires -Version 7.0
$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot 'Coverage.psm1') -Force
try {
    Invoke-UnitCoverageGate -RepositoryRoot (Split-Path $PSScriptRoot -Parent) | Out-Null
    exit 0
}
catch {
    Write-Error $_ -ErrorAction Continue
    exit 1
}
