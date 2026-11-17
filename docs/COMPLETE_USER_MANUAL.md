# DDHI Smart Pharmacy - Complete User Manual

Digos Doctors Hospital, Inc.

Client 0.5.0 | Server Control 0.8.0 | API 0.3.0

Developer: Vicente C. Cavalida Jr. MD

17 November 2026 | Hospital evaluation edition

This manual explains the released client screens, the server connection workflow, generic groups and brands, authorized brand substitution, and mixed-source discharge returns. It describes software operation, not prescribing instructions or permission to substitute a medicine. Follow hospital policy, the authorized order and the responsible pharmacist's review. Live Cerebro posting and automatic patient billing/refunds remain disabled. Separate hospital-computer, physical scanner and clean-server acceptance remain pending.

## 1. Start here: connect, sign in and navigate

### What is on each computer

The dedicated server holds the central PostgreSQL database, pharmacy API and maintenance worker. Each client computer runs the Windows pharmacy application and connects over HTTPS. Stock is shared across clients. The client has no independent inventory database and cannot dispense offline. The server must be running before clients can work.

### First connection

1. On the server, open Server Control as Administrator using the original Windows setup account. Open the existing prepared server profile; do not initialize over an existing database.
2. On Server status, check the endpoint and health. For a prepared localhost instance, open Connect computers, select the hospital network adapter and choose Enable LAN. Confirm the action.
3. On each client, open DDHI Smart Pharmacy and choose Find server if discovery has not started. Select the intended server and compare its connection code with the code displayed on the server computer.
4. Confirm the matching code once. Never approve a different or unexpected identity. If discovery is blocked, save a .ddhiconnect file on the server, transfer it to the client, and choose Open connection file. The file provides address/identity information; the server must still be reachable.
5. Enter your individual staff email and password and sign in. The sidebar shows your assigned roles and the pages those roles can view.

On different computers, localhost means the computer you are currently using. Do not point a client to localhost when its server is on another computer. Simple LAN mode manages its own paired HTTPS identity. Advanced TLS is an optional IT-managed alternative.

### Daily use

Use Refresh to reload a page from the central server. Select a row before using an action referring to a selected record. Forms may scroll; review all fields before choosing Review and confirm. A final confirmation sends the request to the server, which validates permissions, quantities and safety rules. Read the result/status message after every transaction.

Use User manual at the top of the client, or Open complete user manual in About Application, to open this PDF. Sign out when leaving the workstation. Never share an account or save a password in a connection file.

If a request times out after confirmation, its outcome can be uncertain. Refresh the relevant records and check the transaction reference before submitting again. Do not create duplicate receiving, dispensing or return entries merely because a response was delayed.

## 2. Roles and common screen rules

### Which sidebar items appear

The sidebar depends on the roles assigned to the signed-in email. Multiple roles combine their permitted pages. A visible page may be read-only for that account. The server enforces access even if a button or client screen is modified locally.

- All recognized staff roles can view Dashboard, Scan Medicine, Medicine Master, Inventory Management, Medicine Recalls, Scanner Settings and About Application.
- Chief Pharmacist and Pharmacy Administrator maintain the medicine catalog and barcode mappings. Read-only staff may browse generic groups and their brands but cannot add or change catalog records.
- Chief Pharmacist, Pharmacy Administrator, Pharmacist and Inventory Clerk can receive stock and transfer it.
- Chief Pharmacist and Pharmacist perform dispensing and patient return/intake workflows. A technical System Administrator role alone does not authorize those actions.
- Chief Pharmacist, Pharmacy Administrator, Pharmacist and Auditor can view reports, audit logs, expiration monitoring and Cerebro synchronization records.
- System Administrator can access Workstation Management and User Administration. Staff creation does not automatically give the creator dispensing authority.
- System Settings is visible to catalog roles, but the manual dispensing policy action requires Chief Pharmacist authority.

### Numbers and confirmations

Stock quantities are normally base units, such as tablets, capsules or bottles. Receiving asks for packages and a conversion factor; the resulting stock is packages multiplied by base units per package. Dispensing, transfers, counts and patient returns use base units. A generic group is not a pooled stock balance.

An approval reference is a document/order/policy reference that staff must verify outside the app where applicable. Entering a reference does not cause the app to issue an approval or verify an external document. Many changes also require a reason so another reviewer can understand what happened.

Screens and exports can have result limits. CSV and PDF exports are snapshots of the records currently loaded; they are not complete database backups. Handle patient references, staff details and exported records under hospital privacy procedures.

## 3. One generic medicine with several brands

### The catalog model

One generic group can contain multiple brand products. The grouping definition is the generic name, strength, dosage form, route and base unit. Generic name, form and route ignore letter case; repeated spaces are normalized. Strength and base-unit notation must also match letter case because unit symbols can be case-sensitive. Different definitions stay in separate groups. The app does not equate synonyms or convert dose units automatically.

For example, a fictional training group named Training Generic, 5 mg, Tablet, Oral, tablet may contain Training Brand A and Training Brand B. These are examples only, not treatment recommendations. Each brand product has its own product ID, manufacturer, importer/distributor information, package barcodes, stock batches, locations and reorder level.

An existing blank brand name is treated as an unbranded label in the group's brand count. Several products bearing the same brand label can have different manufacturers, so product count and brand count can differ. Existing records are grouped for browsing; their IDs, stock and history are not merged.

### Add and use several brands

