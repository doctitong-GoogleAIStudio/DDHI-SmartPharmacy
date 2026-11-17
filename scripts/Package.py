from pathlib import Path
import datetime
import hashlib
import json
import shutil
import xml.etree.ElementTree as ET
import zipfile

ROOT = Path(__file__).resolve().parents[1]
RELEASE = ROOT / 'release'
ARCHIVE = RELEASE / 'DDHI-SmartPharmacy-Windows11-Release.zip'


def sha(path):
    with path.open('rb') as stream:
        return hashlib.file_digest(stream, 'sha256').hexdigest()


def test_result(name):
    node = ET.parse(RELEASE / 'Tests' / name).getroot()
    counters = next(x for x in node.iter() if x.tag.endswith('Counters'))
    result = {k: int(counters.attrib[k]) for k in ('total', 'passed', 'failed')}
    if result['failed'] or result['total'] != result['passed']:
        raise RuntimeError(f'Tests are not all passing: {name}: {result}')
    return result


def main():
    for path in RELEASE.rglob('*'):
        if path.is_file() and (path.suffix.lower() in ('.pfx', '.p12', '.dpapi') or path.name == 'PG_VERSION'):
            raise RuntimeError(f'Private key, credential store or database data found in release: {path}')
    executables = [
        'Client/DDHI-SmartPharmacy.exe',
        'Installers/DDHI-SmartPharmacy-Setup-x64.exe',
        'Installers/DDHI-SmartPharmacy-Server-Setup-x64.exe',
        'Server/Api/DDHI.Pharmacy.Api.exe',
        'Server/Worker/DDHI.Pharmacy.Worker.exe',
        'Server/Tools/DDHI.Pharmacy.Tools.exe',
        'Server/Manager/DDHI-SmartPharmacy-Server.exe',
        'Server/PostgreSQL/bin/postgres.exe',
    ]
    for name in executables:
        path = RELEASE / name
        with path.open('rb') as stream:
            if stream.read(2) != b'MZ':
                raise RuntimeError(f'Not a Windows executable: {path}')
        if path.stat().st_size < 10000:
            raise RuntimeError(f'Unexpected executable size: {path}')
    for name in ('Installation Guide', 'Administrator Guide', 'Pharmacy User Manual',
                 'Barcode Scanner Setup', 'Cerebro Integration Guide',
                 'Workstation Deployment Guide', 'Backup Recovery Guide', 'DDHI-Server-and-Client-Connection-Guide'):
        with (RELEASE / 'Documentation' / (name + '.pdf')).open('rb') as stream:
            if stream.read(5) != b'%PDF-':
                raise RuntimeError(f'Invalid PDF: {name}')
    if sha(ROOT / 'assets/branding/DIDH LOGO.jpg') != sha(RELEASE / 'Branding/DIDH LOGO.jpg'):
        raise RuntimeError('Original hospital logo differs from the release copy.')
    results = {
        'unit': test_result('UnitTestsFinal.trx'),
        'integration': test_result('IntegrationTestsFinal.trx'),
        'restoredDatabase': test_result('RestoredDatabaseFinal.trx'),
        'serverManager': test_result('ServerManagerTests.trx'),
    }
    evidence = {}
    for filename, required in {
        'MedicineTradeSmoke.json': ['catalogPersisted', 'scanFields', 'schemaUpdatePreservedRecords', 'runtimeRole'],
        'MedicineSchemaUpgrade.json': ['countsPreserved', 'legacyFieldsPreserved', 'isolatedDatabase'],
        'MedicineTradePresentation.json': ['addFieldsReviewed', 'editFieldsReviewed', 'scanFieldsReviewed'],
        'ScanAccountSmoke.json': ['publishedRuntimeRole', 'profileEdit', 'roleReplacement', 'accountDeletion', 'clinicalDeleteDenied', 'structuredScan'],
        'ScanAccountPresentation.json': ['scanLayoutReviewed', 'accountEditorReviewed', 'noRawJson'],
        'BackupRestore.json': ['backupAuthenticated', 'restoreSucceeded', 'countsMatch'],
        'PublishedServerSmoke.json': ['httpsHealth', 'testCertificatePinned', 'anonymousDenied',
                                     'bootstrapSucceeded', 'staffAuthentication',
                                     'catalogAndReceiving', 'auditChainValid', 'workerStarted'],
        'InstallerPayloadSmoke.json': ['clientPayloadVerified', 'serverPayloadVerified', 'serverSupplementalPayloadVerified'],
        'DesktopLaunch.json': ['launched', 'logoVisible', 'loginLayoutReviewed'],
        'NavigationContrast.json': ['normalAndSelectedRendered', 'textBindingVerified', 'hoverAndFocusPaletteChecked'],
        'ServerDesktopPreview.json': ['DesktopWindowLoaded', 'PreparationOnly', 'LocalInitializationDisabled', 'LanConnectionControlsLoaded', 'ConnectionGuideLoaded', 'SimpleLanControlsLoaded', 'DatabaseAccessControlsLoaded', 'ItLoginControlsLoaded'],
        'ItDatabaseLoginSmoke.json': ['scramAuthentication', 'wrongPasswordRejected', 'readonlyViewsWork', 'sensitiveTablesDenied', 'writesDenied', 'maintenanceExplicit', 'noSuperuser', 'duplicatePreserved', 'rollbackAtomic', 'namespaceCollisionRejected', 'excessivePublicGrantsRejected', 'applicationCredentialsUnchanged', 'stockAndLedgerUnchanged', 'plaintextPasswordsAbsentFromLogs'],
        'DatabaseAccessSmoke.json': ['pinnedHttps', 'staffAuthentication', 'roleDenied', 'readOnlyRecords', 'viewConversion', 'credentialsHidden', 'signOutClearsAccess'],
        'ClientRoleNavigation.json': ['roleLabelReviewed', 'roleMenusReviewed', 'noAuthenticationBypass', 'testsPassed'],
        'SimpleConnectionSmoke.json': ['automaticDiscovery', 'pinnedHttps', 'wrongKeyRejected', 'existingAdministratorLogin',
                                       'databaseAndStockPreserved', 'credentialsUnchanged', 'publicConnectionFile', 'identitySurvivesReenable', 'restoreAndRestart'],
        'LanUpgradeSmoke.json': ['managedProcessConversion', 'tlsHostnameVerified', 'existingAdministratorLogin',
                                 'medicineAndUsersPreserved', 'stockAndLotsPreserved', 'credentialsUnchanged',
                                 'dnsFailureRejectedBeforeChange', 'wrongPfxPasswordRejected', 'concurrentChangeRejected',
                                 'corruptBackupRejected', 'manualRestore', 'failedActivationRolledBack', 'interruptedChangeRecovery'],
    }.items():
        data = json.loads((RELEASE / 'Tests' / filename).read_text(encoding='utf-8-sig'))
        if any(data.get(key) is not True for key in required):
            raise RuntimeError(f'Incomplete release evidence: {filename}')
        evidence[filename] = data
    desktop = evidence['DesktopLaunch.json']
    if evidence['MedicineTradePresentation.json'].get('clientSha256', '').lower() != sha(RELEASE / executables[0]):
        raise RuntimeError('Importer/distributor presentation evidence does not match the packaged client.')
    if evidence['NavigationContrast.json'].get('executableSha256', '').lower() != sha(RELEASE / executables[0]):
        raise RuntimeError('Navigation contrast evidence does not match the packaged client.')
    if evidence['ClientRoleNavigation.json'].get('executableSha256', '').lower() != sha(RELEASE / executables[0]):
        raise RuntimeError('Role-navigation evidence does not match the packaged client.')
    if desktop.get('executableSha256', '').lower() != sha(RELEASE / executables[0]):
        raise RuntimeError('Desktop launch evidence does not match the packaged EXE.')
    server_preview = evidence['ServerDesktopPreview.json']
    if server_preview.get('Version') != '0.7.1' or server_preview.get('executableSha256', '').lower() != sha(RELEASE / executables[6]):
        raise RuntimeError('Server desktop launch evidence does not match version 0.7.1 and the current EXE.')
    installers = evidence['InstallerPayloadSmoke.json']
    for key, name in [('clientInstallerSha256', executables[1]), ('serverInstallerSha256', executables[2])]:
        if installers.get(key, '').lower() != sha(RELEASE / name):
            raise RuntimeError('Installer payload evidence does not match the current installer.')
    audit = json.loads((RELEASE / 'Tests/DependencyAudit.json').read_text(encoding='utf-8-sig'))
    if audit.get('problems'):
        raise RuntimeError('Dependency audit did not complete successfully.')
    for project in audit.get('projects', []):
        for framework in project.get('frameworks', []):
            for package in framework.get('topLevelPackages', []) + framework.get('transitivePackages', []):
                if package.get('vulnerabilities'):
                    raise RuntimeError(f'Unresolved package advisory: {package.get("id")}')

    for name in ('INITIAL_AUDIT.md', 'ARCHITECTURE.md', 'IMPLEMENTATION_PLAN.md',
                 'DATA_OWNERSHIP.md', 'CEREBRO_INTEGRATION_ASSUMPTIONS.md',
                 'RISK_REGISTER.md', 'RELEASE_STATUS.md', 'SERVER_DESKTOP_SETUP.md', 'SERVER_CLIENT_CONNECTION_GUIDE.md'):
        shutil.copy2(ROOT / 'docs' / name, RELEASE / 'Documentation' / name)
    shutil.copy2(ROOT / 'README.md', RELEASE / 'README.md')
    shutil.copy2(ROOT / 'LICENSE.txt', RELEASE / 'LICENSE.txt')
    shutil.copy2(ROOT / 'THIRD_PARTY_NOTICES.md', RELEASE / 'THIRD_PARTY_NOTICES.md')

    # Archive source only. Never package live databases, backup files, credentials or caches.
    source_archive = RELEASE / 'DDHI-SmartPharmacy-Source.zip'
    source_files = []
    for directory in ('assets', 'src', 'tests', 'database', 'installer', 'deployment', 'scripts', 'docs', 'samples'):
        for path in (ROOT / directory).rglob('*'):
            if path.is_file() and not any(p in ('bin', 'obj', '__pycache__') for p in path.relative_to(ROOT).parts):
                source_files.append(path)
    for name in ('AGENTS.md', '.gitignore', '.gitattributes', 'README.md', 'LICENSE.txt', 'THIRD_PARTY_NOTICES.md',
                 'DDHI.SmartPharmacy.sln', 'Directory.Build.props', 'global.json', 'NuGet.Config'):
        source_files.append(ROOT / name)
    with zipfile.ZipFile(source_archive, 'w', zipfile.ZIP_DEFLATED, compresslevel=6) as archive:
        for path in sorted(source_files):
            archive.write(path, 'DDHI-SmartPharmacy-Source/' + path.relative_to(ROOT).as_posix())

    timestamp = datetime.datetime.now(datetime.timezone(datetime.timedelta(hours=8))).isoformat()
    manifest = {
        'application': 'DDHI Smart Pharmacy Barcode Manager',
        'version': '0.7.1-medicine-trade', 'components': {'client': '0.4.1', 'clientInstaller': '0.4.1', 'api': '0.2.1', 'worker': '0.3.1', 'serverControl': '0.7.1', 'serverInstaller': '0.7.1', 'postgresql': '17.12'}, 'classification': 'Hospital evaluation build; production acceptance pending',
        'hospital': 'Digos Doctors Hospital, Inc.',
        'developer': 'Vicente C. Cavalida Jr. MD', 'verifiedAtAsiaManila': timestamp,
        'tests': results, 'evidence': evidence,
        'artifacts': [{'path': name, 'bytes': (RELEASE / name).stat().st_size,
                       'sha256': sha(RELEASE / name)} for name in executables],
        'sourceSha256': sha(source_archive),
        'productionAccepted': False, 'liveCerebroEnabled': False,
        'cleanMachineInstallerAcceptance': False, 'physicalScannerAcceptance': False,
    }
    (RELEASE / 'Checksums/ReleaseVerification.json').write_text(json.dumps(manifest, indent=2) + '\n', encoding='utf-8')
    files = sorted(p for p in RELEASE.rglob('*') if p.is_file()
                   and p != ARCHIVE and not p.name.endswith('.partial') and p.parent.name != 'Checksums')
    files.append(RELEASE / 'Checksums/ReleaseVerification.json')
    sums = ''.join(f'{sha(path)}  {path.relative_to(RELEASE).as_posix()}\n' for path in sorted(files))
    (RELEASE / 'Checksums/SHA256SUMS.txt').write_text(sums, encoding='utf-8')
    files.append(RELEASE / 'Checksums/SHA256SUMS.txt')
    partial = ARCHIVE.with_suffix('.zip.partial')
    with zipfile.ZipFile(partial, 'w', zipfile.ZIP_DEFLATED, compresslevel=3) as archive:
        for path in sorted(files):
            archive.write(path, 'DDHI-SmartPharmacy-Release/' + path.relative_to(RELEASE).as_posix())
    with zipfile.ZipFile(partial) as archive:
        bad = archive.testzip()
        if bad:
            raise RuntimeError(f'Release ZIP CRC check failed: {bad}')
    partial.replace(ARCHIVE)
    digest = sha(ARCHIVE)
    (RELEASE / 'Checksums' / (ARCHIVE.name + '.sha256')).write_text(f'{digest}  {ARCHIVE.name}\n', encoding='utf-8')
    # Independently verify every checksum in the manifest after packaging.
    for line in sums.splitlines():
        expected, name = line.split('  ', 1)
        if sha(RELEASE / name) != expected:
            raise RuntimeError(f'Post-package checksum mismatch: {name}')
    print(json.dumps({'release': str(ARCHIVE), 'bytes': ARCHIVE.stat().st_size,
                      'sha256': digest, 'filesVerified': len(files), 'tests': results}, indent=2))


if __name__ == '__main__':
    main()
