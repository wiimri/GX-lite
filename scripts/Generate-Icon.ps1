param(
    [string]$OutputPath = ""
)

$ErrorActionPreference = "Stop"
$Root = Split-Path -Parent $PSScriptRoot
if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path $Root "assets\GXLight.ico"
}

Add-Type -AssemblyName System.Drawing
$directory = Split-Path -Parent $OutputPath
New-Item -ItemType Directory -Force $directory | Out-Null

$bitmap = New-Object System.Drawing.Bitmap 64, 64
$graphics = [System.Drawing.Graphics]::FromImage($bitmap)
$graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$graphics.Clear([System.Drawing.Color]::FromArgb(13, 15, 20))

$accent = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(114, 245, 255)), 6
$accent.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
$accent.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
$graphics.DrawArc($accent, 10, 10, 44, 44, 35, 290)

$inner = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(255, 61, 96))
$graphics.FillEllipse($inner, 25, 25, 14, 14)

$icon = [System.Drawing.Icon]::FromHandle($bitmap.GetHicon())
$stream = [System.IO.File]::Open($OutputPath, [System.IO.FileMode]::Create)
$icon.Save($stream)
$stream.Dispose()
$icon.Dispose()
$inner.Dispose()
$accent.Dispose()
$graphics.Dispose()
$bitmap.Dispose()

Write-Host "Generated $OutputPath"
