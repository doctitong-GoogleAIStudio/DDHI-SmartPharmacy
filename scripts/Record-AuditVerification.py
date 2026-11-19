"""Record evidence after tests and visual review; never substitutes for those checks."""
from pathlib import Path
import hashlib,json,subprocess,xml.etree.ElementTree as ET

ROOT=Path(__file__).resolve().parents[1]
TESTS=ROOT/'release/Tests'
def sha(path):
    with path.open('rb') as stream:return hashlib.file_digest(stream,'sha256').hexdigest()
def passed(file,names):
    results=[n for n in ET.parse(TESTS/file).getroot().iter() if n.tag.endswith('UnitTestResult')]
    for name in names:
        found=[n for n in results if name in n.attrib.get('testName','')]
        if not found or any(n.attrib.get('outcome')!='Passed' for n in found):raise RuntimeError('Missing successful check: '+name)

passed('IntegrationTestsFinal.trx',[
    'AuditSnapshotsSurviveProfileChangesAndDeletionAndNeverContainCredentials',
    'AuditRecordsDeniedAndFailedChangesWithoutClaimingCommit',
    'AuditAccessFiltersDatesAndPagingMatchServerRecords',
    'TimedSessionsAreOwnedHeartbeatBoundedAndSignoutIsConfirmed',
    'SessionCorrelationCannotBeForgedAndExpiredSessionCannotHeartbeat',
    'FileActivityIsMinimalClientReportedAndIdempotent',
    'DailyTotalsSplitAtManilaMidnightAndCountOverlapOnce'])
passed('UnitTestsFinal.trx',['LegacyAuditHashesRemainByteForByteCompatible','DurationUsesLastContactUntilConfirmedSignoutAndCapsAtExpiry','TotalsDoNotDoubleCountOverlappingSessions','SafeDetailsExcludePasswordsRawFileContentsAndPatientPayloads'])
passed('ClientSessionTests.trx',['ADelayedHeartbeatFromTheOldLoginCannotClearTheNextStaffAccount','UnauthorizedHeartbeatClearsTheCurrentIdentityAndNotifiesTheWindow'])
runtime=json.loads((TESTS/'AuditRuntimeSmoke.json').read_text(encoding='utf-8-sig'))
if not runtime or not all(runtime.values()):raise RuntimeError('Published runtime audit smoke did not pass.')
images={name:sha(ROOT/'work'/name) for name in ['audit-activities-preview.png','audit-sessions-preview.png','audit-totals-preview.png','client-role-sidebars.png']}
data={'snapshotsAndDeletion':True,'safeChanges':True,'deniedAndFailed':True,'timedSessionOwnership':True,'dailyTotals':True,'hashCompatibility':True,'presentationReviewed':True,
      'clientSha256':sha(ROOT/'release/Client/DDHI-SmartPharmacy.exe'),'apiSha256':sha(ROOT/'release/Server/Api/DDHI.Pharmacy.Api.exe'),'apiAssemblySha256':sha(ROOT/'release/Server/Api/DDHI.Pharmacy.Api.dll'),
      'runtimeChecks':runtime,'reviewedImagesSha256':images,'observedNotAttendance':True,'physicalHospitalAcceptance':False,
      'note':'Server-clock Asia/Manila activity timestamps, bounded safe before/after fields, immutable snapshots and legacy hashes. Actual MainWindow audit toolbar/grid and role sidebar reviewed with synthetic data; native client launch reviewed without staff authentication. Session end can remain unconfirmed. Manual chapter 21 and all latest page renders reviewed.'}
(TESTS/'AuditTrailSmoke.json').write_text(json.dumps(data,indent=2)+'\n',encoding='utf8')
print(json.dumps(data,indent=2))
