using System;
using System.Data;
using Npgsql;
using WarehouseManagementSystem.Services;
using WarehouseManagementSystem.Models;
using WarehouseManagementSystem.Helpers;

namespace WarehouseManagementSystem.Models
{
    public class AuthService
    {
        // Вход пользователя
        public User Login(string email, string password)
        {
            string query = "SELECT Id, FullName, Email, Role, PasswordHash FROM Users WHERE Email = @Email";
            var dt = DatabaseHelper.ExecuteQuery(query, new[] { new NpgsqlParameter("@Email", email) });

            if (dt.Rows.Count == 0)
                throw new Exception("Пользователь не найден");

            var row = dt.Rows[0];
            string passwordHash = row["PasswordHash"].ToString();

            if (!PasswordHasher.VerifyPassword(password, passwordHash))
                throw new Exception("Неверный пароль");

            return new User
            {
                Id = Convert.ToInt32(row["Id"]),
                FullName = row["FullName"].ToString(),
                Email = row["Email"].ToString(),
                Role = row["Role"].ToString()
            };
        }

        // Регистрация кладовщика
        public User RegisterStorekeeper(string fullName, string email, string password)
        {
            // Проверка уникальности email
            string checkQuery = "SELECT COUNT(*) FROM Users WHERE Email = @Email";
            int count = Convert.ToInt32(DatabaseHelper.ExecuteScalar(checkQuery, new[] { new NpgsqlParameter("@Email", email) }));

            if (count > 0)
                throw new Exception("Пользователь с таким email уже существует");

            // Хешируем пароль
            string hashedPassword = PasswordHasher.HashPassword(password);

            // Добавляем пользователя
            string insertQuery = @"INSERT INTO Users (FullName, Email, PasswordHash, Role) 
                                  VALUES (@FullName, @Email, @PasswordHash, 'Storekeeper') RETURNING Id";
            var parameters = new[]
            {
                new NpgsqlParameter("@FullName", fullName),
                new NpgsqlParameter("@Email", email),
                new NpgsqlParameter("@PasswordHash", hashedPassword)
            };

            int newId = Convert.ToInt32(DatabaseHelper.ExecuteScalar(insertQuery, parameters));

            return new User
            {
                Id = newId,
                FullName = fullName,
                Email = email,
                Role = "Storekeeper"
            };
        }

        // Проверка существования email
        public bool IsEmailExists(string email)
        {
            string query = "SELECT COUNT(*) FROM Users WHERE Email = @Email";
            int count = Convert.ToInt32(DatabaseHelper.ExecuteScalar(query, new[] { new NpgsqlParameter("@Email", email) }));
            return count > 0;
        }
    }
}
