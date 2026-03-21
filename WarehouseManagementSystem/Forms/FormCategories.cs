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
            this.btnAdd.Click += new EventHandler(this.btnAdd_Click);
            this.dgvCategories.CellClick += new DataGridViewCellEventHandler(this.dgvCategories_CellClick);
            LoadCategories();
        }

        private void LoadCategories()
        {
            try
            {
                string sql = @"SELECT c.Id, c.Name, c.Description, COUNT(p.Id) as ProductsCount
                              FROM Categories c
                              LEFT JOIN Products p ON c.Id = p.CategoryId
                              GROUP BY c.Id, c.Name, c.Description
                              ORDER BY c.Name";

                DataTable data = DatabaseHelper.ExecuteQuery(sql);
                dgvCategories.DataSource = data;

                if (dgvCategories.Columns.Contains("Id"))
                    dgvCategories.Columns["Id"].Visible = false;
                if (dgvCategories.Columns.Contains("Name"))
                    dgvCategories.Columns["Name"].HeaderText = "Название категории";
                if (dgvCategories.Columns.Contains("Description"))
                    dgvCategories.Columns["Description"].HeaderText = "Описание";
                if (dgvCategories.Columns.Contains("ProductsCount"))
                    dgvCategories.Columns["ProductsCount"].HeaderText = "Товаров";

                if (!dgvCategories.Columns.Contains("btnEdit"))
                {
                    DataGridViewButtonColumn editCol = new DataGridViewButtonColumn();
                    editCol.Name = "btnEdit";
                    editCol.HeaderText = "Действия";
                    editCol.Text = "Изменить";
                    editCol.UseColumnTextForButtonValue = true;
                    dgvCategories.Columns.Add(editCol);

                    DataGridViewButtonColumn deleteCol = new DataGridViewButtonColumn();
                    deleteCol.Name = "btnDelete";
                    deleteCol.Text = "Удалить";
                    deleteCol.UseColumnTextForButtonValue = true;
                    dgvCategories.Columns.Add(deleteCol);
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

            int id = Convert.ToInt32(dgvCategories.Rows[e.RowIndex].Cells["Id"].Value);
            string name = dgvCategories.Rows[e.RowIndex].Cells["Name"].Value.ToString();
            string description = dgvCategories.Rows[e.RowIndex].Cells["Description"].Value?.ToString();

            if (dgvCategories.Columns[e.ColumnIndex].Name == "btnEdit")
            {
                FormCategoryEdit editForm = new FormCategoryEdit(id, name, description);
                if (editForm.ShowDialog() == DialogResult.OK)
                    LoadCategories();
            }
            else if (dgvCategories.Columns[e.ColumnIndex].Name == "btnDelete")
            {
                int productCount = Convert.ToInt32(dgvCategories.Rows[e.RowIndex].Cells["ProductsCount"].Value);

                if (productCount > 0)
                {
                    MessageBox.Show($"Нельзя удалить категорию '{name}', так как в ней есть товары",
                        "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (MessageBox.Show($"Удалить категорию '{name}'?", "Подтверждение",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    try
                    {
                        string deleteSql = "DELETE FROM Categories WHERE Id = @Id";
                        DatabaseHelper.ExecuteNonQuery(deleteSql, new[] { new NpgsqlParameter("@Id", id) });
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
