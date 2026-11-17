using DDHI.Pharmacy.Barcode;
using DDHI.Pharmacy.Application;
using Xunit;
public class BarcodeIdentityTests
{
    [Fact]public void ZeroPaddingAliasesDoNotDiscardANonzeroPackagingIndicator()
    {
        var retail=BarcodeParser.Parse("4006381333931");Assert.Contains("04006381333931",BarcodeIdentity.EquivalentCodes(retail));Assert.Contains("4006381333931",BarcodeIdentity.EquivalentCodes(retail));
        var package=BarcodeParser.Parse("18904083844874");Assert.Equal(new[]{"18904083844874"},BarcodeIdentity.EquivalentCodes(package));
        var upc=BarcodeParser.Parse("036000291452");Assert.Contains("00036000291452",BarcodeIdentity.EquivalentCodes(upc));Assert.Contains("036000291452",BarcodeIdentity.EquivalentCodes(upc));
    }
    [Fact]public void PlainCode128RequiresAnExplicitRegistrationFormatAndPreservesCase()
    {
        Assert.Throws<FormatException>(()=>BarcodeIdentity.Registration("QaMedicine-A123","Auto"));
        var code=BarcodeIdentity.Registration("QaMedicine-A123","Code128");Assert.Equal("QaMedicine-A123",code.Raw);Assert.Null(code.Gtin);
        Assert.True(BarcodeIdentity.PlainCode128Candidate(code.Raw));Assert.False(BarcodeIdentity.PlainCode128Candidate("(01)bad"));Assert.False(BarcodeIdentity.PlainCode128Candidate("01badbarcode12345678"));
    }
    [Fact]public void MixedRolesSelectTheRoleRequiredByTheOperation()
    {
        Assert.Equal(Roles.Pharmacist,ActorRoles.ForOperation([Roles.SystemAdmin,Roles.Reader,Roles.Pharmacist],"/api/dispensing"));
        Assert.Equal(Roles.SystemAdmin,ActorRoles.ForOperation([Roles.Pharmacist,Roles.SystemAdmin],"/api/users/qa/edit"));
        Assert.Equal(Roles.Chief,ActorRoles.ForOperation([Roles.SystemAdmin,Roles.Chief],"/api/settings"));
    }
}
