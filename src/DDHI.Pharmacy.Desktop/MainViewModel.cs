using System.ComponentModel;
using System.Data;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.IO;
using DDHI.Pharmacy.Contracts;
namespace DDHI.Pharmacy.Desktop;
public sealed class MainViewModel:INotifyPropertyChanged {
 public event PropertyChangedEventHandler? PropertyChanged;
 public event Action? SessionExpired;
 string title="Dashboard",status="Connect to the central server to continue.",footer="";DataView? rows;
 public string Title{get=>title;set{title=value;Changed();}} public string Status{get=>status;set{status=value;Changed();}}
 public string Footer{get=>footer;set{footer=value;Changed();}} public DataView? Rows{get=>rows;set{rows=value;Changed();}}
 void Changed([CallerMemberName]string? name=null)=>PropertyChanged?.Invoke(this,new(name));
 public string[] Pages {get;private set;}=[];
 public string UserId {get;private set;}="";
 public string StaffEmail {get;private set;}="";
 public string RoleLabel=>Roles.Length==0?"Not signed in":string.Join(" • ",Roles);
 public int SessionVersion {get;private set;}
 internal void ApplyIdentity(string email,IEnumerable<string> roles){StaffEmail=email;Roles=roles.Distinct().ToArray();Pages=ClientAccessPolicy.PagesFor(Roles);Changed(nameof(StaffEmail));Changed(nameof(RoleLabel));Changed(nameof(Pages));}
 public void ClearSession(){heartbeat.Stop();auditSession="";Client.DefaultRequestHeaders.Remove("X-Audit-Session");SessionVersion++;Client.DefaultRequestHeaders.Authorization=null;Roles=[];Pages=[];StaffEmail="";UserId="";Rows=null;Footer="";Endpoint="";pendingKeys.Clear();Changed(nameof(StaffEmail));Changed(nameof(RoleLabel));Changed(nameof(Pages));}

