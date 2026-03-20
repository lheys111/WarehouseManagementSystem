using System;
using Npgsql;
using System.Windows.Forms;
using WarehouseManagementSystem.Helpers;

namespace WarehouseManagementSystem.Forms
{
    public partial class FormCategoryEdit : Form
    {
        private int categoryId = -1;

        public FormCategoryEdit()
        {
            InitializeComponent();
            this.Text = "Добавление категории";
        }

        public FormCategoryEdit(int id, string name, string description)
        {
            InitializeComponent();
            this.Text = "Редактирование категории";
            categoryId = id;
            txtName.Text = name;
            txtDescription.Text = description;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtName.Text))
            {
                MessageBox.Show("Введите название категории", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                if (categoryId == -1)
                {
                    string query = "INSERT INTO Categories (Name, Description) VALUES (@Name, @Description)";
                    NpgsqlParameter[] parameters = {
                        new NpgsqlParameter("@Name", txtName.Text),
                        new NpgsqlParameter("@Description", string.IsNullOrEmpty(txtDescription.Text) ? (object)DBNull.Value : txtDescription.Text)
                    };
                    DatabaseHelper.ExecuteNonQuery(query, parameters);
                }
                else
                {
                    string query = "UPDATE Categories SET Name = @Name, Description = @Description WHERE Id = @Id";
                    NpgsqlParameter[] parameters = {
                        new NpgsqlParameter("@Name", txtName.Text),
                        new NpgsqlParameter("@Description", string.IsNullOrEmpty(txtDescription.Text) ? (object)DBNull.Value : txtDescription.Text),
                        new NpgsqlParameter("@Id", categoryId)
                    };
                    DatabaseHelper.ExecuteNonQuery(query, parameters);
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
