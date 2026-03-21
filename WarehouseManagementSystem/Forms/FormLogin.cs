using System;
using System.Data;
using Npgsql;
using System.Windows.Forms;
using WarehouseManagementSystem.Models;
using WarehouseManagementSystem.Helpers;
using WarehouseManagementSystem.Forms;

namespace WarehouseManagementSystem.Forms
{
    public partial class FormLogin : Form
    {
        public FormLogin()
        {
            InitializeComponent();

            this.btnLogin.Click += new EventHandler(this.btnLogin_Click);
            this.lnkRegister.LinkClicked += new LinkLabelLinkClickedEventHandler(this.lnkRegister_LinkClicked);
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите email и пароль", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string sql = "SELECT Id, FullName, Role FROM Users WHERE Email = @Email AND PasswordHash = @Password";
                NpgsqlParameter[] parameters = {
                    new NpgsqlParameter("@Email", email),
                    new NpgsqlParameter("@Password", password)
                };

                DataTable result = DatabaseHelper.ExecuteQuery(sql, parameters);

                if (result.Rows.Count > 0)
                {
                    Session.CurrentUser = new User
                    {
                        Id = Convert.ToInt32(result.Rows[0]["Id"]),
                        FullName = result.Rows[0]["FullName"].ToString(),
                        Email = email,
                        Role = result.Rows[0]["Role"].ToString()
                    };

                    this.Hide();

                    if (Session.CurrentUser.Role == "Admin")
                    {
                        FormAdminMain adminForm = new FormAdminMain();
                        adminForm.Show();
                    }
                    else
                    {
                        FormStorekeeperMain storekeeperForm = new FormStorekeeperMain();
                        storekeeperForm.Show();
                    }
                }
                else
                {
                    MessageBox.Show("Неверный email или пароль", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void lnkRegister_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            FormRegistration regForm = new FormRegistration();
            regForm.ShowDialog();
        }
    }
}
