# FINAL COMPLETE MASTER DEVELOPMENT PROMPT
# DDHI SMART PHARMACY BARCODE MANAGER
## Hospital Pharmacy Inventory, Barcode Scanning, Dispensing Safety, and Cerebro HIS Integration
### Windows 11 Desktop and Dedicated Server Edition

**Hospital:** Digos Doctors Hospital, Inc. (DDHI)  
**Developer:** Vicente C. Cavalida Jr. MD  
**Development Platform:** ChatGPT Codex Desktop for Windows  
**Client Operating System:** Windows 11 x64  
**Server:** New dedicated physical hospital pharmacy server  
**Server Operating System:** Windows Server 2025 Standard or Ubuntu Server LTS  
**Desktop Technology:** C# / .NET 10 / WPF / MVVM  
**Backend:** ASP.NET Core Web API  
**Database:** PostgreSQL  
**Network:** Hospital LAN  
**Existing HIS:** Cerebro Hospital Information System  
**Initial Deployment:** Four Windows 11 pharmacy workstations  
**Barcode Scanners:** Optional and dynamically configurable  
**Expansion:** Additional workstations and scanners without architectural redesign  
**Final Deliverables:** Windows EXE, client installer, server installer, source code, tests, documentation, and release ZIP

---

# 1. YOUR ROLE AND PRIMARY MISSION

Act as a senior hospital information systems architect, pharmacy informatics specialist, C#/.NET developer, Windows desktop engineer, GS1 barcode specialist, database architect, cybersecurity engineer, integration engineer, and deployment specialist.

Develop a complete, secure, reliable, maintainable, and production-oriented hospital pharmacy barcode and inventory management system for Digos Doctors Hospital, Inc.

The application must support medicine receiving, barcode registration, inventory tracking, batch and expiration monitoring, dispensing verification, stock transfers, recalls, reporting, and safe integration with Cerebro HIS.

Do not stop at architecture, pseudocode, or source code. Implement, compile, test, package, and verify the application.

Preserve existing working functionality and data.

Do not modify Cerebro production software, databases, billing, or inventory records without explicit vendor and hospital authorization.

---

# 2. HOSPITAL LOGO AND OFFICIAL BRANDING

The user has supplied the official Digos Doctors Hospital, Inc. logo as an image attachment named:

`DIDH LOGO.jpg`

**This uploaded image is the authoritative branding reference.**

Before development, locate the uploaded logo in the project workspace. If it is not accessible to Codex, request that the user copy the image into the project's `assets/branding/` directory.

Do not substitute an AI-generated logo.

Do not redraw, recolor, distort, crop, or alter the hospital seal without approval.

The logo contains:

- Circular hospital seal.
- Digos Doctors' Hospital, Inc. lettering.
- Green medical cross.
- Medical staff symbol.
- Five yellow stars.
- Hospital motto and values.
- Year 1991.

Preserve the logo's original appearance, proportions, colors, and text.

## Branding requirements

Display the logo on:

1. Application splash screen.
2. Login screen.
3. Main dashboard.
4. About page.
5. Installer welcome screen where supported.
6. Application documentation.
7. Appropriate printed reports.
8. Optional desktop shortcut icon, using a properly generated icon asset.

Create an application icon from the supplied logo using a transparent or appropriately padded square canvas without distorting the circular seal.

Produce suitable PNG and ICO assets.

Retain the original image in the project assets.

Use the following branding:

**Application name:** DDHI Smart Pharmacy Barcode Manager  
**Hospital:** Digos Doctors Hospital, Inc.  
**Developer:** Vicente C. Cavalida Jr. MD  
**Application type:** Hospital Pharmacy Inventory and Barcode Management System

Do not use `aivicventures` as the displayed developer name.

Use the hospital's official name consistently.

The About page must display:

- Application name.
- Hospital name.
- Developer name.
- Application version.
- Build date.
- Copyright notice.
- Technical support contact only if supplied.
- Software license information.

Do not invent contact information.

---

# 3. MANDATORY INITIAL CODEBASE AUDIT

Before modifying code:

