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
            InitializeEvents();
            LoadCategories();
        }

        private void InitializeEvents()
        {
            btnAdd.Click += btnAdd_Click;
            dgvCategories.CellClick += dgvCategories_CellClick;
        }

        private void LoadCategories()
        {
            try
            {
                var data = DatabaseHelper.ExecuteQuery(Constants.Queries.GetCategoriesWithCount);
                dgvCategories.DataSource = data;

                ConfigureGrid();
            }
            catch (Exception ex)
            {
                AppLogger.Error(ex, "Ошибка загрузки категорий");
                MessageBox.Show(Constants.Messages.ConnectionError, Constants.FormTitles.Categories,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigureGrid()
        {
            if (dgvCategories.Columns.Contains("Id"))
                dgvCategories.Columns["Id"].Visible = false;
            if (dgvCategories.Columns.Contains("Name"))
                dgvCategories.Columns["Name"].HeaderText = Constants.GridHeaders.Category;
            if (dgvCategories.Columns.Contains("Description"))
                dgvCategories.Columns["Description"].HeaderText = Constants.GridHeaders.Description;
            if (dgvCategories.Columns.Contains("ProductsCount"))
                dgvCategories.Columns["ProductsCount"].HeaderText = Constants.GridHeaders.ProductsCount;

            if (!dgvCategories.Columns.Contains("btnEdit"))
            {
                var editCol = new DataGridViewButtonColumn
                {
                    Name = "btnEdit",
                    HeaderText = Constants.GridHeaders.Actions,
                    Text = Constants.ButtonText.Edit,
                    UseColumnTextForButtonValue = true
                };
                dgvCategories.Columns.Add(editCol);

                var deleteCol = new DataGridViewButtonColumn
                {
                    Name = "btnDelete",
                    Text = Constants.ButtonText.Delete,
                    UseColumnTextForButtonValue = true
                };
                dgvCategories.Columns.Add(deleteCol);
            }

            dgvCategories.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var editForm = new FormCategoryEdit();
            if (editForm.ShowDialog() == DialogResult.OK)
                LoadCategories();
        }

        private void dgvCategories_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var id = Convert.ToInt32(dgvCategories.Rows[e.RowIndex].Cells["Id"].Value);
            var name = dgvCategories.Rows[e.RowIndex].Cells["Name"].Value.ToString();
            var description = dgvCategories.Rows[e.RowIndex].Cells["Description"].Value?.ToString();

            if (dgvCategories.Columns[e.ColumnIndex].Name == "btnEdit")
            {
                var editForm = new FormCategoryEdit(id, name, description);
                if (editForm.ShowDialog() == DialogResult.OK)
                    LoadCategories();
            }
            else if (dgvCategories.Columns[e.ColumnIndex].Name == "btnDelete")
            {
                var productCount = Convert.ToInt32(dgvCategories.Rows[e.RowIndex].Cells["ProductsCount"].Value);

                if (productCount > 0)
                {
                    MessageBox.Show(Constants.Messages.CategoryHasProducts, Constants.FormTitles.Categories,
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var confirm = MessageBox.Show($"Удалить категорию '{name}'? {Constants.Messages.DeleteConfirm}",
                    "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (confirm != DialogResult.Yes) return;

                try
                {
                    var parameters = new[] { new NpgsqlParameter("@Id", id) };
                    DatabaseHelper.ExecuteNonQuery("DELETE FROM Categories WHERE Id = @Id", parameters);
                    AppLogger.Info($"Удалена категория: {name}");
                    LoadCategories();
                }
                catch (Exception ex)
                {
                    AppLogger.Error(ex, "Ошибка удаления категории");
                    MessageBox.Show(Constants.Messages.ConnectionError, Constants.FormTitles.Categories,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}