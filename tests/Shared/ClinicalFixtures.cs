using DDHI.Pharmacy.Contracts;
using DDHI.Pharmacy.Domain;

// Fictional engineering fixtures only. Never production clinical seed data.
public static class ClinicalFixtures
{
    public static ClinicalPackage Package(string? version=null,string severity="Major")
    {
        var today=Safety.Today;
        var source=new ClinicalSource("fixture","Engineering fixture - not a clinical authority","Synthetic software test only","https://example.invalid/engineering-fixture","Developer-authored fictional test fixture; never clinical content",today,today);
        ClinicalDrugDefinition Drug(string key,params string[] ingredients)=>new(key,"Fictional "+key,"10 mg","tablet","oral",ingredients.Select(i=>new IngredientAmount(i,10,"mg")).ToArray(),["fixture-class"],[]);
        MonographDefinition Mono(string key)=>new(key,[new("Identification","Fictional software fixture; not drug information.",["fixture"])],["fixture"],today,today.AddDays(90));
        return new(1,version??"fixture-"+Guid.NewGuid().ToString("N"),"Fictional engineering data; not clinical reference content.",[source],
            [new("fixture-a","Fictional ingredient A",[]),new("fixture-b","Fictional ingredient B",[])],
            [Drug("fixture-single","fixture-a"),Drug("fixture-combo","fixture-a","fixture-b"),Drug("fixture-other","fixture-b")],
            [new("fixture-product","fixture-single","","QA maker","","","Unknown","","Unknown",null,"Unknown",["fixture"]),new("fixture-other-product","fixture-other","","QA maker","","","Unknown","","Unknown",null,"Unknown",["fixture"])],[],
            [Mono("fixture-single"),Mono("fixture-combo"),Mono("fixture-other")],
            [new("fixture-rule","DrugDrug","Ingredient","fixture-a","Ingredient","fixture-b",severity,"Fictional source severity","Synthetic engineering declaration only","Fictional test finding","Fictional test mechanism","Fictional test consequence","Do not use as clinical guidance.","Engineering verification only.",["fixture"],today,today.AddDays(90),[])]);
    }
}
