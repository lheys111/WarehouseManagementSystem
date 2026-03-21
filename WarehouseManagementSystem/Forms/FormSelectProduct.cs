using Npgsql;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using WarehouseManagementSystem.Helpers;

namespace WarehouseManagementSystem.Forms
{
    public partial class FormSelectProduct : Form
    {
        public class SelectedProductItem
        {
            public int ProductId { get; set; }
            public string Article { get; set; }
            public string Name { get; set; }
            public decimal Quantity { get; set; }
            public decimal Price { get; set; }
            public decimal AvailableStock { get; set; }
        }

        private SelectedProductItem _selectedProduct;
        private int _selectedProductId = -1;
        private decimal _availableStock;
        private decimal _productPrice;

        public FormSelectProduct()
        {
            InitializeComponent();
            InitializeEvents();
            LoadProducts();
            SetupButtons();
            Text = Constants.FormTitles.SelectProduct;
        }

        public FormSelectProduct(int productId, string article, string name, decimal stock, decimal price)
        {
            InitializeComponent();
            SetupButtons();

            _selectedProductId = productId;
            _availableStock = stock;
            _productPrice = price;

            var data = new DataTable();
            data.Columns.Add("Id", typeof(int));
            data.Columns.Add("Article", typeof(string));
            data.Columns.Add("Name", typeof(string));
            data.Columns.Add("StockQuantity", typeof(decimal));
            data.Rows.Add(productId, article, name, stock);

            dgvProducts.DataSource = data;
            dgvProducts.Columns["Id"].Visible = false;
            dgvProducts.Columns["Article"].HeaderText = Constants.GridHeaders.Article;
            dgvProducts.Columns["Name"].HeaderText = Constants.GridHeaders.Name;
            dgvProducts.Columns["StockQuantity"].HeaderText = Constants.GridHeaders.Stock;

            if (dgvProducts.Rows.Count > 0)
                dgvProducts.Rows[0].Selected = true;

            lblAvailable.Text = $"Доступно: {stock}";
            dgvProducts.Enabled = false;
            txtSearch.Enabled = false;
            btnSearch.Enabled = false;
        }

        public SelectedProductItem SelectedProduct => _selectedProduct;

        private void InitializeEvents()
        {
            btnSearch.Click += btnSearch_Click;
            btnAdd.Click += btnAdd_Click;
            btnCancel.Click += btnCancel_Click;
            dgvProducts.SelectionChanged += dgvProducts_SelectionChanged;
            txtQuantity.TextChanged += txtQuantity_TextChanged;
            txtQuantity.KeyPress += txtQuantity_KeyPress;
            dgvProducts.CellDoubleClick += dgvProducts_CellDoubleClick;
        }

        private void SetupButtons()
        {
            var buttons = new[] { btnSearch, btnAdd, btnCancel };
            foreach (var btn in buttons)
            {
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderColor = Color.Black;
                btn.FlatAppearance.BorderSize = 1;
                btn.BackColor = Color.White;
            }
            btnSearch.Text = Constants.ButtonText.Search;
            btnAdd.Text = Constants.ButtonText.Add;
            btnCancel.Text = Constants.ButtonText.Cancel;
        }

        private void LoadProducts(string searchText = "")
        {
            try
            {
                var sql = @"SELECT Id, Article, Name, UnitOfMeasure, StockQuantity, PurchasePrice
                           FROM vw_ProductsWithStock
                           WHERE StockQuantity > 0";

                if (!string.IsNullOrEmpty(searchText))
                {
                    sql += " AND (Article ILIKE @Search OR Name ILIKE @Search)";
                }

                sql += " ORDER BY Name";

                NpgsqlParameter[] parameters = null;
                if (!string.IsNullOrEmpty(searchText))
                {
                    parameters = new[] { new NpgsqlParameter("@Search", "%" + searchText + "%") };
                }

                var data = DatabaseHelper.ExecuteQuery(sql, parameters);
                dgvProducts.DataSource = data;

                dgvProducts.Columns["Id"].Visible = false;
                dgvProducts.Columns["Article"].HeaderText = Constants.GridHeaders.Article;
                dgvProducts.Columns["Name"].HeaderText = Constants.GridHeaders.Name;
                dgvProducts.Columns["UnitOfMeasure"].HeaderText = Constants.GridHeaders.Unit;
                dgvProducts.Columns["StockQuantity"].HeaderText = Constants.GridHeaders.Stock;
                dgvProducts.Columns["PurchasePrice"].Visible = false;

                dgvProducts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                txtQuantity.Text = "";
                lblAvailable.Text = "Доступно: 0";
                _selectedProductId = -1;
                _availableStock = 0;
            }
            catch (Exception ex)
            {
                AppLogger.Error(ex, "Ошибка загрузки товаров");
                MessageBox.Show(Constants.Messages.ConnectionError, Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            LoadProducts(txtSearch.Text);
        }

        private void dgvProducts_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvProducts.CurrentRow != null)
            {
                _selectedProductId = Convert.ToInt32(dgvProducts.CurrentRow.Cells["Id"].Value);
                _availableStock = Convert.ToDecimal(dgvProducts.CurrentRow.Cells["StockQuantity"].Value);
                _productPrice = Convert.ToDecimal(dgvProducts.CurrentRow.Cells["PurchasePrice"].Value);

                lblAvailable.Text = $"Доступно: {_availableStock}";
                txtQuantity.Text = "";
                txtQuantity.Focus();
            }
        }

        private void txtQuantity_TextChanged(object sender, EventArgs e)
        {
            if (decimal.TryParse(txtQuantity.Text, out var quantity))
            {
                if (quantity > _availableStock)
                {
                    lblAvailable.ForeColor = Color.Red;
                    lblAvailable.Text = $"Доступно: {_availableStock} (превышено!)";
                }
                else
                {
                    lblAvailable.ForeColor = SystemColors.ControlText;
                    lblAvailable.Text = $"Доступно: {_availableStock}";
                }
            }
            else
            {
                lblAvailable.ForeColor = SystemColors.ControlText;
                lblAvailable.Text = $"Доступно: {_availableStock}";
            }
        }

        private void txtQuantity_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
                e.KeyChar != ',' && e.KeyChar != '.')
            {
                e.Handled = true;
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (_selectedProductId == -1)
            {
                MessageBox.Show("Выберите товар из списка", Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(txtQuantity.Text))
            {
                MessageBox.Show("Введите количество", Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(txtQuantity.Text, out var quantity))
            {
                MessageBox.Show(Constants.Messages.QuantityInvalid, Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (quantity <= 0)
            {
                MessageBox.Show("Количество должно быть больше нуля", Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (quantity > _availableStock)
            {
                MessageBox.Show($"{Constants.Messages.StockInsufficient} Доступно: {_availableStock}",
                    Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _selectedProduct = new SelectedProductItem
            {
                ProductId = _selectedProductId,
                Article = dgvProducts.CurrentRow.Cells["Article"].Value.ToString(),
                Name = dgvProducts.CurrentRow.Cells["Name"].Value.ToString(),
                Quantity = quantity,
                Price = _productPrice,
                AvailableStock = _availableStock
            };

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void dgvProducts_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                btnAdd_Click(sender, e);
            }
        }
    }
}