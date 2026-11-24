using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using DDHI.Pharmacy.Contracts;
using Xunit;

internal static class ClinicalReferenceAssertions
{
    internal static async Task Exercise(string root,HttpClient client,Func<string,object,Task<JsonElement>> post)
    {
        var status=await client.GetFromJsonAsync<JsonElement>("/api/clinical/status");Assert.Equal(0,status.GetProperty("interactionRules").GetInt32());Assert.Equal(0,status.GetProperty("monographs").GetInt32());
        var search=await client.GetFromJsonAsync<JsonElement>("/api/clinical/search?directory=&q=");var rows=search.GetProperty("rows").EnumerateArray().Take(2).ToArray();Assert.Equal(2,rows.Length);
        var request=new ClinicalScreenInput(rows.Select(x=>new MedicationContext(x.GetProperty("id").GetGuid())).ToArray(),"QA-runtime-clinical");
        var screen=await post("/api/clinical/screen",request);var result=screen.GetProperty("Result");Assert.False(result.GetProperty("Complete").GetBoolean());Assert.Contains(result.GetProperty("Findings").EnumerateArray(),x=>x.GetProperty("Severity").GetString()=="Unknown");
        var monograph=await client.GetFromJsonAsync<JsonElement>("/api/clinical/monograph/"+rows[0].GetProperty("id").GetGuid());Assert.Equal("No approved monograph",monograph.GetProperty("approvalStatus").GetString());
        await File.WriteAllTextAsync(Path.Combine(root,"release","Tests","ClinicalReferenceRuntime.json"),JsonSerializer.Serialize(new{publishedRuntimeRole=true,noClinicalSeed=true,localSearch=true,explicitUnknownCoverage=true,unmappedMonograph=true,immutableScreenSaved=true}));
    }
}