1. Inspect the current project directory.
2. Identify existing source files.
3. Review the application architecture.
4. Inspect database schemas and migrations.
5. Inspect scanner integration.
6. Inspect Cerebro integration.
7. Review authentication and security.
8. Inspect installer configurations.
9. Verify installed .NET and Windows build tools.
10. Confirm that the hospital logo is accessible.
11. Identify missing features and technical risks.

If an existing project exists, upgrade it rather than rebuilding unnecessarily.

Create a Git branch or backup before significant changes.

Generate:

- `INITIAL_AUDIT.md`
- `ARCHITECTURE.md`
- `IMPLEMENTATION_PLAN.md`
- `DATA_OWNERSHIP.md`
- `CEREBRO_INTEGRATION_ASSUMPTIONS.md`
- `RISK_REGISTER.md`

Then proceed with development.

---

# 4. ACTUAL DDHI DEPLOYMENT

The hospital currently has four Windows 11 pharmacy workstations.

All four computers already run Cerebro HIS.

A new dedicated physical server will host the DDHI Smart Pharmacy backend and central database.

The application must support:

- Zero barcode scanners.
- One barcode scanner.
- Two barcode scanners.
- Three barcode scanners.
- Four barcode scanners.
- More than four barcode scanners.
- Additional workstations.
- Multiple scanners per workstation where supported.
- Any combination of scanner-equipped and scannerless workstations.

Never hardcode scanner count or workstation count.

The initial deployment has four workstations, but the system must support expansion subject to tested capacity.

---

# 5. SYSTEM ARCHITECTURE

```text
                    DDHI HOSPITAL LAN
                            |
                 +----------------------+
                 | EXISTING CEREBRO HIS |
                 | SERVER               |
                 | Remains unchanged    |
                 +----------------------+
                            |
                    Approved Interface
                            |
                 +----------------------+
                 | CEREBRO INTEGRATION  |
                 | SERVICE / ADAPTER    |
                 +----------------------+
                            |
                 +----------------------+
                 | NEW DEDICATED DDHI   |
                 | PHARMACY SERVER      |
                 |                      |
                 | ASP.NET CORE API     |
                 | POSTGRESQL DATABASE  |
                 | AUTHENTICATION       |
                 | INVENTORY ENGINE     |
                 | AUDIT / BACKUP       |
                 +----------------------+
                            |
                       HOSPITAL LAN
                            |
           +----------------+----------------+
           |                |                |
      WORKSTATION 1    WORKSTATION 2    WORKSTATION 3+
      Windows 11       Windows 11       Windows 11
      Cerebro HIS      Cerebro HIS      Cerebro HIS
      DDHI EXE         DDHI EXE         DDHI EXE
      Scanner optional Scanner optional Scanner optional
```

All clients communicate with the central API over HTTPS.

Clients must not connect directly to PostgreSQL.

No separate authoritative inventory database is allowed on each workstation.

Cerebro integration must use only approved interfaces.

Do not expose the database or API publicly by default.

---

# 6. TECHNOLOGY STACK

Use:

- C# / .NET 10.
- WPF with MVVM.
- ASP.NET Core Web API.
- PostgreSQL.
- Entity Framework Core.
- Dependency injection.
- GS1-compliant barcode parsing.
- ZXing.Net or equivalent for image barcode decoding.
- ASP.NET Core Identity or approved hospital identity integration.
- Serilog.
- .NET BackgroundService.
- OpenAPI.
- xUnit.
- WiX Toolset or equivalent installer framework.
- Git.

Prefer stable, supported, maintainable, and appropriately licensed components.

Avoid unnecessary paid libraries.

Do not embed production secrets in desktop executables.

---

# 7. SOLUTION STRUCTURE

Create or extend:

