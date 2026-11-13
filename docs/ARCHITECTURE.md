# Architecture
Windows WPF MVVM desktop → HTTPS ASP.NET Core API → EF Core / PostgreSQL. Clients never hold database credentials or an authoritative offline inventory.
Domain and barcode libraries contain safety rules; Application orchestrates transactions; Infrastructure provides Identity, persistence, audit and serialization; Contracts contains request DTOs; Integration isolates Cerebro; Worker performs maintenance.
Inventory writes acquire a PostgreSQL transaction-scoped advisory lock, then validate and commit balances, immutable ledger, idempotency key and chained audit together. This conservative global serialization favors correctness at the initial scale; load capacity must be measured before expansion.
Identity bearer sessions, lockout, role policies and API rate limits protect the API. Technical administrators do not inherit dispensing privileges. All clinical actions require confirmation. HID keyboard input is always classified as manual/unverified because keystrokes alone cannot establish physical provenance.
PostgreSQL is server-only. TLS, DNS, firewall scope, backups and hospital acceptance are deployment prerequisites.
