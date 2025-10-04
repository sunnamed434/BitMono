# BitMono Unity - Setup Test Project
Write-Host "BitMono Unity - Setup Test Project" -ForegroundColor Green

$TestProject = Join-Path (Split-Path $PSScriptRoot -Parent) "..\..\..\test\BitMono.Unity.TestProject\Assets\BitMono.Unity"

# Clean and create directories
if (Test-Path $TestProject) { Remove-Item -Recurse -Force $TestProject }
New-Item -ItemType Directory -Path $TestProject -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $TestProject "Editor") -Force | Out-Null

# Copy files from main source to test project
Copy-Item "..\Editor\*.cs" (Join-Path $TestProject "Editor") -Force
Copy-Item "..\Editor\*.asmdef" (Join-Path $TestProject "Editor") -Force
Copy-Item "..\package.json" $TestProject -Force
Copy-Item "..\README.md" $TestProject -Force

# Copy asset files if they exist
if (Test-Path "..\BitMonoConfig.asset") { Copy-Item "..\BitMonoConfig.asset" $TestProject -Force }
if (Test-Path "..\BitMonoConfig.asset.meta") { Copy-Item "..\BitMonoConfig.asset.meta" $TestProject -Force }

# Build BitMono.CLI if needed
$CliSourcePath = "..\..\..\src\BitMono.CLI\bin\Release\net462\BitMono.CLI.exe"
if (!(Test-Path $CliSourcePath)) {
    Write-Host "Building BitMono.CLI..." -ForegroundColor Yellow
    dotnet build "..\..\..\src\BitMono.CLI\BitMono.CLI.csproj" --configuration Release
}

# Copy BitMono.CLI to project root (outside Assets)
$ProjectRoot = Split-Path $TestProject -Parent
$CliPath = Join-Path (Split-Path $ProjectRoot -Parent) "BitMono.CLI"
if (!(Test-Path $CliPath)) { New-Item -ItemType Directory -Path $CliPath -Force | Out-Null }
if (Test-Path $CliSourcePath) { 
    Copy-Item "..\..\..\src\BitMono.CLI\bin\Release\net462\*" $CliPath -Recurse -Force
    Write-Host "BitMono.CLI copied successfully" -ForegroundColor Green
} else {
    Write-Host "ERROR: BitMono.CLI build failed" -ForegroundColor Red
}

# Config files are now auto-detected from BitMono.CLI location

Write-Host "✅ Done! Open Unity and refresh (Ctrl+R)" -ForegroundColor Green
Read-Host "Press Enter to continue"
