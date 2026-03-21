using System;
using Npgsql;
using System.Windows.Forms;
using WarehouseManagementSystem.Helpers;

namespace WarehouseManagementSystem.Forms
{
    public partial class FormRegistration : Form
    {
        public FormRegistration()
        {
            InitializeComponent();
            InitializeEvents();
        }

        private void InitializeEvents()
        {
            btnRegister.Click += btnRegister_Click;
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            var fullName = txtFullName.Text.Trim();
            var email = txtEmail.Text.Trim();
            var password = txtPassword.Text;
            var confirmPassword = txtConfirmPassword.Text;

            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(password))
            {
                MessageBox.Show(Constants.Messages.FillAllFields, Constants.FormTitles.Registration,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show(Constants.Messages.PasswordMismatch, Constants.FormTitles.Registration,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var checkParams = new[] { new NpgsqlParameter("@Email", email) };
                var count = Convert.ToInt32(DatabaseHelper.ExecuteScalar(
                    "SELECT COUNT(*) FROM Users WHERE Email = @Email", checkParams));

                if (count > 0)
                {
                    MessageBox.Show(Constants.Messages.EmailExists, Constants.FormTitles.Registration,
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var insertParams = new[]
                {
                    new NpgsqlParameter("@FullName", fullName),
                    new NpgsqlParameter("@Email", email),
                    new NpgsqlParameter("@PasswordHash", password)
                };

                DatabaseHelper.ExecuteNonQuery(
                    "INSERT INTO Users (FullName, Email, PasswordHash, Role) VALUES (@FullName, @Email, @PasswordHash, 'Storekeeper')",
                    insertParams);

                AppLogger.Info($"Зарегистрирован новый пользователь: {email}");
                MessageBox.Show(Constants.Messages.RegistrationSuccess, Constants.FormTitles.Registration,
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                Close();
            }
            catch (Exception ex)
            {
                AppLogger.Error(ex, "Ошибка регистрации");
                MessageBox.Show(Constants.Messages.ConnectionError, Constants.FormTitles.Registration,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}