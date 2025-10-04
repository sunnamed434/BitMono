# BitMono Unity - Create Unity Package
param(
    [string]$OutputPath = ""
)

Write-Host "BitMono Unity - Create Unity Package" -ForegroundColor Green

# Check if output path argument is provided
if ([string]::IsNullOrEmpty($OutputPath)) {
    Write-Host "No output path provided, using default test project location" -ForegroundColor Yellow
    $OutputPath = Join-Path (Split-Path $PSScriptRoot -Parent) "..\..\..\test\BitMono.Unity.TestProject\Assets\BitMono.Unity"
    $IsTestProject = $true
} else {
    Write-Host "Output path provided: $OutputPath" -ForegroundColor Green
    $IsTestProject = $false
}

# Clean and create output directory
if (Test-Path $OutputPath) { Remove-Item -Recurse -Force $OutputPath }
New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $OutputPath "Editor") -Force | Out-Null

# Copy Unity package files
Copy-Item "..\Editor\*.cs" (Join-Path $OutputPath "Editor") -Force
Copy-Item "..\Editor\*.asmdef" (Join-Path $OutputPath "Editor") -Force
Copy-Item "..\package.json" $OutputPath -Force
Copy-Item "..\README.md" $OutputPath -Force

# Copy asset files if they exist
if (Test-Path "..\BitMonoConfig.asset") { Copy-Item "..\BitMonoConfig.asset" $OutputPath -Force }
if (Test-Path "..\BitMonoConfig.asset.meta") { Copy-Item "..\BitMonoConfig.asset.meta" $OutputPath -Force }

# If this is for the test project, also copy BitMono.CLI
if ($IsTestProject) {
    Write-Host "Setting up test project with BitMono.CLI..." -ForegroundColor Yellow
    
    # Build BitMono.CLI if needed
    $CliSourcePath = "..\..\..\src\BitMono.CLI\bin\Release\net462\BitMono.CLI.exe"
    if (!(Test-Path $CliSourcePath)) {
        Write-Host "Building BitMono.CLI..." -ForegroundColor Yellow
        dotnet build "..\..\..\src\BitMono.CLI\BitMono.CLI.csproj" --configuration Release
    }
    
    # Copy BitMono.CLI to test project root
    $ProjectRoot = Split-Path $OutputPath -Parent
    $CliPath = Join-Path (Split-Path $ProjectRoot -Parent) "BitMono.CLI"
    if (!(Test-Path $CliPath)) { New-Item -ItemType Directory -Path $CliPath -Force | Out-Null }
    
    if (Test-Path $CliSourcePath) { 
        Copy-Item "..\..\..\src\BitMono.CLI\bin\Release\net462\*" $CliPath -Recurse -Force
        Write-Host "BitMono.CLI copied successfully" -ForegroundColor Green
    } else {
        Write-Host "ERROR: BitMono.CLI build failed" -ForegroundColor Red
    }
} else {
    Write-Host "Creating standalone Unity package (no BitMono.CLI included)" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "✅ Unity package created successfully!" -ForegroundColor Green
Write-Host "Location: $OutputPath" -ForegroundColor Cyan

if ($IsTestProject) {
    Write-Host "Type: Test Project Setup (includes BitMono.CLI)" -ForegroundColor Yellow
    Write-Host "Next: Open Unity and refresh (Ctrl+R)" -ForegroundColor Yellow
} else {
    Write-Host "Type: Standalone Unity Package" -ForegroundColor Yellow
    Write-Host "Next: Import this package into your Unity project" -ForegroundColor Yellow
}

Write-Host ""
Read-Host "Press Enter to continue"
