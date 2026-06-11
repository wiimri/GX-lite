param(
    [string]$OutputPath = ""
)

$ErrorActionPreference = "Stop"
$Root = Split-Path -Parent $PSScriptRoot
if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path $Root "assets\GanBrowser.ico"
}

Add-Type -AssemblyName System.Drawing
$directory = Split-Path -Parent $OutputPath
New-Item -ItemType Directory -Force $directory | Out-Null

$bitmap = New-Object System.Drawing.Bitmap 64, 64
$graphics = [System.Drawing.Graphics]::FromImage($bitmap)
$graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$graphics.Clear([System.Drawing.Color]::FromArgb(10, 13, 18))

$accent = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(76, 224, 190)), 6
$accent.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
$accent.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
$graphics.DrawArc($accent, 9, 9, 46, 46, 35, 290)

$bar = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(245, 248, 250)), 6
$bar.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
$bar.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
$graphics.DrawLine($bar, 34, 32, 52, 32)
$graphics.DrawLine($bar, 51, 32, 51, 46)

$icon = [System.Drawing.Icon]::FromHandle($bitmap.GetHicon())
$stream = [System.IO.File]::Open($OutputPath, [System.IO.FileMode]::Create)
$icon.Save($stream)
$stream.Dispose()
$icon.Dispose()
$bar.Dispose()
$accent.Dispose()
$graphics.Dispose()
$bitmap.Dispose()

Write-Host "Generated $OutputPath"