1. In Medicine Master, add the first medicine/brand with the correct generic definition, or find an existing product with that definition.
2. Choose Browse generics. Select the appropriate group and choose View brands to inspect its existing products.
3. Choose Add brand to selected generic. The form displays the generic definition being reused. Enter the new brand, manufacturer, optional importer/distributor, reorder level and catalog approval reason.
4. Review and confirm. The new brand is a separate active catalog product with no automatically copied barcode or stock.
5. In Barcode Registration, register the exact package barcode and conversion against the new brand product.
6. In Inventory Receiving, select that brand product and receive its own lot, expiration, supplier, location and quantity.

Use All brand products to return to the ordinary product list. Refresh reloads the normal Medicine Master view. Product-only actions, such as changing active status or importer/distributor, require an individual brand row rather than a generic summary row.

### Brand substitution when the ordered brand is unavailable

Grouping alone does not perform a substitution. A pharmacist can explicitly authorize a different brand during Verify and dispense, after checking the order restrictions, formulation/release characteristics and the hospital's authorization process. The recorded generic definition must match; the app does not prove therapeutic equivalence or support dose conversion.

Select the actual alternate brand's stock batch. If scanning, scan that actual package, not the unavailable brand's barcode. Tick the substitution verification/authorization checkbox and provide its authorization reference and reason, for example original brand unavailable. Ordinary patient verification, manual verification policy, stock, expiry, recall and reservation checks still apply.

The order retains the originally requested product. The dispensing transaction records both requested and dispensed product IDs/brands, the pharmacist, reference and reason. Only the selected actual stock is deducted. If unused items later return, link them to the actual dispensing line so the correct brand and batch are quarantined.

## 4. Mixed-source medicines on patient discharge

### Sort before entering quantities

Relatives may present unused medicines from both the hospital and an outside pharmacy. Separate items physically and establish their source before making a stock return. Matching generic name, brand or barcode identifies an item; it does not prove where it was bought. If individual units cannot be reliably separated by source, treat them as Uncertain/mixed.

| Source assessment | App workflow | Hospital inventory effect | Billing effect |
| --- | --- | --- | --- |
| Exact units verified as hospital-issued and linked to original dispensing | Find discharge medicines, then Hospital discharge return | Added to the original product/batch's quarantine balance only | No automatic credit/refund |
| Purchased outside the hospital pharmacy | Outside / uncertain intake; select Outside pharmacy | No stock increase | No hospital refund credit |
| Origin unknown or hospital/outside units inseparably mixed | Outside / uncertain intake; select Uncertain/mixed | No stock increase | No hospital refund credit |

### Verified hospital portion

1. In Returns and Adjustments, choose Find discharge medicines. Enter the exact patient/encounter reference from the hospital order.
2. Check the patient/encounter, order reference, actual dispensed brand, lot, expiration and returnable quantity. Select the original dispensing line.
3. Choose Hospital discharge return. Enter the unused base units physically returned and a unique return-line reference.
4. Enter the discharge/ward-return document reference, package condition, hospital issue/receipt evidence reference and quality-review notes.
5. Confirm that the items are unused and that these exact units were issued by the hospital. If source cannot be verified, stop this stock-return workflow and use uncertain-source intake instead.
6. Review and confirm. The server checks cumulative returns against the quantity originally dispensed. The accepted quantity goes to quarantine, never directly to Available stock.

Several medicine lines can share the same discharge document reference, but each returned line needs its own unique return reference. Previously returned quantities reduce the remaining returnable quantity. The original order's dispensed count is not erased or automatically reopened.

### Outside or uncertain portion

Choose Outside / uncertain intake. Record the encounter, unique intake-line reference, source assessment, item description, quantity/unit, condition, optional lot/expiry/barcode and receipt information. Record the custody decision and notes, and acknowledge that this entry creates no hospital stock or refund credit.

Custody choices are Held separately pending review, Returned to patient/family, or Disposal referred for authorization. These describe the documented handling decision under hospital policy. Recording a disposal referral does not perform or approve disposal. If items are held, keep them separate from hospital stock. No action in this workflow converts an outside/uncertain intake into saleable stock.

### Worked example

A hospital dispensing line shows 10 tablets and 2 have already been returned, leaving 8 returnable. The family presents 5 verifiable hospital tablets, 4 outside-purchased tablets and 3 units whose source cannot be separated. Record 5 tablets against the hospital line into quarantine. Record the other two quantities as separate outside and uncertain intake lines. Only the verified hospital portion changes pharmacy inventory. Any refund or charge adjustment follows a separate authorized billing process.

## 5. Dashboard

**Access:** All recognized staff roles. **Purpose:** Review a current summary of central pharmacy activity before starting work.

The cards show medicine product count, stock-unit total, near-expiry batches, expired batches, open recalls and registered workstations. Medicine count means individual products, including separate brands; it is not a count of generic groups. Stock-unit total is a broad total and does not mean every unit is available for dispensing.

1. Choose Dashboard after signing in and use Refresh to reload current values.
2. Investigate unusual stock totals in Inventory Management. Review near-expiry/expired stock in Expiration Monitoring and recall flags in Medicine Recalls.
3. Use the detailed record and correct brand/batch to make an authorized transaction. Dashboard cards do not receive, reserve or dispense stock.

The integration message states that live Cerebro posting is disabled. A normal dashboard does not establish that an external billing system was updated. Sign out from the top of the screen when finished.

## 6. Scan Medicine

**Access:** All recognized staff roles. **Purpose:** Identify an approved registered product/package and inspect its recorded stock.

1. Click the barcode box, scan a package using a keyboard-style scanner, or type the code. Press Enter or choose Look up barcode. Scan / enter barcode returns focus to that box.
2. Alternatively, choose Decode barcode image and select a clear image, or Read COM barcode with the configured port and baud rate. COM diagnostic lists detected serial ports; it does not verify that a valid scanner is attached.
3. Read the result: generic/brand, strength, dosage form, route, manufacturer, imported by, distributed by, active status and registered package conversion.
4. Distinguish Batch encoded on package and Expiration encoded on package from the separate recorded stock table. Not encoded means the barcode did not supply that field. Not recorded means the catalog lacks that information.
5. Verify the physical package against the result. Copy product code copies the identifier; it does not register or change anything.

