[Setup]
AppId={{C477C2FA-2E3B-4310-A7D2-5B6C4020F286}
AppName=DDHI Smart Pharmacy Server
AppVersion=0.7.1
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
CloseApplications=yes
RestartApplications=no
[Files]
Source: "..\deployment\Stop-ServerForMaintenance.ps1"; Flags: dontcopy
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

function RunMaintenance(ScriptPath: String; RemoveServices: Boolean): Boolean;
var Code: Integer; Arguments: String;
begin
  Arguments := '-NoProfile -NonInteractive -ExecutionPolicy Bypass -File "' + ScriptPath + '" -InstallRoot "' + ExpandConstant('{app}') + '"';
  if RemoveServices then Arguments := Arguments + ' -RemoveServices';
  Result := Exec(ExpandConstant('{sys}\WindowsPowerShell\v1.0\powershell.exe'), Arguments, '', SW_HIDE, ewWaitUntilTerminated, Code);
  if Result then Result := Code = 0;
end;

function PrepareToInstall(var NeedsRestart: Boolean): String;
begin
  if ExtractOnlyCheck then begin Result := ''; exit; end;
  if not FileExists(ExpandConstant('{sys}\vcruntime140.dll')) then begin
    Result := 'Install the Microsoft Visual C++ x64 runtime on this destination server first. Official download: https://aka.ms/vs/17/release/vc_redist.x64.exe';
    exit;
  end;
  ExtractTemporaryFile('Stop-ServerForMaintenance.ps1');
  if not RunMaintenance(ExpandConstant('{tmp}\Stop-ServerForMaintenance.ps1'), False) then begin
    Result := 'Could not safely stop the existing DDHI services, or service ownership belongs to a different installation. Stop the owned server during approved downtime and review service permissions before retrying. Database/configuration files were not replaced.';
    exit;
  end;
  Result := '';
end;
procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  if CurUninstallStep = usUninstall then begin
    if not RunMaintenance(ExpandConstant('{app}\deployment\Stop-ServerForMaintenance.ps1'), True) then
      RaiseException('Could not safely stop/remove the owned server services. Uninstall was interrupted; review service ownership and permissions.');
  end;
end;

