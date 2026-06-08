$ErrorActionPreference = "Stop"
$Root = Split-Path -Parent $PSScriptRoot
$Bin = Join-Path $Root "bin"
$Out = Join-Path $Bin "PrivacyFirewallProbe.exe"
$Csc = "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe"

if (!(Test-Path $Csc)) {
    throw "Could not find .NET Framework C# compiler at $Csc"
}

New-Item -ItemType Directory -Force $Bin | Out-Null

& $Csc /nologo /target:exe /platform:x64 `
    /out:$Out `
    /reference:System.dll `
    (Join-Path $Root "src\PrivacyFirewall.cs") `
    (Join-Path $Root "tests\PrivacyFirewallProbe.cs")

if ($LASTEXITCODE -ne 0) {
    throw "C# compiler failed with exit code $LASTEXITCODE."
}

& $Out
if ($LASTEXITCODE -ne 0) {
    throw "Privacy firewall probe failed with exit code $LASTEXITCODE."
}
