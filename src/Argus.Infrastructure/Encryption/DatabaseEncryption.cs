using Microsoft.AspNetCore.DataProtection;

namespace Argus.Infrastructure.Encryption
{
    public interface IDataEncryption
    {
        string Encrypt(string plainText);
        string Decrypt(string cipherText);
    }

    public class DatabaseEncryption : IDataEncryption
    {
        private readonly IDataProtectionProvider _dataProtection;
        private readonly string _purpose = "DatabaseEncryption";

        public DatabaseEncryption(IDataProtectionProvider dataProtection)
        {
            _dataProtection = dataProtection;
        }

        public string Encrypt(string plainText)
        {
            var protector = _dataProtection.CreateProtector(_purpose);
            return protector.Protect(plainText);
        }

        public string Decrypt(string cipherText)
        {
            var protector = _dataProtection.CreateProtector(_purpose);
            return protector.Unprotect(cipherText);
        }
    }
}