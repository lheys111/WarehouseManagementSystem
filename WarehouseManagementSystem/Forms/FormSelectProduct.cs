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

        private SelectedProductItem selectedProduct;
        private int selectedProductId = -1;
        private decimal availableStock = 0;
        private decimal productPrice = 0;

        // Конструктор для выбора товара из списка
        public FormSelectProduct()
        {
            InitializeComponent();
            LoadProducts();
            SetupButtons();
        }

        // Конструктор для предварительно выбранного товара (из FormNewShipment)
        public FormSelectProduct(int productId, string article, string name, decimal stock, decimal price)
        {
            InitializeComponent();
            SetupButtons();

            selectedProductId = productId;
            availableStock = stock;
            productPrice = price;

            // Создаем временную таблицу с выбранным товаром
            DataTable dt = new DataTable();
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("Article", typeof(string));
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("StockQuantity", typeof(decimal));
            dt.Rows.Add(productId, article, name, stock);

            dgvProducts.DataSource = dt;
            dgvProducts.Columns["Id"].Visible = false;
            dgvProducts.Columns["Article"].HeaderText = "Артикул";
            dgvProducts.Columns["Name"].HeaderText = "Название";
            dgvProducts.Columns["StockQuantity"].HeaderText = "Остаток";

            // Выбираем строку
            if (dgvProducts.Rows.Count > 0)
                dgvProducts.Rows[0].Selected = true;

            lblAvailable.Text = $"Доступно: {stock}";
            dgvProducts.Enabled = false; // Запрещаем выбор других товаров
            txtSearch.Enabled = false;
            btnSearch.Enabled = false;
        }

        public SelectedProductItem SelectedProduct
        {
            get { return selectedProduct; }
        }

        private void SetupButtons()
        {
            // Кнопка Найти
            btnSearch.FlatStyle = FlatStyle.Flat;
            btnSearch.FlatAppearance.BorderColor = Color.Black;
            btnSearch.FlatAppearance.BorderSize = 1;
            btnSearch.BackColor = Color.White;

            // Кнопка Добавить
            btnAdd.FlatStyle = FlatStyle.Flat;
            btnAdd.FlatAppearance.BorderColor = Color.Black;
            btnAdd.FlatAppearance.BorderSize = 1;
            btnAdd.BackColor = Color.White;

            // Кнопка Отмена
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.FlatAppearance.BorderColor = Color.Black;
            btnCancel.FlatAppearance.BorderSize = 1;
            btnCancel.BackColor = Color.White;
        }

        private void LoadProducts(string searchText = "")
        {
            try
            {
                string query = @"SELECT Id, Article, Name, UnitOfMeasure, StockQuantity, PurchasePrice
                                FROM vw_ProductsWithStock
                                WHERE StockQuantity > 0";

                if (!string.IsNullOrEmpty(searchText))
                {
                    query += " AND (Article ILIKE @Search OR Name ILIKE @Search)";
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

                dgvProducts.Columns["Id"].Visible = false;
                dgvProducts.Columns["Article"].HeaderText = "Артикул";
                dgvProducts.Columns["Name"].HeaderText = "Название";
                dgvProducts.Columns["UnitOfMeasure"].HeaderText = "Ед. изм.";
                dgvProducts.Columns["StockQuantity"].HeaderText = "Остаток";
                dgvProducts.Columns["PurchasePrice"].Visible = false;

                dgvProducts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                // Очищаем поле количества и метку
                txtQuantity.Text = "";
                lblAvailable.Text = "Доступно: 0";
                selectedProductId = -1;
                availableStock = 0;
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

        private void dgvProducts_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvProducts.CurrentRow != null)
            {
                selectedProductId = Convert.ToInt32(dgvProducts.CurrentRow.Cells["Id"].Value);
                availableStock = Convert.ToDecimal(dgvProducts.CurrentRow.Cells["StockQuantity"].Value);
                productPrice = Convert.ToDecimal(dgvProducts.CurrentRow.Cells["PurchasePrice"].Value);

                lblAvailable.Text = $"Доступно: {availableStock}";
                txtQuantity.Text = "";
                txtQuantity.Focus();
            }
        }

        private void txtQuantity_TextChanged(object sender, EventArgs e)
        {
            // Проверка введенного количества
            if (decimal.TryParse(txtQuantity.Text, out decimal quantity))
            {
                if (quantity > availableStock)
                {
                    lblAvailable.ForeColor = Color.Red;
                    lblAvailable.Text = $"Доступно: {availableStock} (превышено!)";
                }
                else
                {
                    lblAvailable.ForeColor = SystemColors.ControlText;
                    lblAvailable.Text = $"Доступно: {availableStock}";
                }
            }
            else
            {
                lblAvailable.ForeColor = SystemColors.ControlText;
                lblAvailable.Text = $"Доступно: {availableStock}";
            }
        }

        private void txtQuantity_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Разрешаем цифры, запятую, точку и Backspace
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
                e.KeyChar != ',' && e.KeyChar != '.')
            {
                e.Handled = true;
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            // Проверка выбора товара
            if (selectedProductId == -1)
            {
                MessageBox.Show("Выберите товар из списка", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Проверка ввода количества
            if (string.IsNullOrEmpty(txtQuantity.Text))
            {
                MessageBox.Show("Введите количество", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal quantity;
            if (!decimal.TryParse(txtQuantity.Text, out quantity))
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

            if (quantity > availableStock)
            {
                MessageBox.Show($"Недостаточно товара на складе. Доступно: {availableStock}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Создаем выбранный товар
            selectedProduct = new SelectedProductItem
            {
                ProductId = selectedProductId,
                Article = dgvProducts.CurrentRow.Cells["Article"].Value.ToString(),
                Name = dgvProducts.CurrentRow.Cells["Name"].Value.ToString(),
                Quantity = quantity,
                Price = productPrice,
                AvailableStock = availableStock
            };

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        // Двойной клик по товару для быстрого добавления
        private void dgvProducts_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                btnAdd.PerformClick();
            }
        }
    }
}