```text
DDHI.SmartPharmacy/
|
+-- assets/
|   +-- branding/
|       +-- DIDH LOGO.jpg
|       +-- hospital-logo.png
|       +-- hospital-icon.ico
|
+-- src/
|   +-- DDHI.Pharmacy.Desktop/
|   +-- DDHI.Pharmacy.Api/
|   +-- DDHI.Pharmacy.Application/
|   +-- DDHI.Pharmacy.Domain/
|   +-- DDHI.Pharmacy.Infrastructure/
|   +-- DDHI.Pharmacy.Contracts/
|   +-- DDHI.Pharmacy.Barcode/
|   +-- DDHI.Pharmacy.Integration/
|   +-- DDHI.Pharmacy.Worker/
|
+-- tests/
|   +-- UnitTests/
|   +-- IntegrationTests/
|   +-- BarcodeTests/
|   +-- ScannerHardwareTests/
|   +-- SecurityTests/
|   +-- ConcurrencyTests/
|   +-- WorkflowTests/
|   +-- InstallerTests/
|
+-- database/
+-- installer/
+-- deployment/
+-- scripts/
+-- docs/
+-- samples/
|
+-- DDHI.SmartPharmacy.sln
```

Use clean architecture, dependency injection, and separation of concerns.

---

# 8. PRIMARY FUNCTIONAL REQUIREMENTS

Implement:

1. Medicine master catalog.
2. Physical barcode scanning.
3. Optional scanner configuration.
4. GS1 barcode interpretation.
5. Barcode registration.
6. Inventory receiving.
7. Batch and lot tracking.
8. Expiration monitoring.
9. Real-time inventory.
10. FEFO stock selection.
11. Dispensing verification.
12. Authorized manual workflows.
13. Stock transfers.
14. Returns.
15. Inventory adjustments.
16. Low-stock alerts.
17. Recall management.
18. Inventory counting.
19. Reconciliation.
20. Reports.
21. User accounts and permissions.
22. Audit trails.
23. Cerebro synchronization.
24. Duplicate transaction prevention.
25. Backup and recovery.
26. Multi-workstation operation.
27. Windows EXE deployment.

---

# 9. OPTIONAL AND SCALABLE BARCODE SCANNERS

Barcode scanners must be optional on every workstation.

Support:

- USB HID keyboard-wedge scanners.
- Wireless scanners with USB receivers.
- Bluetooth HID scanners.
- USB virtual COM scanners.
- Optional vendor SDK scanners.

Support zero-to-many configured scanners per workstation.

Do not require a scanner for startup, login, inventory lookup, reporting, or other permitted scannerless functions.

## Scanner configuration

Provide:

- Scanner enabled/disabled.
- Scanner type.
- Input mode.
- Input suffix.
- Test scan.
- GS1 separator test.
- DataMatrix test.
- Workstation-specific configuration.
- Diagnostics.
- Last successful scan.

Do not falsely claim a USB keyboard-wedge scanner is connected unless reliable device-level detection is available.

## Scanning workflow

1. Capture barcode input.
2. Validate data.
3. Identify symbology where possible.
4. Parse GS1 fields.
5. Search medicine catalog.
6. Display product.
7. Display batch and expiry if encoded.
8. Verify stock and recall status.
9. Display warnings.
10. Require appropriate transaction confirmation.

Scanning alone must never automatically complete dispensing or billing.

---

# 10. SCANNERLESS OPERATION

Workstations without scanners must support permitted functions such as:

- Medicine search.
- Inventory lookup.
- Receiving.
- Stock counting.
- Expiration monitoring.
- Reports.
- Administration.
- Reconciliation.

Clearly distinguish:

- Physical scan verification.
- Manual barcode entry.
- Manual medicine selection.

Never record manually typed barcode data as a verified physical scan.

If manual dispensing verification is permitted, require a separately approved hospital workflow, appropriate pharmacist authorization, and an audit trail.

The server must enforce these safeguards.

---

# 11. GS1 BARCODE SUPPORT

Support:

- EAN-8.
- EAN-13.
- UPC-A.
- Code 128.
- GS1-128.
- GS1 DataMatrix.
- Internal hospital barcodes.

Recognize:

| GS1 AI | Field |
|---|---|
| 01 | GTIN |
| 10 | Batch / Lot |
| 17 | Expiration |
| 21 | Serial Number |

Implement GTIN check-digit validation, FNC1 handling, group separators, variable-length fields, and invalid barcode rejection.

Never assume all manufacturer barcodes include batch and expiration.

---

# 12. MEDICINE MASTER CATALOG

Store:

