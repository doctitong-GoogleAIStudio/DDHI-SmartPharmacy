# Clear barcode results and staff account management

This update requires client 0.4.0 and server installer / Server Control 0.7.0 (API 0.2.0). Back up the server database before upgrading. Install the server update on the server computer and the client update on each workstation. Open the updated Server Control as Administrator and click **Start server**. This also applies the narrowly scoped account-management permissions to the existing database. Do not repeat first-administrator setup. Existing data and credentials stay in place; no new database schema migration is needed for these features.

## Scan Medicine

Scan into the barcode box, or type the barcode and press Enter. Image decoding and COM scans use the same lookup. Results show the registered generic/brand name, strength, dosage form, manufacturer, package conversion and current stock by batch, expiration, location and status. Batch, expiration and serial fields explicitly say **Not encoded** when the scanned barcode does not carry them. Stock quantities are base units; they are not inferred package counts.

The hospital medicine catalog is the source of medicine details. A normal EAN/GTIN identifies a product/package; it does not necessarily contain its name, batch or expiration. Register the exact package barcode and conversion in Barcode Registration using an authorized catalog account. Choose Code128 explicitly for a non-GS1 Code128 label. Approved legacy GTINs with equivalent leading zero padding are recognized. A different nonzero packaging indicator remains a distinct package. Conflicting mappings are blocked rather than guessed. Unregistered codes show a clear warning and no invented medicine information. A GS1 lot/date narrows the stock results; an internal stock label resolves to its exact recorded balance.

Verify the physical medicine, strength, dosage form and package against the result. No scan alone receives, reserves or dispenses stock. Recalled, expired and inactive warnings remain visible; transaction permissions and safety checks still apply. Scanner configuration and image quality can affect decoding, and physical scanner acceptance remains pending.

## User Administration

Sign in to a client as **System Administrator**, then open **User Administration**. Select a staff row and click **Edit profile / roles**. Edit the email, optional full name and phone, and one or more roles. Clear optional name/phone fields to remove those details. Enter a reason and review the change. The server validates duplicate emails, permissions and stale edits. Changes end the affected account's current sessions; sign in again to load the new roles and sidebar.

Select **Delete selected account** to remove a login and its account profile. Type the selected email and provide a reason to confirm. This is an irreversible account deletion; use Lock instead when suspension is sufficient. Clinical transactions, stock records and audit history are retained. An administrator cannot delete their own signed-in account or change their own roles. The last active System Administrator cannot be deleted, locked or demoted. Password changes/reset are not part of this editor.

Only application identity tables receive the additional DELETE permission needed for profile/role editing and account deletion. Runtime DELETE access to clinical, stock and audit tables is not granted by this update.

The release remains for hospital evaluation. Live Cerebro posting is disabled and acceptance on separate hospital computers is still required.
