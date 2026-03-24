
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
            LoadProducts();
            SetupForm();
        }

        public FormSelectProduct(int productId, string article, string name, decimal stock, decimal price)
        {
            InitializeComponent();
            SetupForm();

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
            dgvProducts.Columns["Article"].HeaderText = "Артикул";
            dgvProducts.Columns["Name"].HeaderText = "Название";
            dgvProducts.Columns["StockQuantity"].HeaderText = "Остаток";

            if (dgvProducts.Rows.Count > 0)
                dgvProducts.Rows[0].Selected = true;

            lblAvailable.Text = $"Доступно: {stock}";
            dgvProducts.Enabled = false;
            txtSearch.Enabled = false;
            btnSearch.Enabled = false;
        }

        public SelectedProductItem SelectedProduct => _selectedProduct;

        private void SetupForm()
        {
            this.Text = "Выбор товара и количества";
            this.StartPosition = FormStartPosition.CenterParent;

            this.btnAdd.Click += BtnAdd_Click;
            this.btnCancel.Click += BtnCancel_Click;
            this.btnSearch.Click += BtnSearch_Click;
            this.dgvProducts.SelectionChanged += DgvProducts_SelectionChanged;
            this.txtQuantity.TextChanged += TxtQuantity_TextChanged;
            this.txtQuantity.KeyPress += TxtQuantity_KeyPress;
            this.dgvProducts.CellDoubleClick += DgvProducts_CellDoubleClick;

     
            btnSearch.FlatStyle = FlatStyle.Flat;
            btnSearch.FlatAppearance.BorderColor = Color.Black;
            btnSearch.FlatAppearance.BorderSize = 1;
            btnSearch.BackColor = Color.White;
            btnSearch.Text = "Найти";

            btnAdd.FlatStyle = FlatStyle.Flat;
            btnAdd.FlatAppearance.BorderColor = Color.Black;
            btnAdd.FlatAppearance.BorderSize = 1;
            btnAdd.BackColor = Color.White;
            btnAdd.Text = "Добавить";

            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.FlatAppearance.BorderColor = Color.Black;
            btnCancel.FlatAppearance.BorderSize = 1;
            btnCancel.BackColor = Color.White;
            btnCancel.Text = "Отмена";
        }

        private void LoadProducts(string searchText = "")
        {
            try
            {
                string sql = @"SELECT Id, Article, Name, UnitOfMeasure, StockQuantity, PurchasePrice
                              FROM vw_ProductsWithStock
                              WHERE StockQuantity > 0
                              ORDER BY Name";

                var data = DatabaseHelper.ExecuteQuery(sql);
                dgvProducts.DataSource = data;

         
                dgvProducts.Columns["Id"].Visible = false;
                dgvProducts.Columns["Article"].HeaderText = "Артикул";
                dgvProducts.Columns["Name"].HeaderText = "Название";
                dgvProducts.Columns["UnitOfMeasure"].HeaderText = "Ед. изм.";
                dgvProducts.Columns["StockQuantity"].HeaderText = "Остаток";
                dgvProducts.Columns["PurchasePrice"].HeaderText = "Цена";  

                dgvProducts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                txtQuantity.Text = "";
                lblAvailable.Text = "Доступно: 0";
                _selectedProductId = -1;
                _availableStock = 0;
                _productPrice = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки: " + ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            LoadProducts(txtSearch.Text);
        }

        private void DgvProducts_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvProducts.CurrentRow != null)
            {
                _selectedProductId = Convert.ToInt32(dgvProducts.CurrentRow.Cells["Id"].Value);
                _availableStock = Convert.ToDecimal(dgvProducts.CurrentRow.Cells["StockQuantity"].Value);

             
                if (dgvProducts.Columns.Contains("PurchasePrice") &&
                    dgvProducts.CurrentRow.Cells["PurchasePrice"].Value != null)
                {
                    _productPrice = Convert.ToDecimal(dgvProducts.CurrentRow.Cells["PurchasePrice"].Value);
                }
                else
                {
                    _productPrice = 0;
                }

                lblAvailable.Text = $"Доступно: {_availableStock}";
                txtQuantity.Text = "";
                txtQuantity.Focus();
            }
        }

        private void TxtQuantity_TextChanged(object sender, EventArgs e)
        {
            if (decimal.TryParse(txtQuantity.Text, out var quantity) && _availableStock > 0)
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
                lblAvailable.Text = _availableStock > 0 ? $"Доступно: {_availableStock}" : "Доступно: 0";
            }
        }

        private void TxtQuantity_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
                e.KeyChar != ',' && e.KeyChar != '.')
            {
                e.Handled = true;
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (_selectedProductId == -1)
            {
                MessageBox.Show("Выберите товар из списка", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(txtQuantity.Text))
            {
                MessageBox.Show("Введите количество", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(txtQuantity.Text, out var quantity))
            {
                MessageBox.Show("Введите корректное количество", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (quantity <= 0)
            {
                MessageBox.Show("Количество должно быть больше нуля", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (quantity > _availableStock)
            {
                MessageBox.Show($"Недостаточно товара на складе. Доступно: {_availableStock}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void DgvProducts_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                BtnAdd_Click(sender, e);
            }
        }
    }
}