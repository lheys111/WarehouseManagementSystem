using Npgsql;
using Org.BouncyCastle.Utilities;
using System;
using System.Data;
using System.Windows.Forms;
using WarehouseManagementSystem.Helpers;


namespace WarehouseManagementSystem.Forms
{
    public partial class FormProducts : Form
    {
        public FormProducts()
        {
            InitializeComponent();
            InitializeEvents();
            LoadProducts();
        }

        private void InitializeEvents()
        {
            btnSearch.Click += btnSearch_Click;
            btnAdd.Click += btnAdd_Click;
            btnEdit.Click += btnEdit_Click;
            btnDelete.Click += btnDelete_Click;
            
        }

        private void LoadProducts(string searchText = "")
        {
            try
            {
                string sql = @"
            SELECT 
                p.Id,
                p.Article,
                p.Name,
                c.Name AS CategoryName,
                p.UnitOfMeasure,
                p.PurchasePrice,
                COALESCE(s.Quantity, 0) AS StockQuantity,
                p.ExpiryDate,
                CASE 
                    WHEN p.ExpiryDate IS NULL THEN 'Годен'
                    WHEN p.ExpiryDate < CURRENT_DATE THEN 'Просрочен'
                    WHEN p.ExpiryDate < CURRENT_DATE + INTERVAL '30 days' THEN 'Скоро истекает'
                    ELSE 'Годен'
                END AS Status
            FROM Products p
            LEFT JOIN Categories c ON p.CategoryId = c.Id
            LEFT JOIN StockBalances s ON p.Id = s.ProductId";

                if (!string.IsNullOrEmpty(searchText))
                {
                    sql += " WHERE p.Article ILIKE @Search OR p.Name ILIKE @Search";
                }
                sql += " ORDER BY p.Name";

                NpgsqlParameter[] parameters = null;
                if (!string.IsNullOrEmpty(searchText))
                {
                    parameters = new[] { new NpgsqlParameter("@Search", "%" + searchText + "%") };
                }

                var data = DatabaseHelper.ExecuteQuery(sql, parameters);
                dgvProducts.DataSource = data;

                ConfigureGrid();
            }
            catch (Exception ex)
            {
                AppLogger.Error(ex, "Ошибка загрузки товаров");
                MessageBox.Show(Constants.Messages.ConnectionError, Constants.FormTitles.Products,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigureGrid()
        {
            if (dgvProducts.Columns.Contains("Id"))
                dgvProducts.Columns["Id"].Visible = false;

            if (dgvProducts.Columns.Contains("Article"))
                dgvProducts.Columns["Article"].HeaderText = Constants.GridHeaders.Article;

            if (dgvProducts.Columns.Contains("Name"))
                dgvProducts.Columns["Name"].HeaderText = Constants.GridHeaders.Name;

            if (dgvProducts.Columns.Contains("CategoryName"))
                dgvProducts.Columns["CategoryName"].HeaderText = Constants.GridHeaders.Category;

            if (dgvProducts.Columns.Contains("UnitOfMeasure"))
                dgvProducts.Columns["UnitOfMeasure"].HeaderText = Constants.GridHeaders.Unit;

            if (dgvProducts.Columns.Contains("PurchasePrice"))
            {
                dgvProducts.Columns["PurchasePrice"].HeaderText = Constants.GridHeaders.Price;
                dgvProducts.Columns["PurchasePrice"].DefaultCellStyle.Format = "N2";
                dgvProducts.Columns["PurchasePrice"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }

            if (dgvProducts.Columns.Contains("StockQuantity"))
            {
                dgvProducts.Columns["StockQuantity"].HeaderText = Constants.GridHeaders.Stock;
                dgvProducts.Columns["StockQuantity"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }

            if (dgvProducts.Columns.Contains("Status"))
            {
                dgvProducts.Columns["Status"].HeaderText = "Статус";
                dgvProducts.Columns["Status"].Width = 120;
            }
            if (dgvProducts.Columns.Contains("ExpiryDate"))
            {
                dgvProducts.Columns["ExpiryDate"].Visible = false;
            }

            dgvProducts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            LoadProducts(txtSearch.Text);
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var editForm = new FormProductEdit();
            if (editForm.ShowDialog() == DialogResult.OK)
                LoadProducts(txtSearch.Text);
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dgvProducts.CurrentRow == null)
            {
                MessageBox.Show(Constants.Messages.SelectItem, Constants.FormTitles.Products,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var productId = Convert.ToInt32(dgvProducts.CurrentRow.Cells["Id"].Value);
            var editForm = new FormProductEdit(productId);
            if (editForm.ShowDialog() == DialogResult.OK)
                LoadProducts(txtSearch.Text);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvProducts.CurrentRow == null)
            {
                MessageBox.Show(Constants.Messages.SelectItem, Constants.FormTitles.Products,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var productId = Convert.ToInt32(dgvProducts.CurrentRow.Cells["Id"].Value);
            var productName = dgvProducts.CurrentRow.Cells["Name"].Value.ToString();

            var confirm = MessageBox.Show(string.Format(String.DeleteProductConfirm, productName),
    String.ConfirmTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            try
            {
                var parameters = new[] { new NpgsqlParameter("@Id", productId) };
                DatabaseHelper.ExecuteNonQuery("DELETE FROM Products WHERE Id = @Id", parameters);
                AppLogger.Info($"Удален товар: {productName}");
                LoadProducts(txtSearch.Text);
            }
            catch (Exception ex)
            {
                AppLogger.Error(ex, "Ошибка удаления товара");
                MessageBox.Show(Constants.Messages.ConnectionError, Constants.FormTitles.Products,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
       
        private void dgvProducts_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}