An EAN/GTIN normally identifies a product/package, not necessarily its lot, expiry, serial number or company names. Those details come from encoded GS1 elements or the hospital catalog/stock database as labeled. Unknown or conflicting approved mappings select no medicine. Ask authorized catalog staff to correct them.

Expired, recalled and inactive warnings remain relevant even when a barcode matches. A scan alone never receives or dispenses stock and never proves physical provenance. The same product barcode can occur on an outside-purchased package.

## 7. Medicine Master

**View access:** All recognized roles. **Catalog changes:** Chief Pharmacist or Pharmacy Administrator.

Browse generics displays one row per recorded generic definition with brand/unbranded-label count, separate-product count and active-product count. View brands shows the products in the selected group. All brand products returns to the normal product list. Select a row before View brands or Add brand to selected generic.

Add medicine creates a first product or a separate generic definition. Enter generic name, brand, strength, dosage form, route, manufacturer, optional Imported by and Distributed by, category, base unit and nonnegative reorder level. Use consistent verified names/units; spelling changes and unit synonyms are not automatically reconciled.

Add brand to selected generic reuses the selected definition and asks for brand-specific details. It does not copy stock, barcodes or patient orders. A duplicate brand/manufacturer under that definition is refused by this action; inspect the existing product first.

Edit importer / distributor changes only those optional company fields for a selected individual product. Clear a value to remove it, and record a reason. These are catalog-level details from the package label; the delivery supplier is separate.

Set active status changes whether the selected product can be used in new clinical/stock workflows. Inactivation preserves stock and history; it is not deletion or disposal. Do not use a generic summary row to inactivate a particular brand.

Import approved CSV accepts the original ten-column samples/medicines.csv or the twelve-column samples/medicines-with-trade.csv with final ImportedBy/DistributedBy columns. Headers must match exactly. Enter the approval reference, review the rows and confirm. Each row is a separate product. Review for duplicates and correct generic/brand definitions before import.

## 8. Barcode Registration

**Access:** Chief Pharmacist or Pharmacy Administrator. **Purpose:** Approve a unique package-to-product mapping.

1. Select Register barcode. Choose the exact generic, brand, manufacturer, strength and dosage form from the product list.
2. Enter or scan the package barcode. Use Auto for recognized EAN/GTIN, GS1/AIM or internal formats. Select Code128 explicitly for a non-GS1 Code128 identifier.
3. Enter the packaging description and positive base units per package. For example, a box containing 100 tablets maps to 100 tablet base units; verify the actual package before approving.
4. Review and confirm. Scan the package in Scan Medicine and check the product and conversion.

One barcode cannot map to different brands/products. Equivalent leading-zero GTIN representations are checked; a different nonzero packaging indicator remains a distinct package identity. A conflict must be reviewed rather than bypassed by guessing a medicine.

Register each brand's actual package barcode separately. Do not assign Brand A's barcode to Brand B because both share a generic name. Catalog approval does not prove that a particular returned package originated in the hospital.

## 9. Inventory Receiving

**Access:** Chief Pharmacist, Pharmacy Administrator, Pharmacist or Inventory Clerk. **Purpose:** Record a verified delivery into central stock.

1. Use Add supplier if the supplier is not already listed. A supplier is the business delivering the goods, not necessarily the manufacturer, importer or distributor printed on the label.
2. Choose Receive stock. Select the exact brand product, supplier and receiving location.
3. Enter a unique delivery-line reference, physical batch/lot, approved expiration, packages received, base units per package and cost per base unit.
4. Review the calculated meaning of units before confirming. Ten packages at 20 tablets each add 200 tablets. A unit cost is per tablet in this example, not per box.
5. Confirm, then refresh Inventory Management and verify product, brand, lot, location, status and quantity.

Receive different brands and different lots as separate lines. Reuse a delivery document reference only with a distinct line suffix where needed; duplicate supplier/reference receiving is blocked. Routine receiving rejects expired stock and affected recalled batches. Follow the separately authorized quarantine process for exceptional receipts.

Receiving creates stock movements and receipt records. If it was wrong, use an authorized correction/reversal workflow; do not hide the error by silently editing a balance or entering an unrelated negative delivery.

## 10. Inventory Management

**View access:** All recognized roles. **Changes:** Vary by action.

Each row is a particular product, batch, location and stock status. Look at brand as well as generic name. Quantity is in base units. Recall and expiry checks can prevent dispensing even when the status text still reads Available.

- FEFO by medicine lists eligible stock for the selected individual product in earliest-expiry order. It does not pool brands or authorize substitution.
- Print stock label prints an internal balance identifier and its product/batch information. The label aids hospital traceability; it is not a manufacturer authenticity label or proof of where an outside package was purchased.
- Add location creates a named stock location and requires catalog authority.
- Quarantine / release stock moves a specified quantity between allowed statuses with reference/approval details. Only Chief Pharmacist authority can release held stock to Available, and expired/recalled stock remains blocked.
- Record disposition records an approved disposal/supplier-return stock deduction. Physical disposal/return and its authorization occur under the referenced hospital procedure.
- Reconcile ledger compares balances with their movement totals. Review discrepancies; the action does not silently repair counts.

Export CSV / Excel saves a CSV snapshot of the loaded rows; Print / PDF uses the Windows print workflow. Files are not database backups. Available quantities can change as other clients transact, so refresh before committing a decision.

