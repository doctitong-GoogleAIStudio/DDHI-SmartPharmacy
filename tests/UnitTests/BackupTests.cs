using DDHI.Pharmacy.Tools;
using Xunit;
using System.Security.Cryptography;
public class BackupTests {
 const string Password="Test-only-long-backup-passphrase!";
 [Theory][InlineData(0)][InlineData(100)][InlineData(1048576)][InlineData(1049000)]public void RoundTrip(int size){var data=RandomNumberGenerator.GetBytes(size);using var encrypted=new MemoryStream();BackupCrypt.Encrypt(new MemoryStream(data),encrypted,Password);encrypted.Position=0;using var plain=new MemoryStream();BackupCrypt.Decrypt(encrypted,plain,Password);Assert.Equal(data,plain.ToArray());}
 [Fact]public void TamperRejected(){using var encrypted=new MemoryStream();BackupCrypt.Encrypt(new MemoryStream(new byte[100]),encrypted,Password);var data=encrypted.ToArray();data[70]^=1;Assert.ThrowsAny<CryptographicException>(()=>BackupCrypt.Decrypt(new MemoryStream(data),Stream.Null,Password));}
 [Fact]public void TruncationRejected(){using var encrypted=new MemoryStream();BackupCrypt.Encrypt(new MemoryStream(new byte[100]),encrypted,Password);var data=encrypted.ToArray()[..^32];Assert.ThrowsAny<Exception>(()=>BackupCrypt.Decrypt(new MemoryStream(data),Stream.Null,Password));}
 [Fact]public void WrongPasswordRejected(){using var encrypted=new MemoryStream();BackupCrypt.Encrypt(new MemoryStream(new byte[100]),encrypted,Password);encrypted.Position=0;Assert.ThrowsAny<CryptographicException>(()=>BackupCrypt.Decrypt(encrypted,Stream.Null,"Wrong-but-long-password!"));}
}
