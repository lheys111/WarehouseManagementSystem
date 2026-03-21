using System;
using Npgsql;
using System.Windows.Forms;
using WarehouseManagementSystem.Helpers;

namespace WarehouseManagementSystem.Forms
{
    public partial class FormCategoryEdit : Form
    {
        private int _categoryId = -1;

        public FormCategoryEdit()
        {
            InitializeComponent();
            InitializeEvents();
            Text = Constants.FormTitles.AddCategory;
        }

        public FormCategoryEdit(int id, string name, string description)
        {
            InitializeComponent();
            InitializeEvents();
            Text = Constants.FormTitles.EditCategory;
            _categoryId = id;
            txtName.Text = name;
            txtDescription.Text = description;
        }

        private void InitializeEvents()
        {
            btnSave.Click += btnSave_Click;
            btnCancel.Click += btnCancel_Click;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtName.Text))
            {
                MessageBox.Show("Введите название категории", Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                if (_categoryId == -1)
                {
                    var sql = "INSERT INTO Categories (Name, Description) VALUES (@Name, @Description)";
                    var parameters = new[]
                    {
                        new NpgsqlParameter("@Name", txtName.Text),
                        new NpgsqlParameter("@Description", string.IsNullOrEmpty(txtDescription.Text) ? (object)DBNull.Value : txtDescription.Text)
                    };
                    DatabaseHelper.ExecuteNonQuery(sql, parameters);
                    AppLogger.Info($"Добавлена категория: {txtName.Text}");
                }
                else
                {
                    var sql = "UPDATE Categories SET Name = @Name, Description = @Description WHERE Id = @Id";
                    var parameters = new[]
                    {
                        new NpgsqlParameter("@Name", txtName.Text),
                        new NpgsqlParameter("@Description", string.IsNullOrEmpty(txtDescription.Text) ? (object)DBNull.Value : txtDescription.Text),
                        new NpgsqlParameter("@Id", _categoryId)
                    };
                    DatabaseHelper.ExecuteNonQuery(sql, parameters);
                    AppLogger.Info($"Обновлена категория: {txtName.Text}");
                }

                MessageBox.Show(Constants.Messages.SaveSuccess, Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                AppLogger.Error(ex, "Ошибка сохранения категории");
                MessageBox.Show(Constants.Messages.ConnectionError, Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}