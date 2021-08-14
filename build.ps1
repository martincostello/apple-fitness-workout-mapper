#! /usr/bin/pwsh
param(
    [Parameter(Mandatory = $false)][string] $OutputPath = "",
    [Parameter(Mandatory = $false)][switch] $SkipTests
)

$ErrorActionPreference = "Stop"
$global:ProgressPreference = "SilentlyContinue"

$solutionPath = Split-Path $MyInvocation.MyCommand.Definition
$sdkFile = Join-Path $solutionPath "global.json"

$dotnetVersion = (Get-Content $sdkFile | Out-String | ConvertFrom-Json).sdk.version

if ($OutputPath -eq "") {
    $OutputPath = Join-Path "$(Convert-Path "$PSScriptRoot")" "artifacts"
}

$installDotNetSdk = $false;

if (($null -eq (Get-Command "dotnet" -ErrorAction SilentlyContinue)) -and ($null -eq (Get-Command "dotnet.exe" -ErrorAction SilentlyContinue))) {
    Write-Host "The .NET Core SDK is not installed."
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
        Write-Host "The required version of the .NET Core SDK is not installed. Expected $dotnetVersion."
        $installDotNetSdk = $true
    }
}

if ($installDotNetSdk -eq $true) {

    $env:DOTNET_INSTALL_DIR = Join-Path "$(Convert-Path "$PSScriptRoot")" ".dotnetcli"
    $sdkPath = Join-Path $env:DOTNET_INSTALL_DIR "sdk\$dotnetVersion"

    if (!(Test-Path $sdkPath)) {
        if (!(Test-Path $env:DOTNET_INSTALL_DIR)) {
            mkdir $env:DOTNET_INSTALL_DIR | Out-Null
        }
        [Net.ServicePointManager]::SecurityProtocol = [Net.ServicePointManager]::SecurityProtocol -bor "Tls12"

        if (($PSVersionTable.PSVersion.Major -ge 6) -And !$IsWindows) {
            $installScript = Join-Path $env:DOTNET_INSTALL_DIR "install.sh"
            Invoke-WebRequest "https://dot.net/v1/dotnet-install.sh" -OutFile $installScript -UseBasicParsing
            chmod +x $installScript
            & $installScript --version "$dotnetVersion" --install-dir "$env:DOTNET_INSTALL_DIR" --no-path
        }
        else {
            $installScript = Join-Path $env:DOTNET_INSTALL_DIR "install.ps1"
            Invoke-WebRequest "https://dot.net/v1/dotnet-install.ps1" -OutFile $installScript -UseBasicParsing
            & $installScript -Version "$dotnetVersion" -InstallDir "$env:DOTNET_INSTALL_DIR" -NoPath
        }
    }
}
else {
    $env:DOTNET_INSTALL_DIR = Split-Path -Path (Get-Command dotnet).Path
}

$dotnet = Join-Path "$env:DOTNET_INSTALL_DIR" "dotnet"

if ($installDotNetSdk -eq $true) {
    $env:PATH = "$env:DOTNET_INSTALL_DIR;$env:PATH"
}

function DotNetTest {
    param([string]$Project)

    $nugetPath = $env:NUGET_PACKAGES ?? (Join-Path ($env:USERPROFILE ?? "~") ".nuget\packages")
    $propsFile = Join-Path $solutionPath "Directory.Packages.props"
    $reportGeneratorVersion = (Select-Xml -Path $propsFile -XPath "//PackageVersion[@Include='ReportGenerator']/@Version").Node.'#text'
    $reportGeneratorPath = Join-Path $nugetPath "reportgenerator\$reportGeneratorVersion\tools\net5.0\ReportGenerator.dll"

    $coverageOutput = Join-Path $OutputPath "coverage.cobertura.xml"
    $reportOutput = Join-Path $OutputPath "coverage"

    & $dotnet test $Project --output $OutputPath -- RunConfiguration.TestSessionTimeout=1200000

    $dotNetTestExitCode = $LASTEXITCODE

    if (Test-Path $coverageOutput) {
        & $dotnet `
            $reportGeneratorPath `
            `"-reports:$coverageOutput`" `
            `"-targetdir:$reportOutput`" `
            -reporttypes:HTML `
            -verbosity:Warning
    }

    if ($dotNetTestExitCode -ne 0) {
        throw "dotnet test failed with exit code $dotNetTestExitCode"
    }
}

function DotNetPublish {
    param([string]$Project, [string] $Runtime, [string] $PublishPath)

    $additionalArgs = @()

    if (![string]::IsNullOrEmpty($Runtime)) {
        $additionalArgs += "--runtime"
        $additionalArgs += $Runtime
    }

    & $dotnet publish $Project --output $PublishPath --configuration "Release" $additionalArgs

    if ($LASTEXITCODE -ne 0) {
        throw "dotnet publish failed with exit code $LASTEXITCODE"
    }
}

$testProjects = @(
    (Join-Path $solutionPath "tests\AppleFitnessWorkoutMapper.Tests\AppleFitnessWorkoutMapper.Tests.csproj")
)

$publishProjects = @(
    (Join-Path $solutionPath "src\AppleFitnessWorkoutMapper\AppleFitnessWorkoutMapper.csproj")
)

Write-Host "Publishing solution..." -ForegroundColor Green
ForEach ($project in $publishProjects) {

    $runtimes = @(
        "linux-x64",
        "osx-x64",
        "win-x64"
    )

    $publishRootPath = (Join-Path $OutputPath "publish")

    ForEach ($runtime in $runtimes) {

        $publishPath = (Join-Path $publishRootPath $runtime)
        DotNetPublish $project $runtime $publishPath

        if ($null -ne $env:GITHUB_ACTIONS) {
            $zipPath = (Join-Path $publishRootPath ("AppleFitnessWorkoutMapper-" + $runtime + ".zip"))
            Compress-Archive -Path ($publishPath + "/*") -DestinationPath $zipPath -Force
        }
    }

    $publishPath = (Join-Path $publishRootPath "portable")
    DotNetPublish $project "" $publishPath

    if ($null -ne $env:GITHUB_ACTIONS) {
        $zipPath = (Join-Path $publishRootPath ("AppleFitnessWorkoutMapper.zip"))
        Compress-Archive -Path ($publishPath + "/*") -DestinationPath $zipPath -Force
    }
}

if ($SkipTests -eq $false) {
    Write-Host "Testing $($testProjects.Count) project(s)..." -ForegroundColor Green
    ForEach ($project in $testProjects) {
        DotNetTest $project
    }
}
