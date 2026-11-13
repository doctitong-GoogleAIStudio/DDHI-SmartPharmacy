using System.Globalization;
using System.Text.RegularExpressions;
namespace DDHI.Pharmacy.Barcode;
public record BarcodeData(string Raw,string Symbology,string? Gtin,string? Lot,DateOnly? Expiration,string? Serial);
public static partial class BarcodeParser {
 public static bool ValidGtin(string value) {
  if(value.Length is not (8 or 12 or 13 or 14) || value.Any(c=>c<'0'||c>'9')) return false;
  var sum=0;var weight=3;for(var i=value.Length-2;i>=0;i--){sum+=(value[i]-'0')*weight;weight=4-weight;}
  return (10-sum%10)%10==value[^1]-'0';
 }
 public static BarcodeData Parse(string input) {
  if(string.IsNullOrWhiteSpace(input)||input.Length>512) throw new FormatException("Empty or oversized barcode.");
  var raw=input.TrimEnd('\r','\n');var s=raw;var sym="Code128 / unspecified";
  if(s.StartsWith("]d2")||s.StartsWith("]C1")){sym=s.StartsWith("]d2")?"GS1 DataMatrix":"GS1-128";s=s[3..];}
  if(s.StartsWith('\u001d')){sym="GS1 (FNC1)";s=s.TrimStart('\u001d');}
  if(s.Any(c=>char.IsControl(c)&&c!='\u001d'))throw new FormatException("Unexpected control character.");
  if(s.StartsWith("]C0")){var code=s[3..];if(code.Length<1||code.Length>80||code.Any(c=>c<32||c>126))throw new FormatException("Invalid Code 128 data.");return new(code,"Code 128",null,null,null,null);}
  if(s.StartsWith("DDHI-")){if(!Regex.IsMatch(s,"^DDHI-[A-Z0-9-]{1,64}$"))throw new FormatException("Invalid internal barcode.");return new(raw,"DDHI internal",null,null,null,null);}
  if(s.All(char.IsAsciiDigit)&&s.Length is 8 or 12 or 13 or 14) {
   if(!ValidGtin(s))throw new FormatException("Invalid GTIN check digit.");
   return new(raw,s.Length==8?"EAN-8":s.Length==12?"UPC-A":"EAN / GTIN",s.PadLeft(14,'0'),null,null,null);
  }
  var fields=new Dictionary<string,string>();
  if(s.StartsWith('(')) {
   var matches=Regex.Matches(s,@"\((01|10|17|21)\)([^()]*)");
   if(string.Concat(matches.Select(m=>m.Value))!=s)throw new FormatException("Unsupported or malformed GS1 application identifier.");
   foreach(Match m in matches)if(!fields.TryAdd(m.Groups[1].Value,m.Groups[2].Value))throw new FormatException("Duplicate GS1 field.");
   sym="GS1 human readable";
  } else if(sym.StartsWith("GS1")||s.StartsWith("01")&&s.Length>=16) {
   s=s.TrimStart('\u001d');int pos=0;
   while(pos<s.Length){if(s[pos]=='\u001d'){pos++;continue;}if(pos+2>s.Length)throw new FormatException("Truncated AI.");var ai=s.Substring(pos,2);pos+=2;
    int length=ai switch {"01"=>14,"17"=>6,"10" or "21"=>-1,_=>throw new FormatException("Unsupported GS1 AI.")};string value;
    if(length>0){if(pos+length>s.Length)throw new FormatException("Truncated GS1 field.");value=s.Substring(pos,length);pos+=length;}
    else {var end=s.IndexOf('\u001d',pos);if(end<0)end=s.Length;value=s[pos..end];pos=end;}
    if(!fields.TryAdd(ai,value))throw new FormatException("Duplicate GS1 field.");
   }
  } else throw new FormatException("Unregistered non-GS1 barcode. Use an approved DDHI internal namespace.");
  if(!fields.TryGetValue("01",out var gtin)||gtin.Length!=14||!ValidGtin(gtin))throw new FormatException("GS1 requires valid AI 01 GTIN-14.");
  foreach(var ai in new[]{"10","21"})if(fields.TryGetValue(ai,out var value)&&(value.Length<1||value.Length>20||value.Any(c=>c<32||c>126)))throw new FormatException("Invalid variable-length GS1 field.");
  DateOnly? expiry=null;
  if(fields.TryGetValue("17",out var date)) {
   if(date.Length!=6||!date.All(char.IsAsciiDigit))throw new FormatException("Invalid GS1 expiry.");
   try{var year=2000+int.Parse(date[..2],CultureInfo.InvariantCulture);var month=int.Parse(date[2..4],CultureInfo.InvariantCulture);var day=int.Parse(date[4..],CultureInfo.InvariantCulture);
    if(day==0)throw new FormatException("Month-only expiration requires approved pharmacy policy; enter an approved explicit date.");expiry=new DateOnly(year,month,day);
   }catch(ArgumentOutOfRangeException){throw new FormatException("Invalid GS1 expiration date.");}
  }
  return new(raw,sym,gtin,fields.GetValueOrDefault("10"),expiry,fields.GetValueOrDefault("21"));
 }
}
