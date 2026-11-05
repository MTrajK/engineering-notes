#requires -Version 7.0
$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot 'Coverage.psm1') -Force
$workspace = Join-Path (Split-Path $PSScriptRoot -Parent) "artifacts/coverage/gate-verification/$([guid]::NewGuid().ToString('N'))"
New-Item -ItemType Directory -Path $workspace | Out-Null
$script:checks = 0
function Assert-Check([bool]$Condition, [string]$Message) {
    if (-not $Condition) { throw "Verification failed: $Message" }
    $script:checks++
}
function Assert-Rejected([scriptblock]$Action, [string]$Message) {
    $rejected = $false
    try { & $Action | Out-Null } catch { $rejected = $true }
    Assert-Check $rejected $Message
}
function New-Report([string]$Directory, [int]$DomainCovered = 10, [int]$DomainValid = 10, [int]$ApplicationCovered = 10, [int]$ApplicationValid = 10, [switch]$OmitApplication) {
    New-Item -ItemType Directory -Path $Directory -Force | Out-Null
    $modules = foreach ($name in @('WorkshopBooking.Domain', 'WorkshopBooking.Application')) {
        if ($name -eq 'WorkshopBooking.Application' -and $OmitApplication) { continue }
        $covered = if ($name -eq 'WorkshopBooking.Domain') { $DomainCovered } else { $ApplicationCovered }
        $valid = if ($name -eq 'WorkshopBooking.Domain') { $DomainValid } else { $ApplicationValid }
        $points = for ($i = 1; $i -le $valid; $i++) { '<SequencePoint vc="{0}" sl="{1}" el="{1}" fileid="1" />' -f $(if ($i -le $covered) { 1 } else { 0 }), $i }
        '<Module><ModuleName>{0}</ModuleName><Files><File uid="1" fullPath="{0}.cs" /></Files><Classes><Class><Methods><Method><SequencePoints>{1}</SequencePoints></Method></Methods></Class></Classes></Module>' -f $name, ($points -join '')
    }
    '<CoverageSession><Modules>{0}</Modules></CoverageSession>' -f ($modules -join '') | Set-Content (Join-Path $Directory 'coverage.opencover.xml')
}
function Evaluate([string]$Name, [int]$DC, [int]$DV, [int]$AC, [int]$AV, [bool]$Pass) {
    $directory = Join-Path $workspace $Name
    New-Report $directory $DC $DV $AC $AV
    $results = @(Get-UnitCoverage $directory)
    Assert-Check ((@($results | Where-Object { -not $_.Passed }).Count -eq 0) -eq $Pass) $Name
}
Evaluate 'below' 8 10 10 10 $false
Evaluate 'exactly90' 9 10 10 10 $false
Evaluate 'above90' 91 100 91 100 $true
Evaluate 'otherAssemblyFails' 10 10 9 10 $false
Evaluate 'combinedAveragePasses' 100 100 8 10 $false
Evaluate 'displayRoundsTo90' 90001 100000 10 10 $true
$missingAssembly = Join-Path $workspace 'missingAssembly'; New-Report $missingAssembly -OmitApplication
Assert-Rejected { Get-UnitCoverage $missingAssembly } 'missing assembly'
$zero = Join-Path $workspace 'zero'; New-Report $zero 0 0
Assert-Rejected { Get-UnitCoverage $zero } 'zero lines'
foreach ($kind in @('missing', 'empty', 'malformed', 'unusable', 'invalidPoint')) {
    $directory = Join-Path $workspace $kind; New-Item -ItemType Directory -Path $directory | Out-Null
    $path = Join-Path $directory 'coverage.opencover.xml'
    switch ($kind) {
        'empty' { Set-Content $path '' -NoNewline }
        'malformed' { Set-Content $path '<CoverageSession' }
        'unusable' { Set-Content $path '<Unrelated />' }
        'invalidPoint' { New-Report $directory; (Get-Content $path -Raw).Replace('vc="1"', 'vc="invalid"') | Set-Content $path }
    }
    Assert-Rejected { Get-UnitCoverage $directory } $kind
}
$multiple = Join-Path $workspace 'multiple'
New-Report (Join-Path $multiple 'one') 5 10 5 10
New-Report (Join-Path $multiple 'two') 10 10 10 10
$merged = @(Get-UnitCoverage $multiple)
Assert-Check ($merged[0].Valid -eq 10 -and $merged[0].Covered -eq 10 -and $merged[1].Valid -eq 10) 'multiple reports merge source lines, not counts'
$root = Join-Path $workspace 'fakeRepository'; New-Item -ItemType Directory -Path $root | Out-Null
New-Report (Join-Path $root 'artifacts/coverage/unit/stale')
Assert-Rejected { Invoke-UnitCoverageGate $root -UnitTestRunner { param($r, $d) } } 'stale output is not reused'
Assert-Rejected { Invoke-UnitCoverageGate $root -UnitTestRunner { param($r, $d) New-Report $d; throw 'Unit tests failed (exit 17).' } } 'test failure wins over passing data'
$results = @(Invoke-UnitCoverageGate $root -UnitTestRunner { param($r, $d) New-Report $d })
Assert-Check ($results.Count -eq 2 -and $results[0].Passed -and $results[1].Passed) 'complete passing gate'
Assert-Rejected { Invoke-UnitCoverageGate $root -UnitTestRunner { param($r, $d) New-Report $d 9 10 } } 'gate enforces strict threshold'
# Check actual command exit behavior with a fake dotnet command, never business-code edits.
$command = Join-Path $PSScriptRoot 'Test-UnitCoverage.ps1'
$escaped = $command.Replace("'", "''")
$failureOutput = & pwsh -NoProfile -Command "function global:dotnet { `$global:LASTEXITCODE = 17 }; & '$escaped'" 2>&1
$failureOutput | Out-Host
Assert-Check ($LASTEXITCODE -ne 0 -and ($failureOutput -join ' ') -match 'Unit tests failed \(exit 17\)') 'unit test exit code propagates through command'
Write-Host "Coverage verification passed: $script:checks checks. Synthetic artifacts: $workspace"
