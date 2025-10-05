# BitMono Unity - Create Unity Package
param(
    [string]$OutputPath = "",
    [string]$Version = "",
    [switch]$IncludeCli,
    [switch]$Verbose,
    [switch]$TestProject
)

if ($Verbose) {
    $VerbosePreference = "Continue"
}

Write-Host "BitMono Unity - Create Unity Package" -ForegroundColor Green
Write-Host "Version: $Version" -ForegroundColor Cyan
Write-Host "Output Path: $OutputPath" -ForegroundColor Cyan
Write-Host "Include CLI: $IncludeCli" -ForegroundColor Cyan

# Set defaults for manual usage
if ([string]::IsNullOrEmpty($OutputPath)) {
    if ($TestProject) {
        $OutputPath = Join-Path (Split-Path $PSScriptRoot -Parent) "..\..\..\test\BitMono.Unity.TestProject\Assets\BitMono.Unity"
        Write-Host "Using test project location: $OutputPath" -ForegroundColor Yellow
    } else {
        Write-Error "OutputPath parameter is required for CI/CD usage. Use -TestProject switch for manual test project setup."
        exit 1
    }
}

if ([string]::IsNullOrEmpty($Version)) {
    if ($TestProject) {
        $Version = "0.1.0"
        Write-Host "Using default version for test project: $Version" -ForegroundColor Yellow
    } else {
        Write-Error "Version parameter is required for CI/CD usage"
        exit 1
    }
}

# Clean and create output directory
if (Test-Path $OutputPath) { 
    Write-Verbose "Removing existing output directory: $OutputPath"
    Remove-Item -Recurse -Force $OutputPath 
}
New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $OutputPath "Editor") -Force | Out-Null

Write-Host "Created output directory: $OutputPath" -ForegroundColor Green

# Copy Unity package files
Write-Host "Copying Unity package files..." -ForegroundColor Yellow

# Copy Editor scripts
$editorSource = Join-Path $PSScriptRoot "..\Editor"
if (Test-Path $editorSource) {
    Copy-Item "$editorSource\*.cs" (Join-Path $OutputPath "Editor") -Force
    Copy-Item "$editorSource\*.asmdef" (Join-Path $OutputPath "Editor") -Force -ErrorAction SilentlyContinue
    Write-Host "Copied Editor scripts" -ForegroundColor Green
} else {
    Write-Error "Editor directory not found: $editorSource"
    exit 1
}

# Copy package.json and update version
$packageJsonSource = Join-Path $PSScriptRoot "..\package.json"
if (Test-Path $packageJsonSource) {
    $packageJson = Get-Content $packageJsonSource -Raw | ConvertFrom-Json
    $packageJson.version = $Version
    $packageJson | ConvertTo-Json -Depth 10 | Set-Content (Join-Path $OutputPath "package.json")
    Write-Host "Copied and updated package.json with version: $Version" -ForegroundColor Green
} else {
    Write-Error "package.json not found: $packageJsonSource"
    exit 1
}

# Copy asset files if they exist
$assetSource = Join-Path $PSScriptRoot "..\BitMonoConfig.asset"
if (Test-Path $assetSource) { 
    Copy-Item $assetSource $OutputPath -Force
    Write-Host "Copied BitMonoConfig.asset" -ForegroundColor Green
}

$assetMetaSource = Join-Path $PSScriptRoot "..\BitMonoConfig.asset.meta"
if (Test-Path $assetMetaSource) { 
    Copy-Item $assetMetaSource $OutputPath -Force
    Write-Host "Copied BitMonoConfig.asset.meta" -ForegroundColor Green
}

# Copy BitMono.CLI if requested (default: true)
if ($IncludeCli -or (-not $PSBoundParameters.ContainsKey('IncludeCli'))) {
    Write-Host "Including BitMono.CLI..." -ForegroundColor Yellow
    
    # For test project, copy CLI to project root
    if ($TestProject) {
        $projectRoot = Split-Path $OutputPath -Parent
        $cliDestPath = Join-Path (Split-Path $projectRoot -Parent) "BitMono.CLI"
    } else {
        $cliDestPath = Join-Path $OutputPath "BitMono.CLI"
    }
    
    # Look for BitMono.CLI in various locations
    $cliPaths = @(
        "..\..\..\src\BitMono.CLI\bin\Release\net462\win-x64",
        "..\..\..\src\BitMono.CLI\bin\Release\net462",
        "..\..\..\BitMono.CLI",
        "..\..\..\src\BitMono.CLI\bin\Release\net462\linux-x64"
    )
    
    $cliFound = $false
    foreach ($cliPath in $cliPaths) {
        $fullCliPath = Join-Path $PSScriptRoot $cliPath
        if (Test-Path $fullCliPath) {
            if (!(Test-Path $cliDestPath)) { 
                New-Item -ItemType Directory -Path $cliDestPath -Force | Out-Null 
            }
            
            Copy-Item "$fullCliPath\*" $cliDestPath -Recurse -Force
            Write-Host "Copied BitMono.CLI from: $fullCliPath to: $cliDestPath" -ForegroundColor Green
            $cliFound = $true
            break
        }
    }
    
    if (-not $cliFound) {
        Write-Warning "BitMono.CLI not found in any expected location. Creating package without CLI."
    }
}


Write-Host ""
Write-Host "✅ Unity package created successfully!" -ForegroundColor Green
Write-Host "Location: $OutputPath" -ForegroundColor Cyan
Write-Host "Version: $Version" -ForegroundColor Cyan

if ($TestProject) {
    Write-Host "Type: Test Project Setup (includes BitMono.CLI)" -ForegroundColor Yellow
    Write-Host "Next: Open Unity and refresh (Ctrl+R)" -ForegroundColor Yellow
} else {
    Write-Host "Type: Standalone Unity Package" -ForegroundColor Yellow
    Write-Host "Next: Import this package into your Unity project" -ForegroundColor Yellow
}

# List contents for verification
Write-Host ""
Write-Host "Package contents:" -ForegroundColor Yellow
Get-ChildItem $OutputPath -Recurse | ForEach-Object {
    $relativePath = $_.FullName.Substring($OutputPath.Length + 1)
    Write-Host "  $relativePath" -ForegroundColor Gray
}

# Pause for manual usage
if ($TestProject) {
    Write-Host ""
    Read-Host "Press Enter to continue"
}
