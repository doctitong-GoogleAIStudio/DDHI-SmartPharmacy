namespace DDHI.Pharmacy.Application;
public static class Roles {
 public const string SystemAdmin="System Administrator",PharmacyAdmin="Pharmacy Administrator",Chief="Chief Pharmacist",Pharmacist="Pharmacist",Assistant="Pharmacy Assistant",Clerk="Inventory Clerk",Auditor="Auditor",Reader="Read-Only User";
 public static readonly string[] All=[SystemAdmin,PharmacyAdmin,Chief,Pharmacist,Assistant,Clerk,Auditor,Reader];
 public static readonly string[] Clinical=[Chief,Pharmacist];
 public static readonly string[] Inventory=[PharmacyAdmin,Chief,Pharmacist,Clerk];
 public static readonly string[] Catalog=[PharmacyAdmin,Chief];
}
public record Actor(string UserId,string Role,string Workstation);
