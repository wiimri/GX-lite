param(
    [Parameter(Mandatory = $true)]
    [string]$Version,

    [Parameter(Mandatory = $true)]
    [string[]]$Assets,

    [string]$Repository = "wiimri/GX-lite",
    [string]$Title = "",
    [string]$Notes = ""
)

$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($Title)) {
    $Title = "GX Light Browser $Version"
}
if ([string]::IsNullOrWhiteSpace($Notes)) {
    $Notes = "Instalador de GX Light Browser $Version para Windows 10/11 x64."
}

$credentialLines = "protocol=https`nhost=github.com`n`n" | git credential fill
$credential = @{}
foreach ($line in $credentialLines) {
    $equals = $line.IndexOf("=")
    if ($equals -gt 0) {
        $credential[$line.Substring(0, $equals)] = $line.Substring($equals + 1)
    }
}

if (!$credential.ContainsKey("password") -or [string]::IsNullOrWhiteSpace($credential["password"])) {
    throw "Git Credential Manager no entrego una credencial utilizable para GitHub."
}

$headers = @{
    Accept = "application/vnd.github+json"
    Authorization = "Bearer " + $credential["password"]
    "X-GitHub-Api-Version" = "2022-11-28"
    "User-Agent" = "GXLightBrowser-Release-Script"
}

$tag = "v" + $Version
$releaseApi = "https://api.github.com/repos/$Repository/releases/tags/$tag"
$release = $null
try {
    $release = Invoke-RestMethod -Uri $releaseApi -Headers $headers -Method Get
}
catch {
    if ($_.Exception.Response.StatusCode.value__ -ne 404) {
        throw
    }
}

if ($null -eq $release) {
    $payloadJson = @{
        tag_name = $tag
        target_commitish = "main"
        name = $Title
        body = $Notes
        draft = $false
        prerelease = $false
    } | ConvertTo-Json -Compress
    $payload = [System.Text.Encoding]::UTF8.GetBytes($payloadJson)
    $release = Invoke-RestMethod -Uri "https://api.github.com/repos/$Repository/releases" `
        -Headers $headers -Method Post -Body $payload -ContentType "application/json; charset=utf-8"
}

$uploadBase = $release.upload_url.Split("{")[0]
$expandedAssets = @()
foreach ($assetGroup in $Assets) {
    $expandedAssets += $assetGroup.Split(",", [System.StringSplitOptions]::RemoveEmptyEntries)
}

foreach ($asset in $expandedAssets) {
    $resolved = Resolve-Path $asset
    $name = [Uri]::EscapeDataString([System.IO.Path]::GetFileName($resolved))

    foreach ($existing in $release.assets) {
        if ($existing.name -eq [System.IO.Path]::GetFileName($resolved)) {
            Invoke-RestMethod -Uri "https://api.github.com/repos/$Repository/releases/assets/$($existing.id)" `
                -Headers $headers -Method Delete | Out-Null
        }
    }

    Write-Host "Uploading $resolved..."
    Invoke-RestMethod -Uri ($uploadBase + "?name=" + $name) -Headers $headers -Method Post `
        -ContentType "application/octet-stream" -InFile $resolved | Out-Null
}

Write-Host "Published $($release.html_url)"
