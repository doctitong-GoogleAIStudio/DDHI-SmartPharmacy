# Database Access — Server Control 0.6.0

Install the updated server installer over the same installation using the original Windows setup account. Preserve the original data folder and take an encrypted backup before the upgrade. Open Server Control as Administrator. The new Database Access tab is immediately after Activity. Existing client 0.2.1 remains compatible.

## Connection and health

The tab shows database ddhi_pharmacy, the loopback PostgreSQL host/port, runtime/migration role names, data folder and HTTPS API address. Check database and API performs live checks. Copy connection details excludes all passwords and private keys. A refused connection means the existing server must be started; this page never initializes another database.

## View and export

Sign in with your individual pharmacy application email and password. Select a dataset and click View records. Existing server authorization applies: for example, staff accounts require System Administrator, receiving requires an inventory role, audit events require a report role, and orders require a clinical role. A technical administrator does not gain a clinical role through this tab.

The grid is read-only. There is no SQL editor and no record creation/update/delete command. The API's result limits apply; the row count and loaded dataset are shown. Export loaded records saves that snapshot to CSV or JSON, not a full database backup. CSV string cells that could be interpreted as spreadsheet formulas are escaped. Protect exports using hospital data procedures. Sign out clears the visible records and in-memory token; sessions expire within 20 minutes and are cleared when the selected instance/endpoint changes or the window closes.

## Database administration tools

Open SQL Shell launches the bundled psql on 127.0.0.1 using the displayed PostgreSQL port and database. Enter an IT-provisioned PostgreSQL username. The shell prompts for that database account's password; your pharmacy staff password is not a PostgreSQL login. No saved application/database passwords are displayed or passed. SQL Shell starts with transactions read-only and a 30-second statement timeout; PostgreSQL account permissions remain authoritative. It is an IT tool, separate from the role-controlled application viewer.

Open installed pgAdmin lets IT select an existing pgAdmin4.exe. pgAdmin is optional and is not bundled/downloaded automatically. Configure it with the displayed host/port/database and a separately provisioned database login. Opening a tool does not share saved passwords or alter PostgreSQL networking. Server Control 0.6.0 adds a separate IT-login creation form in this tab; see IT_DATABASE_LOGIN.md. It creates a new account with the explicitly selected access level and does not reset existing accounts. Client PCs continue to connect only to the central HTTPS API.

## Validation limits

Automated checks cover staff authentication, role denial, session clearing, HTTPS, read-only record requests, export conversion and spreadsheet escaping. Destination-server launch/configuration of external pgAdmin/SQL Shell still needs local IT verification. Do not run arbitrary SQL against the hospital database as part of an acceptance test.
