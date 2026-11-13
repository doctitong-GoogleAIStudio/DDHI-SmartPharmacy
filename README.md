# DDHI Smart Pharmacy Barcode Manager

Digos Doctors Hospital, Inc. | Developer: Vicente C. Cavalida Jr. MD

Version 0.1.0 is a hospital evaluation build. Source, tests, deployment scripts and verified release artifacts live permanently in this E: project. See docs/MASTER_SPECIFICATION.md for the requested target and release/Documentation/RELEASE_STATUS.md for implemented, tested, pending and blocked items. This release must not be represented as hospital-accepted or production-ready.

## Build

Use PowerShell from this project directory. Run scripts/Environment.ps1 to select the local .NET 10 SDK. scripts/Build.ps1 restores, builds, tests, publishes and packages. Set DDHI_TEST_CONNECTION to an isolated PostgreSQL database named ddhi_test* and keep that server alive during testing. Test credentials and production secrets must never be committed.

## Run and deploy

Install the central server and trusted HTTPS configuration before using the client. Read release/Documentation/Installation Guide.pdf and SERVER_DESKTOP_SETUP.md. The client has no local inventory database and cannot dispense offline. No default production account or password is shipped. Create the first administrator in desktop Server Control, then create clinical roles separately.

Cerebro live posting is disabled. Vendor-approved adapters require an actual interface contract and written authorization. Scanner keystrokes and images remain unverified; physical-device attestation has not been implemented. Manual dispensing is blocked until a Chief Pharmacist records hospital policy approval.

Installer /EXTRACTONLY=1 is for package payload smoke tests only: it suppresses uninstaller registration and shortcuts. It is not an installation acceptance test. Normal installation supports Start Menu shortcuts, optional desktop shortcut and uninstall. Run Server Control as Administrator to provision PostgreSQL and configure Windows services on the destination server.

Keep the original hospital logo unchanged in assets/branding/DIDH LOGO.jpg.

Server Control 0.2.0 adds a desktop setup window with the six-step installation guide. Run the server installer as Administrator on the destination Windows Server with Desktop Experience. It bundles PostgreSQL 17.12; Initial setup provisions ddhi_pharmacy and separate generated migration/runtime credentials. Use Local evaluation for localhost-only HTTPS until IT supplies an approved hospital hostname and trusted PFX. Follow SERVER_DESKTOP_SETUP.md for setup, first administrator, certificate trust and service operation. Elevated service installation and reboot acceptance remain pending on the destination server.

For a fresh source checkout, run scripts/Bootstrap-Tools.ps1. Stage-PostgreSQL.ps1 stages the pinned server PostgreSQL distribution. The test helpers require their separate development PostgreSQL fixture. The packaging gate also requires current published-server and manual desktop-launch evidence. Test-Local.ps1 and Test-PublishedServer.ps1 are helpers for this project-local development fixture; destination-server installation uses the desktop setup guide.
