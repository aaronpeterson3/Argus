using Xunit;
using Argus.Core.Infrastructure;

namespace Argus.Core.Tests.Infrastructure
{
    public class PasswordHasherTests
    {
        [Fact]
        public void HashPassword_WithSameSalt_ProducesSameHash()
        {
            // Arrange
            var password = "TestPassword123!";
            var salt = new byte[16] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };

            // Act
            var hash1 = PasswordHasher.HashPassword(password, salt);
            var hash2 = PasswordHasher.HashPassword(password, salt);

            // Assert
            Assert.Equal(hash1, hash2);
        }

        [Fact]
        public void HashPassword_WithDifferentPasswords_ProducesDifferentHashes()
        {
            // Arrange
            var password1 = "TestPassword123!";
            var password2 = "DifferentPassword123!";
            var salt = new byte[16] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };

            // Act
            var hash1 = PasswordHasher.HashPassword(password1, salt);
            var hash2 = PasswordHasher.HashPassword(password2, salt);

            // Assert
            Assert.NotEqual(hash1, hash2);
        }
    }
}