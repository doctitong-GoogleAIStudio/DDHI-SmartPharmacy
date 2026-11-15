# DDHI Smart Pharmacy
## Separate server and client connection guide

Digos Doctors Hospital, Inc. | Prepared 15 November 2026
Applies to Server Control 0.3.0 and client/API 0.1.0, evaluation build.

This guide explains how one dedicated Windows server serves multiple Windows 11 pharmacy computers. Instructions marked SERVER run on the dedicated server; instructions marked CLIENT run on each pharmacy workstation. IT tasks require the hospital's authorized network or certificate administrator.

Inside Server Control, open the Connection guide tab for the short server/client checklist. Its Open complete connection guide (PDF) button opens this bundled manual. The PDF is also in the installed Documentation folder and the release Documentation folder.

### How the connection works

```text
Client PC 1 --+
Client PC 2 --+-- HTTPS / TCP 8443 --> Server API
Client PC 3 --+                          |
                                 Local PostgreSQL
                                 ddhi_pharmacy
```

Clients use the server's HTTPS hostname. They do not connect directly to PostgreSQL, copy database files, or share a database folder. PostgreSQL stays on server loopback; Server Control's default database port is 55433. The API is the only pharmacy service exposed to approved client computers.

### Your current situation: read this first

Your screenshots show an administrator was already created. Later messages, "Bootstrap refused: users already exist," are from repeating first-administrator creation. They do not establish that the API is running.

The client address `https://localhost:8443` targets the client computer itself. The existing localhost evaluation also binds the API to server loopback, so replacing the client address alone cannot enable LAN access.

**Upgrade your existing installation:** Server Control 0.2.0 cannot convert a prepared localhost instance. Version 0.3.0 adds a LAN connection page for that conversion, preserving the database, accounts and credentials. Follow page 4 to install the update over the existing installation and configure its approved hostname and certificate. Do not initialize a replacement database or reset the administrator.

Complete the network and certificate work in pages 2-3 first. Page 5's initial setup applies only to a genuinely new, approved empty instance. Verify the supplied installer version and release checksums before use.

<!-- PAGE -->
## 1. Plan the server identity and network

### IT: record the real values

| Item | Value to obtain |
| --- | --- |
| Server Windows computer name | Run `hostname` on the server |
| Server IPv4 address | Run `ipconfig`; use the active hospital network adapter |
| Address assignment | IT-approved static address or DHCP reservation |
| Approved full DNS hostname | A name under the hospital's actual DNS domain |
| Hospital DNS server | The resolver used by the server and client computers |
| Allowed client addresses | Individual workstation IPs or the approved subnet |
| API port | 8443 unless deliberately changed and documented |
| Approval reference | IT change ticket or other real approval reference |

Throughout this guide, `pharmacy.hospital.example` and `192.168.10.20` are examples only. Replace them with the approved values. Do not invent an approval reference or rename the Windows server merely to match an example.

### IT: create the DNS mapping

On the hospital DNS server, open DNS Manager, expand the correct Forward Lookup Zone, and use New Host (A or AAAA) to add the chosen host label with the pharmacy server's IPv4 address. Check an existing record before changing it. Do not publish an incorrect AAAA record. Server and clients may be on different routed VLANs if hospital routing and access rules allow the connection. [DNS records](https://learn.microsoft.com/en-us/windows-server/networking/dns/manage-resource-records)

### CLIENT and SERVER: verify name resolution

Open PowerShell and run, replacing the example name:

```powershell
Resolve-DnsName -Name pharmacy.hospital.example -Type A
```

Expected: the result contains the approved server IPv4 address. If it fails, IT must correct the record or the computer's DNS configuration. Public DNS resolvers generally do not know private hospital records. DNS success does not prove that port 8443 or HTTPS is working. [Resolve-DnsName](https://learn.microsoft.com/en-us/powershell/module/dnsclient/resolve-dnsname)

