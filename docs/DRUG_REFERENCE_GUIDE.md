# DDHI Drug Reference & Interaction Checker

Client 0.7.0 / API 0.5.0 / Server Control and server installer 0.9.0. Foundation evaluation edition, 24 November 2026.

## What this release provides

The new Drug Reference sidebar is part of SmartPharmacy, using its hospital logo, accounts, role permissions, central server, inventory IDs, barcode resolver, HTTPS connection and audit trail. All workstations use the same published local content. Internet access is not needed for routine searches/screens; the hospital server must be available. Historical dispensing is never automatically treated as the current medication list.

The user selected a governed workflow without a licensed clinical dataset. No real-drug interaction rules, copied commercial monographs or IV compatibility tables are shipped. The supplied source registry is informational metadata, not clinical knowledge. Unknown coverage stays UNKNOWN. The application is not equivalent in coverage to a licensed comprehensive reference.

## Install and migrate

1. Retain a verified independent encrypted database/configuration backup and the original server setup Windows account. Close pharmacy clients for the maintenance window.
2. Install the matching server package over the existing installation. Do not initialize a new instance over existing data.
3. Open Server Control as Administrator under the original setup account and choose Server status > Apply database updates. This now stops the owned server, creates an authenticated encrypted local pre-schema backup under its protected migration-backups folder, applies additive migrations and restarts. A backup failure stops the migration.
4. Update all clients to 0.7.0. Older clients cannot silently bypass the new dispensing review gate.
5. Check health, existing accounts/catalog/stock and Audit Trail. Existing clinical/stock/audit history is retained. The idempotency-result field is widened to text without rewriting results.
6. Initially clinical coverage is empty and manual handling of incomplete coverage is disabled. A Chief Pharmacist may enable Drug Reference > Database Administration > Hospital safety policy ONLY after obtaining the hospital's written medication-safety policy. Otherwise hold/escalate incomplete assessments. The development build never enables this policy on the hospital server.

Automatic local migration backups use an encrypted archive and a CurrentUser DPAPI-protected key sidecar. Keep that folder and original Windows account protected. They support local authorized rollback, not independent off-site disaster recovery. Continue the existing portable backup/recovery procedure. Direct advanced CLI migrations require an independently verified backup; the automatic backup described above is the Server Control workflow.

## Search, stock, barcode and monographs

Open Drug Reference. The inner navigation includes Drug Search, Generic Drug Directory, Brand Directory, Therapeutic Classification, Drug Interaction Checker, Drug Allergy Checker, Drug-Disease Checker, Dosage Reference, Renal Dosage Guide, Hepatic Dosage Guide, Pregnancy and Lactation, Antibiotic Reference, IV Compatibility, DDHI Hospital Formulary, AI Drug Assistant, Drug Safety Alerts, Database Administration, and Clinical References and Updates.

Type generic/brand/ingredient/synonym/class/manufacturer/registration text or a registered barcode. Search updates after a short pause. Spelling suggestions require explicit exact-product verification; they never create an ingredient mapping. Results are bounded to 200 displayed rows; narrow the query when the limited-results flag is shown. Inventory scans use existing approved package mappings and GTIN checks. Unknown/ambiguous codes do not become products. The stock filter excludes expired/recalled unavailable stock; final quantity/reservation checks still apply.

Double-click an item or choose Open monograph. Concise / expanded switches the amount of approved content shown. Missing monographs/sections and overdue reviews are explicit. Catalog descriptions and unit costs are not clinical advice or selling prices. Current stock is read from the central inventory, with reservations and batch/location/status shown. Reference-only ingredients/drugs/products do not create inventory records. Generic grouping never authorizes clinical substitution by itself.

The existing Scan Medicine screen and Medicine Master also have an Open drug reference / Clinical reference action. Scanners remain optional. Favorite/unfavorite stores your own reference choices. Recent searches are held only in that client session and cleared at sign-out. Source links and FDA verification dates are shown as supplied in published content. Philippine registration and hospital formulary approval remain separate; neither is inferred from an international medicine name.