```text
MedicineId
CerebroMedicineId
GenericName
BrandName
Strength
DosageForm
Route
Manufacturer
Category
BaseUnit
PackagingUnit
ConversionFactor
ReorderLevel
ActiveStatus
CreatedAt
UpdatedAt
```

Maintain separate barcode, packaging, and Cerebro mapping tables.

Support multiple barcodes per medicine.

Require approval for ambiguous mappings.

Do not merge medicines based only on similar names.

---

# 13. BARCODE REGISTRATION

Workflow:

1. Select medicine.
2. Scan or enter barcode through an authorized workflow.
3. Decode barcode.
4. Verify medicine identity.
5. Confirm packaging.
6. Configure quantity conversion.
7. Check duplicate mappings.
8. Obtain authorized approval.
9. Save centrally.
10. Record audit event.

Unknown barcodes must not automatically become active medicines.

---

# 14. INVENTORY RECEIVING

Implement:

1. Supplier selection.
2. Purchase order or delivery reference.
3. Medicine scanning or selection.
4. Product verification.
5. Batch entry.
6. Expiration entry.
7. Quantity entry.
8. Packaging conversion.
9. Cost entry when authorized.
10. Receiving validation.
11. Transaction confirmation.
12. Central stock update.
13. Authorized Cerebro synchronization.

Support partial deliveries, multiple batches, and duplicate receipt prevention.

---

# 15. CENTRAL INVENTORY MANAGEMENT

Track inventory by:

- Medicine.
- Batch.
- Expiration.
- Location.
- Status.

Statuses:

- Available.
- Reserved.
- Quarantined.
- Recalled.
- Expired.
- Damaged.
- Pending verification.

Implement FEFO, stock transfers, stock reservations, returns, adjustments, stock counts, and reconciliation.

Use an immutable stock movement ledger.

Prevent negative stock through atomic transactions and concurrency controls.

---

# 16. EXPIRATION MANAGEMENT

Provide configurable alerts at:

- 180 days.
- 90 days.
- 60 days.
- 30 days.
- Expired.

Display medicine, batch, quantity, location, expiration, and stock value.

Block routine dispensing of expired stock.

Use Asia/Manila for hospital-facing timestamps.

Handle month-only expiration according to approved pharmacy policy.

---

# 17. MEDICATION DISPENSING VERIFICATION

When authorized medication orders are available:

1. Select patient or encounter.
2. Retrieve active medication order.
3. Verify patient identity.
4. Display prescribed medicine.
5. Scan or use approved manual verification.
6. Verify medicine, strength, form, and quantity.
7. Verify batch and expiry.
8. Check recall and quarantine status.
9. Display discrepancies.
10. Require pharmacist confirmation.
11. Commit dispensing.
12. Update inventory according to approved ownership rules.
13. Synchronize with Cerebro where authorized.
14. Record acknowledgment.

Block routine dispensing of wrong, expired, recalled, quarantined, or unavailable medicines.

Do not automatically substitute medicines.

Do not equate dispensing with administration.

---

# 18. CEREBRO HIS INTEGRATION

Create a modular integration abstraction.

```csharp
public interface ICerebroIntegration
{
    Task<IntegrationResult> TestConnectionAsync(
        CancellationToken cancellationToken);

    Task<MedicineSyncResult> SyncMedicinesAsync(
        CancellationToken cancellationToken);

    Task<OrderLookupResult> GetMedicationOrderAsync(
        string orderId,
        CancellationToken cancellationToken);

    Task<PostingResult> PostDispensingAsync(
        DispensingPayload payload,
        CancellationToken cancellationToken);
}
```

Implement:

- Mock adapter.
- Vendor-approved REST API adapter.
- Vendor-approved read-only database adapter.
- Approved CSV import/export adapter.

Do not invent Cerebro endpoints or schemas.

Do not directly modify Cerebro production tables.

Keep live synchronization disabled until authorized.

---

# 19. CEREBRO DATA OWNERSHIP AND RECONCILIATION

Define ownership for:

- Medicine master.
- Patient references.
- Medication orders.
- Stock balances.
- Receiving.
- Dispensing.
- Billing.
- Returns.
- Adjustments.

Prevent duplicate billing and double stock deduction.

Implement reconciliation reports.

