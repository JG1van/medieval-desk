# Run this from src-tauri directory
$ErrorActionPreference = "Stop"

# Source the vcvars script
& "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\VC\Auxiliary\Build\vcvars64.bat"

if ($env:PATH -notmatch "buildtools\bin") {
    Write-Host "VCvars64.bat failed" -ForegroundColor Red
    exit 1
}

# Ensure cargo is in PATH
if ($env:PATH -notmatch "\.cargo\\bin") {
    $env:PATH += ";$env:USERPROFILE\.cargo\bin"
}

Write-Host "PATH=$env:PATH"
Write-Host "link.exe:" (Get-Command link.exe 2>$null)

# Run cargo check
& cargo check 2>&1
