using System;
using System.Security.Cryptography;
using System.Text;

namespace WarehouseManagementSystem.Services
{
    public static class PasswordHasher
    {
        public static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }

        public static bool VerifyPassword(string password, string hash)
        {
            string computedHash = HashPassword(password);
            return computedHash == hash;
        }
    }
}