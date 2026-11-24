# Empty clinical import templates

These original templates contain no clinical drug advice, monographs or interaction rules. They are not preapproved. source-registry.json is informational link metadata only and is not a clinical package. Test fixtures under tests are never production seed data.

Use one of:

- clinical-package-template.json: all seven arrays are required; populate with lawfully supplied content, exact references and a new Version.
- clinical-csv-bundle-template.zip: manifest.json plus sources.csv, ingredients.csv, drugs.csv, products.csv, packages.csv, monographs.csv and rules.csv. Every CSV needs its header, even when empty. Array/complex cells contain JSON, escaped with ordinary CSV quoting.
- clinical-records-template.csv: Kind,Json header; exactly one Manifest row; additional rows use Sources, Ingredients, Drugs, Products, Packages, Monographs or Rules and a JSON record of the corresponding schema.
- .xlsx: create exactly named Manifest, Sources, Ingredients, Drugs, Products, Packages, Monographs and Rules sheets. Manifest has columns Schema,Version,LicenseStatement and one data row (Schema 1). The other sheets use the corresponding CSV headers. Put arrays/objects in literal JSON text cells. Dates must be literal yyyy-MM-dd text. Formula cells, macros, external links and Excel serial dates are rejected. Empty tables still need headers.

Stable keys use lowercase letters/digits/dot/underscore/hyphen, max 64 characters. Each product links a clinical drug; each drug lists all ingredient keys, amounts/units where known, class tags and approved synonyms. Existing inventory IDs are mapped separately after publication, never created by clinical import.

Every clinical monograph section/rule needs source keys. Source dates must be real, not invented. Monograph headings must match ClinicalReference.cs / DrugReferenceSections.MonographFields. Missing sections are explicit. Record last-review and review-due dates. Rule conditions support eq/lt/lte/gt/gte; numerical dose conditions require a matching exact dose-unit condition. IV rules additionally require exact ivConcentrationA, ivConcentrationB, ivDiluent, ivTemperature, ivContainer and ivContact values, including units.

Rule severities: Contraindicated, Major, Moderate, Minor, Unknown, NoKnownInteractionFound. Preserve OriginalSeverity. A missing rule is always UNKNOWN. A NoKnownInteractionFound rule is an affirmative reviewed statement scoped to its source/conditions; it is never inferred from an empty collection. Symmetric reversed duplicates are rejected; directional effects retain orientation but are detected regardless of the list-entry order.

Use separate draft author and reviewer, then Chief Pharmacist approval and publication. Empty/source-only and overdue packages cannot publish. Never enable incomplete-coverage handling without the hospital's authorized written policy. See the complete guide and clinical validation checklist.