## Build and review a clinical database

Only Chief Pharmacists and Pharmacists author/import/review clinical packages. Technical System Administrators can inspect permitted status/history but cannot grant clinical approval. Existing roles are reused; a pharmacist acts as clinical reviewer. Separate Physician/reviewer identity roles are not introduced in this foundation release.

1. Start from samples/clinical-reference. Use a schema-1 JSON package, Kind/Json CSV, CSV-bundle ZIP or an .xlsx workbook with Manifest, Sources, Ingredients, Drugs, Products, Packages, Monographs and Rules worksheets. Array/complex cells contain JSON; dates are ISO yyyy-MM-dd text. Do not use formulas, macros, external links or Excel date serial numbers. Limits: 20 MB input archive, 2,000,000-character clinical package and 5,000 records per collection. See the import README.
2. For every clinical section/rule, record the source key, organization, document, public HTTPS URL/identifier, revision/retrieval dates and lawful-use basis. Preserve original severity separately from the hospital's mapped severity. Do not place access tokens, patient identifiers or secret URLs in source records. Do not import unlicensed vendor data.
3. Use Import / preview package. Verify counts, SHA256, references, duplicates, reversed ingredient-pair duplicates, required conditions, review dates and license permissions. Confirm rights and enter the authorization reference. Commit creates a Draft transactionally; failed imports do not partially populate content.
4. Select the draft in Content versions and Submit it. A different pharmacist must Review it. The author cannot review or approve their own package. A Chief Pharmacist other than the author must Approve it; the reviewing Chief may also be the approver. None of these actions is performed automatically by AI or by importing.
5. Publish only after actual clinical/source validation. Empty/source-only and overdue packages cannot be published. Publishing creates an append-only publication revision. Drafts and unpublished changes do not screen medications.
6. Edit source package creates a new version; change Version before preview/import. Published payloads/rules/reviews cannot be edited in place. Restore publication appends a pointer to an earlier approved, non-overdue published version. It preserves history and invalidates existing pending assessments.
7. Map inventory medicine to an exact published product after verifying ingredients, generic/approved synonym, brand, strength, form, route and package evidence. Mismatches are rejected. New versions require current verified mappings; no name-based auto-mapping occurs. Inventory stock is not duplicated or altered by mapping.

The current schema supports ingredients, combination-drug ingredients, products, packages/barcodes, source records, structured monographs, original/normalized severity, ingredient/class/disease/food/supplement/allergy/duplication rules, IV condition predicates, publication/review history, mappings, screenings and interventions. Original package content is pinned and checksum-verified. Review dates, sources, absent sections and limited coverage remain visible.

## Run a reference safety screen

Select medicines and choose Add selected, or use barcode lookup. You can add published reference-only drugs/products, including outside brands, without creating hospital stock. Remove individual medicines or clear the list. Choose Run safety check.

Enter only the minimal encounter reference. Reconcile the actual active medication list, including outside/OTC/supplement use; the app does not infer it from old issues. Enter optional daily dose, explicit units, route and frequency. Daily dose is not the quantity being dispensed. Ambiguous decimal/group separators are rejected or marked incomplete. The application does not invent missing doses or calculate cumulative exposure/dose conversions in this foundation release.

Keep allergy history Unknown unless reviewed. Document exact ingredient/class/brand identity, reaction, severity/date, confirmed/suspected status, intolerance, information source and verification state. A brand allergy needs its exact reference/inventory ID and a current verified ingredient mapping; unresolved identities remain incomplete. Intolerance is distinguished from confirmed allergy. Class cross-reactivity requires a published rule and is not assumed for every medicine in a broad class. Add relevant condition/food/supplement keys and rule context; unrecognized terms and missing conditions remain unknown.

