namespace DDHI.Pharmacy.Desktop;
public static class BarcodeImageIdentity
{
    public static string Input(ZXing.Result decoded)
    {
        var text=decoded.Text;
        if(text.StartsWith(']')||text.StartsWith('\u001d')||text.StartsWith('(')||text.StartsWith("DDHI-",StringComparison.Ordinal))return text;
        var identifier=decoded.ResultMetadata!=null&&decoded.ResultMetadata.TryGetValue(ZXing.ResultMetadataType.SYMBOLOGY_IDENTIFIER,out var value)?value?.ToString():null;
        if(identifier is "]C1" or "]d2")return identifier+text;
        return decoded.BarcodeFormat==ZXing.BarcodeFormat.CODE_128?"]C0"+text:text;
    }
}