Patient returns are first quarantined. An outside/uncertain patient intake never appears here as new stock. A stock count must not be used to disguise an outside-purchased patient item as hospital inventory.

## 11. Medication Dispensing

**Access:** Chief Pharmacist or Pharmacist. **Purpose:** Record a pharmacist-verified issue against an authorized order.

### Record and fulfill an order

1. Choose Record authorized order. Enter its authorized reference and minimal patient/encounter reference. Select the intended individual product and ordered base-unit quantity.
2. Review the product's generic, brand, strength and form. Recording a reference does not create a medical prescription or replace the authorized source order.
3. Choose Verify and dispense. Select the order, actual stock batch and base-unit quantity. Choose ManualSelection, ManualBarcode or HidUnverified as appropriate.
4. For barcode modes, scan the actual package being issued. For ManualSelection, leave barcode blank. Verify patient identity and enter the approved manual verification reason/reference.
5. If issuing a different brand, follow the explicit substitution steps below. Otherwise leave substitution approval unchecked and its optional fields blank.
6. Review and confirm. Verify the transaction reference and remaining order quantity. Refresh stock and dispensing reports if the response is delayed or unclear.

All current dispensing modes require the Chief Pharmacist's documented hospital manual-dispensing policy to be enabled. A keyboard-style scanner or decoded image is not authenticated physical-device proof. The server blocks an inactive product, excessive order quantity, insufficient/unavailable/expired/recalled stock and stock reserved for another order.

### Authorized different-brand issue

Check that the authorized order and hospital process permit the change, including formulation/release characteristics. Select the alternate brand's own stock and scan that package if using a barcode. Tick the checkbox confirming your review and authorization. Enter the substitution authorization reference and reason, such as original brand unavailable.

The server requires matching recorded generic name, strength, dosage form, route and base unit. It does not convert strengths or certify interchangeability from the names alone. The app retains the original request and records the actual product, both brands, approval reference, reason and pharmacist. Only actual stock is deducted. Inspect Reports > dispensing for the record.

Reserve stock creates a 15-minute reservation for the order's exact product, subject to available unreserved quantity. This reservation action does not authorize an alternate brand. Approved alternate-brand dispensing still checks other orders' reservations. Reservations belonging to the fulfilled order are released as appropriate; the worker expires stale reservations.

## 12. Stock Transfers

**Access:** Chief Pharmacist, Pharmacy Administrator, Pharmacist or Inventory Clerk.

Choose Transfer stock. Select the source stock balance, destination location, base-unit quantity, reference and reason. Verify generic, brand, lot and source status before confirming. The destination must differ from the source location.

A transfer deducts the source and adds the same product/batch/status at the destination. It does not change brands, create a new purchase or increase total hospital quantity. Insufficient unreserved source stock is blocked. Refresh Inventory Management to inspect both sides.

To correct a completed transfer, authorized catalog staff use the linked stock-movement reversal workflow in Returns and Adjustments. Do not create unrelated counts to conceal one side of a transfer.

## 13. Returns and Adjustments

**Clinical patient workflows:** Chief Pharmacist or Pharmacist. **Counts and stock reversals:** Chief Pharmacist or Pharmacy Administrator. The page displays different actions for each role.

### Discharge and mixed-source actions

Find discharge medicines searches by the exact original patient/encounter reference. Read the actual dispensed product, lot, quantity dispensed, quantity already returned and remaining returnable quantity. Select a line before Hospital discharge return.

Hospital discharge return records unused, verified hospital-issued units against that original line. Complete the discharge reference, unique return-line reference, condition, source evidence, verification checkboxes and reason. The returned quantity is limited by previous returns. It goes to quarantine at the original product/batch/location. It does not automatically reverse an order, credit a bill or authorize resale.

Outside / uncertain intake records physically separate outside purchases or inseparable/unknown-origin items. Specify the source assessment, item description, quantity/unit, condition, optional package/document details, custody decision and notes. Acknowledge no hospital stock or refund credit. This is documentation/custody handling, not hospital receiving. Outside intake history lists these entries with no stock/billing effect.

Patient return history lists hospital return records, discharge/source evidence and original encounter links. Historical entries from earlier builds may lack new evidence fields; a blank historical field must not be interpreted as verified origin.

### Other actions

Return to quarantine is the ordinary hospital-issued return workflow. Select the original dispense line and quantity, supply a unique return reference/reason, and verify hospital source evidence. Outside or uncertain items use separate intake even if their barcode matches a catalog product.

Count / adjust records a physically counted balance with reference and reason. It cannot make stock negative or consume reserved quantities. It is not a way to receive external patient purchases or fix a bill.

View movements displays movement rows. To Reverse a stock movement, select an eligible receiving, count or transfer movement and enter a correction reason. The server creates linked reversal entries and blocks a second reversal. Dispensing corrections use quarantined returns rather than deleting a dispensing movement.

## 14. Expiration Monitoring

**Access:** Chief Pharmacist, Pharmacy Administrator, Pharmacist or Auditor.

The default view shows upcoming expirations within 180 days. Use Within 180 days, Within 90 days, Within 60 days or Within 30 days to narrow the loaded upcoming-expiry records. Expired loads records already past their recorded expiration date. These are stock-batch dates, not an estimate from a normal product-only barcode.

Check product, brand, batch, location, status and quantity. Coordinate quarantine, approved disposition or authorized stock movement through the appropriate inventory workflow. Export or print a snapshot for review if permitted.

This screen does not extend an expiration date, dispose of medicine or approve a release. Physical segregation and hospital quality procedures remain necessary. A returned expired batch is not made eligible by moving it to a different location or by recording a discharge return.

## 15. Medicine Recalls

