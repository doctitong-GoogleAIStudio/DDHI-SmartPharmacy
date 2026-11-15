# DDHI Smart Pharmacy
## Simple automatic server and client connection

Digos Doctors Hospital, Inc. | 15 November 2026
Server Control 0.4.0 | Client 0.2.0 | Windows evaluation build

### The normal connection takes three steps

1. SERVER COMPUTER: open Server Control as Administrator, select Connect computers and click Enable LAN.
2. CLIENT COMPUTERS: install the updated client and open it. The app searches the local network automatically.
3. FIRST TIME ONLY: check that the client's connection code matches the server screen, confirm it, then sign in with your own staff account.

```text
One server: Enable LAN
       |
       +--- Client 1: finds server -> confirm once -> sign in
       +--- Client 2: finds server -> confirm once -> sign in
       +--- Client 3: finds server -> confirm once -> sign in
```

After pairing, the client remembers the server identity and connects automatically on later launches. Staff still sign in normally; the app does not save staff passwords. If the server's IP changes on the same adapter, discovery can locate the same saved identity at the new address.

### What you no longer have to prepare

Simple mode does not require a DNS hostname, manually exported PFX, certificate password, Windows certificate import or an approval-ticket field. The app creates its own encrypted HTTPS identity. It pairs each client to the exact server public key rather than requiring a hospital certificate authority.

The one-time code check matters: discovery finds servers but cannot prove which server belongs to you. Confirm the code only when it matches the server's screen. No sign-in information is sent during discovery or the pairing health check.

### Before starting

Use the same trusted hospital local network for the server and clients. Keep the server powered on. Avoid guest Wi-Fi that isolates computers. The server setup needs a Windows administrator; normal client use does not. This guide covers Windows Server with Desktop Experience and Windows pharmacy clients.

Your existing database, stock and application accounts are retained. Do not create a replacement database or repeat first-administrator setup just to enable connections. Live Cerebro remains disabled.

<!-- PAGE -->
## 1. Set up the server computer

### If this server already has your pharmacy database

1. Verify an encrypted database backup and record the existing Data location. Arrange downtime, stop the old server through Server Control and close its window.
2. Copy the new server installer to the server computer and run it as Administrator. Install over the same folder, using the same Windows setup account. Do not uninstall or delete the existing data folder.

```text
E:\ChatGPT Codex\DDHI-SmartPharmacy\release\Installers\DDHI-SmartPharmacy-Server-Setup-x64.exe
```

The E: path is the development computer's output. The dedicated server may use a different drive. Its usual installation folder is `C:\Program Files\DDHI-SmartPharmacy-Server`.

3. Open the updated Server Control as Administrator. Confirm the Data location is the original instance. If a custom instance is not selected, use Open existing server profile and select its original `profile.json`.
4. Select Connect computers. The network adapter is selected automatically. Change it only if the server has multiple networks and the selected address is not the pharmacy LAN.
5. Click Enable LAN and confirm the local-network/downtime prompt. The app creates its certificate, starts the server, and configures narrow TCP/UDP firewall rules for this adapter's local subnet. It does not disable the firewall or expose PostgreSQL.
6. Keep the displayed Connection code available while clients pair. Existing application users sign in with their existing accounts. They do not need new database credentials.

### If this is a genuinely new, empty installation

Run the server installer as Administrator. In Initial setup click Set up server, keeping automatic LAN and Windows services checked. Default database, credentials, services and encrypted network setup are handled by the app. Advanced setup is optional.

Open Accounts and create the first application administrator once. Use your own email and a strong password meeting the displayed policy. Then use Start server if needed. If the app says an administrator already exists, use that account; do not recreate it.

### If enabling LAN fails

The manager saves connection settings before changes and attempts to restore the previous connection if activation fails. Use Activity and Open logs for the current error. Restore previous connection provides recovery for interrupted changes. Connection snapshots are not database backups. Actual Windows service and firewall behavior must be checked on the destination server.

<!-- PAGE -->
## 2. Connect each client computer

Install this updated client on EVERY workstation. Older clients do not understand app-managed pairing.

```text
E:\ChatGPT Codex\DDHI-SmartPharmacy\release\Installers\DDHI-SmartPharmacy-Setup-x64.exe
```

