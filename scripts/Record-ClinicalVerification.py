"""Record release evidence only after completing native and synthetic layout/PDF review.

Requires the reviewed images under work/ and the final published binaries/docs.
This command records engineering review, never clinical acceptance.
"""
import argparse
parser=argparse.ArgumentParser()
parser.add_argument('--confirm-visual-review',action='store_true',help='Confirm the final native windows, clinical components and all rendered PDF pages were actually reviewed.')
if not parser.parse_args().confirm_visual_review:
    parser.error('Complete visual review first, then pass --confirm-visual-review.')
from pathlib import Path
import subprocess,hashlib,json,re,xml.etree.ElementTree as ET
from pypdf import PdfReader
r=Path(__file__).resolve().parents[1];t=r/'release/Tests';git=['git','--git-dir='+str(r/'.git'),'--work-tree='+str(r)]
def sha(p):
 with p.open('rb') as f:return hashlib.file_digest(f,'sha256').hexdigest()
def write(n,d):(t/n).write_text(json.dumps(d,indent=2)+'\n',encoding='utf8')
def old(n):return subprocess.check_output(git+['show','74ee51a:'+n]).decode('utf8').replace('\r\n','\n')
unchanged={}
for name in ['App.xaml','UserEditWindow.cs','MainWindow.DischargeReturns.cs','MainWindow.ExternalIntakes.cs','MainWindow.MedicineTrade.cs','MainWindow.GenericCatalog.cs']:
 name='src/DDHI.Pharmacy.Desktop/'+name;assert old(name)==(r/name).read_text(encoding='utf8'),name;unchanged[name]=sha(r/name)
name='src/DDHI.Pharmacy.Desktop/ScanResultView.cs';current=(r/name).read_text(encoding='utf8');current=re.sub(r'^    public Guid\? SelectedMedicineId=>result\?\.Medicine\?\.Id;\n','',current,flags=re.M);assert old(name)==current;unchanged[name+' (visual component; new read-only selected ID accessor excluded)']=sha(r/name)
client=sha(r/'release/Client/DDHI-SmartPharmacy.exe')
for name in ['GenericReturnPresentation.json','MedicineTradePresentation.json','ScanAccountPresentation.json']:
 data=json.loads((t/name).read_text(encoding='utf-8-sig'));data['clientSha256']=client;data['priorReviewedComponentsUnchangedFrom']='74ee51a';data['unchangedComponentSha256']=unchanged;data['reviewScope']='Previously reviewed generic/brand/return/trade/account forms and scan visual component remain unchanged, verified against prior release source. Scan adds only a read-only selected-ID accessor. New clinical search, governance, review and context layouts, current role sidebars and audit grids separately rendered and reviewed using synthetic identities. Native client sign-in reviewed without entering credentials. No clinical deployment acceptance inferred.';write(name,data)
pdf=r/'release/Documentation/DDHI-SmartPharmacy-Complete-User-Manual.pdf';reader=PdfReader(pdf);text=' '.join(p.extract_text() for p in reader.pages);names=re.findall(r'"([^"]+)"',re.search(r'AllPages = \[(.*?)\];',(r/'src/DDHI.Pharmacy.Contracts/ClientAccessPolicy.cs').read_text(encoding='utf8'),re.S).group(1));assert len(names)==20 and all(n in text for n in names);assert len(reader.pages)==36 and len(reader.outline)==28;assert len(list((r/'work/manual-review-clinical').glob('page-*.png')))==36
terms=len(re.findall(r'^- \*\*',(r/'docs/COMPLETE_USER_MANUAL.md').read_text(encoding='utf8').split('## 28. Glossary')[1],re.M));assert terms==112
write('CompleteManualReview.json',dict(allSidebarSections=True,glossaryPresent=True,allPagesRendered=True,layoutReviewed=True,pages=36,chapters=28,sidebarSections=20,glossaryTerms=terms,pdfSha256=sha(pdf),note='All 36 latest renders/contact sheets reviewed; clinical chapter pages inspected at full size. Contents, 28 bookmarks, 20 sidebar sections and 112 glossary terms verified.'))
data=json.loads((t/'DesktopLaunch.json').read_text(encoding='utf-8-sig'));data['executableSha256']=client;data['note']='Published client 0.7.0 native sign-in window and logo reviewed; no staff credentials entered or server pairing approved. New clinical components reviewed separately with synthetic data.';write('DesktopLaunch.json',data)
manager=json.loads((r/'runtime/manager-preview.json').read_text(encoding='utf-8-sig'));assert manager['Version']=='0.9.0';manager['executableSha256']=sha(r/'release/Server/Manager/DDHI-SmartPharmacy-Server.exe');write('ServerDesktopPreview.json',manager)
for file in ['UnitTestsFinal.trx','IntegrationTestsFinal.trx']:
 results=[n for n in ET.parse(t/file).iter() if n.tag.endswith('UnitTestResult') and ('Clinical' in n.attrib.get('testName','') or 'FixtureDeclaredSeverity' in n.attrib.get('testName',''))];assert results and all(n.attrib['outcome']=='Passed' for n in results)
guide=r/'release/Documentation/DDHI-Drug-Reference-Guide.pdf';assert len(PdfReader(guide).pages)==11 and len(list((r/'work/drug-guide-review').glob('page-*.png')))==11
package=json.loads((r/'samples/clinical-reference/clinical-package-template.json').read_text(encoding='utf8'));assert all(not package[n] for n in ['Sources','Ingredients','Drugs','Products','Packages','Monographs','Rules']);registry=json.loads((r/'samples/clinical-reference/source-registry.json').read_text(encoding='utf8'));assert registry['clinicalRules']==registry['clinicalMonographs']==0
write('ClinicalReferenceRelease.json',dict(importReviewTestsPassed=True,screeningAndDispensingTestsPassed=True,clinicalLayoutsReviewed=True,guidePagesReviewed=True,noClinicalDatasetShipped=True,clientSha256=client,apiAssemblySha256=sha(r/'release/Server/Api/DDHI.Pharmacy.Api.dll'),serverControlSha256=sha(r/'release/Server/Manager/DDHI-SmartPharmacy-Server.exe'),guideSha256=sha(guide),guidePages=11,externalAIEnabled=False,clinicalAcceptance=False,reviewedImagesSha256={name:sha(r/'work'/name) for name in ['clinical-reference-preview.png','clinical-admin-preview.png','clinical-safety-preview.png','clinical-context-preview.png']},note='Deterministic engineering fixtures are fictional and never production seeded. Four new actual WPF component layouts reviewed; all guide pages rendered/reviewed. Comprehensive content, clinical datasets and hospital validation remain pending.'))
print(json.dumps({'manualPages':36,'glossaryTerms':terms,'guidePages':11,'clientSha256':client}))
