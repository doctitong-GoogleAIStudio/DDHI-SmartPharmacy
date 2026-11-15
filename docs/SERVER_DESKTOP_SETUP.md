# Desktop server setup - destination server

Use the dedicated destination Windows Server with Desktop Experience. MSI is preparation/preview only. The installer includes PostgreSQL 17.12, API, worker and Server Control. It does not ship a clinical database or shared passwords.

1. Install the Microsoft Visual C++ x64 runtime directly from Microsoft if missing. Run DDHI-SmartPharmacy-Server-Setup-x64.exe as Administrator. Open DDHI Smart Pharmacy Server Control from the Start Menu or desktop shortcut.
2. In Initial setup choose a persistent data folder and ports. Choose Local evaluation for the currently requested localhost-only setup. Click Set up database and HTTPS. The wizard creates an independent loopback PostgreSQL cluster, ddhi_pharmacy, and separate ddhi_migration and ddhi_runtime accounts with unique random passwords. It applies migrations and runtime grants automatically. Credentials are protected with Windows DPAPI; do not send passwords in chat.
3. Keep Configure Windows services checked and run as Administrator. The wizard configures DDHIPostgreSQL, DDHIPharmacyApi and DDHIPharmacyWorker under separate virtual Windows accounts. Service secrets are machine-protected and restricted by NTFS ACLs. Registry environment settings contain only configuration paths, not passwords. PostgreSQL starts so administrator creation can complete.
4. In Administrator and certificate enter the first administrator email and a strong password, then click Create first administrator. This one-time bootstrap refuses an existing user database. Assign individual staff and clinical roles separately.
5. Click Start server, then Check health. Windows services support unattended operation. Managed local processes are an evaluation fallback; closing the control window does not stop started processes/services, but managed processes do not provide service reboot/logoff guarantees. Stop only during approved downtime.
6. Copy the displayed Server endpoint into the pharmacy client. The evaluation default is https://localhost:8443 and applies only to the same computer. The generated PFX matches localhost and is pinned for control-window health checks. Windows/browser/client trust is NOT installed automatically. An authorized operator may deliberately use Trust localhost certificate for this Windows account after checking its fingerprint. Do not bypass certificate warnings.

## Hospital LAN

For multiple workstations hospital IT must approve the DNS hostname and provide a matching PFX trusted on the server and workstations. Select Hospital LAN during initial setup and enter the approval reference, PFX path and password locally. The application cannot invent IT approval or issue a hospital CA identity. IT must configure DNS, firewall scope and certificate trust. Live Cerebro stays disabled.

## Upgrade an existing localhost instance to Server Control 0.3.0

Back up the database and protected recovery material, record the original data location, arrange downtime, stop the old server and close its manager. Install version 0.3.0 over the same installation path using the same Windows setup account; do not uninstall or delete data. The installer validates ownership and waits for owned services to stop. Open the prepared instance; use Open existing server profile for an original custom data folder if needed.

In LAN connection enter the approved full DNS hostname (no scheme/port), matching trusted PFX, its password locally and the IT approval reference. Click Apply LAN connection and start server. DNS must resolve only to this server's LAN interfaces. The manager preserves the database, users and credential store, backs up connection settings, validates the certificate, changes the HTTPS listener and verifies startup. Failed activation attempts automatic rollback; Restore previous connection provides persisted recovery for an interrupted change. The configuration snapshots are not database backups. No DNS, firewall or trust-store changes are made automatically. Test the copied base endpoint and /health from every workstation. See SERVER_CLIENT_CONNECTION_GUIDE.md for the complete procedure.

The Connection guide tab gives separate server/client instructions inside Server Control. Open complete connection guide (PDF) opens the bundled nine-page manual. The same PDF is available in the installed Documentation folder and release Documentation folder.

## Storage and recovery

Keep live data outside installation binaries and source/release archives. Choose an IT-approved persistent volume. Credentials are bound to Windows protection scopes; do not deploy by copying runtime directories between machines. Use approved encrypted PostgreSQL backups and a tested recovery procedure.

## Prerequisite

Microsoft Visual C++ x64 runtime: https://aka.ms/vs/17/release/vc_redist.x64.exe . Obtain it directly from Microsoft on the destination server. It is not redistributed inside this package.

## Validation limits

Actual elevated Windows Server service registration, ACL behavior, reboot recovery, local trust installation and hospital network/certificate acceptance require destination-server validation. No permanent pharmacy server is left enabled on MSI.
