using System.Net;
using System.Net.Http;
using System.Reflection;
using DDHI.Pharmacy.Desktop;
using Xunit;

public class AuditSessionClientTests
{
    sealed class DelayedHandler:HttpMessageHandler
    {
        public TaskCompletionSource<HttpResponseMessage> Reply=new(TaskCreationOptions.RunContinuationsAsynchronously);
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken cancellationToken)=>Reply.Task;
    }
    [Fact] public async Task ADelayedHeartbeatFromTheOldLoginCannotClearTheNextStaffAccount()
    {
        var vm=new MainViewModel();var handler=new DelayedHandler();using var client=new HttpClient(handler){BaseAddress=new Uri("https://localhost/")};
        typeof(MainViewModel).GetProperty("Client")!.SetValue(vm,client);typeof(MainViewModel).GetField("auditSession",BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(vm,Guid.NewGuid().ToString());
        var notifications=0;vm.SessionExpired+=()=>notifications++;
        vm.ApplyIdentity("old@example.invalid",["Pharmacist"]);var pending=vm.Heartbeat();vm.ClearSession();vm.ApplyIdentity("new@example.invalid",["Read-Only User"]);
        handler.Reply.SetResult(new HttpResponseMessage(HttpStatusCode.Unauthorized));await pending;
        Assert.Equal("new@example.invalid",vm.StaffEmail);Assert.Equal("Read-Only User",vm.RoleLabel);Assert.Equal(0,notifications);vm.ClearSession();
    }
    [Fact] public async Task UnauthorizedHeartbeatClearsTheCurrentIdentityAndNotifiesTheWindow()
    {
        var vm=new MainViewModel();var handler=new DelayedHandler();using var client=new HttpClient(handler){BaseAddress=new Uri("https://localhost/")};
        typeof(MainViewModel).GetProperty("Client")!.SetValue(vm,client);typeof(MainViewModel).GetField("auditSession",BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(vm,Guid.NewGuid().ToString());
        vm.ApplyIdentity("current@example.invalid",["Pharmacist"]);var notifications=0;vm.SessionExpired+=()=>notifications++;
        var pending=vm.Heartbeat();handler.Reply.SetResult(new HttpResponseMessage(HttpStatusCode.Unauthorized));await pending;
        Assert.Empty(vm.StaffEmail);Assert.Empty(vm.Pages);Assert.Equal(1,notifications);
    }
}
