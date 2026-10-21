[CmdletBinding()]
param(
    [string]$Configuration = 'Release',
    [string]$Runtime = 'win-x64',
    [int]$ExpectedAssets = 24
)

$ErrorActionPreference = 'Stop'
$final = Split-Path -Parent $PSScriptRoot
$project = Join-Path $final 'src\MedievalDesk\MedievalDesk.csproj'
$out = Join-Path $final "installers\$Runtime"
$staging = Join-Path $final "installers\.staging-$Runtime"

if (-not (Test-Path -LiteralPath $project)) {
    throw "Project tidak ditemukan: $project"
}

if (Test-Path -LiteralPath $staging) {
    Remove-Item -LiteralPath $staging -Recurse -Force
}

dotnet publish $project -c $Configuration -r $Runtime --self-contained true `
    -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true `
    -o $staging
if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish gagal dengan exit code $LASTEXITCODE"
}

$exe = Join-Path $staging 'MedievalDesk.exe'
$assets = Join-Path $staging 'Assets'
if (-not (Test-Path -LiteralPath $exe)) {
    throw "MedievalDesk.exe tidak dihasilkan."
}
if (-not (Test-Path -LiteralPath $assets)) {
    throw "Folder Assets tidak dihasilkan."
}

$assetCount = (Get-ChildItem -LiteralPath $assets -Recurse -File | Measure-Object).Count
if ($assetCount -ne $ExpectedAssets) {
    throw "Jumlah aset $assetCount, seharusnya $ExpectedAssets. Periksa metadata Content/ExcludeFromSingleFile."
}

if (Test-Path -LiteralPath $out) {
    Remove-Item -LiteralPath $out -Recurse -Force
}
Move-Item -LiteralPath $staging -Destination $out
Remove-Item -LiteralPath (Split-Path -Parent $staging) -Filter '.staging-*' -Recurse -Force -ErrorAction SilentlyContinue

$size = (Get-Item -LiteralPath (Join-Path $out 'MedievalDesk.exe')).Length
Write-Output "Publish selesai: $out"
Write-Output ("MedievalDesk.exe: {0:N0} byte" -f $size)
Write-Output "Assets: $assetCount file"
