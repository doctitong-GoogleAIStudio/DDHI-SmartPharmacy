using System.Text.RegularExpressions;
namespace DDHI.Pharmacy.Domain;
public sealed record GenericDefinition(string Name,string Strength,string Form,string Route,string Unit);
public static class MedicineGrouping
{
    public static string Normalize(string? value)=>Regex.Replace(value?.Trim()??"",@"\s+"," ").ToUpperInvariant();
    public static string ExactNotation(string? value)=>Regex.Replace(value?.Trim()??"",@"\s+"," ");
    public static string Brand(string? value)=>string.IsNullOrWhiteSpace(value)?"UNBRANDED":Normalize(value);
    public static GenericDefinition Definition(Medicine medicine)=>new(Normalize(medicine.GenericName),ExactNotation(medicine.Strength),Normalize(medicine.DosageForm),Normalize(medicine.Route),ExactNotation(medicine.BaseUnit));
}
