# Installation Guide

## Install on the destination server
Run DDHI-SmartPharmacy-Server-Setup-x64.exe as Administrator on the dedicated Windows Server with Desktop Experience. It includes PostgreSQL 17.12, API, worker and desktop Server Control. Install the Microsoft Visual C++ x64 runtime directly from Microsoft if missing. No shared production passwords are supplied.

## Create the database in the window
In Initial setup choose a persistent data folder. Choose Local evaluation / localhost for the requested evaluation. Click Set up database and HTTPS. The wizard creates ddhi_pharmacy and separate ddhi_migration and ddhi_runtime accounts, applies migrations and grants, and protects generated passwords with Windows DPAPI. There is no need to copy SQL commands.

## Configure Windows services
Keep the Windows services option checked and run as Administrator. The wizard registers DDHIPostgreSQL, DDHIPharmacyApi and DDHIPharmacyWorker under separate virtual accounts, applies restricted ACLs and machine-protects service secrets. The registry contains configuration paths, not database passwords. Elevated destination-server validation is required.

## Administrator and start
In Administrator and certificate enter the first administrator email and strong password. Click Create first administrator, then Start server and Check health. Bootstrap refuses an existing user database. Create individual staff and a Chief Pharmacist separately. The six-step guide is shown in the desktop window.

## HTTPS and connect
The default evaluation endpoint is https://localhost:8443 and applies only to the same computer. A localhost PFX is generated for pinned control-window health checks. Windows trust is not installed automatically; an authorized user may explicitly trust the exact local certificate. Hospital LAN mode requires an approved DNS hostname and a matching hospital-issued PFX trusted by all workstations. Do not bypass warnings. See SERVER_DESKTOP_SETUP.md.

## Existing localhost server: update to LAN
Install Server Control 0.3.0 over the existing installation during approved downtime using the same Windows setup account. Open the original prepared instance; use Open existing server profile for a custom data folder. In LAN connection enter the approved full hostname, trusted matching PFX, password and IT approval reference, then Apply LAN connection and start server. The database, users, stock and credentials remain in place. Connection settings are backed up; failed activation attempts rollback. Restore previous connection handles interrupted changes. IT must first configure DNS and certificate trust, then permit the API port from approved clients. Copy client address and test /health on every workstation. Do not recreate the administrator or initialize a replacement database. See the complete Server and Client Connection Guide.