# Pharmacy User Manual

## Start safely
Connect to the central HTTPS server and sign in using your individual account. Confirm the correct workstation and role. Scannerless login, lookup and inventory operations are supported. If the network fails, do not dispense offline or assume an uncertain transaction failed; review reports and reconcile before resubmitting.

## Catalog and receiving
Authorized catalog staff create medicine identity with explicit strength, dosage form and base unit. Barcode registration requires medicine, packaging conversion and confirmation; duplicate mappings are blocked. Receiving requires supplier, location, lot, explicit expiration, quantity, conversion and unit cost. Each delivery line needs a unique reference; separate partial deliveries and batches use separate references. Expired routine receiving is blocked.

## Inventory and dispensing
Use FEFO availability to choose the earliest eligible batch. Record an authorized order and minimal encounter reference. Confirm patient identity, exact medicine, strength, form, quantity, batch and expiration. The server checks remaining order quantity, available unreserved stock, recalls and the approved manual verification policy. Explicit pharmacist confirmation is required. A lookup scan never changes stock or bills a patient. Dispensing is not medication administration.

## Transfers, returns and counts
Transfers preserve source status and produce paired ledger entries. Returned dispensed stock enters quarantine and cannot be routinely dispensed. Returns cannot exceed the original dispensed line. Authorized stock counts create immutable adjustment movements with a reason. Recall notices block affected batches without erasing historical balances. Authorized disposition reduces stock with a reason. Recall closure requires zero remaining affected stock and does not remove historical lot blocks. Chief Pharmacist quality approval is required to release quarantined stock. Receiving, count adjustments and paired transfers can be reversed once, provided sufficient unreserved stock remains; dispensing corrections use quarantined returns.

## Reports and uncertainty
Reports support CSV compatible with Excel and branded printing through the Windows print dialog; choose Microsoft Print to PDF if available. Expiration views include 180, 90, 60 and 30 day filters. Month-only barcode expiry is rejected until an approved hospital policy is implemented. For a timeout inspect movements, orders and reconciliation before retrying. Obtain pharmacist and IT assistance through hospital procedures; no support contact has been invented.