Do not assume the new system owns all pharmacy data.

---

# 20. INTEGRATION TRANSACTION SAFETY

Use states:

```text
Draft
Validated
Confirmed
Queued
Posting
Acknowledged
Rejected
NeedsReconciliation
Reversed
```

Implement idempotency keys, transaction queues, acknowledgments, duplicate detection, and controlled reconciliation.

A timeout does not prove that a Cerebro transaction failed.

Do not blindly retry uncertain financial transactions.

---

# 21. CEREBRO API CONTINGENCY

If no approved Cerebro API exists:

- Import approved medicine CSV files.
- Import approved stock snapshots.
- Support manual order references.
- Export authorized transactions.
- Provide manual posting confirmation.
- Generate reconciliation reports.

The application must remain usable for standalone inventory management.

Do not use undocumented Cerebro database writes or automated UI billing.

---

# 22. DATABASE DESIGN

Create migrations for:

```text
Medicines
MedicineBarcodes
PackagingUnits
Suppliers
PurchaseOrders
GoodsReceipts
InventoryLocations
InventoryBatches
InventoryBalances
StockMovements
StockReservations
DispenseOrders
DispenseTransactions
DispenseTransactionLines
Returns
StockAdjustments
RecallNotices
RecallAffectedBatches
CerebroMedicineMappings
CerebroOrderMappings
IntegrationOutbox
IntegrationInbox
IntegrationAcknowledgments
ReconciliationRuns
ReconciliationDiscrepancies
Workstations
ScannerConfigurations
ScannerEvents
Users
Roles
Permissions
UserSessions
AuditEvents
SystemSettings
```

Use foreign keys, unique constraints, appropriate indexes, decimal quantities, UTC timestamps, and date-only expiration fields.

Support zero-to-many scanner configurations per workstation.

Apply database migrations safely.

---

# 23. MULTI-WORKSTATION CONCURRENCY

Support simultaneous operations across all registered workstations.

Implement:

- Atomic inventory updates.
- Concurrency control.
- Duplicate transaction prevention.
- Stock reservations.
- Automatic stock refresh.
- Network reconnection.
- Workstation identification.
- Server-side validation.

Test simultaneous dispensing of the final available stock unit.

Only one valid stock deduction may succeed.

Do not allow uncontrolled offline dispensing.

---

# 24. USER ACCOUNTS AND PERMISSIONS

Roles:

- System Administrator.
- Pharmacy Administrator.
- Chief Pharmacist.
- Pharmacist.
- Pharmacy Assistant.
- Inventory Clerk.
- Auditor.
- Read-Only User.

Use individual accounts.

Apply least privilege.

Require appropriate pharmacist authorization for dispensing and clinical exceptions.

Technical administrators must not automatically receive clinical approval rights.

---

# 25. WINDOWS DESKTOP USER INTERFACE

Create a professional WPF application with:

1. Dashboard.
2. Scan Medicine.
3. Medicine Master.
4. Barcode Registration.
5. Inventory Receiving.
6. Inventory Management.
7. Medication Dispensing.
8. Stock Transfers.
9. Returns and Adjustments.
10. Expiration Monitoring.
11. Medicine Recalls.
12. Cerebro Synchronization.
13. Reports.
14. Scanner Settings.
15. Workstation Management.
16. User Administration.
17. Audit Logs.
18. System Settings.
19. About Application.

Display real database values rather than hardcoded metrics.

Provide clear warnings, readable typography, and accessible controls.

Use the official hospital logo in the interface.

---

# 26. BARCODE LABEL PRINTING

Support optional thermal barcode printers.

Generate:

- Medicine labels.
- Internal stock labels.
- Batch labels.
- Shelf/bin labels.
- Repackaged medicine labels.

Use an internal barcode namespace distinct from manufacturer GTINs.

Preserve source-lot traceability.

Follow approved pharmacy labeling procedures.

---

# 27. MEDICINE RECALL MANAGEMENT

Implement:

1. Recall registration.
2. Affected medicine identification.
3. Batch identification.
4. Stock location search.
5. Quarantine.
6. Dispensing block.
7. Traceability reports.
8. Return/disposal recording.
9. Authorized recall closure.

