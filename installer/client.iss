#define MyAppName "DDHI Smart Pharmacy Barcode Manager"
[Setup]
AppId={{4CC80B9D-79E7-43F6-998B-953D197F12AD}
AppName={#MyAppName}
AppVersion=0.4.0
AppPublisher=Digos Doctors Hospital, Inc.
DefaultDirName={localappdata}\Programs\DDHI-SmartPharmacy
DefaultGroupName=DDHI Smart Pharmacy
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
OutputDir=..\release\Installers
OutputBaseFilename=DDHI-SmartPharmacy-Setup-x64
SetupIconFile=..\assets\branding\hospital-icon.ico
Uninstallable=not ExtractOnlyCheck
WizardStyle=modern
WizardImageFile=..\assets\branding\installer-welcome.bmp
WizardSmallImageFile=..\assets\branding\installer-small.bmp
LicenseFile=..\LICENSE.txt
Compression=lzma2/fast
SolidCompression=yes
UninstallDisplayIcon={app}\DDHI-SmartPharmacy.exe
CloseApplications=yes
[Tasks]
Name: desktopicon; Description: "Create a desktop shortcut"; Flags: unchecked
[Files]
Source: "..\release\Client\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
[Icons]
Name: "{group}\DDHI Smart Pharmacy"; Filename: "{app}\DDHI-SmartPharmacy.exe"; Check: not ExtractOnlyCheck
Name: "{group}\Connection guide"; Filename: "{app}\Documentation\DDHI-Server-and-Client-Connection-Guide.pdf"; Check: not ExtractOnlyCheck
Name: "{autodesktop}\DDHI Smart Pharmacy"; Filename: "{app}\DDHI-SmartPharmacy.exe"; Tasks: desktopicon; Check: not ExtractOnlyCheck
[Run]
Filename: "{app}\DDHI-SmartPharmacy.exe"; Description: "Configure server and sign in"; Flags: nowait postinstall skipifsilent; Check: not ExtractOnlyCheck


[Code]
function ExtractOnlyCheck: Boolean;
begin
 Result := ExpandConstant('{param:EXTRACTONLY|0}') = '1';
end;
