using System.Security.Cryptography;
using System.Text;

namespace MyIndustry.Domain.Provider;

public class SecurityProvider : ISecurityProvider
{
    public string HMacSha512(string data, string secretKey)
    {
        var bytes = Encoding.UTF8.GetBytes(data);
        return new HMACSHA512(Encoding.UTF8.GetBytes(secretKey)).ComputeHash(bytes).Aggregate(string.Empty, (Func<string, byte, string>) ((current, x) => current +
            $"{(object) x:x2}"));
    }

    public string EncryptAes256(string value)
    {
        var cryptoServiceProvider = Aes.Create();
        cryptoServiceProvider.BlockSize = 128;
        cryptoServiceProvider.KeySize = 256;
        cryptoServiceProvider.IV = "!QXZ2BSX#EIC4PFV"u8.ToArray();
        cryptoServiceProvider.Key = "1CZB&YON7UJM(IK<5TGB&YHN7UJM(IK<"u8.ToArray();
        cryptoServiceProvider.Mode = CipherMode.CBC;
        cryptoServiceProvider.Padding = PaddingMode.PKCS7;
        var bytes = Encoding.Unicode.GetBytes(value);
        using var encryptor = cryptoServiceProvider.CreateEncryptor();
        return Convert.ToBase64String(encryptor.TransformFinalBlock(bytes, 0, bytes.Length));
    }

    public string DecryptAes256(string value)
    {
        var cryptoServiceProvider = Aes.Create();
        cryptoServiceProvider.BlockSize = 128;
        cryptoServiceProvider.KeySize = 256;
        cryptoServiceProvider.IV = "!QXZ2BSX#EIC4PFV"u8.ToArray();
        cryptoServiceProvider.Key = "1CZB&YON7UJM(IK<5TGB&YHN7UJM(IK<"u8.ToArray();
        cryptoServiceProvider.Mode = CipherMode.CBC;
        cryptoServiceProvider.Padding = PaddingMode.PKCS7;
        var inputBuffer = Convert.FromBase64String(value);
        using var decryptor = cryptoServiceProvider.CreateDecryptor();
        return Encoding.Unicode.GetString(decryptor.TransformFinalBlock(inputBuffer, 0, inputBuffer.Length));
    }   
}