$ErrorActionPreference = "Stop"
$Root = Split-Path -Parent $PSScriptRoot
$Exe = Join-Path $Root "bin\GXLightBrowser.exe"

if (!(Test-Path $Exe)) {
    & (Join-Path $PSScriptRoot "Build.ps1")
}

Start-Process -FilePath $Exe