**View access:** All recognized roles. **Recall changes:** Chief Pharmacist or Pharmacy Administrator.

Register and block batch records a recall notice reference and reason against the actual affected batch. Verify generic, brand, lot and manufacturer details before confirmation. The server's recall check blocks affected stock from routine receiving, dispensing and release, while preserving balances and traceability.

Closing a recall requires selecting the relevant affected recall record, entering an authorized closure reason and having zero affected stock remaining after approved disposition. Close selected recall does not erase historical affected-lot restrictions.

A recall flag can block a balance even if its status label is not literally Recalled. Check both the flag and status. Do not relabel a recalled brand as another brand in the same generic group. An alternate-brand issue requires its own eligible stock and explicit pharmacist authorization for substitution.

## 16. Cerebro Synchronization

**Access:** Report roles. **Manual acknowledgment:** Catalog roles.

The page lists integration outbox records associated with dispensing. Test mock adapter exercises the isolated mock behavior; it does not post a patient charge or stock update to live Cerebro. Live vendor integration remains disabled in this evaluation build.

Manual acknowledgment records a verified external acknowledgment reference against a selected record where that action is authorized. Enter a real reviewed reference under the hospital's integration procedure. The button does not itself send a transaction to Cerebro or establish that an external posting happened.

Review pending, rejected or NeedsReconciliation states with authorized IT and pharmacy staff. Use the integration/reconciliation reports to investigate. Do not repeatedly dispense or return medicine to force a billing sync. Hospital returns and outside intakes in this build do not issue automatic patient refunds or billing credits.

## 17. Reports

**Access:** Chief Pharmacist, Pharmacy Administrator, Pharmacist or Auditor. Choose a report button, then refresh/export/print as needed.

| Report | What to inspect |
| --- | --- |
| inventory | Current product/batch/location/status balances |
| low-stock | Individual products whose eligible quantity is at or below reorder level |
| near-expiry / expired | Recorded batch expiration and remaining stock |
| valuation | Quantity multiplied by recorded batch unit cost; not a patient bill |
| receiving | Receiving movements and their references |
| dispensing | Encounter/order, actual dispensing line and any recorded brand substitution |
| movements | Stock increases/decreases and references |
| traceability | Movement-based product/batch tracing |
| adjustments | Count-adjustment movements |
| returns | Hospital return-to-quarantine movements; outside intake is separate |
| recalls | Affected lots and recall status |
| integration / failed-transactions | Outbox status; not proof of live posting |
| reconciliation | Recorded ledger/balance discrepancies |
| user-activity | Audited actions, role, workstation and timestamp |

Export CSV / Excel creates an Excel-compatible CSV file of the loaded rows. Print / PDF opens the Windows print workflow, where a PDF printer may be selected. Verify filters and loaded rows before sharing. Some reports load a limited number of records, so they are not full database backups.

For hospital source evidence and discharge condition, use Patient return history. For outside/uncertain items and custody decisions, use Outside intake history. Those items must not be added to valuation as hospital stock through an unrelated count.

## 18. Scanner Settings

**Access:** All recognized roles for viewing/configuring their workstation; catalog authority permits broader configuration under server rules.

The app supports working with no scanner. A keyboard-style/HID scanner generally types into the active barcode input and sends the configured Enter suffix. Keep focus in the correct field before scanning.

Add scanner profile records workstation, name, enabled state, type, optional COM port and suffix. Edit selected profile changes that configuration. Use HID, COM or Bluetooth HID according to the actual device. Vendor SDK is an extension boundary, not a completed trusted-device integration.

COM diagnostic lists operating-system serial ports. Read COM barcode is on Scan Medicine and asks for port and baud rate, waiting up to 15 seconds. Confirm the device's settings separately. A profile or COM port entry does not prove a scanner is connected, authenticated or clinically validated.

Image decoding needs a clear supported barcode image. Neither HID input nor an image proves package origin. Test a sample barcode against its readable result before operational use and complete hospital physical-device acceptance.

## 19. Workstation Management

**Access:** System Administrator.

Register workstation creates or identifies a workstation by a unique name. Use a recognizable hospital workstation name so audits can identify where work occurred. Client sign-in also registers its workstation identity automatically as part of normal operation.

Review names for confusing duplicates or unsuitable shared labels. A workstation record is not a staff account and does not grant clinical permissions. User roles belong to the signed-in staff email. Several workstations can connect to the same central server, but each staff member must use their own authorized account.

Registering a workstation does not open firewall ports or install hardware drivers. Resolve network and scanner configuration through their respective workflows.

## 20. User Administration

**Access:** System Administrator. **Purpose:** Manage individual application accounts, not PostgreSQL/Windows accounts.

Create individual account asks for email, an initial password and one role. Password rules require at least 12 characters with uppercase, lowercase, number and symbol. Assign only the duties approved for that staff member. Clinical authority must be deliberately assigned; System Administrator alone does not grant dispensing rights.

Select an account and choose Edit profile / roles to edit email, optional full name, optional phone and one or more roles. Clear optional fields to remove them. Enter a reason, review and save. The affected staff member must sign in again; existing sessions are rejected on subsequent server requests. A stale editor or duplicate email is refused.

Lock / unlock account records an authorized reason and suspends or restores access. It is often preferable to deletion when access is temporarily suspended. Delete selected account requires the selected email to be typed and a reason. It removes the login/profile but keeps clinical transactions and audit history.

You cannot change your own roles or delete/lock your signed-in account. The last active System Administrator cannot be removed, locked or demoted. Another authorized administrator must handle protected self-role changes.

The current editor does not provide password reset/change or a forced first-login password change. Use the hospital's authorized support process. Repeated failed sign-ins can trigger lockout; do not ask staff to send passwords through chat or email. Application staff passwords are separate from IT PostgreSQL credentials.

