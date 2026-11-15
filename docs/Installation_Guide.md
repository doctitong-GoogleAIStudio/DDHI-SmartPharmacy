# Installation Guide

## Install the server update
On the dedicated server, run DDHI-SmartPharmacy-Server-Setup-x64.exe as Administrator. For an existing instance, keep the same installation path, data folder and Windows setup account; do not uninstall or recreate the database. Keep an encrypted backup and use approved downtime. For a new empty server, click Set up server in Initial setup; database, protected credentials and Windows services are configured automatically.

## Enable the local network
Open Connect computers and click Enable LAN. The app selects a network adapter automatically, creates an encrypted server identity and configures narrow local-subnet firewall rules. Simple mode needs no DNS name, manual PFX certificate, certificate password or approval-ticket field. Existing users and stock remain in the same database. First-administrator creation is needed only when no application users exist.

## Install every client
Install DDHI-SmartPharmacy-Setup-x64.exe version 0.2.0 on every workstation. Older clients cannot use app-managed pairing. The new client searches the local network automatically. Check the connection code against the server screen, confirm it once, then sign in with the existing staff account.

## Automatic future connections
The client remembers the server public key and reconnects without repeating certificate setup. It can rediscover a changed server IP with the same identity on the local network. Staff still sign in; passwords are not saved. HTTPS stays encrypted and a different server key is rejected. Do not confirm a first-time pairing code that differs from the server screen.

## If discovery is blocked
On the server click Save connection file. Copy that small file to a client and click Open connection file, then check the code once. The file contains only the address and public server identity, not database or staff passwords or a private key. Guest Wi-Fi isolation, separate VLANs or managed network policy can still prevent traffic; a file does not bypass blocked networking.

## Guide and acceptance
Server Control and the client sign-in screen include Connection guide, and the client Start Menu includes the same PDF. The complete four-page guide covers the simple flow and troubleshooting. TCP 8443 and UDP 8444 are the defaults. Windows certificate trust is not changed; verify readiness in the paired app rather than bypassing browser warnings. Physical workstation, firewall, service/reboot and hospital acceptance remain pending. Live Cerebro is disabled.