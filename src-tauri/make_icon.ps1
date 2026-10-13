$w = 64
$h = 64

function New-IcoBmp {
    param([int]$w, [int]$h, [string]$path)

    $xorSize = $w * $h * 4
    $maskRow = [math]::Floor(($w + 31) / 32) * 4
    $maskSize = $maskRow * $h
    $bihSize = 40
    $imgSize = $bihSize + $xorSize + $maskSize

    $ms = New-Object System.IO.MemoryStream
    $bw = New-Object System.IO.BinaryWriter $ms

    $bw.Write([UInt16]0)
    $bw.Write([UInt16]1)
    $bw.Write([UInt16]1)
    $bw.Write([Byte]$w)
    $bw.Write([Byte]$h)
    $bw.Write([Byte]0)
    $bw.Write([Byte]0)
    $bw.Write([UInt16]1)
    $bw.Write([UInt16]32)
    $bw.Write([UInt32]$imgSize)
    $bw.Write([UInt32]22)

    $bw.Write([UInt32]40)
    $bw.Write([Int32]$w)
    $bw.Write([Int32]($h * 2))
    $bw.Write([UInt16]1)
    $bw.Write([UInt16]32)
    $bw.Write([UInt32]0)
    $bw.Write([UInt32]$xorSize)
    $bw.Write([Int32]2835)
    $bw.Write([Int32]2835)
    $bw.Write([UInt32]0)
    $bw.Write([UInt32]0)

    for ($y = $h - 1; $y -ge 0; $y--) {
        for ($x = 0; $x -lt $w; $x++) {
            $dx = $x - 32
            $dy = $y - 32
            $d = [math]::Sqrt($dx * $dx + $dy * $dy)
            if ($d -lt 30) {
                if ($d -lt 18) { $b = 60; $g = 40; $r = 30 }
                elseif ($d -lt 28) { $b = 220; $g = 175; $r = 45 }
                else { $b = 70; $g = 48; $r = 36 }
                $bw.Write([Byte]$b)
                $bw.Write([Byte]$g)
                $bw.Write([Byte]$r)
                $bw.Write([Byte]255)
            } else {
                $bw.Write([Byte]0)
                $bw.Write([Byte]0)
                $bw.Write([Byte]0)
                $bw.Write([Byte]0)
            }
        }
    }

    for ($i = 0; $i -lt $maskSize; $i++) { $bw.Write([Byte]0) }

    $bw.Flush()
    [System.IO.File]::WriteAllBytes($path, $ms.ToArray())
    $ms.Dispose()
}

$icoDir = Join-Path $PSScriptRoot "icons"
New-Item -ItemType Directory -Force -Path $icoDir | Out-Null
New-IcoBmp -w $w -h $h -path "$icoDir\icon.ico"
Write-Output "Icon written: $((Get-Item "$icoDir\icon.ico").Length) bytes"