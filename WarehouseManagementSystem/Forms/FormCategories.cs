using System;
using System.Data;
using Npgsql;
using System.Windows.Forms;
using WarehouseManagementSystem.Helpers;

namespace WarehouseManagementSystem.Forms
{
    public partial class FormCategories : Form
    {
        public FormCategories()
        {
            InitializeComponent();
            LoadCategories();
        }

        private void LoadCategories()
        {
            try
            {
                string query = @"SELECT c.Id, c.Name, c.Description, COUNT(p.Id) as ProductsCount
                                FROM Categories c
                                LEFT JOIN Products p ON c.Id = p.CategoryId
                                GROUP BY c.Id, c.Name, c.Description
                                ORDER BY c.Name";

                DataTable dt = DatabaseHelper.ExecuteQuery(query);
                dgvCategories.DataSource = dt;

                if (dgvCategories.Columns.Contains("Id"))
                    dgvCategories.Columns["Id"].Visible = false;
                if (dgvCategories.Columns.Contains("Name"))
                    dgvCategories.Columns["Name"].HeaderText = "Название категории";
                if (dgvCategories.Columns.Contains("Description"))
                    dgvCategories.Columns["Description"].HeaderText = "Описание";
                if (dgvCategories.Columns.Contains("ProductsCount"))
                    dgvCategories.Columns["ProductsCount"].HeaderText = "Товаров";

                // Добавляем кнопки
                if (!dgvCategories.Columns.Contains("btnEdit"))
                {
                    DataGridViewButtonColumn btnEdit = new DataGridViewButtonColumn();
                    btnEdit.Name = "btnEdit";
                    btnEdit.HeaderText = "Действия";
                    btnEdit.Text = "Изменить";
                    btnEdit.UseColumnTextForButtonValue = true;
                    dgvCategories.Columns.Add(btnEdit);

                    DataGridViewButtonColumn btnDelete = new DataGridViewButtonColumn();
                    btnDelete.Name = "btnDelete";
                    btnDelete.Text = "Удалить";
                    btnDelete.UseColumnTextForButtonValue = true;
                    dgvCategories.Columns.Add(btnDelete);
                }

                dgvCategories.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            FormCategoryEdit editForm = new FormCategoryEdit();
            if (editForm.ShowDialog() == DialogResult.OK)
                LoadCategories();
        }

        private void dgvCategories_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            int categoryId = Convert.ToInt32(dgvCategories.Rows[e.RowIndex].Cells["Id"].Value);
            string categoryName = dgvCategories.Rows[e.RowIndex].Cells["Name"].Value.ToString();
            string categoryDesc = dgvCategories.Rows[e.RowIndex].Cells["Description"].Value?.ToString();

            if (dgvCategories.Columns[e.ColumnIndex].Name == "btnEdit")
            {
                FormCategoryEdit editForm = new FormCategoryEdit(categoryId, categoryName, categoryDesc);
                if (editForm.ShowDialog() == DialogResult.OK)
                    LoadCategories();
            }
            else if (dgvCategories.Columns[e.ColumnIndex].Name == "btnDelete")
            {
                int productsCount = Convert.ToInt32(dgvCategories.Rows[e.RowIndex].Cells["ProductsCount"].Value);

                if (productsCount > 0)
                {
                    MessageBox.Show($"Нельзя удалить категорию '{categoryName}', так как в ней есть товары",
                        "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (MessageBox.Show($"Удалить категорию '{categoryName}'?", "Подтверждение",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    try
                    {
                        string query = "DELETE FROM Categories WHERE Id = @Id";
                        DatabaseHelper.ExecuteNonQuery(query, new[] { new NpgsqlParameter("@Id", categoryId) });
                        LoadCategories();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}