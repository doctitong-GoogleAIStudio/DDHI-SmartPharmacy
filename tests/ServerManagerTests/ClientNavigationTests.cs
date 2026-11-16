using DDHI.Pharmacy.Contracts;
using DDHI.Pharmacy.Desktop;
using DDHI.Pharmacy.Application;
using Xunit;

public class ClientNavigationTests
{
    [Fact] public void KnownRolesMatchTheApiRoleDefinitions() => Assert.Equal(Roles.All.Order(),ClientAccessPolicy.KnownRoles.Order());
    [Theory][InlineData("Read-Only User")][InlineData("Pharmacy Assistant")]
    public void LookupRolesCannotSeeClinicalCatalogOrAdministrationModules(string role)
    {
        var pages=ClientAccessPolicy.PagesFor([role]);
        Assert.Contains("Scan Medicine",pages);Assert.Contains("Inventory Management",pages);
        foreach(var restricted in new[]{"User Administration","Medication Dispensing","Barcode Registration","Inventory Receiving","Reports","System Settings"})Assert.DoesNotContain(restricted,pages);
        Assert.False(ClientAccessPolicy.CanAction("Medicine Master","Add medicine",[role]));
        Assert.False(ClientAccessPolicy.CanAction("Inventory Management","Quarantine / release stock",[role]));
        Assert.True(ClientAccessPolicy.CanAction("Inventory Management","FEFO by medicine",[role]));
    }
    [Fact] public void PharmacistSeesClinicalModulesButNotTechnicalAdministrationOrCatalogWrites()
    {
        var pages=ClientAccessPolicy.PagesFor([Roles.Pharmacist]);
        Assert.Contains("Medication Dispensing",pages);Assert.Contains("Returns and Adjustments",pages);Assert.Contains("Reports",pages);
        Assert.DoesNotContain("User Administration",pages);Assert.DoesNotContain("Barcode Registration",pages);
        Assert.True(ClientAccessPolicy.CanAction("Returns and Adjustments","Return to quarantine",[Roles.Pharmacist]));
        Assert.False(ClientAccessPolicy.CanAction("Returns and Adjustments","Count / adjust",[Roles.Pharmacist]));
    }
    [Fact] public void TechnicalAdministratorDoesNotInheritClinicalPermissions()
    {
        var pages=ClientAccessPolicy.PagesFor([Roles.SystemAdmin]);
        Assert.Contains("User Administration",pages);Assert.Contains("Workstation Management",pages);
        Assert.DoesNotContain("Medication Dispensing",pages);Assert.DoesNotContain("Reports",pages);Assert.DoesNotContain("System Settings",pages);
    }
    [Fact] public void MultipleRolesReceiveTheUnionAndUnknownRolesReceiveNothing()
    {
        var combined=ClientAccessPolicy.PagesFor([Roles.SystemAdmin,Roles.Pharmacist]);
        Assert.Contains("Medication Dispensing",combined);Assert.Contains("User Administration",combined);
        Assert.Empty(ClientAccessPolicy.PagesFor([]));Assert.Empty(ClientAccessPolicy.PagesFor(["Unknown role"]));
        Assert.False(ClientAccessPolicy.CanView("Hidden future page",[Roles.SystemAdmin]));
    }
    [Fact] public void InventoryClerkAndAuditorHaveDifferentOperationalMenus()
    {
        Assert.Contains("Inventory Receiving",ClientAccessPolicy.PagesFor([Roles.Clerk]));
        Assert.DoesNotContain("Medication Dispensing",ClientAccessPolicy.PagesFor([Roles.Clerk]));
        Assert.Contains("Audit Logs",ClientAccessPolicy.PagesFor([Roles.Auditor]));
        Assert.DoesNotContain("Inventory Receiving",ClientAccessPolicy.PagesFor([Roles.Auditor]));
        Assert.False(ClientAccessPolicy.CanAction("Cerebro Synchronization","Manual acknowledgment",[Roles.Auditor]));
    }
    [Fact] public void CatalogAdministratorCannotApproveChiefPharmacistPolicy()
    {
        Assert.Contains("System Settings",ClientAccessPolicy.PagesFor([Roles.PharmacyAdmin]));
        Assert.False(ClientAccessPolicy.CanAction("System Settings","Manual dispensing policy",[Roles.PharmacyAdmin]));
        Assert.True(ClientAccessPolicy.CanAction("System Settings","Manual dispensing policy",[Roles.Chief]));
    }
    [Fact] public void RoleBadgeAndMenuClearWhenAnAccountSignsOutOrChanges()
    {
        var vm=new MainViewModel();
        Assert.Empty(vm.Pages);Assert.Equal("Not signed in",vm.RoleLabel);
        vm.ApplyIdentity("pharmacist@example.invalid",[Roles.Pharmacist]);
        Assert.Equal("Pharmacist",vm.RoleLabel);Assert.Equal("pharmacist@example.invalid",vm.StaffEmail);
        Assert.Contains("Medication Dispensing",vm.Pages);
        var version=vm.SessionVersion;vm.ClearSession();
        Assert.True(vm.SessionVersion>version);Assert.Empty(vm.Pages);Assert.Empty(vm.Roles);Assert.Empty(vm.StaffEmail);
        vm.ApplyIdentity("reader@example.invalid",[Roles.Reader]);
        Assert.Equal("Read-Only User",vm.RoleLabel);Assert.DoesNotContain("Medication Dispensing",vm.Pages);
    }
}
