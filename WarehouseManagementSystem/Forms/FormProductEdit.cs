using System;
using System.Data;
using Npgsql;
using System.Windows.Forms;
using WarehouseManagementSystem.Helpers;

namespace WarehouseManagementSystem.Forms
{
    public partial class FormProductEdit : Form
    {
        private int _productId = -1;
        private bool _isEdit = false;

        public FormProductEdit()
        {
            InitializeComponent();
            InitializeEvents();
            Text = Constants.FormTitles.AddProduct;
            LoadCategories();
        }

        public FormProductEdit(int id)
        {
            InitializeComponent();
            InitializeEvents();
            Text = Constants.FormTitles.EditProduct;
            _productId = id;
            _isEdit = true;
            LoadCategories();
            LoadProductData();
        }

        private void InitializeEvents()
        {
            btnSave.Click += btnSave_Click;
            btnCancel.Click += btnCancel_Click;
            txtPrice.KeyPress += txtPrice_KeyPress;
            txtShelfLife.KeyPress += txtShelfLife_KeyPress;
        }

        private void LoadCategories()
        {
            try
            {
                var data = DatabaseHelper.ExecuteQuery(Constants.Queries.GetCategories);
                cmbCategory.DataSource = data;
                cmbCategory.DisplayMember = "Name";
                cmbCategory.ValueMember = "Id";

                if (data.Rows.Count == 0)
                {
                    cmbCategory.Items.Add("Нет категорий");
                }
            }
            catch (Exception ex)
            {
                AppLogger.Error(ex, "Ошибка загрузки категорий");
                MessageBox.Show(Constants.Messages.ConnectionError, Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadProductData()
        {
            if (!_isEdit) return;

            try
            {
                var parameters = new[] { new NpgsqlParameter("@Id", _productId) };
                var data = DatabaseHelper.ExecuteQuery("SELECT * FROM Products WHERE Id = @Id", parameters);

                if (data.Rows.Count > 0)
                {
                    var row = data.Rows[0];
                    txtArticle.Text = row["Article"].ToString();
                    txtName.Text = row["Name"].ToString();

                    if (row["CategoryId"] != DBNull.Value)
                    {
                        var catId = Convert.ToInt32(row["CategoryId"]);
                        cmbCategory.SelectedValue = catId;
                    }

                    txtUnit.Text = row["UnitOfMeasure"].ToString();
                    txtPrice.Text = row["PurchasePrice"].ToString();

                    if (row["ShelfLife"] != DBNull.Value)
                    {
                        txtShelfLife.Text = row["ShelfLife"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                AppLogger.Error(ex, "Ошибка загрузки данных товара");
                MessageBox.Show(Constants.Messages.ConnectionError, Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtArticle.Text))
            {
                MessageBox.Show("Введите артикул товара!", Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtArticle.Focus();
                return;
            }

            if (string.IsNullOrEmpty(txtName.Text))
            {
                MessageBox.Show("Введите название товара!", Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }

            if (string.IsNullOrEmpty(txtUnit.Text))
            {
                MessageBox.Show("Введите единицу измерения!", Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUnit.Focus();
                return;
            }

            if (!decimal.TryParse(txtPrice.Text, out var price))
            {
                MessageBox.Show(Constants.Messages.PriceInvalid, Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPrice.Focus();
                return;
            }

            if (price <= 0)
            {
                MessageBox.Show("Цена должна быть больше нуля!", Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPrice.Focus();
                return;
            }

            int? shelfLife = null;
            if (!string.IsNullOrEmpty(txtShelfLife.Text))
            {
                if (int.TryParse(txtShelfLife.Text, out var sl) && sl > 0)
                {
                    shelfLife = sl;
                }
            }

            int? categoryId = null;
            if (cmbCategory.SelectedValue != null && cmbCategory.SelectedValue is int)
            {
                categoryId = (int)cmbCategory.SelectedValue;
            }

            try
            {
                if (_isEdit)
                {
                    var sql = @"UPDATE Products 
                                SET Article = @Article, Name = @Name, CategoryId = @CategoryId,
                                    UnitOfMeasure = @Unit, PurchasePrice = @Price, ShelfLife = @ShelfLife
                                WHERE Id = @Id";

                    var parameters = new[]
                    {
                        new NpgsqlParameter("@Article", txtArticle.Text),
                        new NpgsqlParameter("@Name", txtName.Text),
                        new NpgsqlParameter("@CategoryId", categoryId.HasValue ? (object)categoryId.Value : DBNull.Value),
                        new NpgsqlParameter("@Unit", txtUnit.Text),
                        new NpgsqlParameter("@Price", price),
                        new NpgsqlParameter("@ShelfLife", shelfLife.HasValue ? (object)shelfLife.Value : DBNull.Value),
                        new NpgsqlParameter("@Id", _productId)
                    };

                    DatabaseHelper.ExecuteNonQuery(sql, parameters);
                    AppLogger.Info($"Обновлен товар: {txtArticle.Text}");
                }
                else
                {
                    var insertSql = @"INSERT INTO Products (Article, Name, CategoryId, UnitOfMeasure, PurchasePrice, ShelfLife) 
                                      VALUES (@Article, @Name, @CategoryId, @Unit, @Price, @ShelfLife)";

                    var insertParams = new[]
                    {
                        new NpgsqlParameter("@Article", txtArticle.Text),
                        new NpgsqlParameter("@Name", txtName.Text),
                        new NpgsqlParameter("@CategoryId", categoryId.HasValue ? (object)categoryId.Value : DBNull.Value),
                        new NpgsqlParameter("@Unit", txtUnit.Text),
                        new NpgsqlParameter("@Price", price),
                        new NpgsqlParameter("@ShelfLife", shelfLife.HasValue ? (object)shelfLife.Value : DBNull.Value)
                    };

                    DatabaseHelper.ExecuteNonQuery(insertSql, insertParams);

                    var lastIdSql = "SELECT lastval()";
                    var newId = Convert.ToInt32(DatabaseHelper.ExecuteScalar(lastIdSql));

                    var stockSql = "INSERT INTO StockBalances (ProductId, Quantity) VALUES (@ProductId, 0)";
                    var stockParams = new[] { new NpgsqlParameter("@ProductId", newId) };
                    DatabaseHelper.ExecuteNonQuery(stockSql, stockParams);

                    AppLogger.Info($"Добавлен новый товар: {txtArticle.Text}");
                }

                MessageBox.Show(Constants.Messages.SaveSuccess, Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                AppLogger.Error(ex, "Ошибка сохранения товара");
                MessageBox.Show(Constants.Messages.ConnectionError, Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void txtPrice_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
                e.KeyChar != ',' && e.KeyChar != '.')
            {
                e.Handled = true;
            }
        }

        private void txtShelfLife_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }
    }
}
