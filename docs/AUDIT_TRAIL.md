# Audit Trail and observed session times

Client 0.6.0 / API 0.4.0. Server Control remains 0.8.0.

After an approved backup, install the new server package on the existing separate server, open Server Control as Administrator under the original setup account, and choose Server status > Apply database updates. Update clients with the matching installer. Never initialize a new database over the existing pharmacy database.

In the client sidebar choose **Audit Trail**. System Administrators, Chief Pharmacists, Pharmacy Administrators, Pharmacists and Auditors have access.

- **Activities**: staff email/name, assigned roles, client-reported computer, server date/time to seconds in Asia/Manila, activity, outcome and references. Filter and page through results; select **View details** for safe before/after values and integrity hashes.
- **Timed sessions**: server-recorded start, last contact, confirmed sign-out (when available), status and observed duration in hours/minutes.
- **Daily totals**: observed intervals grouped by account/computer/Manila date, overlap counted once, midnight split correctly. Default last 30 days; explicit range up to 366 days. These totals are not active work or attendance.
- **Verify hash chain**: validates the existing and new audit formats without rewriting historical records. Database triggers reject updates/deletions; use verified backups and protect database superuser access.
- **Export CSV / Excel** and **Print / PDF** include only the loaded page, up to 200 records. Apply filters/page selection before export.

CSV imports/exports and print/manual actions are reported by the app with base filename, row count, page and outcome. They are not independent verification of file or printer delivery. There is no monitoring of unrelated operating-system files.

Successful central mutations and their audit events are atomic. Failed/denied requests and read access use separate audit writes and may be unavailable during a database outage. Passwords, security stamps, tokens, private keys and raw imported files are excluded. Change details are bounded and identify omitted fields with a digest; they do not provide unlimited field reconstruction. Legacy identity names are not invented retroactively.

A heartbeat runs approximately once per minute. Sudden shutdown, revoked login or lost network leaves session end unconfirmed; duration stops at last contact and never extends beyond the recorded authentication expiry. The authentication lifetime remains 20 minutes. Computer names come from the client and are not hardware authentication.

The full sidebar manual includes the same workflow in chapter 21. Hospital validation on the real separate server/client computers remains required.
