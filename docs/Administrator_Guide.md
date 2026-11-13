# Administrator Guide

## Accounts and separation of duties
The bootstrap account is a System Administrator, which can create users but cannot dispense medicines. Clinical roles are Chief Pharmacist and Pharmacist. Catalog approval and adjustments require Pharmacy Administrator or Chief Pharmacist. Inventory Clerk can receive and transfer. Auditors can inspect permitted reports. Individual accounts and strong passwords are mandatory; never share credentials.

## Sessions and security
Bearer access tickets expire after 20 minutes. Tickets are held only in desktop memory. Five failed login attempts trigger account lockout for 15 minutes. HTTP is accepted only in the Development environment for isolated loopback testing. Production requires TLS and encrypted data-protection keys. Keep PostgreSQL on loopback and restrict API ingress to hospital subnets. Production executable signing requires a hospital code-signing certificate.

## Manual verification policy
ManualDispensingApproved defaults to absent/disabled. Only the Chief Pharmacist can record a true policy setting, with a written hospital approval reference. Every manual dispensing also requires pharmacist authorization, confirmed patient identity and a documented reason. HID keyboard input and image decoding are unverified; this build never permits a client to assert trusted physical-device provenance.

## Monitoring and safe upgrades
Review near-expiry, low-stock, recalls, integration outbox and reconciliation daily. Verify the audit hash chain. Before upgrade stop DDHI services, verify an encrypted backup and preserve protected configuration and keys. Review migration SQL, apply as schema owner and reapply runtime grants. The installer does not replace database files or ProgramData settings. Installer service lifecycle on Windows Server requires a clean-server acceptance test.

## Boundaries
Vendor REST and read-only database adapters intentionally fail closed. The maintenance worker expires reservations; it does not post Cerebro billing. Quarantine release requires a Chief Pharmacist approval reference, and recall closure requires zero affected stock. Stock reversals preserve linked immutable movements and reject repeated reversal. Full purchase-order fulfillment and vendor-specific hardware attestation require additional approved workflows. Consult release status for the complete acceptance gap list.