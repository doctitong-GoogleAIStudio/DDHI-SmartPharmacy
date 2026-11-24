using System.IO.Compression;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml;
using System.Xml.Linq;
using Microsoft.VisualBasic.FileIO;
namespace DDHI.Pharmacy.Contracts;

// Converts bounded local files to the same server-validated package. No formulas,
// macros, remote links, source URLs or code in an import are executed or fetched.
public static class ClinicalImportFormats
{
    public static readonly Dictionary<string,Type> Tables=new(StringComparer.OrdinalIgnoreCase){["Sources"]=typeof(ClinicalSource),["Ingredients"]=typeof(IngredientDefinition),["Drugs"]=typeof(ClinicalDrugDefinition),["Products"]=typeof(ProductDefinition),["Packages"]=typeof(PackageDefinition),["Monographs"]=typeof(MonographDefinition),["Rules"]=typeof(ClinicalRuleDefinition)};
    public static string Read(string path)
    {
        if(new FileInfo(path).Length>20000000)throw new InvalidOperationException("Clinical import exceeds 20 MB.");
        if(Path.GetExtension(path).Equals(".json",StringComparison.OrdinalIgnoreCase))return File.ReadAllText(path);
        if(Path.GetExtension(path).Equals(".csv",StringComparison.OrdinalIgnoreCase)){
            var rows=Csv(File.ReadAllText(path));if(rows.Length==0||!rows[0].SequenceEqual(new[]{"Kind","Json"}))throw new InvalidOperationException("Single CSV header must be Kind,Json.");
            var document=new JsonObject();foreach(var name in Tables.Keys)document[name]=new JsonArray();var manifest=false;
            foreach(var row in rows.Skip(1)){if(row.Length!=2)throw new InvalidOperationException("Invalid clinical CSV row.");if(row[0]=="Manifest"){if(manifest)throw new InvalidOperationException("Duplicate manifest.");var meta=JsonNode.Parse(row[1])!.AsObject();foreach(var item in meta)document[item.Key]=item.Value?.DeepClone();manifest=true;}else if(Tables.ContainsKey(row[0]))document[row[0]]!.AsArray().Add(JsonNode.Parse(row[1]));else throw new InvalidOperationException("Unknown clinical record type.");}
            if(!manifest)throw new InvalidOperationException("Clinical CSV needs a Manifest row.");return document.ToJsonString();
        }
        using var zip=ZipFile.OpenRead(path);if(zip.Entries.Count>128||zip.Entries.Sum(e=>e.Length)>20000000)throw new InvalidOperationException("Import archive exceeds limits.");
        if(zip.Entries.Any(e=>e.FullName.Contains("vbaProject",StringComparison.OrdinalIgnoreCase)||e.FullName.Contains("externalLinks",StringComparison.OrdinalIgnoreCase)))throw new InvalidOperationException("Macro/external-link workbooks are not accepted.");
        string Entry(string name){var entry=zip.GetEntry(name)??throw new InvalidOperationException("Missing import component: "+name);using var stream=entry.Open();using var reader=new StreamReader(stream);return reader.ReadToEnd();}
        if(Path.GetExtension(path).Equals(".zip",StringComparison.OrdinalIgnoreCase)){
            var manifest=JsonNode.Parse(Entry("manifest.json"))!.AsObject();foreach(var table in Tables){var rows=Csv(Entry(table.Key.ToLowerInvariant()+".csv"));manifest[table.Key]=ConvertTable(table.Value,rows);}return manifest.ToJsonString();
        }
        if(!Path.GetExtension(path).Equals(".xlsx",StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("Use a JSON package, clinical CSV, CSV bundle ZIP or .xlsx template.");
        static XDocument Xml(string text){using var input=new StringReader(text);using var reader=XmlReader.Create(input,new XmlReaderSettings{DtdProcessing=DtdProcessing.Prohibit,XmlResolver=null,MaxCharactersInDocument=20000000});return XDocument.Load(reader);}
        var shared=zip.GetEntry("xl/sharedStrings.xml")==null?Array.Empty<string>():Xml(Entry("xl/sharedStrings.xml")).Descendants().Where(x=>x.Name.LocalName=="si").Select(x=>string.Concat(x.Descendants().Where(n=>n.Name.LocalName=="t").Select(n=>n.Value))).ToArray();
        var relationships=Xml(Entry("xl/_rels/workbook.xml.rels")).Descendants().Where(x=>x.Name.LocalName=="Relationship").ToDictionary(x=>(string)x.Attribute("Id")!,x=>(string)x.Attribute("Target")!);
        var sheets=new Dictionary<string,string[][]>(StringComparer.OrdinalIgnoreCase);
        foreach(var sheet in Xml(Entry("xl/workbook.xml")).Descendants().Where(x=>x.Name.LocalName=="sheet")){
            var name=(string)sheet.Attribute("name")!;if(name!="Manifest"&&!Tables.ContainsKey(name))continue;
            var relation=sheet.Attributes().Single(a=>a.Name.LocalName=="id").Value;var target=relationships[relation].Replace('\\','/');if(target.Contains("..")||target.Contains(':'))throw new InvalidOperationException("Invalid worksheet target.");var full=target.StartsWith('/')?target.TrimStart('/'):"xl/"+target;
            var rows=new List<string[]>();foreach(var row in Xml(Entry(full)).Descendants().Where(x=>x.Name.LocalName=="row")){
                var cells=new SortedDictionary<int,string>();foreach(var cell in row.Elements().Where(x=>x.Name.LocalName=="c")){
                    if(cell.Elements().Any(x=>x.Name.LocalName=="f"))throw new InvalidOperationException("Formulas must be replaced by reviewed values before import.");var address=(string?)cell.Attribute("r")??throw new InvalidOperationException("Cell reference missing.");int column=0;foreach(var ch in address.TakeWhile(char.IsAsciiLetter))column=column*26+char.ToUpperInvariant(ch)-'A'+1;if(column is <1 or >64)throw new InvalidOperationException("Too many worksheet columns.");
                    var value=cell.Elements().FirstOrDefault(x=>x.Name.LocalName=="v")?.Value??string.Concat(cell.Descendants().Where(x=>x.Name.LocalName=="t").Select(x=>x.Value));if((string?)cell.Attribute("t")=="s"){if(!int.TryParse(value,out var n)||n<0||n>=shared.Length)throw new InvalidOperationException("Invalid shared text.");value=shared[n];}cells[column-1]=value;
                }
                if(cells.Count>0)rows.Add(Enumerable.Range(0,cells.Keys.Max()+1).Select(i=>cells.GetValueOrDefault(i,"")).ToArray());if(rows.Count>5001)throw new InvalidOperationException("Maximum 5,000 rows per worksheet.");
            }if(!sheets.TryAdd(name,rows.ToArray()))throw new InvalidOperationException("Duplicate worksheet.");
        }
        if(!sheets.TryGetValue("Manifest",out var metadata)||metadata.Length!=2||!metadata[0].SequenceEqual(new[]{"Schema","Version","LicenseStatement"})||metadata[1].Length!=3||metadata[1][0]!="1")throw new InvalidOperationException("Manifest needs Schema,Version,LicenseStatement and exactly one row (schema 1). Dates must be ISO text, never Excel serial numbers.");
        var package=new JsonObject{["Schema"]=1,["Version"]=metadata[1][1],["LicenseStatement"]=metadata[1][2]};foreach(var table in Tables){if(!sheets.TryGetValue(table.Key,out var rows))throw new InvalidOperationException("Missing sheet: "+table.Key);package[table.Key]=ConvertTable(table.Value,rows);}return package.ToJsonString();
    }
    public static string[][] Csv(string text){using var parser=new TextFieldParser(new StringReader(text));parser.SetDelimiters(",");parser.HasFieldsEnclosedInQuotes=true;var rows=new List<string[]>();while(!parser.EndOfData){rows.Add(parser.ReadFields()!);if(rows.Count>5001)throw new InvalidOperationException("Maximum 5,000 rows per CSV table.");}return rows.ToArray();}
    static JsonArray ConvertTable(Type type,string[][] rows)
    {
        if(rows.Length==0)throw new InvalidOperationException("CSV/workbook table needs a header even when empty.");var fields=type.GetProperties(BindingFlags.Instance|BindingFlags.Public).ToDictionary(x=>x.Name,StringComparer.OrdinalIgnoreCase);var header=rows[0];
        if(header.Distinct(StringComparer.OrdinalIgnoreCase).Count()!=header.Length||header.Any(x=>!fields.ContainsKey(x)))throw new InvalidOperationException("Unknown or duplicate table column.");var result=new JsonArray();
        foreach(var row in rows.Skip(1)){
            if(row.All(string.IsNullOrWhiteSpace))continue;if(row.Length>header.Length)throw new InvalidOperationException("Too many values in table row.");var item=new JsonObject();for(var i=0;i<header.Length;i++){
                var value=i<row.Length?row[i]:"";var property=fields[header[i]];var underlying=Nullable.GetUnderlyingType(property.PropertyType)??property.PropertyType;
                if(value==""&&Nullable.GetUnderlyingType(property.PropertyType)!=null)item[property.Name]=null;
                else if(underlying==typeof(string)||underlying==typeof(DateOnly))item[property.Name]=value;
                else if(underlying==typeof(bool)){if(!new[]{"true","false","1","0"}.Contains(value,StringComparer.OrdinalIgnoreCase))throw new InvalidOperationException("Boolean cells must be true, false, 1 or 0.");item[property.Name]=value.Equals("true",StringComparison.OrdinalIgnoreCase)||value=="1";}
                else item[property.Name]=JsonNode.Parse(value);
            }result.Add(item);
        }return result;
    }
}
