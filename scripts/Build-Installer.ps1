param(
    [string]$InnoCompiler = ""
)

$ErrorActionPreference = "Stop"
$Root = Split-Path -Parent $PSScriptRoot
$Prerequisites = Join-Path $Root "prerequisites"
$Dist = Join-Path $Root "dist"

& (Join-Path $PSScriptRoot "Build.ps1")

New-Item -ItemType Directory -Force $Prerequisites | Out-Null
New-Item -ItemType Directory -Force $Dist | Out-Null

$WebView2Setup = Join-Path $Prerequisites "MicrosoftEdgeWebview2Setup.exe"
$DotNet48Setup = Join-Path $Prerequisites "ndp48-web.exe"

if (!(Test-Path $WebView2Setup)) {
    Write-Host "Downloading official Microsoft WebView2 Evergreen Bootstrapper..."
    Invoke-WebRequest -Uri "https://go.microsoft.com/fwlink/p/?LinkId=2124703" -OutFile $WebView2Setup
}

if (!(Test-Path $DotNet48Setup)) {
    Write-Host "Downloading official Microsoft .NET Framework 4.8 web installer..."
    Invoke-WebRequest -Uri "https://go.microsoft.com/fwlink/?linkid=2088631" -OutFile $DotNet48Setup
}

if ([string]::IsNullOrWhiteSpace($InnoCompiler)) {
    $Candidates = @(
        "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe",
        "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
        "$env:ProgramFiles\Inno Setup 6\ISCC.exe"
    )
    $InnoCompiler = $Candidates | Where-Object { Test-Path $_ } | Select-Object -First 1
}

if ([string]::IsNullOrWhiteSpace($InnoCompiler) -or !(Test-Path $InnoCompiler)) {
    throw "Inno Setup 6 no esta instalado. Instala Inno Setup y vuelve a ejecutar scripts\Build-Installer.ps1."
}

& $InnoCompiler (Join-Path $Root "installer\GXLightBrowser.iss")
if ($LASTEXITCODE -ne 0) {
    throw "Inno Setup fallo con codigo $LASTEXITCODE."
}

Write-Host "Installer created in $Dist"
