# Hospital clinical validation and release checklist

Evaluation software. A responsible hospital pharmacist must complete this checklist against authorized clinical sources before any clinical reliance. Software tests using fictional ingredients verify engineering behavior only.

For each clinical case record: source title/URL/document identifier and permitted-use basis; exact product/ingredient/form/route/dose/frequency; patient risk/allergy context and reconciliation completeness; expected source-specific severity/evidence/management; expected original-to-normalized severity mapping; exact dataset version; actual result; reviewer name/date; discrepancy/resolution and hospital approval reference. Do not invent an expected result from a test name.

| Required real clinical scenario | Expected clinical outcome | Validation status |
| --- | --- | --- |
| Warfarin + an NSAID | To be established from authorized references and patient context | Pending |
| Warfarin + trimethoprim-sulfamethoxazole | To be established from authorized references and patient context | Pending |
| Sildenafil + a nitrate | To be established from authorized references and patient context | Pending |
| ACE inhibitor + potassium-sparing diuretic | To be established from authorized references and patient context | Pending |
| Opioid + benzodiazepine | To be established from authorized references and patient context | Pending |
| Serotonergic antidepressant + MAO inhibitor | To be established from authorized references and patient context | Pending |
| Metformin + cimetidine | To be established from authorized references and patient context | Pending |
| Multiple QT-prolonging medicines | To be established from authorized references and patient context | Pending |
| Duplicate paracetamol-containing products | Verify ingredients, strengths, regimen, duplication and documented management | Pending |
| Single ingredient + combination product duplication | Verify all ingredient links, regimen and documented management | Pending |

Additional acceptance:

- Verify permitted redistribution/access for every clinical source; no unlicensed vendor database or unsupported ATC/SNOMED content.
- Independently confirm PH FDA registration and last-verification dates; separate it from hospital formulary approval.
- Review combination-product expansion, aliases, ingredient salts/forms, strength/route/form/brand identity and package mappings.
- Validate conditional and directional effects, theoretical/limited evidence, unknown pairs, overdue content and false-positive/false-negative handling.
- Verify allergy reaction/severity/date/certainty/intolerance/source/verification and evidence-based cross-reactivity; never assume undocumented history is negative.
- Review the hospital's incomplete-coverage/manual-review policy. Initially disabled. Do not enable it merely to dismiss unavailable clinical data.
- Verify Contraindicated/potential-Contraindicated clarification, Major management decisions, held/escalated/canceled transactions and staff role separation.
- Validate exact order/batch/brand/quantity/encounter binding, policy/mapping/publication change invalidation, expiry, retries and concurrent last-unit behavior.
- Test server loss, four physical workstations, firewall/DNS/pairing, optional scanners and no independent unsynchronized clinical databases.
- Verify protected automatic local backup and independent disaster-recovery backup; restore to an isolated database and verify records/audit before recovery approval.
- Review Source-only/empty-data screens, missing renal/hepatic/pregnancy/IV guidance, AI-disabled state and all capability-matrix limitations.
- Record pharmacy leadership, P&T where applicable, IT, privacy officer and hospital production acceptance. No approval is supplied by the development agent.