Preserve historical records.

---

# 28. SECURITY AND PHILIPPINE DATA PRIVACY

Design for compliance with hospital policies and Republic Act No. 10173.

Implement:

- HTTPS/TLS.
- Secure authentication.
- Password hashing.
- Role-based authorization.
- Session expiration.
- Account lockout.
- API rate limiting.
- Input validation.
- Secret protection.
- Encrypted backups.
- Firewall restrictions.
- Tamper-evident audit logs.
- Secure updates.
- Least-privilege service accounts.

Minimize patient data collection and storage.

Do not log passwords, tokens, or unnecessary patient information.

---

# 29. AUDIT LOGGING

Record:

- User.
- Role.
- Workstation.
- Timestamp.
- Action.
- Record identifier.
- Transaction identifier.
- Relevant previous and new values.
- Approval.
- Override reason.
- Integration status.

Protect audit logs from ordinary-user modification.

---

# 30. REPORTS AND ANALYTICS

Implement:

1. Daily receiving.
2. Daily dispensing.
3. Current inventory.
4. Low-stock inventory.
5. Near-expiry medicines.
6. Expired medicines.
7. Batch traceability.
8. Stock movements.
9. Stock valuation.
10. Inventory adjustments.
11. Medicine recalls.
12. Cerebro synchronization.
13. Failed transactions.
14. Reconciliation.
15. User activity.

Support PDF, CSV, and Excel-compatible exports.

Apply report access permissions.

Use the hospital logo on appropriate report headers.

---

# 31. BACKUP AND DISASTER RECOVERY

Implement:

- Scheduled PostgreSQL backups.
- Encrypted off-server copies.
- Backup integrity verification.
- Restore testing.
- Disaster recovery documentation.
- Server restart procedures.
- Approved manual downtime workflow.

Coordinate recovery objectives with hospital IT.

RAID is not a substitute for backup.

---

# 32. DEDICATED PHYSICAL SERVER

Target one dedicated physical server.

Recommended starting specifications:

- Business-class server hardware.
- Server-grade CPU.
- 32 GB ECC RAM.
- Two enterprise SSDs in RAID 1.
- Gigabit Ethernet or better.
- UPS.
- Windows Server 2025 Standard or Ubuntu Server LTS.
- Separate backup storage.

Install:

1. Server operating system.
2. Security updates.
3. PostgreSQL.
4. Pharmacy API.
5. Cerebro integration worker.
6. HTTPS certificate.
7. Managed server services.
8. Firewall rules.
9. Backup services.
10. Monitoring and logging.

Use a server hostname rather than a hardcoded IP address.

Restrict PostgreSQL from direct workstation access.

Use Windows Services on Windows Server and systemd services on Linux.

---

# 33. WORKSTATION INSTALLATION

Use one universal client installer:

`DDHI-SmartPharmacy-Setup-x64.exe`

Install on the initial four Windows 11 pharmacy computers.

Each workstation must:

1. Install the client.
2. Configure the central API.
3. Register its workstation identity.
4. Test connectivity.
5. Configure a scanner if available.
6. Operate normally without a scanner.
7. Authenticate individual staff.
8. Access permitted pharmacy functions.

Additional workstations must be installable later using the same installer.

No separate database installation is required on client computers.

---

# 34. NETWORK CONFIGURATION

Use hospital IT-approved network settings.

Example only:

```text
Pharmacy Server:
DDHI-PHARM-SRV
192.168.10.20

Pharmacy PC 1:
192.168.10.101

Pharmacy PC 2:
192.168.10.102

Pharmacy PC 3:
192.168.10.103

Pharmacy PC 4:
192.168.10.104
```

Example API endpoint:

`https://ddhi-pharm-srv.ddhi.internal:8443`

Configure internal DNS and trusted TLS certificates.

Allow authorized clients to access the API.

Restrict PostgreSQL to local connections.

Allow Cerebro integration only through approved interfaces.

Deny public inbound access.

---

# 35. AUTOMATED AND HARDWARE TESTING

Implement tests for:

**Barcode:** EAN, GS1, malformed input, duplicates, missing fields, and check digits.

