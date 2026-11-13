[Setup]
AppId={{C477C2FA-2E3B-4310-A7D2-5B6C4020F286}
AppName=DDHI Smart Pharmacy Server
AppVersion=0.2.0
AppPublisher=Digos Doctors Hospital, Inc.
DefaultDirName={autopf}\DDHI-SmartPharmacy-Server
PrivilegesRequired=admin
PrivilegesRequiredOverridesAllowed=commandline
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
OutputDir=..\release\Installers
OutputBaseFilename=DDHI-SmartPharmacy-Server-Setup-x64
SetupIconFile=..\assets\branding\hospital-icon.ico
Uninstallable=not ExtractOnlyCheck
WizardStyle=modern
WizardImageFile=..\assets\branding\installer-welcome.bmp
WizardSmallImageFile=..\assets\branding\installer-small.bmp
LicenseFile=..\LICENSE.txt
Compression=lzma2/fast
SolidCompression=yes
[Files]
Source: "..\release\Server\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\release\Database\*"; DestDir: "{app}\Database"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\release\Documentation\*"; DestDir: "{app}\Documentation"; Flags: ignoreversion recursesubdirs createallsubdirs
[Tasks]
Name: desktopicon; Description: "Create a Server Control desktop shortcut"; Flags: unchecked
[Icons]
Name: "{commonprograms}\DDHI Smart Pharmacy Server Control"; Filename: "{app}\Manager\DDHI-SmartPharmacy-Server.exe"; WorkingDir: "{app}"; Check: not ExtractOnlyCheck
Name: "{autodesktop}\DDHI Smart Pharmacy Server"; Filename: "{app}\Manager\DDHI-SmartPharmacy-Server.exe"; Tasks: desktopicon; Check: not ExtractOnlyCheck
[Run]
Filename: "{app}\Manager\DDHI-SmartPharmacy-Server.exe"; Description: "Open desktop Server Control"; Flags: nowait postinstall skipifsilent runascurrentuser; Check: not ExtractOnlyCheck
[Code]
function ExtractOnlyCheck: Boolean;
begin
 Result := ExpandConstant('{param:EXTRACTONLY|0}') = '1';
end;

function OwnPostgreSQLService: Boolean;
var ImagePath: String;
begin
  Result := False;
  if RegQueryStringValue(HKLM, 'SYSTEM\CurrentControlSet\Services\DDHIPostgreSQL', 'ImagePath', ImagePath) then
    Result := Pos(Lowercase(ExpandConstant('{app}\PostgreSQL\bin\pg_ctl.exe')), Lowercase(ImagePath)) > 0;
end;

function PrepareToInstall(var NeedsRestart: Boolean): String;
var Code: Integer;
begin
  if ExtractOnlyCheck then begin Result := ''; exit; end;
  if not FileExists(ExpandConstant('{sys}\vcruntime140.dll')) then begin
    Result := 'Install the Microsoft Visual C++ x64 runtime on this destination server first. Official download: https://aka.ms/vs/17/release/vc_redist.x64.exe';
    exit;
  end;
  // Stop application services before their privately bundled PostgreSQL instance.
  Exec(ExpandConstant('{sys}\sc.exe'), 'stop DDHIPharmacyApi', '', SW_HIDE, ewWaitUntilTerminated, Code);
  Exec(ExpandConstant('{sys}\sc.exe'), 'stop DDHIPharmacyWorker', '', SW_HIDE, ewWaitUntilTerminated, Code);
  if OwnPostgreSQLService then
    Exec(ExpandConstant('{sys}\net.exe'), 'stop DDHIPostgreSQL', '', SW_HIDE, ewWaitUntilTerminated, Code);
  Result := '';
end;
procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var Code: Integer;
begin
  if CurUninstallStep = usUninstall then begin
    Exec(ExpandConstant('{sys}\sc.exe'), 'stop DDHIPharmacyApi', '', SW_HIDE, ewWaitUntilTerminated, Code);
    Exec(ExpandConstant('{sys}\sc.exe'), 'stop DDHIPharmacyWorker', '', SW_HIDE, ewWaitUntilTerminated, Code);
    Exec(ExpandConstant('{sys}\sc.exe'), 'delete DDHIPharmacyApi', '', SW_HIDE, ewWaitUntilTerminated, Code);
    Exec(ExpandConstant('{sys}\sc.exe'), 'delete DDHIPharmacyWorker', '', SW_HIDE, ewWaitUntilTerminated, Code);
    if OwnPostgreSQLService then begin
      Exec(ExpandConstant('{sys}\net.exe'), 'stop DDHIPostgreSQL', '', SW_HIDE, ewWaitUntilTerminated, Code);
      Exec(ExpandConstant('{sys}\sc.exe'), 'delete DDHIPostgreSQL', '', SW_HIDE, ewWaitUntilTerminated, Code);
    end;
  end;
end;

