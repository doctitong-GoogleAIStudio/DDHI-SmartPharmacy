using System.IO.Compression;
using System.Reflection;
using System.Text.Json;
using System.Xml.Linq;
using DDHI.Pharmacy.Application;
using DDHI.Pharmacy.Contracts;
using DDHI.Pharmacy.Integration;
using Xunit;

public class ClinicalImportTests
{
    [Fact] public void ClinicalCsvPreservesStructuredRecordsAndRemainsUnapproved()
    {
        var p=ClinicalFixtures.Package();List<string[]> rows=[["Kind","Json"],["Manifest",JsonSerializer.Serialize(new{p.Schema,p.Version,p.LicenseStatement})]];
        foreach(var table in ClinicalImportFormats.Tables){var records=(Array)typeof(ClinicalPackage).GetProperty(table.Key)!.GetValue(p)!;foreach(var record in records)rows.Add([table.Key,JsonSerializer.Serialize(record)]);}
        var path=Path.Combine(Path.GetTempPath(),"clinical-"+Guid.NewGuid()+".csv");try{File.WriteAllText(path,CsvAdapter.Export(rows));var parsed=ClinicalPackageValidation.Parse(ClinicalImportFormats.Read(path));Assert.Equal(p.Version,parsed.Version);Assert.Equal(p.Rules[0].OriginalSeverity,parsed.Rules[0].OriginalSeverity);Assert.Equal(2,parsed.Ingredients.Length);}finally{File.Delete(path);}
    }
    static void Add(ZipArchive zip,string name,string text){using var writer=new StreamWriter(zip.CreateEntry(name).Open());writer.Write(text);}
    static string Sheet(string[][] rows)=>new XElement("worksheet",new XElement("sheetData",rows.Select((row,i)=>new XElement("row",new XAttribute("r",i+1),row.Select((value,j)=>new XElement("c",new XAttribute("r",((char)('A'+j)).ToString()+(i+1)),new XAttribute("t","inlineStr"),new XElement("is",new XElement("t",value)))))))).ToString();
    [Fact] public void XlsxTemplateUsesLiteralIsoValuesAndDoesNotExecuteFormulas()
    {
        var path=Path.Combine(Path.GetTempPath(),"clinical-"+Guid.NewGuid()+".xlsx");try{
            using(var zip=ZipFile.Open(path,ZipArchiveMode.Create)){
                XNamespace relation="http://schemas.openxmlformats.org/officeDocument/2006/relationships";
                var names=new[]{"Manifest"}.Concat(ClinicalImportFormats.Tables.Keys).ToArray();Add(zip,"xl/workbook.xml",new XElement("workbook",new XAttribute(XNamespace.Xmlns+"r",relation),new XElement("sheets",names.Select((name,i)=>new XElement("sheet",new XAttribute("name",name),new XAttribute(relation+"id","r"+i))))).ToString());
                Add(zip,"xl/_rels/workbook.xml.rels",new XElement("Relationships",names.Select((_,i)=>new XElement("Relationship",new XAttribute("Id","r"+i),new XAttribute("Target","worksheets/sheet"+i+".xml")))).ToString());
                for(int i=0;i<names.Length;i++){var rows=i==0?new[]{new[]{"Schema","Version","LicenseStatement"},new[]{"1","empty-template","Operator must obtain actual content permissions."}}:new[]{ClinicalImportFormats.Tables[names[i]].GetProperties(BindingFlags.Instance|BindingFlags.Public).Select(x=>x.Name).ToArray()};Add(zip,"xl/worksheets/sheet"+i+".xml",Sheet(rows));}
            }
            Assert.Empty(ClinicalPackageValidation.Parse(ClinicalImportFormats.Read(path)).Drugs);
            using(var zip=ZipFile.Open(path,ZipArchiveMode.Update)){zip.GetEntry("xl/worksheets/sheet0.xml")!.Delete();Add(zip,"xl/worksheets/sheet0.xml","<worksheet><sheetData><row><c r='A1'><f>1+1</f><v>2</v></c></row></sheetData></worksheet>");}
            Assert.Throws<InvalidOperationException>(()=>ClinicalImportFormats.Read(path));
        }finally{File.Delete(path);}
    }
    [Fact] public void DuplicateJsonPropertiesAreRejectedInsteadOfLastValueWinning(){var json=JsonSerializer.Serialize(ClinicalFixtures.Package());json=json.Replace("\"Schema\":1","\"Schema\":1,\"Schema\":1");Assert.Throws<InvalidOperationException>(()=>ClinicalPackageValidation.Parse(json));}
}
