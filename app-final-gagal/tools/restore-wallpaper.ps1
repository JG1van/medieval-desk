[CmdletBinding(SupportsShouldProcess = $true)]
param(
    [Parameter(Mandatory = $true, Position = 0)]
    [ValidateNotNullOrEmpty()]
    [string]$ImagePath
)

$ErrorActionPreference = 'Stop'

$full = [System.IO.Path]::GetFullPath($ImagePath)
if (-not (Test-Path -LiteralPath $full -PathType Leaf)) {
    throw "File wallpaper tidak ditemukan: $full"
}
if ((Get-Item -LiteralPath $full).Length -eq 0) {
    throw "File wallpaper kosong: $full"
}

$allowed = @('.bmp', '.jpg', '.jpeg', '.png', '.gif', '.tif', '.tiff')
$ext = [System.IO.Path]::GetExtension($full).ToLowerInvariant()
if ($allowed -notcontains $ext) {
    throw "Format tidak didukung Windows untuk wallpaper: $ext"
}

if (-not ('MedievalDesk.WallpaperNative' -as [type])) {
    Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;

namespace MedievalDesk
{
    public static class WallpaperNative
    {
        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool SystemParametersInfo(uint action, uint param, string value, uint flags);
    }
}
'@
}

$SPI_SETDESKWALLPAPER = 0x0014
$SPIF_UPDATEINIFILE = 0x01
$SPIF_SENDCHANGE = 0x02

if ($PSCmdlet.ShouldProcess($full, 'Set sebagai wallpaper desktop')) {
    $ok = [MedievalDesk.WallpaperNative]::SystemParametersInfo(
        $SPI_SETDESKWALLPAPER, 0, $full, ($SPIF_UPDATEINIFILE -bor $SPIF_SENDCHANGE))

    if (-not $ok) {
        $code = [System.Runtime.InteropServices.Marshal]::GetLastWin32Error()
        throw "SystemParametersInfo gagal (Win32 error $code)."
    }

    Write-Output "Wallpaper dipulihkan: $full"
}
else {
    Write-Output "WhatIf: wallpaper tidak diubah. Target: $full"
}
