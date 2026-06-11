$ErrorActionPreference = "Stop"
$Target = Join-Path $env:LOCALAPPDATA "GXLightBrowser\filters.txt"
$Dir = Split-Path -Parent $Target
New-Item -ItemType Directory -Force $Dir | Out-Null

$Lists = @(
    "https://easylist.to/easylist/easylist.txt",
    "https://easylist.to/easylist/easyprivacy.txt",
    "https://raw.githubusercontent.com/brave/adblock-lists/master/brave-lists/brave-firstparty-cname.txt"
)

"! Gan Browser filter bundle" | Set-Content -Path $Target -Encoding UTF8
"! Updated $(Get-Date -Format s)" | Add-Content -Path $Target -Encoding UTF8

foreach ($Url in $Lists) {
    Write-Host "Fetching $Url"
    $Content = Invoke-WebRequest -Uri $Url -UseBasicParsing
    "" | Add-Content -Path $Target -Encoding UTF8
    "! Source: $Url" | Add-Content -Path $Target -Encoding UTF8
    $Content.Content | Add-Content -Path $Target -Encoding UTF8
}

Write-Host "Updated $Target"
