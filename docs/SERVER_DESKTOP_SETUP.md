# Simple server setup - Server Control 0.4.0

On an existing server, install the update over the original folder using the same Windows setup account and original data folder. Keep an encrypted backup and schedule downtime. Open Server Control as Administrator, choose Connect computers and Enable LAN. Use Open existing server profile only if a custom prepared data folder was not selected. Do not create a replacement database or administrator.

On a genuinely new server, Initial setup > Set up server handles database, generated protected credentials, services and simple LAN automatically. Create the first application administrator once in Accounts. Existing application users are detected and first-administrator creation is disabled.

On each workstation install client 0.2.0. It finds the server automatically on the same local network. Match the displayed connection code with the server screen and confirm once, then sign in normally. Future connections use the saved identity. If discovery is blocked, save a .ddhiconnect file on the server and open it on the client. The file has no passwords or private keys.

HTTPS uses an app-managed identity pinned to the exact server public key; a different key is rejected. No manual DNS/PFX/Windows root-certificate setup or approval reference is required in simple mode. First-time pairing is deliberate; discovery alone never establishes trust. Server firewall rules are limited to the selected adapter, local subnet and API/worker programs. PostgreSQL stays loopback-only. Default ports are TCP 8443 and UDP 8444.

Automatic discovery requires a working local network; guest Wi-Fi isolation or routing/firewall policy can block it. Advanced TLS remains available for IT-managed certificate deployments. Windows service, firewall, reboot and actual workstation acceptance remain pending. No permanent pharmacy instance is initialized on the development MSI computer. Live Cerebro stays disabled.

See SERVER_CLIENT_CONNECTION_GUIDE.md and the bundled PDF for the complete simple guide.
