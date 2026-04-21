using Microsoft.VisualStudio.TestTools.UnitTesting;
using WarehouseManagementSystem.Helpers;
using WarehouseManagementSystem.Services;

namespace WarehouseManagementSystem2.Tests
{
    [TestClass]
    public class PasswordHasherTests
    {
        [TestMethod]
        public void HashPassword_ShouldReturnSameHash_ForSamePassword()
        {
            // Arrange
            string password = "admin123";

            // Act
            string hash1 = PasswordHasher.HashPassword(password);
            string hash2 = PasswordHasher.HashPassword(password);

            // Assert
            Assert.AreEqual(hash1, hash2, "Хеши одного пароля должны совпадать");
        }

        [TestMethod]
        public void HashPassword_ShouldReturnDifferentHash_ForDifferentPasswords()
        {
            // Arrange
            string password1 = "admin123";
            string password2 = "pass123";

            // Act
            string hash1 = PasswordHasher.HashPassword(password1);
            string hash2 = PasswordHasher.HashPassword(password2);

            // Assert
            Assert.AreNotEqual(hash1, hash2, "Хеши разных паролей должны отличаться");
        }

        [TestMethod]
        public void VerifyPassword_ShouldReturnTrue_ForCorrectPassword()
        {
            // Arrange
            string password = "admin123";
            string hash = PasswordHasher.HashPassword(password);

            // Act
            bool result = PasswordHasher.VerifyPassword(password, hash);

            // Assert
            Assert.IsTrue(result, "Правильный пароль должен проходить проверку");
        }

        [TestMethod]
        public void VerifyPassword_ShouldReturnFalse_ForIncorrectPassword()
        {
            // Arrange
            string correctPassword = "admin123";
            string wrongPassword = "wrong123";
            string hash = PasswordHasher.HashPassword(correctPassword);

            // Act
            bool result = PasswordHasher.VerifyPassword(wrongPassword, hash);

            // Assert
            Assert.IsFalse(result, "Неверный пароль не должен проходить проверку");
        }
    }
}