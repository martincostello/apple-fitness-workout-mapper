#! /usr/bin/env pwsh

#Requires -PSEdition Core
#Requires -Version 7

param(
    [Parameter(Mandatory = $false)][switch] $SkipTests
)

$ErrorActionPreference = "Stop"
$InformationPreference = "Continue"
$global:ProgressPreference = "SilentlyContinue"

$solutionPath = $PSScriptRoot
$sdkFile = Join-Path $solutionPath "global.json"

$dotnetVersion = (Get-Content $sdkFile | Out-String | ConvertFrom-Json).sdk.version

$installDotNetSdk = $false;

if (($null -eq (Get-Command "dotnet" -ErrorAction SilentlyContinue)) -and ($null -eq (Get-Command "dotnet.exe" -ErrorAction SilentlyContinue))) {
    Write-Information "The .NET SDK is not installed."
    $installDotNetSdk = $true
}
else {
    Try {
        $installedDotNetVersion = (dotnet --version 2>&1 | Out-String).Trim()
    }
    Catch {
        $installedDotNetVersion = "?"
    }

    if ($installedDotNetVersion -ne $dotnetVersion) {
        Write-Information "The required version of the .NET SDK is not installed. Expected $dotnetVersion."
        $installDotNetSdk = $true
    }
}

if ($installDotNetSdk) {

    ${env:DOTNET_INSTALL_DIR} = Join-Path $PSScriptRoot ".dotnet"
    $sdkPath = Join-Path ${env:DOTNET_INSTALL_DIR} "sdk" $dotnetVersion

    if (!(Test-Path $sdkPath)) {
        if (!(Test-Path ${env:DOTNET_INSTALL_DIR})) {
            mkdir ${env:DOTNET_INSTALL_DIR} | Out-Null
        }
        [Net.ServicePointManager]::SecurityProtocol = [Net.ServicePointManager]::SecurityProtocol -bor "Tls12"
        if (($PSVersionTable.PSVersion.Major -ge 6) -And !$IsWindows) {
            $installScript = Join-Path ${env:DOTNET_INSTALL_DIR} "install.sh"
            Invoke-WebRequest "https://dot.net/v1/dotnet-install.sh" -OutFile $installScript -UseBasicParsing
            chmod +x $installScript
            & $installScript --jsonfile $sdkFile --install-dir ${env:DOTNET_INSTALL_DIR} --no-path --skip-non-versioned-files
        }
        else {
            $installScript = Join-Path ${env:DOTNET_INSTALL_DIR} "install.ps1"
            Invoke-WebRequest "https://dot.net/v1/dotnet-install.ps1" -OutFile $installScript -UseBasicParsing
            & $installScript -JsonFile $sdkFile -InstallDir ${env:DOTNET_INSTALL_DIR} -NoPath -SkipNonVersionedFiles
        }
    }
}
else {
    ${env:DOTNET_INSTALL_DIR} = Split-Path -Path (Get-Command dotnet).Path
}

$dotnet = Join-Path ${env:DOTNET_INSTALL_DIR} "dotnet"

if ($installDotNetSdk -eq $true) {
    ${env:PATH} = "${env:DOTNET_INSTALL_DIR};${env:PATH}"
}

