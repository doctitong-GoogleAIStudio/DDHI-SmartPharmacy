# Imported by and distributed by

Client 0.4.1 and Server Control 0.7.1 / API 0.2.1 add optional importer and distributor details to **Medicine Master**. These identify the companies printed on the medicine package. They are separate from Manufacturer and the delivery Supplier selected in Inventory Receiving. Enter the verified package label details; a normal barcode does not supply these company names automatically.

**New medicine:** Medicine Master > Add medicine > Imported by / Distributed by.

**Existing medicine:** select its row, then choose Edit importer / distributor. Enter the company names and a reason, review and confirm. Clear a field to remove its optional value. Existing catalog permissions apply; read-only staff cannot change these fields. Changes are audited.

Both fields appear in Medicine Master, its CSV export and the readable Scan Medicine result. Blank catalog values show Not recorded in scan results. The original approved ten-column import CSV remains supported. For bulk entry, use samples/medicines-with-trade.csv; its final columns are ImportedBy and DistributedBy. Missing values remain blank. No existing importer/distributor values are guessed from the receiving supplier.

## Update an existing server

1. Back up the database and protected server configuration. Arrange a brief maintenance period and sign staff out.
2. Install the new server package on the dedicated server. Do not repeat Initial setup or first-administrator creation.
3. Open Server Control as Administrator using the Windows account that originally configured it. Select the existing profile if needed.
4. On Server status, choose **Apply database updates**. Confirm after backing up. This stops only the selected instance, starts its local database, applies the schema migration using its protected migration account, and restarts the API/worker after success.
5. Check that the server reports ready, then install client 0.4.1 on workstations and sign in again.

The migration adds two text columns with empty defaults to existing medicine rows; existing medicines, accounts and stock are retained. If an update fails, review the protected setup log and keep staff signed out until resolved. Do not initialize a new database over existing data. Fresh setup applies the schema normally. The original account/credential protections and runtime restrictions remain in place.

These fields are catalog information, not batch-specific history. If a package with the same barcode changes importer/distributor, have catalog staff verify and update its entry. Receiving supplier, lot and expiration continue to be recorded on receipts. The build remains for hospital evaluation; physical scanner and separate-server acceptance are pending.
