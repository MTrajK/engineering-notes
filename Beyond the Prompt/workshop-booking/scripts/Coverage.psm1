Set-StrictMode -Version Latest

function Get-UnitCoverage {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$ReportDirectory)

    $expected = @('WorkshopBooking.Domain', 'WorkshopBooking.Application')
    $reports = @(Get-ChildItem -LiteralPath $ReportDirectory -Recurse -File -Filter coverage.opencover.xml -ErrorAction Stop)
    if ($reports.Count -eq 0) { throw 'No fresh OpenCover report was produced.' }
    $assemblies = @{}
    foreach ($name in $expected) { $assemblies[$name] = @{} }
    $seen = @{}
    foreach ($report in $reports) {
        if ($report.Length -eq 0) { throw "Empty coverage report: $($report.FullName)" }
        $settings = [System.Xml.XmlReaderSettings]::new()
        $settings.DtdProcessing = [System.Xml.DtdProcessing]::Prohibit
        $settings.XmlResolver = $null
        $reader = [System.Xml.XmlReader]::Create($report.FullName, $settings)
        try { $xml = [System.Xml.XmlDocument]::new(); $xml.Load($reader) }
        finally { $reader.Dispose() }
        $modules = $xml.SelectNodes('/CoverageSession/Modules/Module')
        if ($modules.Count -eq 0) { throw "Unusable OpenCover document: $($report.FullName)" }
        foreach ($module in $modules) {
            $nameNode = $module.SelectSingleNode('ModuleName')
            if ($null -eq $nameNode) { throw 'Coverage module is missing its name.' }
            $name = $nameNode.InnerText
            if ($name -notin $expected) { continue }
            $seen[$name] = $true
            $files = @{}
            foreach ($file in $module.SelectNodes('Files/File')) {
                $uid = $file.GetAttribute('uid')
                $path = $file.GetAttribute('fullPath')
                if ([string]::IsNullOrWhiteSpace($uid) -or [string]::IsNullOrWhiteSpace($path) -or $files.ContainsKey($uid)) { throw 'Invalid coverage file map.' }
                $files[$uid] = [System.IO.Path]::GetFullPath($path)
            }
            foreach ($method in $module.SelectNodes('Classes/Class/Methods/Method')) {
                foreach ($point in $method.SelectNodes('SequencePoints/SequencePoint')) {
                    $id = $point.GetAttribute('fileid')
                    if ([string]::IsNullOrEmpty($id)) {
                        $reference = $method.SelectSingleNode('FileRef')
                        if ($null -ne $reference) { $id = $reference.GetAttribute('uid') }
                    }
                    [long]$start = 0; [long]$end = 0; [long]$visits = 0
                    if (-not $files.ContainsKey($id) -or
                        -not [long]::TryParse($point.GetAttribute('sl'), [ref]$start) -or $start -le 0 -or
                        -not [long]::TryParse($point.GetAttribute('el'), [ref]$end) -or $end -lt $start -or
                        -not [long]::TryParse($point.GetAttribute('vc'), [ref]$visits) -or $visits -lt 0) { throw 'Invalid coverage sequence point.' }
                    for ($line = $start; $line -le $end; $line++) {
                        $key = "$($files[$id]):$line"
                        if (-not $assemblies[$name].ContainsKey($key)) { $assemblies[$name][$key] = $false }
                        if ($visits -gt 0) { $assemblies[$name][$key] = $true }
                    }
                }
            }
        }
    }
    foreach ($name in $expected) {
        if (-not $seen.ContainsKey($name)) { throw "Missing required assembly: $name" }
        [long]$valid = $assemblies[$name].Count
        if ($valid -eq 0) { throw "No measurable lines for $name" }
        [long]$covered = @($assemblies[$name].Values | Where-Object { $_ }).Count
        [pscustomobject]@{ Assembly = $name; Covered = $covered; Valid = $valid; Percent = 100.0 * $covered / $valid; Passed = ([decimal]$covered * 100 -gt [decimal]$valid * 90) }
    }
}

function Invoke-UnitCoverageGate {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string]$RepositoryRoot,
        [scriptblock]$UnitTestRunner = {
            param($root, $directory)
            & dotnet test (Join-Path $root 'tests/WorkshopBooking.UnitTests') '--collect:XPlat Code Coverage' '--results-directory' $directory '--' 'DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover'
            if ($LASTEXITCODE -ne 0) { throw "Unit tests failed (exit $LASTEXITCODE)." }
        }
    )
    # A new run directory avoids deleting developer artifacts and cannot reuse an old report.
    $directory = Join-Path $RepositoryRoot "artifacts/coverage/unit/$([guid]::NewGuid().ToString('N'))"
    New-Item -ItemType Directory -Path $directory -ErrorAction Stop | Out-Null
    Write-Host "Fresh unit coverage: $directory"
    & $UnitTestRunner $RepositoryRoot $directory | Out-Host
    $results = @(Get-UnitCoverage -ReportDirectory $directory)
    foreach ($result in $results) {
        Write-Host ('{0}: {1}/{2} lines ({3:F4}%) - {4}' -f $result.Assembly, $result.Covered, $result.Valid, $result.Percent, $(if ($result.Passed) { 'PASS' } else { 'FAIL' }))
    }
    if (@($results | Where-Object { -not $_.Passed }).Count -gt 0) { throw 'Each required assembly must have strictly greater than 90% unit line coverage.' }
    return $results
}

Export-ModuleMember -Function Get-UnitCoverage, Invoke-UnitCoverageGate