<!-- PAGE -->
## 2. Obtain the matching certificate and PFX

### IT: request the HTTPS identity

Ask the hospital's certificate administrator to issue a server certificate for the approved full hostname. Its Subject Alternative Name must include that DNS name, it must be valid for Server Authentication, and it must have a private key. Supply a password-protected PFX with its certificate chain. The issuing CA chain must be trusted on the server and workstations. Follow the hospital's certificate enrollment process; a PFX export does not issue a new certificate or create DNS.

If no hospital CA or suitable trusted certificate exists, IT must establish an approved certificate/trust approach before Hospital LAN setup. The generated localhost certificate is not the LAN identity.

### SERVER: inspect or export an existing certificate

1. Press Windows + R, enter `certlm.msc`, and press Enter.
2. Open Personal > Certificates. Select the pharmacy server certificate.
3. Open Details > Subject Alternative Name; verify the exact DNS name. Check validity, the Certification Path, and that a corresponding private key exists.
4. If IT authorizes export, right-click > All Tasks > Export.
5. Choose Yes, export the private key, then Personal Information Exchange (.PFX). Include the certification path if possible, protect the export with a strong password, and save it in an access-restricted server location.
6. If private-key export is unavailable, ask the certificate administrator for an appropriate issuance/export. A public `.cer` cannot replace the PFX.

