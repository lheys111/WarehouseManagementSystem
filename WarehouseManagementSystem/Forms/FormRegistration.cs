using System;
using Npgsql;
using System.Windows.Forms;
using WarehouseManagementSystem.Helpers;
using WarehouseManagementSystem.Models;

namespace WarehouseManagementSystem.Forms
{
    public partial class FormRegistration : Form
    {
        public FormRegistration()
        {
            InitializeComponent();

           
            this.btnRegister.Click += new EventHandler(this.btnRegister_Click);
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            string fullName = txtFullName.Text.Trim();
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Text;
            string confirmPassword = txtConfirmPassword.Text;

            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Заполните все поля", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Пароли не совпадают", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string checkQuery = "SELECT COUNT(*) FROM Users WHERE Email = @Email";
                NpgsqlParameter[] checkParams = { new NpgsqlParameter("@Email", email) };
                int count = Convert.ToInt32(DatabaseHelper.ExecuteScalar(checkQuery, checkParams));

                if (count > 0)
                {
                    MessageBox.Show("Пользователь с таким email уже существует", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string insertQuery = @"INSERT INTO Users (FullName, Email, PasswordHash, Role) 
                                      VALUES (@FullName, @Email, @PasswordHash, 'Storekeeper')";
                NpgsqlParameter[] parameters = {
                    new NpgsqlParameter("@FullName", fullName),
                    new NpgsqlParameter("@Email", email),
                    new NpgsqlParameter("@PasswordHash", password)
                };

                DatabaseHelper.ExecuteNonQuery(insertQuery, parameters);

                MessageBox.Show("Регистрация успешно завершена!", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}