The server evaluates every entered medication pair, resolves verified combination products, checks exact ingredient duplication and declared allergies, and applies only published rules. Directional effects retain their source orientation regardless of list-entry order. Conditional missing data or stale rules retain UNKNOWN status and the potential severity. A missing pair rule is UNKNOWN, not a negative interaction. An explicit NoKnownInteractionFound rule applies only to its cited scope and is never a guarantee of safety.

Results show counts, severities, original source classification, evidence, mechanisms/consequences, management/monitoring, supported dose/alternative text, conditions and review dates. Contraindicated/major findings are prominent; moderate findings use amber, minor/unknown findings use neutral styles. Incomplete coverage is prominent. The source identities/version are retained.

Pharmacists can record Hold, Escalate or Proceed with documented review. Management and an authorization/reference are required. Incomplete coverage requires the approved Chief policy plus explicit independent review. Contraindicated or potentially contraindicated findings additionally require documented prescriber clarification/authorized escalation. No AI decides or overrides the decision. Technical/read-only users cannot record a pharmacist decision.

## Dispensing integration

Verify and dispense still checks the original order, actual brand, patient, manual-dispensing policy, barcode/package/batch, expiry, recall, reservation and quantity. After the transaction form, the client adds the actual issue product to the screening list and opens the reconciliation/context screen, then the pharmacist review window. Cancel/Hold/Escalate performs no stock movement.

A proceeding assessment is pinned to the pharmacist, workstation, encounter, exact order/balance/quantity, actual medicine, dataset publication, policy and medicine/brand-allergen mappings. It expires after 15 minutes and can support only one committed issue. A changed context, mapping, policy, publication or batch requires re-screening. The idempotency key still prevents duplicate committed issues on a retry. Old clients/direct calls missing the assessment are refused.

## IV, dosage, formulary and advanced sections

IV compatibility is a separate mode. Rules require exact concentration values with units for both medicines, diluent, temperature, container and contact conditions. Ordinary drug-interaction results do not establish IV compatibility. No validated IV dataset is supplied; without it the result is UNKNOWN.

Dosage/renal/hepatic/pregnancy/lactation/antibiotic/formulary sections display approved sourced monograph sections. Missing content stays unavailable. This foundation does not implement validated dose calculators, CrCl/eGFR/Child-Pugh calculators, cumulative-exposure arithmetic, automatic dose maxima, infusion calculations, susceptibility generation, automated antibiogram processing, a normalized P&T committee/restriction enforcement system or clinical photos. Do not infer guidance from an empty section. Source-specific renal methods and narrative pregnancy/lactation information must be provided and reviewed.

AI Drug Assistant is currently approved-source lookup with external AI disabled. No generated clinical answer, subscription or external patient-data transmission is enabled. Scheduled source harvesting and automated clinical publication are not enabled. Licensed interaction/IV adapters, validated calculators, hospital document ingestion and clinical validation are later controlled phases, not hidden completed features.

## Reports, audit and privacy

Drug Safety Alerts exposes role-authorized screening/intervention history. Open your own screening; Chief Pharmacists/Auditors have the permitted investigation access. Immutable screenings retain the medicine resolution, entered context, results, digest and dataset pointer. Interventions retain actor/role, decision, management, clarification and authorization. Audit Trail also records central actions/access with workstation and server date/time.

Print / PDF uses the Windows print system. Export report writes the visible clinical report, including uncertainties and source/version context. Delivery to paper/PDF/local disk is client-reported, not independently verified by the server. Use only authorized hospital interfaces; keep patient names out of free text. Cerebro's existing CSV exchange and live-posting-disabled state are preserved. There is no direct Cerebro database access.

## Validation and acceptance

Automated synthetic engineering tests cover import/permissions/approval separation, ingredient combinations, declared severities, missing/conditional/stale coverage, dosage-unit ambiguity, allergies/intolerances, immutable records, publication rollback, exact dispensing binding, idempotency and concurrent server clients. They do not validate real clinical claims. See the clinical validation checklist and capability matrix. Hospital pharmacist validation, source permissions, real four-workstation/LAN testing, physical scanners, clean-server installation and DPO/production acceptance remain pending.