[Microsoft PFX export procedure](https://learn.microsoft.com/en-us/windows-server/identity/ad-cs/export-certificate-private-key)

### IT: establish trust on server and clients

Prefer hospital-managed trust distribution. Where manual import is approved, IT can use `certlm.msc` to import the verified public root CA certificate into Trusted Root Certification Authorities and intermediate CA certificates into Intermediate Certification Authorities. Confirm the CA fingerprints through an independent trusted channel. [Certificate stores and scope](https://learn.microsoft.com/en-us/windows-server/identity/ad-cs/inspect-certificates-manage-certificate-stores)

The private-key PFX belongs on the server, not on workstations. Only public CA certificates are distributed for workstation trust. A Current User trust change applies to that account; Local Computer trust covers the machine's applicable account contexts. Do not put an arbitrary server certificate into the root CA store or bypass certificate validation.

<!-- PAGE -->
## 3. Existing localhost installation: preserve it

**This is the route for the installation in your screenshots.** Complete DNS and certificate preparation first, but do not rerun initial database setup to force a mode change.

### What to record before any conversion

1. On SERVER, open Server Control and record its Data location, Server endpoint, configured ports, service/process mode, and version. Use the same Windows setup account; the manager's protected credentials are account-bound.
2. Keep the existing administrator email. Do not repeatedly click Create first administrator. Never send its password or the PFX password in chat.
3. Have IT make an authenticated, encrypted PostgreSQL backup and verify the restore procedure on an isolated instance. Preserve protected configuration separately under the hospital recovery process; do not copy a live PostgreSQL data directory as an ordinary file backup.
4. Arrange downtime and obtain the approved DNS hostname, PFX, password, trusted CA chain, and firewall scope.

### SERVER: install the update and convert this instance

1. During approved downtime, stop the server through the old Server Control, then close its window. Run the version 0.3.0 server installer as Administrator using the same Windows setup account and existing installation folder. Do not uninstall first or remove the data folder. The installer validates ownership and waits for owned Windows services to stop before replacing binaries.

2. Open the new Server Control as Administrator and confirm its Data location is the original instance. If a custom data folder is not selected, use Open existing server profile to select its original `profile.json`. This verifies the prepared profile and protected credentials; it does not create a database.

3. Open LAN connection. Enter the approved full DNS hostname, matching trusted PFX file, its password locally, and the actual IT approval reference. Keep the original database and API ports; this page changes only the connection identity and LAN listener.

4. Click Apply LAN connection and start server, then confirm the downtime prompt. It validates the certificate and checks that every resolved DNS address identifies this server's LAN interfaces. It saves connection settings under the protected data folder, stops the owned API/worker, applies the LAN configuration, and restarts and checks HTTPS with the selected certificate. PostgreSQL data, user accounts and credentials are retained.

5. On success, use Copy client address. Continue at page 6 for firewall checks, then page 7 for every client. If activation fails, the manager attempts automatic rollback and restart. For interrupted or incomplete recovery, reopen this same instance and use Restore previous connection. Restoring a previous localhost connection makes it local-only again. Review logs if recovery fails; do not reinitialize the database.

Connection snapshots have integrity checks and are not database backups. Do not edit `profile.json`, change `Prepared` to false or run the older `Configure-Server.ps1` over this manager instance. Those actions are not the conversion procedure.

### Find the right logs

Use Server Control's Open logs button. Its protected logs directory includes `setup.log` and `postgresql.log`; API logging also uses that directory. Installed default data location is `%ProgramData%\DDHI-SmartPharmacy\DesktopServer`; development builds may use project `runtime\server`. Treat the displayed Data location as authoritative. Share only the relevant redacted error lines.

<!-- PAGE -->
## 4. New server: initial Hospital LAN setup

**Use this page only for a genuinely new instance.** It is not a repair procedure for an existing database. The target Windows Server needs Desktop Experience for Server Control. Hospital IT must verify target-OS compatibility and permissions during acceptance.

### SERVER: install the server components

1. Obtain the verified server installer from the project's local release folder:

```text
E:\ChatGPT Codex\DDHI-SmartPharmacy\release\Installers\DDHI-SmartPharmacy-Server-Setup-x64.exe
```

Copy the installer to the separate server; the server does not need an E: drive matching the development computer.

2. Install Microsoft's Visual C++ x64 runtime if missing, from `https://aka.ms/vs/17/release/vc_redist.x64.exe`. Run the server installer as Administrator. Default install root is `C:\Program Files\DDHI-SmartPharmacy-Server`.
3. Open DDHI Smart Pharmacy Server Control as Administrator. The installer bundles PostgreSQL 17.12, API, worker and the desktop manager. It does not ship an initialized clinical database or shared passwords.

### SERVER: fill Initial setup

| Field | Enter |
| --- | --- |
| Mode | Hospital LAN |
| Data folder | Approved persistent location outside install binaries |
| PostgreSQL binaries | Bundled PostgreSQL\bin, normally prefilled |
| PostgreSQL port | 55433 by default; remains loopback only |
| API port | 8443, matching the client/firewall plan |
| Hostname | Approved DNS name only, without https:// or :8443 |
| PFX / password | Trusted matching server PFX and its local password |
| Approval reference | The actual IT approval or change ticket |
| Configure Windows services | Checked; manager must be elevated |

Click Set up database and HTTPS once. It creates the independent database, separate `ddhi_migration` and `ddhi_runtime` accounts, migrations and runtime grants. Passwords are generated and protected automatically. If setup fails, inspect its log before retrying; do not delete partially created folders. Preserve the selected data location in IT's recovery records.

<!-- PAGE -->
## 5. Start the server and allow approved clients

### SERVER: administrator and service startup

For a NEW database only, open Administrator and certificate, supply the first administrator email and a password meeting the displayed policy, and click Create first administrator once. For an existing database, use its existing account.

Click Start server, then Check health. The database should be connected and the API should report Running / database ready. If Windows services were configured, an elevated PowerShell check is:

```powershell
Get-Service -Name DDHIPostgreSQL,DDHIPharmacyApi,DDHIPharmacyWorker
Get-NetTCPConnection -State Listen -LocalPort 8443
```

All three services should be Running. A LAN API must listen on the approved interface or `0.0.0.0`, not only `127.0.0.1`. Service Running alone does not prove HTTPS or database readiness. In process fallback mode, absent services do not prove a failure; check Server Control and the listener instead.

### IT / SERVER: permit HTTPS through the firewall

Use Windows Defender Firewall with Advanced Security (`wf.msc`) > Inbound Rules > New Rule > Custom. Allow inbound TCP local port 8443, restrict Scope > Remote IP addresses to approved pharmacy clients/subnets, and select only the approved active Domain or Private profile. Name the rule DDHI Pharmacy HTTPS 8443. Do not change a managed network profile just to make a rule match.

Equivalent elevated PowerShell example, after replacing the example subnet and checking for an existing rule:

```powershell
New-NetFirewallRule -DisplayName 'DDHI Pharmacy HTTPS 8443' `
  -Direction Inbound -Action Allow -Protocol TCP -LocalPort 8443 `
  -RemoteAddress '192.168.10.0/24' -Profile Domain,Private
```

IT must also allow any intervening VLAN/network firewall path and applicable client outbound traffic. Do not disable the firewall, expose the API to the Internet, or open PostgreSQL to workstations. [Firewall rule documentation](https://learn.microsoft.com/en-us/powershell/module/netsecurity/new-netfirewallrule)

### SERVER: verify actual HTTPS

Browse to `https://pharmacy.hospital.example:8443/health`, replacing the hostname. Expect JSON containing `"status":"ready"` with no certificate warning. Use the LAN name even when testing on the server; the LAN certificate need not match localhost.

<!-- PAGE -->
## 6. Connect every client computer

### CLIENT: verify each layer in order

Open PowerShell on the workstation. Replace the example DNS name in both commands:

```powershell
Resolve-DnsName -Name pharmacy.hospital.example -Type A
Test-NetConnection -ComputerName pharmacy.hospital.example -Port 8443
```

Expected: the correct server IP and `TcpTestSucceeded : True`. Ping failure alone is not decisive because ICMP may be blocked. TCP success verifies the port, not certificate trust, database readiness or application login. [TCP connection diagnostics](https://learn.microsoft.com/en-us/powershell/module/nettcpip/test-netconnection)

Next, open the workstation browser:

```text
https://pharmacy.hospital.example:8443/health
```

Expected: `"status":"ready"`, with no certificate warning. If either check fails, use page 8 before attempting application sign-in. A raw IP may be used for a TCP diagnostic, but use the certificate's approved DNS name for HTTPS unless IT intentionally issued a matching IP SAN certificate and the application supports that deployment.

### CLIENT: install and sign in

1. Obtain the client installer from the local project release folder:

```text
E:\ChatGPT Codex\DDHI-SmartPharmacy\release\Installers\DDHI-SmartPharmacy-Setup-x64.exe
```

Copy this installer to each workstation. Do not install PostgreSQL or a separate server on each client.

2. Install and open DDHI Smart Pharmacy. IT must already have established the approved public CA trust on that workstation.
3. In Central API address (HTTPS), replace `https://localhost:8443` with `https://pharmacy.hospital.example:8443`, using the actual approved hostname. Enter the base address only, without `/health`, `/api` or `/auth/login`.
4. Sign in with the existing administrator account for initial administration. Create individual staff accounts and assign the Chief Pharmacist role to the appropriate authorized person. Staff use their own accounts thereafter.
5. Confirm the workstation identity and central data in the application. The server address is saved for the Windows user after a successful login. Repeat the DNS, TCP, HTTPS and login checks on every client.

<!-- PAGE -->
## 7. Troubleshooting by the first failing check

| Symptom | Meaning and next action |
| --- | --- |
| Refused connection to localhost:8443 | Client is targeting itself. Use the server DNS name after the server is configured for LAN. |
| Refused connection to the server name | Check its resolved IP, API process/service, port and listener binding. A rejected TCP attempt has not reached application login. |
| TCP timeout / TcpTestSucceeded False | Check server reachability, VLAN routes, host/network firewalls, port, active network profile and listener. |
| DNS name cannot resolve / wrong IP | IT corrects DNS and resolver settings. After correction, clear stale client cache if IT directs it, then repeat resolution. |
| Certificate name mismatch | Use the exact approved DNS name in the certificate SAN, or have IT issue the correct certificate. |
| Certificate chain untrusted / expired | IT checks the issuing CA chain, trust scope, certificate validity and computer clocks. Do not bypass the warning. |
| PFX invalid or password rejected | IT checks that the PFX includes the private key and matching valid certificate; enter its password locally. |
| HTTPS health says database unavailable | API is reachable, but the database is not ready. Check PostgreSQL, protected configuration and logs on the server. |
| Health is ready but sign-in fails | Verify the existing application account, password, lockout status and intended database. Network connectivity is already working. |
| Bootstrap refused: users already exist | First-administrator creation is deliberately blocked. Use the existing account and Start server; do not reset the database. |

### When services fail to start

On SERVER, review Server Control's Activity and Open logs, and Windows Event Viewer > Windows Logs > Application/System for the matching time. Check that the protected configuration and certificate are readable by the configured service identity, that ports are free, and that PostgreSQL is running. Do not replace virtual service accounts with a shared administrator account as a shortcut.

For help, supply the failing check, exact time, server/client computer names, approved hostname, resolved IP, service status and relevant redacted log lines. Do not send PFX files, protected credential stores, passwords or patient information. If the administrator password is lost, a separate authorized recovery procedure is needed; repeated bootstrap is not password recovery.

<!-- PAGE -->
## 8. Daily operation and acceptance checklist

The server must remain powered on and connected. Closing Server Control leaves already started processes/services running. Windows services provide unattended startup, subject to destination-server validation. Process fallback mode does not provide the same logoff/reboot guarantees. Clients require the central server and cannot dispense offline.

IT must schedule authenticated encrypted backups to an approved off-server destination, secure recovery material, monitor backup success and test restoration on an isolated system. Current-user and machine-protected credentials are not portable simply by copying runtime folders to another computer. Preserve the working configuration and approved rollback/recovery plan before changes.

### Verify before hospital use

- [ ] Server IP assignment is stable; DNS resolves correctly on all clients.
- [ ] Approved hostname equals the certificate SAN and application URL.
- [ ] Trusted certificate chain is valid on server and all clients.
- [ ] API listens on the intended LAN interface and TCP port.
- [ ] Only approved clients/subnets can reach API port 8443.
- [ ] PostgreSQL remains loopback-only and clients have no database passwords.
- [ ] `/health` reports ready on the server and every client without TLS warnings.
- [ ] Existing administrator can sign in; individual staff roles are assigned.
- [ ] Multiple approved clients read the same central data in an evaluation exercise.
- [ ] No live medication transaction is used merely to test connectivity.
- [ ] Reboot/service recovery and off-server backup restoration are demonstrated.
- [ ] Hospital IT and pharmacy leadership record acceptance and outstanding issues.

This remains an evaluation build. Destination-server service/ACL/reboot acceptance and hospital clinical acceptance have not been established by this guide. Live Cerebro integration remains disabled.

### Request to give hospital IT

Please provide the pharmacy server's stable IPv4 address, approved full DNS hostname and mapping, a matching Server Authentication PFX with private key and trusted CA chain, secure password handoff, workstation CA trust, the approved client subnet/IP list, and TCP 8443 access. Record the approval reference. Upgrade the existing localhost instance to Server Control 0.3.0 and use its LAN connection page; do not initialize a new database to convert it.

### References and project basis

The linked Microsoft pages on pages 2-3 and 6-7 cover Windows DNS, certificate export/trust, firewall rules and connection tests. Application behavior was checked against local version 0.3.0 sources on 15 November 2026. Build artifacts remain under `E:\ChatGPT Codex\DDHI-SmartPharmacy\release`. See release Tests and Checksums for automated evidence. Elevated Windows service upgrades, certificate infrastructure and physical client connectivity still require acceptance on the destination server and workstations.
