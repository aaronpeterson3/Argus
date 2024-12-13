namespace Argus.Infrastructure.Encryption
{
    public interface IDataEncryption
    {
        string Encrypt(string plainText);
        string Decrypt(string cipherText);
    }
}