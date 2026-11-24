# Capability and acceptance matrix

This release implements the governed foundation selected by the user without a licensed clinical dataset.

| Area | Delivered | Remaining gate / limitation |
| --- | --- | --- |
| Existing project | Additive WPF/API/PostgreSQL integration, branch/backup, preserved inventory/Cerebro/roles/ledger/audit | Real hospital upgrade acceptance |
| Search/navigation | 18 inner sections, partial/fuzzy search, names/synonyms/ingredients/classes/brands/barcodes/manufacturers/registration/indication, stock filter, favorites/recent selections | Bounded results; large licensed-corpus performance/clinical terminology validation |
| Clinical data | Source/version/rule/product/ingredient/package/monograph/mapping structures and readable views | No actual licensed clinical corpus or PH brand registry bundled |
| Governance | Checksum/preview/staging, transactional imports, review/Chief approval, publication, immutable history, pointer rollback | Real source permissions and clinical review by hospital staff |
| Safety engine | Deterministic declared rules, combinations, pair enumeration, conditional/stale unknowns, source classifications, ingredient duplication, exact documented allergies/intolerance and rule-based cross-reactivity | Synthetic engineering tests are not clinical validation; no comprehensive coverage claim |
| Patient context | Entered reconciled medication list, conditions, allergy details, dose/units/route/frequency; minimal encounter references | No approved live MAR/allergy/patient feed; no historical-medication assumptions |
| Dispensing | Assessment/decision gate, 15-minute expiry, exact actor/workstation/encounter/order/balance/quantity binding, content/policy/mapping change detection, idempotent commit | Hospital-approved independent-review policy is initially disabled; clinical validation required |
| Dosage and special populations | Sourced monograph sections and explicit missing-data states | Validated calculators, numerical renal/hepatic guidance, maxima and cumulative exposure not enabled |
| IV | Separate conditional rule mode with six required compatibility conditions | No validated/licensed compatibility data; absent evidence remains UNKNOWN |
| Formulary/antibiotics | Approved sourced sections and stock links | Full structured P&T committee approval/restriction enforcement, antibiogram uploads/analysis and susceptibility data not implemented |
| AI | Approved-source lookup; no network/paid service required | External AI/RAG generation and privacy-approved provider connection disabled |
| Updates | Manual import preview and controlled publication; encrypted backup through Server Control schema update | No scheduled external harvesting; advanced direct CLI requires independent backup |
| Reporting | Readable monograph/screening, print/PDF and text report export, central history and audit | No new independent clinical Excel analytics suite or external Cerebro clinical interface |
| Clinical validation | Checklist for authorized reviewers and software test evidence | All real-drug scenarios, data licensing, DPO approval and hospital acceptance pending |

No missing feature above is represented as a completed clinical capability. No test fixture is production seed data. The initial source registry is metadata-only and has zero clinical interaction rules.
