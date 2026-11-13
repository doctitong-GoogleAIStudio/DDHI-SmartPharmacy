# Workstation Deployment Guide

## Universal installation
Use DDHI-SmartPharmacy-Setup-x64.exe on each of the initial four Windows 11 x64 computers. The client is self-contained and does not require Visual Studio or Codex. The installer creates a Start Menu shortcut, optionally a desktop shortcut and an uninstaller. Each installation connects to the same central API.

## First run
Hospital IT supplies a trusted internal HTTPS hostname and certificate trust chain. Enter the API URL and your individual account. The application registers the computer name centrally and obtains its workstation identity. Only non-secret server address settings are stored locally. Session tokens are kept in memory.

## Scannerless and expansion
Verify login, medicine search, receiving, stock counts, reports and authorized administrative functions without a scanner. Configure optional HID, Bluetooth HID or COM profiles afterward. Additional workstations register through the same API; there is no hardcoded four-workstation limit. Expansion capacity must be measured under representative workloads.

## Connectivity and interruption
The client never connects directly to PostgreSQL. No authoritative offline stock database exists. Network failures block transactions. If a response is lost after submission, review stock movements and reconciliation before resubmitting; the desktop does not persist an offline retry queue.

## Pilot checklist
Record Windows version, client version, hostname, API trust, role, scanner model and test results. Test simultaneous final-unit dispensing from four real computers, expired and recalled blocks, scannerless operation, session expiration, disconnect/reconnect, uninstall/reinstall and settings preservation. Physical four-workstation acceptance remains pending.