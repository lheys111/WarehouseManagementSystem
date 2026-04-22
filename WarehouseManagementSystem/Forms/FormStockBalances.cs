using System;
using System.Data;
using Npgsql;
using System.Windows.Forms;
using WarehouseManagementSystem.Helpers;

namespace WarehouseManagementSystem.Forms
{
    public partial class FormStockBalances : Form
    {
        public FormStockBalances()
        {
            InitializeComponent();
            InitializeEvents();
            LoadStock();
            Text = Constants.FormTitles.StockBalances;
            dgvStock.CellDoubleClick += dgvStock_CellDoubleClick;
        }

        private void InitializeEvents()
        {
            btnSearch.Click += btnSearch_Click;
        }

        private void LoadStock(string searchText = "")
        {
            try
            {
                var sql = @"SELECT Id, Article, Name, Category, UnitOfMeasure, StockQuantity, PurchasePrice
            FROM vw_StockForStorekeeper";

                if (!string.IsNullOrEmpty(searchText))
                {
                    sql += " WHERE Article ILIKE @Search OR Name ILIKE @Search";
                }

                sql += " ORDER BY Name";

                NpgsqlParameter[] parameters = null;
                if (!string.IsNullOrEmpty(searchText))
                {
                    parameters = new[] { new NpgsqlParameter("@Search", "%" + searchText + "%") };
                }

                var data = DatabaseHelper.ExecuteQuery(sql, parameters);
                dgvStock.DataSource = data;

                ConfigureGrid();
            }
            catch (Exception ex)
            {
                AppLogger.Error(ex, "Ошибка загрузки остатков");
                MessageBox.Show(Constants.Messages.ConnectionError, Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigureGrid()
        {
            if (dgvStock.Columns.Contains("Article"))
                dgvStock.Columns["Article"].HeaderText = Constants.GridHeaders.Article;
            if (dgvStock.Columns.Contains("Name"))
                dgvStock.Columns["Name"].HeaderText = Constants.GridHeaders.Name;
            if (dgvStock.Columns.Contains("Category"))
                dgvStock.Columns["Category"].HeaderText = Constants.GridHeaders.Category;
            if (dgvStock.Columns.Contains("UnitOfMeasure"))
                dgvStock.Columns["UnitOfMeasure"].HeaderText = Constants.GridHeaders.Unit;
            if (dgvStock.Columns.Contains("StockQuantity"))
                dgvStock.Columns["StockQuantity"].HeaderText = Constants.GridHeaders.Stock;
            if (dgvStock.Columns.Contains("PurchasePrice"))
                dgvStock.Columns["PurchasePrice"].HeaderText = Constants.GridHeaders.Price;

            dgvStock.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            LoadStock(txtSearch.Text);
        }

        private void FormStockBalances_Load(object sender, EventArgs e)
        {

        }

        private void dgvStock_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                int productId = Convert.ToInt32(dgvStock.Rows[e.RowIndex].Cells["Id"].Value);
                string productName = dgvStock.Rows[e.RowIndex].Cells["Name"].Value.ToString();

                FormBatchDetails batchForm = new FormBatchDetails(productId, productName);
                batchForm.ShowDialog();
            }
        }

    }
}