## 21. Audit Logs

**Access:** Chief Pharmacist, Pharmacy Administrator, Pharmacist or Auditor.

Audit records show who acted, under which role, from which workstation, and when. Transaction references and immutable movement history help investigate receiving, dispensing, substitution, patient returns and catalog/account changes.

Verify hash chain checks the audit chain for consistency and displays its result. A valid chain supports integrity of recorded audit data; it does not prove that physical counts, package identity, an approval document or a clinical decision were correct. A failed verification requires authorized investigation rather than manual log editing.

Export or print loaded logs only for an authorized purpose. Large request payloads may be summarized with hashes rather than copied in full. Deleting a staff login does not remove their historical actions. Do not use SQL tools to edit audit or stock-movement rows to make reports look correct.

## 22. System Settings

**View access:** Chief Pharmacist or Pharmacy Administrator. **Manual dispensing policy action:** Chief Pharmacist only.

Manual dispensing policy controls the ManualDispensingApproved setting. It starts absent/disabled unless an authorized Chief Pharmacist records a true setting and written hospital approval reference. Choose false to disable it under an authorized decision.

This setting permits the supported manual verification workflow; it does not grant roles, certify a scanner, approve every brand substitution or override patient/stock safety checks. Each dispensing still needs pharmacist authority, patient verification and a documented verification reason. A different-brand issue additionally needs its own explicit authorization/reference and matching recorded definition.

The build rejects unsupported settings. Do not use this page as an undocumented database editor or to enable live Cerebro posting.

## 23. About Application

**Access:** All recognized staff roles.

The page identifies DDHI Smart Pharmacy Barcode Manager, Digos Doctors Hospital, Inc., the developer, client version/build date and evaluation license information. Use these details when requesting support, especially when a client and server might be running different versions.

Open complete user manual opens this PDF from the installed client Documentation folder. The User manual button at the top of the main window is another way to reach it. The login screen's Connection guide is specifically for connecting computers.

When reporting an issue, include client and Server Control versions, which sidebar/action was used, the visible error and a non-sensitive transaction reference. Do not provide passwords, private keys, full patient charts or unnecessary personal information.

## 24. Server Control companion guide

### Existing server upgrade

Back up the database and protected configuration/keys first. Install the updated server package on the dedicated server, open Server Control as Administrator using the original setup account and select the existing profile if needed. On Server status choose Apply database updates, review the maintenance confirmation and let it restart after success. This release adds substitution/return-intake recording fields and a separate outside-intake table. Do not repeat Initial setup or first-administrator creation for an existing database.

Update client computers with the matching client installer after the server is ready. If health says database update required, apply the database updates; a running database process alone does not establish schema readiness. On failure, review logs, keep staff signed out and seek authorized support. Do not initialize over existing data.

### Server tabs

- Server status: check API, database and HTTPS; Start server, Stop server, Apply database updates, open logs, select an existing profile and copy the client address.
- Initial setup: prepare a new empty instance only. It creates protected database credentials and applies schema. Existing data must not be reinitialized.
- Connect computers: Enable LAN, compare connection code, save a public fallback connection file and restore a prior connection when needed.
- Advanced TLS: optional hospital IT hostname/certificate configuration. Use a trusted matching certificate when this mode is selected.
- Connection guide: guided steps for separate server/client computers.
- Accounts: create the first application administrator only when no users exist. Repeated bootstrap is refused to protect the existing database.
- Advanced guide: deployment/configuration instructions for authorized IT.
- Activity: operational status/errors. Review protected logs for diagnosis; do not share secrets.
- Database Access: check connection details, sign in with an application role to view permitted records, export a loaded snapshot, and open IT administration tools with a separate PostgreSQL login.

### Accounts and network

Application staff login, Windows login and PostgreSQL login are different accounts. The server uses separate ddhi_migration and ddhi_runtime database roles. Database Access can create a separate ddhi_it_ login under the original elevated setup account. Read-only inventory views are the default. Schema-owner maintenance access is a separate acknowledged privilege; no PostgreSQL superuser is created by that wizard. Choose and retain the IT password securely; app-managed passwords are not displayed.

Clients use HTTPS, normally TCP 8443; local-subnet discovery uses UDP 8444. PostgreSQL stays on the server's loopback address, normally port 55433 in the bundled setup. Hospital IT controls network/firewall scope. Keep outside/uncertain held medicines physically separate even though their intake documentation is stored on the central server.

## 25. Troubleshooting and recovery

| Symptom | What to check |
| --- | --- |
| Connection refused / timeout | Server is running, API health, correct LAN endpoint, same approved network, firewall and selected adapter. A connection file cannot start a stopped server. |
| No connection code after Enable LAN | Run Server Control as Administrator; inspect Activity and health. Do not repeat database initialization. |
| database update required | Back up and use Apply database updates in updated Server Control on the actual server. |
| Bootstrap refused: users already exist | Sign in with an existing administrator. Use client User Administration for additional staff. |
| Session ended / role changed | Sign in again. Account edits/locks/deletion can invalidate existing sessions. |
| Missing sidebar/button | Check the assigned role and whether a generic summary rather than individual product is selected. |
| Barcode not registered / ambiguous | Catalog staff verify the exact brand/package mapping. Do not guess or map several brands to one code. |
| Different-brand issue blocked | Confirm explicit substitution authorization and identical recorded generic, strength, form, route and base unit; check actual package barcode and stock safety. |
| Return blocked by source evidence | Verify hospital issue evidence for those exact units or use outside/uncertain intake with no stock credit. |
| Return exceeds dispensed / duplicate reference | Review the original line, previous returns and unique line reference. Do not increase counts to bypass the limit. |
| Outside item missing from inventory | Expected: an outside/uncertain intake is documentation only and creates no hospital stock. |
| Cannot release quarantine | Chief Pharmacist review is required; expired/recalled stock remains blocked. Follow the physical quality procedure. |
| No billing refund | Returns/intakes do not automatically credit Cerebro or a patient bill in this build. Use the separate authorized billing process. |

