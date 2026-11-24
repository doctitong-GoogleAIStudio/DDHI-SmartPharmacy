namespace DDHI.Pharmacy.Contracts;

// Mirrors API role policies for navigation only; the API remains the authority.
public static class ClientAccessPolicy
{
    public static readonly string[] KnownRoles = ["Read-Only User", "Inventory Clerk", "Pharmacy Assistant", "Pharmacist", "Chief Pharmacist", "Pharmacy Administrator", "Auditor", "System Administrator"];
    public static readonly string[] AllPages = ["Dashboard", "Scan Medicine", "Medicine Master", "Drug Reference", "Barcode Registration", "Inventory Receiving", "Inventory Management", "Medication Dispensing", "Stock Transfers", "Returns and Adjustments", "Expiration Monitoring", "Medicine Recalls", "Cerebro Synchronization", "Reports", "Scanner Settings", "Workstation Management", "User Administration", "Audit Trail", "System Settings", "About Application"];
    public static bool Has(IEnumerable<string> roles, string policy) => roles.Any(role => policy switch {
        "Catalog" => role is "Chief Pharmacist" or "Pharmacy Administrator",
        "Clinical" => role is "Chief Pharmacist" or "Pharmacist",
        "Inventory" => role is "Chief Pharmacist" or "Pharmacy Administrator" or "Pharmacist" or "Inventory Clerk",
        "Audit" => role is "System Administrator" or "Chief Pharmacist" or "Pharmacy Administrator" or "Pharmacist" or "Auditor",
        "Report" => role is "Chief Pharmacist" or "Pharmacy Administrator" or "Pharmacist" or "Auditor",
        "Admin" => role == "System Administrator",
        "Chief" => role == "Chief Pharmacist",
        "SignedIn" => KnownRoles.Contains(role),
        _ => false
    });
    public static bool CanView(string page, IEnumerable<string> roles) => page switch {
        "Drug Reference" or "Dashboard" or "Scan Medicine" or "Medicine Master" or "Inventory Management" or "Medicine Recalls" or "Scanner Settings" or "About Application" => Has(roles,"SignedIn"),
        "Barcode Registration" or "System Settings" => Has(roles,"Catalog"),
        "Inventory Receiving" or "Stock Transfers" => Has(roles,"Inventory"),
        "Medication Dispensing" => Has(roles,"Clinical"),
        "Returns and Adjustments" => Has(roles,"Catalog") || Has(roles,"Clinical"),
        "Expiration Monitoring" or "Cerebro Synchronization" or "Reports" => Has(roles,"Report"),
        "Audit Trail" or "Audit Logs" => Has(roles,"Audit"),
        "Workstation Management" or "User Administration" => Has(roles,"Admin"),
        _ => false
    };
    public static string[] PagesFor(IEnumerable<string> roles) => AllPages.Where(p=>CanView(p,roles)).ToArray();
    public static bool CanAction(string page, string action, IEnumerable<string> roles)
    {
        if (!CanView(page,roles)) return false;
        return page switch {
            "Medicine Master" => action is "Clinical reference" or "Browse generics" or "View brands" or "All brand products" or "Export CSV / Excel" or "Print / PDF" ? Has(roles,"SignedIn") : Has(roles,"Catalog"),
            "Barcode Registration" => Has(roles,"Catalog"),
            "Inventory Management" => action switch {
                "Quarantine / release stock" or "Record disposition" or "Add location" => Has(roles,"Catalog"),
                "Reconcile ledger" => Has(roles,"Report"),
                "Print stock label" => Has(roles,"Inventory"),
                "FEFO by medicine" or "Export CSV / Excel" or "Print / PDF" => Has(roles,"SignedIn"),
                _ => false
            },
            "Returns and Adjustments" => action is "Export CSV / Excel" or "Print / PDF" ? Has(roles,"Clinical")||Has(roles,"Catalog") : action is "Return to quarantine" or "Find discharge medicines" or "Hospital discharge return" or "Patient return history" or "Outside / uncertain intake" or "Outside intake history" ? Has(roles,"Clinical") : Has(roles,"Catalog"),
            "Medicine Recalls" => Has(roles,"Catalog"),
            "Cerebro Synchronization" => action == "Manual acknowledgment" ? Has(roles,"Catalog") : Has(roles,"Report"),
            "System Settings" => Has(roles,"Chief"),
            _ => true
        };
    }
}
