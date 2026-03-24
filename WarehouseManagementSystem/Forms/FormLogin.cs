using Npgsql;
using System;
using System.Data;
using System.Windows.Forms;
using WarehouseManagementSystem.Helpers;
using WarehouseManagementSystem.Models;
using WarehouseManagementSystem.Services;

namespace WarehouseManagementSystem.Forms
{
    public partial class FormLogin : Form
    {
        public FormLogin()
        {
            InitializeComponent();
            InitializeEvents();
            
        }

        private void InitializeEvents()
        {
            btnLogin.Click += btnLogin_Click;
            lnkRegister.LinkClicked += lnkRegister_LinkClicked;
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            var email = txtEmail.Text.Trim();
            var password = txtPassword.Text;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show(Constants.Messages.FillAllFields, Constants.FormTitles.Login,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var parameters = new[]
                {
                    new NpgsqlParameter("@Email", email),
                    new NpgsqlParameter("@Password", password)
                };

                var result = DatabaseHelper.ExecuteQuery(Constants.Queries.GetUserByEmail, parameters);

                if (result.Rows.Count > 0)
                {
                    Session.CurrentUser = new User
                    {
                        Id = Convert.ToInt32(result.Rows[0]["Id"]),
                        FullName = result.Rows[0]["FullName"].ToString(),
                        Email = email,
                        Role = result.Rows[0]["Role"].ToString()
                    };

                    AppLogger.Info($"Пользователь {email} вошел в систему");
                    Hide();

                    if (Session.CurrentUser.Role == Constants.Roles.Admin)
                    {
                        new FormAdminMain().Show();
                    }
                    else
                    {
                        new FormStorekeeperMain().Show();
                    }
                }
                else
                {
                    MessageBox.Show(Constants.Messages.LoginError, Constants.FormTitles.Login,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    AppLogger.Warn($"Неудачная попытка входа: {email}");
                }
            }
            catch (Exception ex)
            {
                AppLogger.Error(ex, "Ошибка при входе в систему");
                MessageBox.Show(Constants.Messages.ConnectionError, Constants.FormTitles.Login,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void lnkRegister_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            new FormRegistration().ShowDialog();
        }
    }
}