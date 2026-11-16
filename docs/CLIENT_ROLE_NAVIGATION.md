# Client 0.3.0 — signed-in roles and navigation

The sidebar shows the roles returned for the signed-in staff email directly below the hospital name. More than one assigned role is shown together, and the available modules are the union of those roles. Sign out and sign back in after a server-side role change. Sign-out clears the badge, menus, displayed records, lookup cache and pending operation keys from the client session.

All recognized roles can use Dashboard, Scan Medicine, Medicine Master (view), Inventory Management (view), Medicine Recalls (view), Scanner Settings for their workstation, and About Application. Read-only users and Pharmacy Assistants see these shared lookup modules, with stock/catalog mutation actions hidden.

Additional modules follow the existing API permissions:

| Role | Additional modules |
|---|---|
| Inventory Clerk | Inventory Receiving, Stock Transfers |
| Pharmacist | Inventory Receiving, Stock Transfers, Medication Dispensing, Returns and Adjustments (returns), Expiration Monitoring, Cerebro Synchronization, Reports, Audit Logs |
| Pharmacy Administrator | Barcode Registration, Inventory Receiving, Stock Transfers, Returns and Adjustments (counts/reversals), Expiration Monitoring, Cerebro Synchronization, Reports, Audit Logs, System Settings (view) |
| Chief Pharmacist | Pharmacy Administrator modules plus Medication Dispensing, returns and Chief Pharmacist policy approval |
| Auditor | Expiration Monitoring, Cerebro Synchronization (view/test), Reports, Audit Logs |
| System Administrator | Workstation Management, User Administration |

Action buttons are filtered as well: for example, Pharmacists cannot edit the medicine catalog, Auditors cannot acknowledge Cerebro postings, and only the Chief Pharmacist can change the manual-dispensing policy. Technical administrators do not inherit clinical or report roles. Unknown/unassigned roles receive no navigation modules.

The sidebar is a usability layer. The API still authenticates and authorizes every operation. This update does not grant new permissions, enable live Cerebro integration, or bypass pharmacist confirmation. Install the client update on each workstation; the existing server API remains compatible.
