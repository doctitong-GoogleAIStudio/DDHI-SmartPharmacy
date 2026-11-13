# Backup Recovery Guide

## Credentials and objectives
Hospital IT must approve recovery time, recovery point, retention, monitoring and off-server storage. RAID is not backup. Use a dedicated PostgreSQL backup role and a hospital-managed encryption passphrase of at least 16 characters. Keep the passphrase separately from encrypted backups and test recovery access.

## Encrypted backup
DDHI.Pharmacy.Tools backup streams pg_dump custom-format output directly into a chunked authenticated AES-256-GCM envelope. PBKDF2-SHA256 derives the encryption key with random salt and 600000 iterations. Every chunk and final terminator is authenticated; truncated or modified backups are rejected. No plaintext dump is written by this tool. A SHA-256 sidecar supports file transfer checks, and the encrypted file is authenticated before success is reported.

## Scheduling
deployment/Backup.ps1 accepts tool, PostgreSQL binary and approved off-server destination paths. Supply PGHOST, PGPORT, PGUSER, PGDATABASE, PGPASSWORD and DDHI_BACKUP_PASSWORD through a protected process secret-store launcher. Register-BackupTask.ps1 schedules that launcher at 02:00 by default; no production task or secret store is installed automatically. Alert on failed tasks and missing copies.

## Isolated restore
Create an empty isolated database with a name beginning ddhi_restore_. Set PGDATABASE to that name and run DDHI.Pharmacy.Tools restore-test with pg_restore and the encrypted backup. The tool authenticates the entire file before sending restore data. Never point restore tests at production. Compare table counts, ledger totals and audit-chain integrity, then rehearse application workflows.

## Disaster recovery and limitations
Stop normal clinical workflows according to the approved downtime procedure, preserve evidence and restore only under hospital authorization. Recover protected configuration, TLS material and data-protection keys with appropriate ACLs. Reconcile all transactions since the backup before resuming. Development backup/restore tests do not establish off-server storage, power-failure resilience, service recovery or clinical disaster-recovery acceptance.