function DotNetTest {
    param([string]$Project)

    $projectName = [System.IO.Path]::GetFileNameWithoutExtension($Project)
    $coverageOutputPath = Join-Path $solutionPath "artifacts" "coverage" $projectName

    & $dotnet test --project $Project --configuration "Release" --timeout "20m"

    if ($LASTEXITCODE -ne 0) {
        throw "dotnet test failed with exit code $LASTEXITCODE"
    }

    $coverageReports = @(
        Get-ChildItem -Path $coverageOutputPath -File -Filter "coverage.xml" -ErrorAction SilentlyContinue
        Get-ChildItem -Path $coverageOutputPath -File -Filter "coverage.*.xml" -ErrorAction SilentlyContinue |
            Where-Object { $_.Name -ne "coverage.xml" }
    ) | Select-Object -ExpandProperty FullName -Unique

    if ($coverageReports.Count -eq 0) {
        return
    }

    $coverageGitHubSummary = Join-Path $coverageOutputPath "SummaryGithub.md"
    $reportTypes = @(
        "Html"
        "MarkdownSummaryGithub"
    ) -join ";"

    $reportGeneratorArgs = @(
        "-reports:$($coverageReports -join ';')"
        "-targetdir:$coverageOutputPath"
        "-reporttypes:$reportTypes"
        "-title:$projectName"
        "-verbosity:Warning"
    )

    & $dotnet tool run reportgenerator @reportGeneratorArgs

    if ($LASTEXITCODE -ne 0) {
        throw "reportgenerator failed with exit code $LASTEXITCODE"
    }

    if (-Not [string]::IsNullOrEmpty(${env:GITHUB_STEP_SUMMARY}) -and (Test-Path -LiteralPath $coverageGitHubSummary)) {
        $targetFramework = $null

        $tfmMatch = $coverageReports |
            ForEach-Object {
                [System.Text.RegularExpressions.Regex]::Match(
                    [System.IO.Path]::GetFileName($_),
                    "^coverage\.(.+)\.xml$"
                )
            } |
            Where-Object Success |
            Select-Object -First 1

        if ($null -ne $tfmMatch) {
            $targetFramework = $tfmMatch.Groups[1].Value
        }

        $frameworkSuffix = if ([string]::IsNullOrWhiteSpace($targetFramework)) {
            ""
        }
        else {
            " ($targetFramework)"
        }

        $summaryContent = @(
            "<details><summary>:chart_with_upwards_trend: <b>$projectName Code Coverage report</b>$frameworkSuffix</summary>"
            ""
            (Get-Content -LiteralPath $coverageGitHubSummary -Raw)
            ""
            "</details>"
        ) -join [System.Environment]::NewLine

        try {
            Add-Content -LiteralPath ${env:GITHUB_STEP_SUMMARY} -Value $summaryContent
        }
        catch {
            Write-Warning "Failed to write GitHub step summary for ${Project}: $_"
        }
    }
}

function DotNetPublish {
    param(
        [string] $Project,
        [string] $Runtime
    )

    $additionalArgs = @()

    if (![string]::IsNullOrEmpty($Runtime)) {
        $additionalArgs += "--self-contained"
        $additionalArgs += "--runtime"
        $additionalArgs += $Runtime
    }

    & $dotnet publish $Project $additionalArgs

    if ($LASTEXITCODE -ne 0) {
        throw "dotnet publish failed with exit code $LASTEXITCODE"
    }
}

$publishProjects = @(
    (Join-Path $solutionPath "src" "AppleFitnessWorkoutMapper" "AppleFitnessWorkoutMapper.csproj")
)

Write-Information "Publishing solution..."
ForEach ($project in $publishProjects) {

    $runtimes = @(
        "linux-arm64",
        "linux-x64",
        "osx-arm64",
        "osx-x64",
        "win-arm64",
        "win-x64"
    )

    $publishRootPath = (Join-Path $PSScriptRoot "artifacts" "publish" "AppleFitnessWorkoutMapper")

    ForEach ($runtime in $runtimes) {

        $publishPath = (Join-Path $publishRootPath "release_$runtime")
        DotNetPublish $project $runtime

        if ($null -ne ${env:GITHUB_ACTIONS}) {
            $zipPath = (Join-Path $publishRootPath ("AppleFitnessWorkoutMapper-" + $runtime + ".zip"))
            Compress-Archive -Path ($publishPath + "/*") -DestinationPath $zipPath -Force
        }
    }

    $publishPath = (Join-Path $publishRootPath "release")
    DotNetPublish $project ""

    if ($null -ne ${env:GITHUB_ACTIONS}) {
        $zipPath = (Join-Path $publishRootPath ("AppleFitnessWorkoutMapper.zip"))
        Compress-Archive -Path ($publishPath + "/*") -DestinationPath $zipPath -Force
    }
}

if (-Not $SkipTests) {
    $testProjects = @(
        (Join-Path $solutionPath "tests" "AppleFitnessWorkoutMapper.Tests" "AppleFitnessWorkoutMapper.Tests.csproj")
    )

    Write-Information "Testing $($testProjects.Count) project(s)..."
    ForEach ($project in $testProjects) {
        DotNetTest $project
    }
}
