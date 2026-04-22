
using System;
using System.Data;
using Npgsql;
using WarehouseManagementSystem.Helpers;
using WarehouseManagementSystem.Models;

namespace WarehouseManagementSystem.Services
{
    public class AuthService
    {
        public User Login(string email, string password)
        {
            string sql = "SELECT Id, FullName, Email, Role, PasswordHash FROM Users WHERE Email = @Email";
            var result = DatabaseHelper.ExecuteQuery(sql, new[] { new NpgsqlParameter("@Email", email) });

            if (result.Rows.Count == 0)
                throw new Exception("Пользователь не найден");

            var userData = result.Rows[0];
            string storedHash = userData["PasswordHash"].ToString();

            if (!PasswordHasher.VerifyPassword(password, storedHash))
                throw new Exception("Неверный пароль");

            string roleString = userData["Role"].ToString();
            UserRole role = roleString == "Admin" ? UserRole.Admin : UserRole.Storekeeper;

            return new User
            {
                Id = Convert.ToInt32(userData["Id"]),
                FullName = userData["FullName"].ToString(),
                Email = userData["Email"].ToString(),
                Role = role
            };
        }

        public User RegisterStorekeeper(string fullName, string email, string password)
        {
            string checkSql = "SELECT COUNT(*) FROM Users WHERE Email = @Email";
            int existingCount = Convert.ToInt32(DatabaseHelper.ExecuteScalar(checkSql, new[] { new NpgsqlParameter("@Email", email) }));

            if (existingCount > 0)
                throw new Exception("Пользователь с таким email уже существует");

            string hashedPassword = PasswordHasher.HashPassword(password);

            string insertSql = @"INSERT INTO Users (FullName, Email, PasswordHash, Role) 
                                VALUES (@FullName, @Email, @PasswordHash, 'Storekeeper') RETURNING Id";

            var insertParams = new[]
            {
                new NpgsqlParameter("@FullName", fullName),
                new NpgsqlParameter("@Email", email),
                new NpgsqlParameter("@PasswordHash", hashedPassword)
            };

            int newUserId = Convert.ToInt32(DatabaseHelper.ExecuteScalar(insertSql, insertParams));

            return new User
            {
                Id = newUserId,
                FullName = fullName,
                Email = email,
                Role = UserRole.Storekeeper
            };
        }

        public bool IsEmailExists(string email)
        {
            string sql = "SELECT COUNT(*) FROM Users WHERE Email = @Email";
            int count = Convert.ToInt32(DatabaseHelper.ExecuteScalar(sql, new[] { new NpgsqlParameter("@Email", email) }));
            return count > 0;
        }
    }
}

