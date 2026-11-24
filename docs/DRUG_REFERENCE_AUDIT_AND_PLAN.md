# Drug Reference integration audit and controlled implementation

24 November 2026. Baseline: 74ee51a; branch: feature/drug-reference-safety.

The existing solution was inventoried before changes: WPF Windows desktop client; .NET 10 minimal API; ASP.NET Identity bearer authentication with live account/stamp checks; EF Core/Npgsql central PostgreSQL; immutable stock movements and versioned hash-chain audit; optional HID/COM/image barcode input; CSV exchange with a mock-only Cerebro boundary; Inno Setup client/server installers; protected Server Control configuration and separate migration/runtime accounts. No technology stack replacement is needed.

Medicine Master has one product record per brand/strength/form and existing immutable IDs. Generic grouping is computed; it does not merge stock. MedicineBarcode and PackagingUnit already supply package identity. Inventory balances link batches/products/locations; dispensing validates quantity, policy, verified patient, exact brand/approved substitution, barcode, expiry, recall and reservations. These checks remain in place. Historical dispensing is NOT a reconciled active medication list. There is no existing allergy, clinical monograph, interaction or patient-condition source.

Reuse existing authentication, role-based navigation, registered workstation IDs, protected HTTPS pairing, transactional/idempotent mutations, audit chain, inventory lookup, barcode resolver, CSV utilities, print/export capabilities, tests, migrations and installer structure. Keep clinical content in additional tables and map existing Medicine.Id values; do not duplicate inventory records. Keep patient context minimal and screening/intervention history append-only. Do not write to Cerebro.

Backups of the previous source/release archives were taken under backups/before-drug-reference-* before changing source. No installed hospital database or separate physical server is accessed by development tests.

## Implementation phases and gates

1. Foundation: normalized ingredient/drug/product/package/source/rule tables, versioned content packages, audited inventory mapping, Drug Reference navigation, local search and readable monographs. Additive migration must preserve old data/audit hashes.
2. Governance: package preview, checksum verification, bounded transactional import, source/license attestations, distinct author/reviewer/Chief approval, publication history and rollback to an earlier reviewed version. Drafts never participate in screening. Existing databases start with no published clinical dataset.
3. Safety: deterministic ingredient/class/condition rules, combination products, exact documented allergy matching, potential ingredient duplication, explicit unknown coverage, immutable screening and pharmacist intervention records. Bind dispensing assessment to actor, encounter, exact order/balance/quantity, content version and mappings; require documented clinical review and clarification for interruptive alerts. Do not infer active medications from history.
4. Reference tools: dataset-driven dosage, renal/hepatic, pregnancy/lactation, antimicrobial and IV sections. Missing evidence stays unavailable. No medication recommendation is invented. Optional external AI remains disabled; local approved-content retrieval works without internet or subscriptions.
5. Verification: synthetic engineering fixtures are not clinical datasets. Test approval separation, unknown/incomplete assessment, combination resolution, rule orientation/conditional handling, stale assessments, unauthorized access, transaction binding, audit immutability, upgrade/restore and existing workflows. Publish evaluation installers only after compilation and payload checks.

## Content and acceptance limits

The user confirmed no licensed dataset is available and requested the governed import/review workflow. Deliver source metadata and empty clinical import templates, not fabricated monographs or interaction rules. Clinical validation by hospital pharmacists, source permissions and hospital acceptance are external gates. All 18 reference sections must state actual coverage. An empty database is not clearance. IV compatibility cannot be inferred from ingredient identity or ordinary interaction rules. The specified real-drug test pairs remain clinical validation scenarios until authorized evidence is supplied.

NLM states its RxNav interaction feature was discontinued in 2024; RxNorm terminology does not supply comprehensive interactions. WHO ATC material has separate usage conditions; no full ATC index is bundled. Philippine registration must be separately verified on the FDA portal and must not be inferred from US terminology. See DRUG_REFERENCE_SOURCES.md.
