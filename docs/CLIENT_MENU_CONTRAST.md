# Client 0.2.1 navigation contrast

The global TextBlock foreground previously overrode the navigation container's light text, making labels hard to see on the dark sidebar. Sidebar items now use an explicit text foreground binding and a dedicated template, with light text normally and dark text on pale green for hover, selection and keyboard focus. Selection stays visible when focus moves to the work area. Long labels wrap.

Close the pharmacy application and run DDHI-SmartPharmacy-Setup-x64.exe on each client computer. The installer retains the same application identity and installation path. Server Control 0.4.0 remains compatible; this visual update does not change credentials, pairing, database records or clinical transactions.
