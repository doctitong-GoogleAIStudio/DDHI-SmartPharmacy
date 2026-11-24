using System.Net.Http.Json;
using System.Text.Json;
using DDHI.Pharmacy.Application;
using DDHI.Pharmacy.Contracts;
using DDHI.Pharmacy.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public partial class WorkflowTests
{
    // Compatibility tests explicitly perform the new QA-only clinical review; no
    // production policy is changed, and safety-gate tests use DirectDispense instead.
    async Task<DispenseInput> ReviewFixtureDispense(HttpClient client,DispenseInput input)
    {
        if(input.SafetyAssessmentId!=null)return input;
        using var me=await client.GetAsync("/api/me");if(!me.IsSuccessStatusCode)return input;var identity=await me.Content.ReadFromJsonAsync<JsonElement>();if(!identity.GetProperty("roles").EnumerateArray().Any(x=>Roles.Clinical.Contains(x.GetString())))return input;
        using var scope=factory.Services.CreateScope();var db=scope.ServiceProvider.GetRequiredService<PharmacyDb>();var order=await db.DispenseOrders.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==input.OrderId);var balance=await db.InventoryBalances.AsNoTracking().Include(x=>x.Batch).SingleOrDefaultAsync(x=>x.Id==input.BalanceId);if(order==null||balance==null)return input;
        await Post(chief,"clinical/policy",new ClinicalSafetyPolicy(true,true,"Engineering compatibility fixture only"));
        var screen=await Post(client,"clinical/screen",new ClinicalScreenInput([new(balance.Batch.MedicineId)],order.EncounterReference,OrderId:order.Id,BalanceId:balance.Id,IssueQuantity:input.Quantity));var id=screen.GetProperty("Id").GetGuid();
        await Post(client,"clinical/interventions",new ClinicalInterventionInput(id,"Proceed with documented review","Engineering fixture review only; not clinical validation.","","QA-only authorization",true,true));return input with{SafetyAssessmentId=id};
    }
    async Task<HttpResponseMessage> DirectDispense(HttpClient client,DispenseInput input,string? key=null)
    {using var request=new HttpRequestMessage(HttpMethod.Post,"/api/dispensing"){Content=JsonContent.Create(input)};request.Headers.Add("Idempotency-Key",key??Guid.NewGuid().ToString());return await client.SendAsync(request);}
}
