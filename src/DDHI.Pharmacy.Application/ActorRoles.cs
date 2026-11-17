namespace DDHI.Pharmacy.Application;
public static class ActorRoles
{
    public static string ForOperation(IEnumerable<string> assigned,string path)
    {
        var roles=assigned.ToArray();
        var order=path.StartsWith("/api/users",StringComparison.Ordinal)?new[]{Roles.SystemAdmin}:
            new[]{"/api/orders","/api/dispensing","/api/reservations","/api/returns","/api/settings"}.Any(p=>path.StartsWith(p,StringComparison.Ordinal))?new[]{Roles.Chief,Roles.Pharmacist}:
            new[]{Roles.Chief,Roles.PharmacyAdmin,Roles.Pharmacist,Roles.Clerk,Roles.Auditor,Roles.SystemAdmin,Roles.Assistant,Roles.Reader};
        return order.FirstOrDefault(roles.Contains)??roles.FirstOrDefault()??"";
    }
}
