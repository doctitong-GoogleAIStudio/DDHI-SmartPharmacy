# Create an IT PostgreSQL login — Server Control 0.6.0

On the server, install the updated server installer over the original installation. Open Server Control as Administrator using the original Windows setup account. Keep the existing database/data folder. Client 0.3.0 does not need to be reinstalled for this server-only feature.

1. Open **Database Access**, after Activity, and scroll to **Create a separate IT PostgreSQL login**.
2. Choose a personal username such as `ddhi_it_jdoe`. Use lowercase letters, numbers and underscores after `ddhi_it_` (63 characters maximum).
3. Enter and confirm your own password: 16–128 printable ASCII characters, including uppercase, lowercase, number and symbol, with no spaces. Retain it in the approved password manager. Server Control does not retain or display this password.
4. Select an access level. **Read-only inventory views** is the default. **Database maintenance** requires the explicit acknowledgement because it permits pharmacy schema/data changes.
5. Click **Create IT database login** and confirm the username/access level. A success message and connection details appear, and the SQL Shell username is filled in automatically.
6. Click **Open SQL Shell**, or select the installed pgAdmin executable. Use host `127.0.0.1`, the displayed PostgreSQL port (normally `55433`), database `ddhi_pharmacy`, your new username, and the password you chose. Run these tools on the server; clients continue to use HTTPS.

## Read-only inventory views

This login can select from `ddhi_it.medicines`, `ddhi_it.inventory`, `ddhi_it.suppliers` and `ddhi_it.locations`. Example:

```sql
SELECT * FROM ddhi_it.inventory LIMIT 100;
```

It has no direct application-table access, stock writes, patient/order data, staff password hashes, schema creation, role creation, server-file access, or superuser privileges. Read-only defaults complement actual object grants; changing a session's read-only setting does not grant write permissions. The wizard refuses provisioning if existing PUBLIC grants would give the new read-only login unintended underlying-table access. It does not change those global grants.

## Database maintenance

This explicit choice also grants non-inherited membership in the pharmacy schema-owner role `ddhi_migration`, without permission to grant that role onward. It is not a PostgreSQL superuser or server-role administrator. Sessions begin read-only. Approved database maintenance requires explicitly entering a write transaction and selecting the schema-owner role, for example `BEGIN READ WRITE; SET LOCAL ROLE ddhi_migration;` followed by the reviewed maintenance operation and COMMIT or ROLLBACK. This access can inspect/change pharmacy application tables and schema; use hospital change procedures and verified backups. Do not alter the passwords of `ddhi_owner`, `ddhi_migration` or `ddhi_runtime` in external tools.

## Preservation and passwords

Existing usernames are refused; this is not a password reset or permission-upgrade tool. An unsuccessful provisioning transaction rolls back the new login/grants. Application credentials, clinical stock and ledger records are retained. No PostgreSQL network/firewall rules are changed. A protected local creation audit records the operator, username and access level, without the entered password. Passwords are converted to salted SCRAM-SHA-256 verifiers before SQL execution, so the cleartext password is not embedded in SQL, command-line arguments or password files.

The reporting schema/views are managed additive objects; existing unrelated schemas/objects with the same names are not overwritten. A future schema change may require reviewing/updating these views. pgAdmin is optional and not bundled. A direct IT database login is separate from a pharmacy application account and grants no pharmacy desktop role.

References: [PostgreSQL 17 CREATE ROLE](https://www.postgresql.org/docs/17/sql-createrole.html), [role membership](https://www.postgresql.org/docs/17/role-membership.html), [CREATE VIEW](https://www.postgresql.org/docs/17/sql-createview.html).