### Automatic discovery: the usual method

1. Open DDHI Smart Pharmacy on the client computer. It searches for the server automatically. You can also click Find server to search again.
2. If one new server is found, the app displays its name, address and connection code. Check that code against Connect computers on the SERVER screen, then confirm the match.
3. If several servers are found, select the correct one and click Pair selected server. Check the code before confirming.
4. Once the app reports a secure connection, enter your individual staff email and password and click Sign in.
5. Repeat this first-time pairing on each client or Windows user account. Later launches use the saved server identity automatically. If discovery is blocked later, the app can still try the saved address.

### Fallback: open the server's connection file

On SERVER, click Save connection file (fallback). Copy the small `.ddhiconnect` file to the client using a trusted transfer, such as a hospital-managed shared folder or USB drive.

On CLIENT, click Open connection file, select it, and check its code against the server screen. After the app verifies the encrypted connection, sign in normally.

The file contains only the server name, address and public key identity. It contains no staff passwords, database passwords, private keys, stock or patient records. Importing it does not copy a database. If the address changes and broadcast discovery is blocked, save/import a current connection file again.

### What is automatic and what remains deliberate

Automatic: local discovery, server address selection, HTTPS setup, public-key verification on every TLS connection and remembering the paired identity. Same-adapter address changes can be rediscovered without a new code confirmation when the key is unchanged.

Deliberate: checking the server identity once, entering your own staff password and confirming pharmacy transactions. The app never accepts a different server key silently and never dispenses offline. Pairing does not grant a clinical role.

The new client also includes Connection guide in its Start Menu group and on the sign-in screen. The server has a Connection guide tab and a button for this PDF.

<!-- PAGE -->
## 3. Quick troubleshooting and acceptance

| What you see | What to do |
| --- | --- |
| No server found | On SERVER click Enable LAN. Check both PCs use the same hospital LAN. Click Find server again; use a connection file if broadcast is blocked. |
| Several servers found | Select the intended server and verify its displayed code against that server's screen. |
| Code does not match | Cancel. Use a fresh connection file directly from the intended server. Do not send sign-in details to an unverified server. |
| Server found but unavailable | In Server Control click Start server and Check health. Review Activity/Open logs if its database or API is not ready. |
| Cannot reach server after importing | A connection file does not bypass blocked networking. Guest Wi-Fi isolation, separate VLANs, VPNs or network policy may need IT assistance. |
| Certificate/server verification error | Confirm you selected the correct server. Check both computer clocks. If the identity was deliberately reset, pair again after verifying the new code. |
| Bootstrap refused: users exist | The first administrator already exists. Use that account; Start server is separate from administrator creation. |
| Sign-in fails after connection succeeds | Check the individual application account, password and lockout state. Database and PFX passwords are not staff sign-in passwords. |
| Windows asks for Administrator | Server setup and its scoped firewall changes need elevation. Run Server Control as Administrator using the original setup account. |

### Network details, if IT needs them

Defaults are HTTPS TCP 8443 and discovery UDP 8444. Simple mode's rules apply to the chosen server adapter, its local subnet and the API/worker programs. It does not open PostgreSQL to clients or change Windows certificate trust. Managed firewall policy may override local rules. Automatic broadcast discovery does not cross arbitrary VLANs; routed deployments may need approved firewall/routing changes.

Simple HTTPS trust is specific to the paired app. A browser may show a certificate warning because the app does not install a Windows root certificate. Verify readiness in the updated client and Server Control; do not bypass browser warnings or install arbitrary certificates. Advanced TLS remains optional for IT-managed certificate deployments.

### Verify before use

- [ ] Existing database and administrator remain available after the upgrade.
- [ ] Every client finds or imports the correct server and matches its code.
- [ ] Staff can sign in and see the same central data on approved workstations.
- [ ] The server restarts correctly and clients reconnect after a planned reboot.
- [ ] Encrypted off-server backups and isolated restoration have been tested.

This is an evaluation build. Automated discovery/TLS/data-preservation tests do not establish physical hospital-workstation, firewall, service/reboot or clinical acceptance. Use the hospital's acceptance and downtime procedures. Existing advanced DNS/PFX configuration remains available, but it is not needed for this normal simple-connection flow.
