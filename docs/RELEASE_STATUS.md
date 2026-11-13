# Release status - Server Control 0.2.0 / client and API 0.1.0

Build date: 13 November 2026 (Asia/Manila).
Hospital: Digos Doctors Hospital, Inc.
Developer: Vicente C. Cavalida Jr. MD.
Permanent workspace: E:\ChatGPT Codex\DDHI-SmartPharmacy.

## Implemented

- Self-contained Windows x64 WPF client, ASP.NET Core central API, EF Core/PostgreSQL migrations, maintenance worker and encrypted backup utility.
- Official original logo retained unchanged; padded PNG/ICO, branded splash, login, dashboard, About screen, installers, manuals and printed reports.
- Individual Identity accounts, strong password rules, lockout, 20-minute bearer sessions, role policies, account locking, rate limits and HTTPS enforcement in Production. Technical administrators do not inherit dispensing privileges.
- Medicine catalog, active status, approved multiple barcode mappings, packaging conversion, receiving by supplier/delivery line, batch, expiry, locations, current balances and FEFO lookup.
- GS1 AI 01/10/17/21, GTIN check digits, AIM/FNC1/group separators, EAN/UPC, explicit Code 128, DataMatrix image decoding and DDHI internal codes. Barcode input never completes a transaction.
- Optional zero-to-many workstation scanner profiles, profile editing, HID/manual input, serial-port capture and diagnostics. HID and image input stay unverified; device presence is never inferred from a configured profile.
- Authorized order-reference recording, pharmacist dispensing confirmation, patient-verification confirmation, exact-medicine matching, expired/recall/quarantine/insufficient-stock blocks, reservations and network-failure handling.
- Approved manual dispensing policy disabled by default; Chief Pharmacist written approval required to enable it. Physical-device attestation is blocked in this build.
- Atomic receiving, transfers, counts, quarantined returns, stock status transitions, approved disposition, recall registration and zero-stock recall closure. Chief Pharmacist approval required for releasing held stock.
- Linked, single-use receiving/count/paired-transfer reversals; dispensing corrections use returns. Immutable movement ledger, nonnegative database constraints, serialized writes, request fingerprints, idempotency and chained audit with database mutation guards.
- Mock Cerebro adapter, fail-closed vendor REST/read-only adapter boundaries, approved catalog CSV import, stock-snapshot comparison API, outbox/manual acknowledgment and reconciliation. No live vendor posting or billing.
- Operational reports, CSV/Excel-compatible export, branded Windows printing/PDF printer support and internal stock-label printing.
- Chunked authenticated encrypted PostgreSQL backup, integrity verification, isolated restore tooling, Windows deployment/service/backup scripts, Linux systemd templates, installers, seven PDF manuals, source archive and release checksums.

## Verified in this development environment

- Latest solution build: zero warnings and zero errors.
- 55 unit tests passed, 49 real PostgreSQL integration tests passed; 21 further read/audit checks passed against an authenticated restored backup.
- Tested concurrent final-unit dispensing, negative-stock constraints, role restrictions, wrong medicine, expired/recalled stock, manual-policy blocks, quarantine/returns, transfers, reversal uniqueness, scanner-profile disable, report queries, audit chain and ledger immutability.
- Published self-contained server tested over HTTPS in Production environment using a pinned, temporary test certificate: migration, bootstrap, staff authentication, catalog, receiving, inventory, audit verification and worker startup succeeded.
- Encrypted backup authenticated and restored into an isolated database; medicine, ledger and audit counts plus total stock matched before additional restored-database checks.
- Native Windows client launch and visible hospital branding/login reviewed. No real staff authentication was automated through the desktop UI.
- Installer compilation and extraction-only payload verification are recorded separately in Tests/InstallerPayloadSmoke.json. Extraction testing does not establish full installation acceptance.
- Dependency advisory scan completed against NuGet metadata at build time; see Tests/DependencyAudit.json. This is not an independent security or clinical certification.

## Pending, unverified or blocked

- Hospital IT and pharmacy leadership production acceptance is mandatory. This release is not declared production-ready or clinically validated.
- Four actual hospital workstations, real scanner devices, thermal printers, service recovery, clean Windows Server installation, upgrade/uninstall and off-server disaster-recovery drills require on-site acceptance. Automated concurrent HTTP tests are not physical multi-workstation tests.
- Live Cerebro REST/read-only contracts, credentials, vendor authorization and ownership/reconciliation agreements have not been provided. Those adapters intentionally fail closed. No Cerebro production database or billing record was accessed or modified.
- No hardware-attested physical-scan verification or vendor SDK adapter is supplied. HID/manual/image/serial test input cannot bypass the approved manual verification policy.
- Hospital TLS certificates, internal DNS/firewall rules, PostgreSQL installation, least-privilege production accounts, off-server secret storage and backup scheduling must be provisioned by hospital IT. The installer now includes PostgreSQL 17.12 and desktop Server Control. Its confirmed wizard creates the database and separate protected credentials on the destination server. It does not install an operating system, approve DNS or silently establish certificate trust.
- Code signing certificate was not provided; executables and installers are unsigned.
- Month-only GS1 expiry is rejected pending an approved pharmacy policy. Advanced repackaging/shelf-label layouts and full purchase-order fulfillment need approved hospital specifications; current stock labels preserve source balance/lot traceability.
- Inventory costing uses the batch unit cost captured at initial receiving; cost revaluation and differing purchase-cost policy are not implemented. Financial acceptance must review valuation before relying on accounting reports.
- Linux service templates are supplied for future deployment; no Ubuntu target was selected or Linux runtime acceptance performed. The packaged server target is Windows x64.
- Retention, capacity, accessibility, penetration testing, secure-update signing and a complete hospital privacy/compliance assessment remain acceptance activities.

## Evidence and entry point

Start with Installation Guide.pdf and Administrator Guide.pdf. The release has no default production password or patient data. Test databases, private keys, caches and development backups are excluded from the release ZIP.

Checksums/ReleaseVerification.json records final artifacts and test results. Checksums/SHA256SUMS.txt covers the release contents; the ZIP has a separate .sha256 sidecar.


## Desktop server update

Server Control shows the six requested steps: install, PostgreSQL/accounts, certificate/services, administrator, start/check, and client connection. MSI is preparation/preview only; installation is for a different server. Localhost-only evaluation is selected for now. Service secrets are machine-protected with restricted ACLs. Local certificate trust requires an explicit operator action. Hospital LAN identity requires IT approval. See SERVER_DESKTOP_SETUP.md.
