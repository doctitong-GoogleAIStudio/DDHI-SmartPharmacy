namespace DDHI.Pharmacy.Barcode;
public static class BarcodeIdentity
{
    public static string[] EquivalentCodes(BarcodeData data)
    {
        if(data.Gtin==null)return [data.Raw];
        var canonical=data.Gtin.PadLeft(14,'0');
        return new[]{8,12,13,14}.Where(n=>canonical[..(14-n)].All(c=>c=='0'))
            .Select(n=>canonical[(14-n)..]).Where(BarcodeParser.ValidGtin).Distinct(StringComparer.Ordinal).ToArray();
    }
    public static BarcodeData Registration(string raw,string format)
    {
        if(string.IsNullOrWhiteSpace(raw))throw new FormatException("Enter the package barcode.");
        if(format=="Code128")return BarcodeParser.Parse(raw.StartsWith("]C0",StringComparison.Ordinal)?raw:"]C0"+raw.TrimEnd('\r','\n'));
        if(format!="Auto")throw new FormatException("Select Auto or Code128 barcode format.");
        return BarcodeParser.Parse(raw);
    }
    public static bool PlainCode128Candidate(string raw) => raw.Length is >0 and <=80 && raw.Any(char.IsAsciiLetter) && raw.All(c=>c>=32&&c<=126)
        && !raw.StartsWith('(') && !raw.StartsWith(']') && !raw.StartsWith("DDHI-",StringComparison.Ordinal) && !raw.StartsWith("01",StringComparison.Ordinal);
}
