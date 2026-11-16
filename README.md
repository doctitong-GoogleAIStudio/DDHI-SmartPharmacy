# DDHI Smart Pharmacy Barcode Manager

Digos Doctors Hospital, Inc. | Developer: Vicente C. Cavalida Jr. MD

Server Control 0.4.0, client/worker 0.2.0 and API 0.1.0 form a hospital evaluation build. Source, tests, deployment scripts and verified release artifacts live permanently in this E: project. See docs/MASTER_SPECIFICATION.md for the requested target and release/Documentation/RELEASE_STATUS.md for implemented, tested, pending and blocked items. This release must not be represented as hospital-accepted or production-ready.

## Build

Use PowerShell from this project directory. Run scripts/Environment.ps1 to select the local .NET 10 SDK. scripts/Build.ps1 restores, builds, tests, publishes and packages. Set DDHI_TEST_CONNECTION to an isolated PostgreSQL database named ddhi_test* and keep that server alive during testing. Test credentials and production secrets must never be committed.

## Run and deploy

Install the central server on the dedicated computer, enable LAN, and install the updated client on each workstation. The client finds the server automatically; compare its connection code with Server Control once, then sign in. Read release/Documentation/DDHI-Server-and-Client-Connection-Guide.pdf and SERVER_DESKTOP_SETUP.md. The client has no local inventory database and cannot dispense offline. No default production account or password is shipped. Create the first administrator in desktop Server Control only when no application users exist, then create clinical roles separately.

Cerebro live posting is disabled. Vendor-approved adapters require an actual interface contract and written authorization. Scanner keystrokes and images remain unverified; physical-device attestation has not been implemented. Manual dispensing is blocked until a Chief Pharmacist records hospital policy approval.

Installer /EXTRACTONLY=1 is for package payload smoke tests only: it suppresses uninstaller registration and shortcuts. It is not an installation acceptance test. Normal installation supports Start Menu shortcuts, optional desktop shortcut and uninstall. Run Server Control as Administrator to provision PostgreSQL and configure Windows services on the destination server.

Keep the original hospital logo unchanged in assets/branding/DIDH LOGO.jpg.

Install the server update over the existing installation using the same Windows setup account and open the original prepared instance. Enable LAN uses an app-managed HTTPS identity and scoped firewall rules. Connection settings are backed up; failed activation attempts rollback. Database, existing administrator, stock and protected credentials are retained. IT-managed DNS and trusted PFX remain available under Advanced TLS. See docs/SERVER_CLIENT_CONNECTION_GUIDE.md and docs/SERVER_DESKTOP_SETUP.md. PostgreSQL 17.12 is bundled. Elevated service upgrade/reboot behavior and physical client acceptance remain pending on the destination server.

The server's Connection guide tab provides separate server/client steps and opens the bundled PDF. Client installer 0.2.0 adds the same offline PDF, a guide button on the sign-in screen and a Connection guide Start Menu shortcut. Install this client update on every workstation to use app-managed pairing.

For a fresh source checkout, run scripts/Bootstrap-Tools.ps1. Stage-PostgreSQL.ps1 stages the pinned server PostgreSQL distribution. The test helpers require their separate development PostgreSQL fixture. The packaging gate also requires current published-server and manual desktop-launch evidence. Test-Local.ps1 and Test-PublishedServer.ps1 are helpers for this project-local development fixture; destination-server installation uses the desktop setup guide.


## Simple automatic connection update

Server Control 0.4.0 and client 0.2.0 add local-network discovery and one-time code-confirmed pairing. On the server click Enable LAN; clients find it automatically, confirm the matching code once and then sign in. Future launches remember the public key and can rediscover the same identity at a new IP. A small connection file is the fallback. HTTPS remains encrypted; no staff password, database password or private key is shared in that file. Simple mode needs no DNS name, manual PFX import or approval-ticket field. Existing accounts, stock and database credentials are retained. Install the new client on every workstation. See the four-page Connection guide. Same-network connectivity and first-time identity confirmation are still required; physical firewall/service/reboot and hospital acceptance remain pending.

Client 0.2.1 fixes sidebar readability. Normal menu labels use light text; hovered, selected and keyboard-focused items use dark text on pale green. Close the client and install the updated client installer on each workstation. The existing server 0.4.0 and paired connection remain compatible.

## Server Control 0.5.0 and client 0.3.0

Database Access follows Activity in Server Control: connection/health details, role-authorized read-only record viewing and CSV/JSON snapshot export, plus optional IT SQL Shell/pgAdmin launch with a separate database login. Client 0.3.0 shows the signed-in role and email below the hospital name and filters both navigation modules and action buttons using the existing API role policies. Sign-out/account changes clear role menus and record caches. See docs/DATABASE_ACCESS.md and docs/CLIENT_ROLE_NAVIGATION.md. Existing pairing and database records are retained; API/worker versions are unchanged.
