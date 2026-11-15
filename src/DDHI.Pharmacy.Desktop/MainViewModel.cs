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
 string title="Dashboard",status="Connect to the central server to continue.",footer="";DataView? rows;
 public string Title{get=>title;set{title=value;Changed();}} public string Status{get=>status;set{status=value;Changed();}}
 public string Footer{get=>footer;set{footer=value;Changed();}} public DataView? Rows{get=>rows;set{rows=value;Changed();}}
 void Changed([CallerMemberName]string? name=null)=>PropertyChanged?.Invoke(this,new(name));
 public string[] Pages {get;}=["Dashboard","Scan Medicine","Medicine Master","Barcode Registration","Inventory Receiving","Inventory Management","Medication Dispensing","Stock Transfers","Returns and Adjustments","Expiration Monitoring","Medicine Recalls","Cerebro Synchronization","Reports","Scanner Settings","Workstation Management","User Administration","Audit Logs","System Settings","About Application"];
 public HttpClient Client {get;private set;}=new(){Timeout=TimeSpan.FromSeconds(25)};
 public string WorkstationId {get;private set;}=Environment.MachineName;
 public string[] Roles {get;private set;}=[];
 public string Endpoint {get;private set;}="";
 public async Task Login(string address,string email,string password){
  var uri=new Uri(address.TrimEnd('/')+"/");if(uri.Scheme!="https"&&!(uri.Scheme=="http"&&uri.IsLoopback))throw new InvalidOperationException("Use a trusted HTTPS hospital server address.");
  var paired=ConnectionSettingsStore.Default.Load();
  if(paired!=null&&!paired.MatchesAddress(address))throw new InvalidOperationException("This address differs from your saved server. Use Find server or open its connection file before signing in.");
  Client.Dispose();Client=new HttpClient(paired?.CreateHandler()??new HttpClientHandler{AllowAutoRedirect=false}){BaseAddress=uri,Timeout=TimeSpan.FromSeconds(25)};Client.DefaultRequestHeaders.Add("X-Workstation",Environment.MachineName);
  using var response=await Client.PostAsJsonAsync("auth/login",new{email,password});await Ensure(response);using var token=JsonDocument.Parse(await response.Content.ReadAsStringAsync());Client.DefaultRequestHeaders.Authorization=new AuthenticationHeaderValue("Bearer",token.RootElement.GetProperty("accessToken").GetString());
  var workstation=await Post("workstations",new{name=Environment.MachineName});WorkstationId=workstation.GetProperty("Id").GetString()!;Client.DefaultRequestHeaders.Remove("X-Workstation");Client.DefaultRequestHeaders.Add("X-Workstation",WorkstationId);
  var me=await Get("me");Roles=me.GetProperty("roles").EnumerateArray().Select(x=>x.GetString()!).ToArray();Footer=$"{email}  ·  {string.Join(", ",Roles)}  ·  {Environment.MachineName}  ·  Asia/Manila";Status="Connected. All transactions require explicit confirmation.";
  var settings=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"DDHI","client.json");Directory.CreateDirectory(Path.GetDirectoryName(settings)!);File.WriteAllText(settings,JsonSerializer.Serialize(new{ApiAddress=address}));
 }
 public static string SavedAddress(){try{using var d=JsonDocument.Parse(File.ReadAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"DDHI","client.json")));return d.RootElement.GetProperty("ApiAddress").GetString()??"";}catch{return "";}}
 public async Task<JsonElement> Get(string endpoint){using var r=await Client.GetAsync("api/"+endpoint);await Ensure(r);return JsonSerializer.Deserialize<JsonElement>(await r.Content.ReadAsStringAsync());}
 readonly Dictionary<string,string> pendingKeys=[];
 public async Task<JsonElement> Post(string endpoint,object input){var fingerprint=endpoint+JsonSerializer.Serialize(input);if(!pendingKeys.TryGetValue(fingerprint,out var key)){key=Guid.NewGuid().ToString();pendingKeys[fingerprint]=key;}using var request=new HttpRequestMessage(HttpMethod.Post,"api/"+endpoint){Content=JsonContent.Create(input)};request.Headers.Add("Idempotency-Key",key);using var r=await Client.SendAsync(request);await Ensure(r);pendingKeys.Remove(fingerprint);return JsonSerializer.Deserialize<JsonElement>(await r.Content.ReadAsStringAsync());}
 static async Task Ensure(HttpResponseMessage r){if(!r.IsSuccessStatusCode){var detail=await r.Content.ReadAsStringAsync();throw new InvalidOperationException(r.StatusCode==System.Net.HttpStatusCode.Unauthorized?"Session expired or sign-in failed. Sign in again.":$"Server rejected operation ({(int)r.StatusCode}): {detail}");}}
 public async Task Load(string endpoint){Endpoint=endpoint;var json=await Get(endpoint);SetRows(json);Status=$"Refreshed {DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(8)):HH:mm:ss} Asia/Manila. Values are from the central server.";}
 public void SetRows(JsonElement json){var table=new DataTable();var list=json.ValueKind==JsonValueKind.Array?json.EnumerateArray().ToArray():new[]{json};foreach(var item in list)foreach(var p in item.EnumerateObject())if(!table.Columns.Contains(p.Name))table.Columns.Add(p.Name);foreach(var item in list){var row=table.NewRow();foreach(var p in item.EnumerateObject())row[p.Name]=DisplayValue(p.Value);table.Rows.Add(row);}Rows=table.DefaultView;}
 static string DisplayValue(JsonElement value){if(value.ValueKind==JsonValueKind.String&&value.GetString() is string text&&text.Contains('T')&&DateTimeOffset.TryParse(text,System.Globalization.CultureInfo.InvariantCulture,System.Globalization.DateTimeStyles.RoundtripKind,out var time))return time.ToOffset(TimeSpan.FromHours(8)).ToString("yyyy-MM-dd HH:mm:ss zzz",System.Globalization.CultureInfo.InvariantCulture);return value.ToString();}
}
