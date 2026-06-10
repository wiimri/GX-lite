#define MyAppName "GX Light Browser"
#define MyAppVersion "1.14"
#define MyAppPublisher "wiimri"
#define MyAppURL "https://github.com/wiimri/GX-lite"
#define MyAppExeName "GXLightBrowser.exe"

[Setup]
AppId={{D742BA34-C13B-4BD9-A2EC-9CB15F7BC744}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
DefaultDirName={autopf}\GXLightBrowser
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
PrivilegesRequired=admin
UsePreviousAppDir=no
CloseApplications=yes
RestartApplications=no
AppMutex=GXLightBrowser-Install-Mutex
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
OutputDir=..\dist
OutputBaseFilename=GXLightBrowser-Setup-{#MyAppVersion}-x64
SetupIconFile=..\assets\GXLight.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
MinVersion=10.0

[Languages]
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"

[Files]
Source: "..\bin\GXLightBrowser.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Microsoft.Web.WebView2.Core.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Microsoft.Web.WebView2.WinForms.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\WebView2Loader.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\prerequisites\MicrosoftEdgeWebview2Setup.exe"; DestDir: "{tmp}"; Flags: deleteafterinstall
Source: "..\prerequisites\ndp48-web.exe"; DestDir: "{tmp}"; Flags: deleteafterinstall

[InstallDelete]
Type: filesandordirs; Name: "{localappdata}\Programs\GXLightBrowser"
Type: files; Name: "{userdesktop}\GX Light Browser.lnk"
Type: files; Name: "{userprograms}\GX Light Browser.lnk"

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Crear un acceso directo en el escritorio"; GroupDescription: "Accesos directos:"

[Run]
Filename: "{tmp}\ndp48-web.exe"; Parameters: "/q /norestart"; StatusMsg: "Instalando .NET Framework 4.8..."; Check: not IsDotNet48Installed; Flags: waituntilterminated
Filename: "{tmp}\MicrosoftEdgeWebview2Setup.exe"; Parameters: "/silent /install"; StatusMsg: "Instalando Microsoft Edge WebView2 Runtime..."; Check: not IsWebView2Installed; Flags: waituntilterminated
Filename: "{app}\{#MyAppExeName}"; Description: "Abrir {#MyAppName}"; Flags: nowait postinstall skipifsilent

[Code]
const
  WebView2ClientId = '{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}';

function IsDotNet48Installed: Boolean;
var
  Release: Cardinal;
begin
  Result := RegQueryDWordValue(HKLM, 'SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full', 'Release', Release)
    and (Release >= 528040);
end;

function HasWebView2Version(RootKey: Integer; KeyName: String): Boolean;
var
  Version: String;
begin
  Result := RegQueryStringValue(RootKey, KeyName, 'pv', Version)
    and (Version <> '')
    and (Version <> '0.0.0.0');
end;

function IsWebView2Installed: Boolean;
begin
  Result :=
    HasWebView2Version(HKLM32, 'SOFTWARE\Microsoft\EdgeUpdate\Clients\' + WebView2ClientId) or
    HasWebView2Version(HKCU, 'Software\Microsoft\EdgeUpdate\Clients\' + WebView2ClientId);
end;

function PrepareToInstall(var NeedsRestart: Boolean): String;
begin
  Result := '';
  if not IsDotNet48Installed then
    Log('.NET Framework 4.8 no esta instalado; se instalara como requisito.');
  if not IsWebView2Installed then
    Log('WebView2 Runtime no esta instalado; se instalara como requisito.');
end;
