# Cerebro Integration Guide

## Authorization boundary
No approved Cerebro interface contract was supplied. Live synchronization is disabled. The mock adapter tests the local integration boundary only. REST and read-only database adapter classes reject use until vendor endpoints/schema, authentication, data ownership and written hospital authorization are supplied. Never infer undocumented endpoints or modify production tables.

## Ownership decisions
Cerebro retains patient, authorized medication-order and billing authority. The new application maintains its own standalone stock ledger. Hospital leadership must decide whether that ledger is operational or shadow inventory, which system deducts each movement and how opening balances and returns are reconciled. Prevent double stock deduction and duplicate billing before enabling any live posting.

## Approved CSV contingency
Import medicine CSV only with an approval reference and exact sample header. External IDs must be unique; similar names are never merged automatically. Authorized stock snapshots can be submitted to the snapshot comparison API; discrepancies are recorded without changing central balances. Export permitted transaction reports as CSV for approved manual posting. Manually acknowledge an outbox item only after verifying an external acknowledgment reference.

## Transaction safety
Inventory operations require UUID idempotency keys, retained with request fingerprints and actor identity. The same key and payload return the prior result. Different payloads with the same key are rejected. A timeout does not prove failure. Unknown vendor outcomes require NeedsReconciliation and must never trigger blind financial retry. The mock adapter does not acknowledge production transactions.

## Before go-live
Obtain vendor documentation and sandbox credentials, approve mapping and ownership, validate retry and delayed acknowledgment scenarios, test reconciliation and duplicate prevention, and document downtime handling. No production Cerebro testing or live writes were performed in this build.