**Scanner configurations:** Zero, one, two, three, four, and more scanner-equipped workstations.

**Inventory:** Receiving, FEFO, expiration, recalls, adjustments, transfers, reversals, and negative-stock prevention.

**Dispensing:** Wrong medicine, wrong strength, insufficient stock, manual verification exceptions, and concurrent dispensing.

**Cerebro:** Mock synchronization, timeouts, duplicate posting, rejection, delayed acknowledgment, and reconciliation.

**Security:** Authentication, authorization, session expiration, audit integrity, and sensitive-data protection.

**Deployment:** Multiple simultaneous clients, scannerless workstations, network interruption, installer operation, and service restart.

**Branding:** Logo loading, correct aspect ratio, application icon, About page, installer branding, and report headers.

Physical scanner tests must be performed on actual hardware before being marked passed.

---

# 36. PHASED DEVELOPMENT PLAN

**Phase 0:** Codebase audit and architecture.

**Phase 1:** Hospital branding, desktop, API, database, authentication, medicine catalog.

**Phase 2:** Optional scanner integration, GS1 parsing, and barcode registration.

**Phase 3:** Receiving, inventory, batch tracking, FEFO, expiration, and reports.

**Phase 4:** Dispensing verification and approved manual exception controls.

**Phase 5:** Cerebro mock integration, approved adapters, and reconciliation.

**Phase 6:** Security, audit logging, backups, recovery, and testing.

**Phase 7:** Windows EXE publishing and installer creation.

**Phase 8:** Dedicated server installation and initial four-workstation pilot.

**Phase 9:** Hospital acceptance and authorized production rollout.

---

# 37. MANDATORY WINDOWS 11 EXE

Compile the desktop application into:

`DDHI-SmartPharmacy.exe`

The application must run on Windows 11 x64 without Visual Studio or Codex installed.

Use self-contained .NET publishing where appropriate.

Example commands:

```powershell
dotnet restore DDHI.SmartPharmacy.sln

dotnet build DDHI.SmartPharmacy.sln `
  --configuration Release

dotnet test DDHI.SmartPharmacy.sln `
  --configuration Release

dotnet publish `
  src/DDHI.Pharmacy.Desktop/DDHI.Pharmacy.Desktop.csproj `
  --configuration Release `
  --runtime win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true `
  -p:EnableWindowsTargeting=true `
  -p:DebugType=None `
  --output publish/windows-x64
```

Configure the assembly name to produce the required EXE filename.

Use actual project paths discovered during the audit.

If single-file publishing is incompatible with required dependencies, use a reliable self-contained multi-file package and include it in the installer.

---

# 38. WINDOWS INSTALLERS

Build:

**Client installer:**

`DDHI-SmartPharmacy-Setup-x64.exe`

Must:

- Install the desktop application.
- Create Start Menu shortcut.
- Optionally create desktop shortcut.
- Display approved hospital branding.
- Configure the API endpoint.
- Test server connectivity.
- Support optional scanner configuration.
- Support scannerless operation.
- Support upgrades and uninstall.
- Preserve approved settings.

**Server installer:**

`DDHI-SmartPharmacy-Server-Setup-x64.exe`

For Windows Server deployment, must:

- Deploy Pharmacy API.
- Deploy integration worker.
- Configure Windows Services.
- Support PostgreSQL initialization.
- Configure HTTPS.
- Configure logging.
- Provide migration scripts.
- Support backup configuration.
- Avoid overwriting production data.
- Support safe upgrades.

For Ubuntu Server, provide an equivalent Linux installation package or scripted deployment instead of attempting to run a Windows EXE installer.

---

# 39. FINAL RELEASE PACKAGE

Generate:

