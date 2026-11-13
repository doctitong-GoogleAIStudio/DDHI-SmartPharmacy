# Data ownership — pending hospital approval
Cerebro remains authoritative for patient identity, medication orders and billing. No billing writes or production stock synchronization are enabled.
This standalone application owns its own catalog mappings, batches, balance ledger, receiving, dispensing, returns, adjustments and audit. Before live operation, hospital IT must approve whether those balances are operational or a shadow ledger, opening balances, transfer boundaries and how Cerebro deductions are reconciled.
Approved CSV transactions require manual posting acknowledgment. Unknown outcomes become NeedsReconciliation, never automatic financial retries. No patient names are required; use minimal approved order/encounter references.
