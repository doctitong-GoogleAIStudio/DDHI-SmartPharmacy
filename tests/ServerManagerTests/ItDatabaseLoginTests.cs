using DDHI.Pharmacy.ServerManager;
using Xunit;

public class ItDatabaseLoginTests
{
    const string Password="Test'Only!Strong12345";
    [Theory][InlineData("ddhi_runtime")][InlineData("ddhi_owner")][InlineData("postgres")][InlineData("ddhi_it_")][InlineData("ddhi_it_BAD")][InlineData("ddhi_it_x;drop role foo")][InlineData("ddhi_it_a-b")]
    public void RejectsReservedMalformedAndInjectedRoleNames(string name) => Assert.Throws<InvalidOperationException>(()=>ItDatabaseLoginRules.Validate(name,Password,ItDatabasePermission.ReadOnlyInventory,false));
    [Theory][InlineData("shortA1!")][InlineData("alllowercase123456!")][InlineData("ALLUPPERCASE123456!")][InlineData("NoNumbersPassword!!")][InlineData("NoSymbolsPassword123")][InlineData("Has spaces!123456A")][InlineData("UnicodeéPassword!123")]
    public void RejectsWeakOrUnnormalizedPasswords(string password) => Assert.Throws<InvalidOperationException>(()=>ItDatabaseLoginRules.Validate("ddhi_it_qa",password,ItDatabasePermission.ReadOnlyInventory,false));
    [Fact] public void MaintenanceRequiresAnExplicitAcknowledgementAndAnExplicitProfile()
    {
        Assert.Throws<InvalidOperationException>(()=>ItDatabaseLoginRules.Validate("ddhi_it_qa",Password,ItDatabasePermission.DatabaseMaintenance,false));
        Assert.Throws<InvalidOperationException>(()=>ItDatabaseLoginRules.Validate("ddhi_it_qa",Password,(ItDatabasePermission)99,true));
        ItDatabaseLoginRules.Validate("ddhi_it_qa",Password,ItDatabasePermission.DatabaseMaintenance,true);
        ItDatabaseLoginRules.Validate("ddhi_it_qa",Password,ItDatabasePermission.ReadOnlyInventory,false);
    }
    [Fact] public void ScramVerifierUsesRandomSaltAndNeverContainsTheCleartextPassword()
    {
        var first=ItDatabaseLoginRules.ScramVerifier(Password);var second=ItDatabaseLoginRules.ScramVerifier(Password);
        Assert.StartsWith("SCRAM-SHA-256$32768:",first);Assert.NotEqual(first,second);Assert.DoesNotContain(Password,first);
        Assert.DoesNotContain("'",first);Assert.DoesNotContain("\n",first);
        var parts=first.Split('$');Assert.Equal(3,parts.Length);
        Assert.Equal(16,Convert.FromBase64String(parts[1].Split(':')[1]).Length);
        foreach(var key in parts[2].Split(':'))Assert.Equal(32,Convert.FromBase64String(key).Length);
    }
    [Fact] public void EnforcesThePostgresqlIdentifierLength()
    {
        ItDatabaseLoginRules.Validate("ddhi_it_"+new string('a',55),Password,ItDatabasePermission.ReadOnlyInventory,false);
        Assert.Throws<InvalidOperationException>(()=>ItDatabaseLoginRules.Validate("ddhi_it_"+new string('a',56),Password,ItDatabasePermission.ReadOnlyInventory,false));
    }
}