If a transaction's outcome is uncertain after a network failure, refresh and reconcile before retrying. For serious discrepancies, record the reference, preserve logs and escalate to authorized staff. Do not delete balances, ledger entries or the database to clear an error.

## 26. Daily checklists and worked workflow

### Start of shift

Confirm server readiness, sign in individually, check your role badge and refresh Dashboard. Review recalls, expiry alerts and unresolved reconciliation issues relevant to your duties. Confirm the physical stock, scanner focus and correct product before a transaction.

### Generic with two brands

Create the first verified product, browse its generic group, add the second brand, register separate package barcodes and receive separate lots. When fulfilling an order, choose the intended brand. If it is unavailable, a pharmacist reviews authorization/restrictions, chooses eligible alternate stock, completes substitution verification/reference/reason, and confirms. Review the dispensing report; the original request and actual issue remain distinguishable.

### Discharge with mixed purchases

Receive the presented items into a controlled review process. Separate verified hospital units from outside or uncertain units. Locate hospital dispensing by exact encounter, record only verified unused hospital quantities against original lines into quarantine, and document external/uncertain quantities separately with custody decisions. Reconcile physical quantities and references. Refer any billing adjustment and quality/disposition decision to the authorized process; neither is automatically completed by intake.

### End of shift

Refresh open work, investigate uncertain transaction results and hand over unresolved references. Secure exported files, labels and physically held items. Verify that authorized server backups and monitoring are assigned to responsible staff. Sign out of the client and do not shut down the dedicated server merely because a workstation is closing.

## 27. Glossary

