using System;
using System.Data;
using Npgsql;
using System.Windows.Forms;
using WarehouseManagementSystem.Helpers;
namespace WarehouseManagementSystem.Forms
{
    public partial class FormProducts : Form
    {
        public FormProducts()
        {
            InitializeComponent();
            LoadProducts();
        }

        private void LoadProducts(string searchText = "")
        {
            try
            {
                string query = @"SELECT Id, Article, Name, CategoryName, UnitOfMeasure, 
                                        PurchasePrice, StockQuantity 
                                FROM vw_ProductsWithStock";

                if (!string.IsNullOrEmpty(searchText))
                {
                    query += " WHERE Article ILIKE @Search OR Name ILIKE @Search";
                }

                query += " ORDER BY Name";

                NpgsqlParameter[] parameters = null;
                if (!string.IsNullOrEmpty(searchText))
                {
                    parameters = new NpgsqlParameter[] {
                        new NpgsqlParameter("@Search", "%" + searchText + "%")
                    };
                }

                DataTable dt = DatabaseHelper.ExecuteQuery(query, parameters);
                dgvProducts.DataSource = dt;

                if (dgvProducts.Columns.Contains("Id"))
                    dgvProducts.Columns["Id"].Visible = false;
                if (dgvProducts.Columns.Contains("Article"))
                    dgvProducts.Columns["Article"].HeaderText = "Артикул";
                if (dgvProducts.Columns.Contains("Name"))
                    dgvProducts.Columns["Name"].HeaderText = "Название";
                if (dgvProducts.Columns.Contains("CategoryName"))
                    dgvProducts.Columns["CategoryName"].HeaderText = "Категория";
                if (dgvProducts.Columns.Contains("UnitOfMeasure"))
                    dgvProducts.Columns["UnitOfMeasure"].HeaderText = "Ед. изм.";
                if (dgvProducts.Columns.Contains("PurchasePrice"))
                    dgvProducts.Columns["PurchasePrice"].HeaderText = "Цена закупки";
                if (dgvProducts.Columns.Contains("StockQuantity"))
                    dgvProducts.Columns["StockQuantity"].HeaderText = "Остаток";

                dgvProducts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки: " + ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            LoadProducts(txtSearch.Text);
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            FormProductEdit editForm = new FormProductEdit();
            if (editForm.ShowDialog() == DialogResult.OK)
                LoadProducts(txtSearch.Text);
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dgvProducts.CurrentRow != null)
            {
                int productId = Convert.ToInt32(dgvProducts.CurrentRow.Cells["Id"].Value);
                FormProductEdit editForm = new FormProductEdit(productId);
                if (editForm.ShowDialog() == DialogResult.OK)
                    LoadProducts(txtSearch.Text);
            }
            else
            {
                MessageBox.Show("Выберите товар", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvProducts.CurrentRow != null)
            {
                int productId = Convert.ToInt32(dgvProducts.CurrentRow.Cells["Id"].Value);
                string productName = dgvProducts.CurrentRow.Cells["Name"].Value.ToString();

                if (MessageBox.Show($"Удалить '{productName}'?", "Подтверждение",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    try
                    {
                        string query = "DELETE FROM Products WHERE Id = @Id";
                        DatabaseHelper.ExecuteNonQuery(query, new[] { new NpgsqlParameter("@Id", productId) });
                        LoadProducts(txtSearch.Text);
                        MessageBox.Show("Товар удален", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
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