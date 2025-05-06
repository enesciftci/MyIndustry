namespace MyIndustry.Domain.Provider;

public interface ISecurityProvider
{
    string HMacSha512(string data, string secretKey);
    string EncryptAes256(string value);
    string DecryptAes256(string value);
}