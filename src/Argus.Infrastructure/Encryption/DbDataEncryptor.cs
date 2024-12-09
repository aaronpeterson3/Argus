using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;

namespace Argus.Infrastructure.Encryption;

public class EncryptedStringConverter : ValueConverter<string, string>
{
    public EncryptedStringConverter(IDataEncryption encryption)
        : base(
            v => encryption.Encrypt(v),
            v => encryption.Decrypt(v))
    {
    }
}

public class EncryptedJsonConverter<T> : ValueConverter<T, string>
{
    public EncryptedJsonConverter(IDataEncryption encryption)
        : base(
            v => encryption.Encrypt(JsonSerializer.Serialize(v, (JsonSerializerOptions)null)),
            v => JsonSerializer.Deserialize<T>(encryption.Decrypt(v), (JsonSerializerOptions)null))
    {
    }
}