- **Active product:** A catalog product enabled for relevant new stock/clinical actions. Inactivation preserves its history and stock.
- **Adjustment:** A documented stock-count correction with a reason; not a substitute for receiving or patient return provenance.
- **Admission/encounter reference:** The minimal hospital reference used to link an order and dispensing to a patient encounter. The app does not contain a full patient chart.
- **API:** The central server application through which clients read records and submit transactions.
- **Approval reference:** A recorded pointer to a reviewed hospital authorization, order or policy. The app does not issue that external approval.
- **Audit chain:** Linked hashes used to check consistency of recorded audit events.
- **Audit log:** Records of actions, actors, roles, workstations, times and references retained for review.
- **Available:** A stock status. Expiry, recall, reservations and other rules can still prevent use of a balance.
- **Barcode:** Encoded identifier/data read from a package or label. Identity alone is not purchase-source evidence.
- **Barcode mapping:** An approved association between a package code, a specific product and its unit conversion.
- **Base unit:** The smallest inventory counting unit chosen for a product, such as tablet or bottle.
- **Batch/lot:** The recorded manufacturer's/stock lot identifier used with product and expiration for traceability.
- **Billing credit/refund:** A financial adjustment handled separately in this build; a return/intake does not automatically create it.
- **Brand:** The verified trade label for a product. Several brands can share a recorded generic definition.
- **Brand substitution:** A pharmacist-authorized issue of a different matching recorded brand product, with reference, reason and retained original/actual identities.
- **Catalog:** The hospital's registered medicine/product definitions and approved mappings.
- **Cerebro:** The external hospital system integration boundary. Live posting is disabled in this evaluation build.
- **Chief Pharmacist:** Clinical role with additional policy and held-stock-release authority under the app's rules.
- **Client:** The Windows workstation application connected to the central server.
- **Code128:** A barcode family; select explicit non-GS1 Code128 registration where appropriate.
- **COM port:** An operating-system serial port used by some scanners. Its presence does not prove a valid scanner is attached.
- **Connection code:** A human-readable representation used to compare the server's paired identity once.
- **Connection file (.ddhiconnect):** A public address/identity fallback for pairing. It is not a password file or server launcher.
- **Conversion factor:** Base units in one package, used when mapping packages or receiving deliveries.
- **CSV:** A text table format used for approved catalog imports and loaded-record exports.
- **Custody/disposition:** The documented handling of outside/uncertain items, such as separate holding or return to family; not stock credit.
- **Damaged:** A stock status or intake condition requiring the appropriate authorized handling process.
- **DataMatrix:** A two-dimensional barcode format that may carry GS1 data when correctly encoded.
- **Database:** The central persistent store for catalog, accounts, stock and history.
- **Database migration/update:** A controlled schema change applied by the server's migration account; not database reinitialization.
- **ddhi_it_ login:** A separately provisioned IT PostgreSQL login, distinct from staff sign-in.
- **ddhi_migration:** The protected database schema-owner role used for migrations.
- **ddhi_runtime:** The restricted database role used by the pharmacy server application.
- **Delivery-line reference:** A unique reference distinguishing a received product/lot line, including partial deliveries.
- **Discharge reference:** The reviewed discharge/ward-return document reference recorded with hospital discharge returns.
- **Discharge return:** Unused, verified hospital-issued units linked to an original dispensing line and returned to quarantine.
- **Dispense line:** The actual issued product/balance/quantity within a dispensing transaction; patient returns link to this record.
- **Dispensing:** A confirmed pharmacist issue against an authorized order after required verification and stock checks.
- **Distributed by:** The distributor recorded from the product's verified package/company information.
- **DNS/hostname:** A network name used by advanced IT-managed connection configuration.
- **EAN/UPC/GTIN:** Product/package identification systems; a normal product code need not encode lot or expiry.
- **Eligible stock:** Stock passing the current status, expiry, recall, quantity and reservation checks for an action.
- **Expiration:** The recorded date after which the server blocks relevant routine use; the app does not extend shelf life.
- **External/outside intake:** Documentation of patient-presented outside-purchased items without hospital stock or refund credit.
- **FEFO:** First Expiry, First Out; an ordering of eligible stock by expiration for a selected product.
- **Formulation/release characteristics:** Product properties that a pharmacist must review when considering substitution; names alone do not certify equivalence.
- **Generic definition/group:** Generic name plus recorded strength, dosage form, route and base unit used to browse multiple brands.
- **GS1 application identifier:** A prefix specifying an encoded element, such as product code, lot, expiration or serial.
- **Held stock:** Stock requiring authorized review before it can become Available.
- **HID:** Human Interface Device/keyboard-style scanner input. The build treats it as unverified provenance.
- **Hospital source evidence:** Verified issue/receipt and package evidence linking exact returned units to hospital supply; not merely a matching barcode.
- **HTTPS:** Encrypted client/server communication authenticated by the configured/paired server identity.
- **Idempotency:** Server protection against repeating the same request identifier; it does not remove the need to inspect uncertain results before resubmitting.
- **Imported by:** The importer recorded from verified package/company details; separate from the delivery supplier.
- **Inactive product:** A retained catalog entry blocked from relevant new actions; not a deleted transaction history.
- **Integration outbox:** Recorded transactions awaiting integration handling; not proof of external posting.
- **Internal stock label:** Hospital label identifying a specific balance for traceability, not manufacturer authenticity or outside-purchase provenance.
- **Inventory balance:** Quantity for one product batch, location and stock status.
- **LAN:** Local area network connecting hospital computers.
- **Ledger/stock movement:** Immutable records of inventory increases/decreases and their references.
- **Localhost:** The computer making the connection; it does not mean a separate server computer.
- **Lockout:** Temporary or administrator-controlled restriction on staff sign-in.
- **Lot traceability:** Following a batch through receiving, movements, issues and eligible returns.
- **Manual dispensing policy:** The Chief Pharmacist-approved setting required for supported manual verification modes.
- **Manufacturer:** The recorded maker of the product; separate from importer, distributor and delivery supplier.
- **Mixed/uncertain source:** Units whose hospital/outside origin cannot be reliably separated or established; no hospital stock credit is created.
- **Not encoded:** A field absent from the scanned barcode payload.
- **Not recorded:** A field missing from the hospital catalog/database.
- **Order:** A recorded authorized order reference, patient/encounter and requested individual product/quantity.
- **Outside pharmacy:** A supplier from which the patient/family obtained the presented item outside the hospital pharmacy.
- **Package condition:** Intake assessment such as sealed/intact, opened, damaged or unknown; not automatic approval for reuse.
- **Package count:** Number of packages received; multiply by conversion factor to obtain base-unit stock.
- **Patient verification:** Staff confirmation of patient identity under the authorized clinical process.
- **PFX certificate:** A certificate/private-key container used in configured HTTPS scenarios; protect it and its password.
- **Pharmacist:** Clinical application role authorized for dispensing and patient return/intake actions under server rules.
- **Physical provenance:** Evidence about an item's real source/handling; barcode text alone does not establish it.
- **PostgreSQL:** The server-side database system. Clients connect to HTTPS rather than directly to it.
- **Product:** A separate catalog item/ID for a particular generic/brand/manufacturer definition with its own stock and mappings.
- **Quarantine:** A non-available stock state for review; verified hospital returns first enter this state.
- **Recall:** A recorded affected-batch restriction preventing relevant routine use while preserving traceability.
- **Reconciliation:** Comparing recorded balances or integration records with their expected counterparts and investigating differences.
- **Reorder level:** Per-product threshold used in the low-stock report, expressed in base units.
- **Reservation:** A temporary hold against an exact order/product balance, normally expiring after 15 minutes.
- **Return reference:** A unique reference for one hospital return line, distinct from a shared discharge document reference.
- **Role:** An assigned set of application duties controlling visible pages and authorized actions.
- **Route:** Recorded administration route used as part of a generic definition; the app does not infer equivalent routes.
- **Runtime account:** Restricted database identity used by the application, not a staff account.
- **Serial number:** An individual package identifier when encoded; not always present in ordinary barcodes.
- **Server Control:** The desktop window for preparing/controlling the dedicated server and reviewing its status.
- **Session:** The signed-in client authorization state; account changes can require a new sign-in.
- **Simple LAN pairing:** Explicit one-time verification of the server's public-key identity on a trusted local network.
- **Snapshot export:** Only the loaded records at the time of export; not a backup of all data.
- **Stock status:** A classification such as Available, Quarantined, Damaged or PendingVerification.
- **Strength:** The recorded medicine strength used for grouping/matching; automatic strength/dose conversion is unsupported.
- **Supplier:** The business delivering a receiving line, which can differ from package company labels.
- **System Administrator:** Technical account-management role; it does not alone authorize clinical work.
- **Transaction reference:** A retained identifier used to review an action and resolve uncertain outcomes.
- **Unbranded:** A product without a trade-brand label, recorded explicitly or represented by an existing blank brand field.
- **Unit cost/valuation:** Recorded cost per base unit and quantity-based stock value; not an automatic selling price or patient charge.
- **Unused verification:** Pharmacist confirmation that presented hospital items match the original dispensing and unused quantity assessment.
- **Worker:** The background server process supporting maintenance such as reservation expiry and local discovery.
- **Workstation:** A registered client-computer identity used in audit records, distinct from the staff user.
