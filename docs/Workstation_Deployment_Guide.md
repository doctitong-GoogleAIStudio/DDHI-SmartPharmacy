# Workstation Deployment Guide

## Universal installation
Use DDHI-SmartPharmacy-Setup-x64.exe on each of the initial four Windows 11 x64 computers. The client is self-contained and does not require Visual Studio or Codex. The installer creates a Start Menu shortcut, optionally a desktop shortcut and an uninstaller. Each installation connects to the same central API.

## First run
Use client version 0.2.0 on the same hospital LAN as the server. The client searches automatically. Compare its connection code with Connect computers in Server Control, confirm once, and sign in with your individual account. Later launches remember the server identity and reconnect automatically. If discovery is blocked, open the public .ddhiconnect file saved by the server and check the same code. The application registers the computer name centrally. Pairing metadata is protected per Windows user; session tokens stay in memory. A manual hostname and trusted PFX are needed only for optional Advanced TLS.

## Scannerless and expansion
Verify login, medicine search, receiving, stock counts, reports and authorized administrative functions without a scanner. Configure optional HID, Bluetooth HID or COM profiles afterward. Additional workstations register through the same API; there is no hardcoded four-workstation limit. Expansion capacity must be measured under representative workloads.

## Connectivity and interruption
The client never connects directly to PostgreSQL. No authoritative offline stock database exists. Network failures block transactions. If a response is lost after submission, review stock movements and reconciliation before resubmitting; the desktop does not persist an offline retry queue.

## Pilot checklist
Record Windows version, client version, hostname, API trust, role, scanner model and test results. Test simultaneous final-unit dispensing from four real computers, expired and recalled blocks, scannerless operation, session expiration, disconnect/reconnect, uninstall/reinstall and settings preservation. Physical four-workstation acceptance remains pending.