$ErrorActionPreference = 'Stop'

function Write-HookResult {
    param([Parameter(Mandatory)][hashtable] $Result)
    [Console]::Out.WriteLine(($Result | ConvertTo-Json -Depth 8 -Compress))
}

function Invoke-VerificationCommand {
    param(
        [Parameter(Mandatory)][string] $Name,
        [Parameter(Mandatory)][string] $FileName,
        [Parameter(Mandatory)][string[]] $Arguments
    )

    $startInfo = [System.Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = $FileName
    $startInfo.WorkingDirectory = $script:RepoRoot
    $startInfo.UseShellExecute = $false
    $startInfo.CreateNoWindow = $true
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true
    foreach ($argument in $Arguments) {
        [void]$startInfo.ArgumentList.Add($argument)
    }

    $process = [System.Diagnostics.Process]::new()
    $process.StartInfo = $startInfo
    try {
        if (-not $process.Start()) {
            throw "Could not start $FileName."
        }
        $stdoutTask = $process.StandardOutput.ReadToEndAsync()
        $stderrTask = $process.StandardError.ReadToEndAsync()
        $process.WaitForExit()
        $stdout = $stdoutTask.GetAwaiter().GetResult()
        $stderr = $stderrTask.GetAwaiter().GetResult()

        $combined = @($stdout.TrimEnd(), $stderr.TrimEnd()) |
            Where-Object { $_ -ne '' } |
            Join-String -Separator "`n"

        [pscustomobject]@{
            Name       = $Name
            ExitCode   = $process.ExitCode
            Output     = $combined
        }
    }
    finally {
        $process.Dispose()
    }
}

function Add-Result {
    param([Parameter(Mandatory)][string] $Name, [Parameter(Mandatory)][string] $Status)
    $script:Results.Add([pscustomobject]@{ Name = $Name; Status = $Status })
}

function Format-Failure {
    param([Parameter(Mandatory)] $Result)
    $output = [string]$Result.Output
    if ($output.Length -gt 12000) {
        $output = "[Output truncated; showing final 12,000 characters]`n" + $output.Substring($output.Length - 12000)
    }
    if ([string]::IsNullOrWhiteSpace($output)) {
        $output = '(The command produced no output.)'
    }
    return "### $($Result.Name) (exit code $($Result.ExitCode))`n$output"
}

$hookInput = $null
try {
    $rawInput = [Console]::In.ReadToEnd()
    if (-not [string]::IsNullOrWhiteSpace($rawInput)) {
        $hookInput = $rawInput | ConvertFrom-Json -AsHashtable
    }

    $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
    $script:Results = [System.Collections.Generic.List[object]]::new()
    Set-Location $script:RepoRoot

    $restore = Invoke-VerificationCommand -Name 'dotnet restore' -FileName 'dotnet' -Arguments @('restore')
    $script:Results.Add($restore)

    if ($restore.ExitCode -eq 0) {
        $format = Invoke-VerificationCommand -Name 'dotnet format --verify-no-changes' -FileName 'dotnet' -Arguments @('format', '--verify-no-changes', '--no-restore')
        $script:Results.Add($format)

        $build = Invoke-VerificationCommand -Name 'dotnet build --no-restore' -FileName 'dotnet' -Arguments @('build', '--no-restore')
        $script:Results.Add($build)

        if ($build.ExitCode -eq 0) {
            $tests = Invoke-VerificationCommand -Name 'dotnet test --no-build --no-restore' -FileName 'dotnet' -Arguments @('test', '--no-build', '--no-restore')
            $script:Results.Add($tests)

            $coverage = Invoke-VerificationCommand -Name 'Test-UnitCoverage.ps1' -FileName 'pwsh' -Arguments @('-NoProfile', '-File', 'scripts/Test-UnitCoverage.ps1')
            $script:Results.Add($coverage)
        }
        else {
            Add-Result -Name 'dotnet test --no-build --no-restore' -Status 'Skipped because the build failed; stale test binaries are not used.'
            Add-Result -Name 'Test-UnitCoverage.ps1' -Status 'Skipped because the build failed.'
        }
    }
    else {
        Add-Result -Name 'dotnet format --verify-no-changes' -Status 'Skipped because restore failed.'
        Add-Result -Name 'dotnet build --no-restore' -Status 'Skipped because restore failed.'
        Add-Result -Name 'dotnet test --no-build --no-restore' -Status 'Skipped because restore failed.'
        Add-Result -Name 'Test-UnitCoverage.ps1' -Status 'Skipped because restore failed.'
    }

    $failed = @($script:Results | Where-Object {
        ($_.PSObject.Properties.Name -contains 'ExitCode' -and $_.ExitCode -ne 0) -or
        ($_.PSObject.Properties.Name -contains 'Status' -and $_.Status -like 'Skipped*')
    })

    $sessionId = [string]$hookInput.session_id
    if ([string]::IsNullOrWhiteSpace($sessionId)) {
        $sessionId = 'unknown-session'
    }
    $safeSessionId = $sessionId -replace '[^A-Za-z0-9_-]', '_'
    $attemptStatePath = Join-Path ([System.IO.Path]::GetTempPath()) "codex-verification-$safeSessionId.json"

    if ($failed.Count -eq 0) {
        Remove-Item -LiteralPath $attemptStatePath -Force -ErrorAction SilentlyContinue
        exit 0
    }

    $failureDetails = foreach ($result in $failed) {
        if ($result.PSObject.Properties.Name -contains 'ExitCode') {
            Format-Failure -Result $result
        }
        else {
            "### $($result.Name)`n$($result.Status)"
        }
    }
    $attemptCount = 0
    if (Test-Path -LiteralPath $attemptStatePath) {
        try {
            $storedState = Get-Content -LiteralPath $attemptStatePath -Raw | ConvertFrom-Json
            $attemptCount = [int]$storedState.attempts
        }
        catch {
            $attemptCount = 0
        }
    }

    $maxRepairPasses = 3
    if ($attemptCount -ge $maxRepairPasses) {
        Remove-Item -LiteralPath $attemptStatePath -Force -ErrorAction SilentlyContinue
        Write-HookResult @{ systemMessage = "Project verification still fails after $maxRepairPasses automatic repair passes. Review and fix the remaining issues before reporting completion.`n`n$($failureDetails -join "`n`n")" }
        exit 0
    }

    $attemptCount++
    @{ attempts = $attemptCount } | ConvertTo-Json -Compress | Set-Content -LiteralPath $attemptStatePath -Encoding utf8
    $reason = @(
        "Project verification did not pass. This is automatic repair pass $attemptCount of $maxRepairPasses. Fix the reported failures, then allow the verification hook to run again. Do not weaken tests or the coverage gate to make them pass."
        ($failureDetails -join "`n`n")
    ) -join "`n`n"

    Write-HookResult @{ decision = 'block'; reason = $reason }
    exit 0
}
catch {
    $message = "Verification hook failed before it could complete: $($_.Exception.Message)"
    if ($hookInput -and $hookInput.stop_hook_active -eq $true) {
        Write-HookResult @{ systemMessage = $message }
    }
    else {
        Write-HookResult @{ decision = 'block'; reason = $message }
    }
    exit 0
}