 readonly System.Windows.Threading.DispatcherTimer heartbeat=new(){Interval=TimeSpan.FromMinutes(1)};
 string auditSession="";bool heartbeatBusy;
 public MainViewModel(){heartbeat.Tick+=async(_,_)=>await Heartbeat();}
 internal async Task Heartbeat(){if(heartbeatBusy||auditSession.Length==0)return;var generation=SessionVersion;heartbeatBusy=true;try{using var timeout=new CancellationTokenSource(TimeSpan.FromSeconds(5));using var response=await Client.PostAsJsonAsync("api/sessions/"+auditSession+"/heartbeat",new{},timeout.Token);if(response.StatusCode==System.Net.HttpStatusCode.Unauthorized&&generation==SessionVersion){ClearSession();SessionExpired?.Invoke();}}catch{}finally{heartbeatBusy=false;}}
 public async Task Logout(){heartbeat.Stop();if(auditSession.Length==0)return;try{using var timeout=new CancellationTokenSource(TimeSpan.FromSeconds(5));using var request=new HttpRequestMessage(HttpMethod.Post,"api/sessions/"+auditSession+"/end"){Content=JsonContent.Create(new{})};request.Headers.Add("Idempotency-Key",Guid.NewGuid().ToString());using var response=await Client.SendAsync(request,timeout.Token);}catch{}finally{ClearSession();}}
 public HttpClient Client {get;private set;}=new(){Timeout=TimeSpan.FromSeconds(25)};
 public string WorkstationId {get;private set;}=Environment.MachineName;
 public string[] Roles {get;private set;}=[];
 public string Endpoint {get;private set;}="";
 public async Task Login(string address,string email,string password){
  ClearSession();
  var uri=new Uri(address.TrimEnd('/')+"/");if(uri.Scheme!="https"&&!(uri.Scheme=="http"&&uri.IsLoopback))throw new InvalidOperationException("Use a trusted HTTPS hospital server address.");
  var paired=ConnectionSettingsStore.Default.Load();
  if(paired!=null&&!paired.MatchesAddress(address))throw new InvalidOperationException("This address differs from your saved server. Use Find server or open its connection file before signing in.");
  Client.Dispose();Client=new HttpClient(paired?.CreateHandler()??new HttpClientHandler{AllowAutoRedirect=false}){BaseAddress=uri,Timeout=TimeSpan.FromSeconds(25)};Client.DefaultRequestHeaders.Add("X-Workstation",Environment.MachineName);
  using var response=await Client.PostAsJsonAsync("auth/login",new{email,password});await Ensure(response);using var token=JsonDocument.Parse(await response.Content.ReadAsStringAsync());Client.DefaultRequestHeaders.Authorization=new AuthenticationHeaderValue("Bearer",token.RootElement.GetProperty("accessToken").GetString());
  var workstation=await Post("workstations",new{name=Environment.MachineName});WorkstationId=workstation.GetProperty("Id").GetString()!;Client.DefaultRequestHeaders.Remove("X-Workstation");Client.DefaultRequestHeaders.Add("X-Workstation",WorkstationId);
  var me=await Get("me");UserId=me.TryGetProperty("id",out var accountId)?accountId.GetString()??"":"";ApplyIdentity(me.GetProperty("user").GetString()??email,me.GetProperty("roles").EnumerateArray().Select(x=>x.GetString()!));Footer=$"{email}  ·  {string.Join(", ",Roles)}  ·  {Environment.MachineName}  ·  Asia/Manila";Status="Connected. All transactions require explicit confirmation.";
  var session=await Post("sessions/start",new{});auditSession=session.GetProperty("Id").GetString()!;Client.DefaultRequestHeaders.Add("X-Audit-Session",auditSession);heartbeat.Start();
  var settings=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"DDHI","client.json");Directory.CreateDirectory(Path.GetDirectoryName(settings)!);File.WriteAllText(settings,JsonSerializer.Serialize(new{ApiAddress=address}));
 }
 public static string SavedAddress(){try{using var d=JsonDocument.Parse(File.ReadAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"DDHI","client.json")));return d.RootElement.GetProperty("ApiAddress").GetString()??"";}catch{return "";}}
 public async Task<JsonElement> Get(string endpoint){var generation=SessionVersion;using var r=await Client.GetAsync("api/"+endpoint);await Ensure(r);if(generation!=SessionVersion)throw new InvalidOperationException("The signed-in account changed; reload this page.");return JsonSerializer.Deserialize<JsonElement>(await r.Content.ReadAsStringAsync());}
 readonly Dictionary<string,string> pendingKeys=[];
 public async Task<JsonElement> Post(string endpoint,object input){var generation=SessionVersion;var fingerprint=endpoint+JsonSerializer.Serialize(input);if(!pendingKeys.TryGetValue(fingerprint,out var key)){key=Guid.NewGuid().ToString();pendingKeys[fingerprint]=key;}using var request=new HttpRequestMessage(HttpMethod.Post,"api/"+endpoint){Content=JsonContent.Create(input)};request.Headers.Add("Idempotency-Key",key);using var r=await Client.SendAsync(request);await Ensure(r);if(generation!=SessionVersion)throw new InvalidOperationException("The signed-in account changed; reload this page.");pendingKeys.Remove(fingerprint);return JsonSerializer.Deserialize<JsonElement>(await r.Content.ReadAsStringAsync());}
 async Task Ensure(HttpResponseMessage r){if(r.StatusCode==System.Net.HttpStatusCode.Unauthorized){ClearSession();throw new StaffSessionEndedException("Your session ended or the account changed. Sign in again to refresh your permissions.");}if(!r.IsSuccessStatusCode){var detail=await r.Content.ReadAsStringAsync();try{using var error=JsonDocument.Parse(detail);if(error.RootElement.TryGetProperty("error",out var message)&&message.ValueKind==JsonValueKind.String)detail=message.GetString()??detail;}catch(JsonException){}throw new InvalidOperationException(r.StatusCode==System.Net.HttpStatusCode.Unauthorized?"Session expired or sign-in failed. Sign in again.":$"Server rejected operation ({(int)r.StatusCode}): {detail}");}}
 public async Task Load(string endpoint){Endpoint=endpoint;var json=await Get(endpoint);SetRows(json);Status=$"Refreshed {DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(8)):HH:mm:ss} Asia/Manila. Values are from the central server.";}
 public void SetRows(JsonElement json){var table=new DataTable();var list=json.ValueKind==JsonValueKind.Array?json.EnumerateArray().ToArray():new[]{json};foreach(var item in list)foreach(var p in item.EnumerateObject())if(!table.Columns.Contains(p.Name))table.Columns.Add(p.Name);foreach(var item in list){var row=table.NewRow();foreach(var p in item.EnumerateObject())row[p.Name]=DisplayValue(p.Value);table.Rows.Add(row);}Rows=table.DefaultView;}
 static string DisplayValue(JsonElement value){if(value.ValueKind==JsonValueKind.Array&&value.EnumerateArray().All(v=>v.ValueKind==JsonValueKind.String))return string.Join(", ",value.EnumerateArray().Select(v=>v.GetString()));if(value.ValueKind==JsonValueKind.String&&value.GetString() is string text&&text.Contains('T')&&DateTimeOffset.TryParse(text,System.Globalization.CultureInfo.InvariantCulture,System.Globalization.DateTimeStyles.RoundtripKind,out var time))return time.ToOffset(TimeSpan.FromHours(8)).ToString("yyyy-MM-dd HH:mm:ss zzz",System.Globalization.CultureInfo.InvariantCulture);return value.ToString();}
}
