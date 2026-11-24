# Source evaluation, licensing and attribution

Checked 24 November 2026. This release contains no proprietary clinical database and no preapproved clinical interaction rules.

| Source | Permitted integration approach and limits |
| --- | --- |
| NLM RxNorm/RxNav | Optional terminology-only staging using nonproprietary RxNorm fields, with NLM attribution and recorded retrieval/version. No RxClass/SNOMED data imported without applicable licensing. Drug interaction service was discontinued; never interpret its absence as a negative screen. https://lhncbc.nlm.nih.gov/RxNav/TermsofService.html and https://www.lhncbc.nlm.nih.gov/RxNav/information/FAQs.html |
| Philippine FDA | Official product/registration verification and source links; no automatic US-to-Philippine registration inference. Bulk reuse/redistribution must be checked before importing. https://www.fda.gov.ph/fda-advisory-no-2023-2238-utilization-of-the-food-and-drug-administration-fda-verification-portal/ and https://verification.fda.gov.ph/ |
| DailyMed/openFDA/manufacturer labels | Source links and authorized staged extracts with provenance. Public availability is not treated as blanket copyright permission or comprehensive pairwise interaction coverage. https://dailymed.nlm.nih.gov/dailymed/ and https://open.fda.gov/terms/ |
| WHO ATC/DDD | Store user-authorized codes and attribution, subject to terms; no copied full index or altered proprietary taxonomy. https://atcddd.fhi.no/copyright_disclaimer/416.html |
| WHO Essential Medicines/AWaRe/PNF/LactMed/guidelines/literature | Source-by-source permission, exact document/version and pharmacist review are required before clinical publication. No automatic redistribution authorization is asserted. |
| Licensed interaction/IV databases | Import only content covered by the hospital license, recording original severity, evidence and conditional context. No vendor content is shipped. |

NLM attribution required when NLM terminology is later imported: This product uses publicly available data from the U.S. National Library of Medicine (NLM), National Institutes of Health, Department of Health and Human Services; NLM is not responsible for the product and does not endorse or recommend this or any other product.

No NLM/WHO/vendor logo is used. The existing approved hospital logo remains unchanged. Source/license attestations in an import are statements by the hospital operator, not an automated legal determination. Approval is performed by authorized hospital staff, never by the development agent.

Renal-method selection must follow the applicable medication reference. NIDDK distinguishes creatinine clearance and eGFR and discusses method/BSA limitations: https://www.niddk.nih.gov/research-funding/research-programs/kidney-clinical-research-epidemiology/laboratory/ckd-drug-dosing-providers . This implementation does not invent medication-specific adjustments when a reviewed source is absent.
