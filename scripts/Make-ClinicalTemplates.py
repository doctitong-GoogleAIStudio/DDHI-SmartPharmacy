"""Create original empty import templates and metadata-only source registry."""
from pathlib import Path
import csv,io,json,re,zipfile
ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'samples/clinical-reference';OUT.mkdir(parents=True,exist_ok=True)
source=(ROOT/'src/DDHI.Pharmacy.Contracts/ClinicalReference.cs').read_text(encoding='utf8')
types={'sources':'ClinicalSource','ingredients':'IngredientDefinition','drugs':'ClinicalDrugDefinition','products':'ProductDefinition','packages':'PackageDefinition','monographs':'MonographDefinition','rules':'ClinicalRuleDefinition'}
manifest={'Schema':1,'Version':'draft-CHANGE-THIS-VERSION','LicenseStatement':'Enter the actual hospital-authorized use and redistribution basis before import.'}
empty={**manifest,**{name.title():[] for name in types}}
(OUT/'clinical-package-template.json').write_text(json.dumps(empty,indent=2)+'\n',encoding='utf8')
(OUT/'manifest.json').write_text(json.dumps(manifest,indent=2)+'\n',encoding='utf8')
for table,record in types.items():
 fields=re.search(r'public sealed record '+record+r'\((.*?)\);',source,re.S).group(1)
 headers=[re.search(r'\s(\w+)(?:\s*=.*)?$',field.strip()).group(1) for field in fields.split(',')]
 with (OUT/(table+'.csv')).open('w',newline='',encoding='utf8') as stream:csv.writer(stream).writerow(headers)
with (OUT/'clinical-records-template.csv').open('w',newline='',encoding='utf8') as stream:
 writer=csv.writer(stream);writer.writerow(['Kind','Json']);writer.writerow(['Manifest',json.dumps(manifest)])
with zipfile.ZipFile(OUT/'clinical-csv-bundle-template.zip','w',zipfile.ZIP_DEFLATED) as archive:
 archive.write(OUT/'manifest.json','manifest.json')
 for table in types:archive.write(OUT/(table+'.csv'),table+'.csv')
registry={'kind':'SourceRegistryOnly','checkedOn':'2026-11-24','clinicalRules':0,'clinicalMonographs':0,'notice':'Original source-link metadata only. Not an interaction dataset and not importable as an approved clinical package.',
 'sources':[{'organization':organization,'url':url,'publicationRevisionDate':None,'status':status} for organization,url,status in [
 ('Philippine FDA','https://verification.fda.gov.ph/','Verify each PH product/registration and permitted reuse.'),
 ('NLM RxNorm/RxNav','https://lhncbc.nlm.nih.gov/RxNav/TermsofService.html','Terminology terms and attribution; no interaction service.'),
 ('NLM DailyMed','https://dailymed.nlm.nih.gov/dailymed/','Source-by-source content permission and pharmacist review required.'),
 ('FDA openFDA','https://open.fda.gov/terms/','Public label data is not comprehensive pairwise interaction coverage.'),
 ('WHO Collaborating Centre ATC/DDD','https://atcddd.fhi.no/copyright_disclaimer/416.html','Separate terms; no copied complete index supplied.'),
 ('NIDDK','https://www.niddk.nih.gov/research-funding/research-programs/kidney-clinical-research-epidemiology/laboratory/ckd-drug-dosing-providers','Reference to renal-method limitations; not medication-specific dosing content.')]]}
(OUT/'source-registry.json').write_text(json.dumps(registry,indent=2)+'\n',encoding='utf8')
print(OUT)
