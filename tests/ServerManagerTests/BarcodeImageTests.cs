using DDHI.Pharmacy.Desktop;
using DDHI.Pharmacy.Barcode;
using Xunit;
public class BarcodeImageTests
{
    [Fact]public void Gs1Code128ImageMetadataIsNotMistakenForOrdinaryCode128()
    {
        var decoded=new ZXing.Result("01095060001343521727123110LOT",[],[],ZXing.BarcodeFormat.CODE_128);
        decoded.putMetadata(ZXing.ResultMetadataType.SYMBOLOGY_IDENTIFIER,"]C1");
        var input=BarcodeImageIdentity.Input(decoded);Assert.StartsWith("]C1",input);Assert.Equal("09506000134352",BarcodeParser.Parse(input).Gtin);
    }
    [Fact]public void PlainCode128ImageAndInternalLabelsRemainDistinct()
    {
        var decoded=new ZXing.Result("QaPackABC",[],[],ZXing.BarcodeFormat.CODE_128);Assert.Equal("]C0QaPackABC",BarcodeImageIdentity.Input(decoded));
        var stock=new ZXing.Result("DDHI-STOCK-"+Guid.NewGuid().ToString("N").ToUpperInvariant(),[],[],ZXing.BarcodeFormat.CODE_128);Assert.Equal(stock.Text,BarcodeImageIdentity.Input(stock));
    }
}
