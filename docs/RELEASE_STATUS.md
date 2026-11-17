# Release status - Server Control 0.7.0 / client 0.4.0 / API 0.2.0

Build date: 17 November 2026 (Asia/Manila).
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
- Hospital network approval, off-server secret storage and backup scheduling remain IT responsibilities. The installer includes PostgreSQL 17.12 and desktop Server Control; the confirmed wizard creates the database, separate protected credentials and services on the destination server. Simple LAN creates its own encrypted server identity and scoped firewall rules, with explicit first-time client code confirmation. Hospital DNS and a trusted PFX are needed only for optional Advanced TLS. Actual elevated rule/service behavior still requires destination-server acceptance.
- Code signing certificate was not provided; executables and installers are unsigned.
- Month-only GS1 expiry is rejected pending an approved pharmacy policy. Advanced repackaging/shelf-label layouts and full purchase-order fulfillment need approved hospital specifications; current stock labels preserve source balance/lot traceability.
- Inventory costing uses the batch unit cost captured at initial receiving; cost revaluation and differing purchase-cost policy are not implemented. Financial acceptance must review valuation before relying on accounting reports.
- Linux service templates are supplied for future deployment; no Ubuntu target was selected or Linux runtime acceptance performed. The packaged server target is Windows x64.
- Retention, capacity, accessibility, penetration testing, secure-update signing and a complete hospital privacy/compliance assessment remain acceptance activities.

## Evidence and entry point

Start with Installation Guide.pdf and Administrator Guide.pdf. The release has no default production password or patient data. Test databases, private keys, caches and development backups are excluded from the release ZIP.

Checksums/ReleaseVerification.json records final artifacts and test results. Checksums/SHA256SUMS.txt covers the release contents; the ZIP has a separate .sha256 sidecar.


## Desktop server update

Server Control opens as a desktop window on the dedicated server. Initial setup defaults to Windows services and automatic LAN setup. For an existing prepared instance, use Connect computers > Enable LAN. MSI is preparation/preview only; installation is for a different server. Service secrets are machine-protected with restricted ACLs. The updated client confirms the server's public-key identity once without changing Windows certificate trust. See SERVER_DESKTOP_SETUP.md.

## Existing-instance LAN conversion in 0.3.0

The LAN connection page converts an existing prepared localhost/LAN instance using a valid trusted matching PFX and approved hostname. It verifies local DNS mappings, preserves users, stock and database credentials, saves protected connection snapshots, starts the new endpoint with the selected certificate, and attempts rollback on failed health checks. Manual restoration handles persisted interrupted changes. A custom-profile picker opens existing data without initialization. Existing application users disable first-administrator creation. The installer now validates service ownership and waits for stop completion during maintenance. Automated evidence is recorded in ServerManagerTests.trx and LanUpgradeSmoke.json; elevated service lifecycle and physical hospital-client acceptance remain pending. Tests use an isolated CA and socket route without altering Windows trust or hospital DNS; QA-only initdb skips durability syncing, which is not a production setting.


## Simple automatic connection in 0.4.0

The new default workflow is Enable LAN on the server, then automatic discovery and one-time code confirmation on each updated client. A public .ddhiconnect file is the fallback. Server/client HTTPS is authenticated by the paired public key rather than requiring a hospital CA in simple mode. Keys are generated/protected on the server; client trust metadata is DPAPI-protected per Windows user. DNS, manual PFX import and an approval-reference field are not required for this mode. Discovery never sends staff credentials or silently establishes a new trust identity. Existing accounts and inventory are retained, and repeated enablement retains the server key. A different key and invalid certificate dates are rejected. The worker serves local-subnet UDP discovery metadata only. Default ports are TCP 8443 and UDP 8444; owned firewall rules are scoped to the selected adapter, local subnet and API/worker programs. Advanced TLS remains optional.

All 37 Server Manager tests passed. SimpleConnectionSmoke.json and ServerManagerTests.trx record isolated discovery, pinned HTTPS, key rejection, login, data/credential preservation and restart checks. Windows trust stores are not changed. Actual elevated firewall/service/reboot behavior and physical workstation connectivity still need destination-server acceptance; automated loopback tests do not prove those. Live Cerebro remains disabled.

## Database Access and client role navigation — 16 November 2026

Server Control 0.5.0 adds Database Access after Activity, with redacted connection details, health checks, application-authenticated read-only records, CSV/JSON snapshots and explicit IT tool launch. Client 0.3.0 displays the signed-in email/role and filters modules/actions using existing API permissions. A 59-test Server Manager suite passed, including new session, role-menu/action, export, real HTTPS login and permission-denial checks. Role sidebars were rendered using actual client XAML and policy data; no real staff authentication was automated. External SQL Shell/pgAdmin operation, destination-server maintenance and physical client acceptance remain pending. API/worker/database schema are unchanged.

## Separate IT PostgreSQL login — Server Control 0.6.0

Database Access includes an elevated, original-setup-account wizard for a new ddhi_it_ login with an operator-chosen password and explicit access level. Read-only inventory views are default; pharmacy schema-owner maintenance requires acknowledgement and explicit role selection in SQL. No superuser is granted. Existing users are refused rather than reset. Provisioning is transactional, validates managed namespaces and effective PUBLIC permissions, uses a salted SCRAM verifier instead of cleartext password SQL, and preserves application credentials, stock and ledger data. All 76 Server Manager tests passed, including real PostgreSQL authentication, wrong-password denial, view access, forbidden writes/credential reads, maintenance scoping, duplicate protection, rollback, namespace collision and unsafe PUBLIC-grant rejection. No actual hospital-server IT login was provisioned by these tests. Client 0.3.0 is unchanged; destination-server installation/tool acceptance remains pending. See IT_DATABASE_LOGIN.md.

## Readable scans and staff editing � 0.7.0

Scan results display named catalog, package and stock fields with explicit missing encoded data. Exact approved mappings, equivalent zero-padded GTIN aliases, distinct packaging indicators and ambiguous mapping denial share one resolver with dispensing. Image decoding preserves GS1 symbology metadata. User Administration supports editable email/name/phone/multiple roles, clearing optional information, and confirmed account deletion with history retained. Live security-stamp validation revokes changed accounts; stale writes, self-role changes and loss of the last active administrator are rejected. Startup applies DELETE only to five Identity account tables, never stock/audit tables. See SCAN_AND_ACCOUNT_GUIDE.md. Isolated tests and synthetic WPF previews are release evidence; physical scanner/workstation and destination-server upgrade acceptance remain pending.