```text
DDHI-SmartPharmacy-Release/
|
+-- Client/
|   +-- DDHI-SmartPharmacy.exe
|
+-- Installers/
|   +-- DDHI-SmartPharmacy-Setup-x64.exe
|   +-- DDHI-SmartPharmacy-Server-Setup-x64.exe
|
+-- Server/
|   +-- Pharmacy API
|   +-- Integration Worker
|   +-- Linux Deployment Scripts
|
+-- Database/
|   +-- PostgreSQL Migrations
|
+-- Branding/
|   +-- DIDH LOGO.jpg
|   +-- hospital-logo.png
|   +-- hospital-icon.ico
|
+-- Documentation/
|   +-- Installation Guide.pdf
|   +-- Administrator Guide.pdf
|   +-- Pharmacy User Manual.pdf
|   +-- Barcode Scanner Setup.pdf
|   +-- Cerebro Integration Guide.pdf
|   +-- Workstation Deployment Guide.pdf
|   +-- Backup Recovery Guide.pdf
|
+-- Tests/
|   +-- Test Results
|
+-- Checksums/
|   +-- SHA256SUMS.txt
```

Create:

`DDHI-SmartPharmacy-Windows11-Release.zip`

Provide source code separately or in a clearly identified source archive.

---

# 40. FINAL ACCEPTANCE CRITERIA

Before declaring completion:

1. The EXE launches successfully on Windows 11.
2. Client and server installers work on their supported operating systems.
3. Four initial workstations connect to the same server.
4. Scanner-equipped workstations scan actual barcodes.
5. Scannerless workstations operate normally.
6. Additional scanners can be configured without rebuilding.
7. Additional workstations can be registered without redesign.
8. Medicine lookup works.
9. Receiving updates centralized inventory.
10. Expired and recalled medicines are blocked.
11. Dispensing requires appropriate authorization.
12. Manual entry cannot masquerade as scan verification.
13. Concurrent transactions cannot create negative stock.
14. Audit logging works.
15. Backup and restore are tested.
16. Cerebro integration is authorized before live posting.
17. Existing Cerebro operations remain unaffected.
18. Hospital logo is correctly displayed throughout the application.
19. Developer is displayed as Vicente C. Cavalida Jr. MD.
20. Release artifacts are verified.

Hospital IT and pharmacy leadership must approve production deployment.

---

# 41. FINAL EXECUTION INSTRUCTION TO CODEX

**BEGIN DEVELOPMENT NOW.**

Perform actual implementation rather than merely describing the solution.

1. Audit the existing codebase.
2. Preserve working functionality.
3. Import and integrate the supplied hospital logo.
4. Set the developer name to Vicente C. Cavalida Jr. MD.
5. Create or update the .NET solution.
6. Implement the Windows desktop application.
7. Implement central API and PostgreSQL.
8. Implement optional, dynamically configurable barcode scanners.
9. Support any practical combination of scanner-equipped and scannerless workstations.
10. Implement medicine master and barcode registration.
11. Implement receiving, inventory, FEFO, expiration, and recalls.
12. Implement dispensing verification and approved manual exception controls.
13. Implement Cerebro integration adapters and mock synchronization.
14. Implement security, audit logs, backups, and reports.
15. Run automated tests.
16. Fix compilation and test failures.
17. Publish `DDHI-SmartPharmacy.exe`.
18. Build `DDHI-SmartPharmacy-Setup-x64.exe`.
19. Build the appropriate server installation package.
20. Generate deployment documentation.
21. Create `DDHI-SmartPharmacy-Windows11-Release.zip`.
22. Verify generated files and checksums.
23. Report implemented, tested, unverified, and blocked items.
24. Provide exact output file locations.

If Windows build tools, physical hardware, vendor documentation, or required permissions are unavailable, clearly report the blocker and complete all feasible work without falsely claiming success.

**DO NOT STOP AT SOURCE CODE GENERATION. CONTINUE THROUGH COMPILATION, TESTING, INSTALLER CREATION, AND RELEASE PACKAGING.**

**FINAL OBJECTIVE: DELIVER A WORKING WINDOWS 11 DDHI SMART PHARMACY BARCODE MANAGER WITH THE OFFICIAL DIGOS DOCTORS HOSPITAL LOGO, DEVELOPER CREDITED AS VICENTE C. CAVALIDA JR. MD, A DEDICATED CENTRAL SERVER, MULTIPLE PHARMACY WORKSTATIONS, OPTIONAL BARCODE SCANNERS, CENTRALIZED INVENTORY, MEDICATION SAFETY VERIFICATION, AND SAFE CEREBRO HIS INTEGRATION.**