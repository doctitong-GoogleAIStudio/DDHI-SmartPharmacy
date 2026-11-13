using System.Security.Cryptography;
using System.Text;
namespace DDHI.Pharmacy.Tools;
public static class BackupCrypt {
 const int Block=1048576;static readonly byte[] Magic=Encoding.ASCII.GetBytes("DDHIBAK1");
 static byte[] Key(string password,byte[] salt){if(password.Length<16)throw new InvalidOperationException("Backup passphrase requires at least 16 characters.");return Rfc2898DeriveBytes.Pbkdf2(password,salt,600000,HashAlgorithmName.SHA256,32);}
 public static void Encrypt(Stream input,Stream output,string password){var salt=RandomNumberGenerator.GetBytes(32);output.Write(Magic);output.Write(salt);var key=Key(password,salt);try{using var aes=new AesGcm(key,16);using var w=new BinaryWriter(output,Encoding.UTF8,true);var plain=new byte[Block];long index=0;
  while(true){int n=0;while(n<Block){var read=input.Read(plain,n,Block-n);if(read==0)break;n+=read;}var nonce=RandomNumberGenerator.GetBytes(12);var cipher=new byte[n];var tag=new byte[16];var aad=Encoding.ASCII.GetBytes($"DDHIBAK1|{Convert.ToHexString(salt)}|{index++}|{n}");aes.Encrypt(nonce,plain.AsSpan(0,n),cipher,tag,aad);w.Write(n);w.Write(nonce);w.Write(cipher);w.Write(tag);if(n==0)break;}
 }finally{CryptographicOperations.ZeroMemory(key);}}
 public static void Decrypt(Stream input,Stream output,string password){var magic=new byte[8];input.ReadExactly(magic);if(!magic.SequenceEqual(Magic))throw new InvalidDataException("Unknown backup format.");var salt=new byte[32];input.ReadExactly(salt);var key=Key(password,salt);try{using var aes=new AesGcm(key,16);using var r=new BinaryReader(input,Encoding.UTF8,true);long index=0;
  while(true){var n=r.ReadInt32();if(n<0||n>Block)throw new InvalidDataException("Invalid backup chunk.");var nonce=r.ReadBytes(12);var cipher=r.ReadBytes(n);var tag=r.ReadBytes(16);if(nonce.Length!=12||cipher.Length!=n||tag.Length!=16)throw new EndOfStreamException();var plain=new byte[n];var aad=Encoding.ASCII.GetBytes($"DDHIBAK1|{Convert.ToHexString(salt)}|{index++}|{n}");aes.Decrypt(nonce,cipher,tag,plain,aad);if(n==0){if(input.ReadByte()!=-1)throw new InvalidDataException("Trailing backup data.");break;}output.Write(plain);}
 }finally{CryptographicOperations.ZeroMemory(key);}}
}
