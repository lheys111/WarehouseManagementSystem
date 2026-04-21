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
            this.Text = "Добавление товара";
            this.btnSave.Click += btnSave_Click;
            this.btnCancel.Click += btnCancel_Click;
            LoadCategories();
            SetupExpiryDate();
        }

        public FormProductEdit(int id)
        {
            InitializeComponent();
            this.Text = "Редактирование товара";
            _productId = id;
            _isEdit = true;
            this.btnSave.Click += btnSave_Click;
            this.btnCancel.Click += btnCancel_Click;
            LoadCategories();
            SetupExpiryDate();
            LoadProductData();
        }
        private void SetupExpiryDate()
        {
            dtpExpiryDate.Format = DateTimePickerFormat.Short;
            dtpExpiryDate.Value = DateTime.Now.AddDays(14);
            dtpExpiryDate.Enabled = false;

            chkNoExpiry.Checked = true;
            chkNoExpiry.CheckedChanged += chkNoExpiry_CheckedChanged;
        }

        private void LoadCategories()
        {
        
            try
            {
                string sql = "SELECT Id, Name FROM Categories ORDER BY Name";
                var data = DatabaseHelper.ExecuteQuery(sql);


                cmbCategory.DataSource = data;
                cmbCategory.DisplayMember = "Name";
                cmbCategory.ValueMember = "Id";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        
        }

        private void LoadProductData()
        {
            if (!_isEdit) return;

            try
            {
                string sql = "SELECT * FROM Products WHERE Id = @Id";
                var parameters = new[] { new NpgsqlParameter("@Id", _productId) };
                var data = DatabaseHelper.ExecuteQuery(sql, parameters);

                if (data.Rows.Count > 0)
                {
                    var row = data.Rows[0];
                    txtArticle.Text = row["Article"].ToString();
                    txtName.Text = row["Name"].ToString();

                    if (row["CategoryId"] != DBNull.Value)
                    {
                        int catId = Convert.ToInt32(row["CategoryId"]);
                        cmbCategory.SelectedValue = catId;
                    }

                    txtUnit.Text = row["UnitOfMeasure"].ToString();
                    txtPrice.Text = row["PurchasePrice"].ToString();

                    if (row["ExpiryDate"] != DBNull.Value)
                    {
                        DateTime expiryDate = Convert.ToDateTime(row["ExpiryDate"]);
                        dtpExpiryDate.Value = expiryDate;
                        chkNoExpiry.Checked = false;
                        dtpExpiryDate.Enabled = true;
                    }
                    else
                    {
                        chkNoExpiry.Checked = true;
                        dtpExpiryDate.Enabled = false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки: " + ex.Message);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
         
            if (string.IsNullOrEmpty(txtArticle.Text))
            {
                MessageBox.Show("Введите артикул товара!");
                txtArticle.Focus();
                return;
            }

            if (string.IsNullOrEmpty(txtName.Text))
            {
                MessageBox.Show("Введите название товара!");
                txtName.Focus();
                return;
            }

            if (string.IsNullOrEmpty(txtUnit.Text))
            {
                MessageBox.Show("Введите единицу измерения!");
                txtUnit.Focus();
                return;
            }

            decimal price;
            if (!decimal.TryParse(txtPrice.Text, out price))
            {
                MessageBox.Show("Введите корректную цену!");
                txtPrice.Focus();
                return;
            }

            if (price <= 0)
            {
                MessageBox.Show("Цена должна быть больше нуля!");
                txtPrice.Focus();
                return;
            }

            DateTime? expiryDate = null;
            if (!chkNoExpiry.Checked)
            {
                expiryDate = dtpExpiryDate.Value;
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

                    string sql = @"UPDATE Products 
              SET Article = @Article, Name = @Name, CategoryId = @CategoryId,
                  UnitOfMeasure = @Unit, PurchasePrice = @Price, ExpiryDate = @ExpiryDate
              WHERE Id = @Id";

                    var parameters = new[]
                    {
                        new NpgsqlParameter("@Article", txtArticle.Text),
                        new NpgsqlParameter("@Name", txtName.Text),
                        new NpgsqlParameter("@CategoryId", categoryId.HasValue ? (object)categoryId.Value : DBNull.Value),
                        new NpgsqlParameter("@Unit", txtUnit.Text),
                        new NpgsqlParameter("@Price", price),
                        new NpgsqlParameter("@ExpiryDate", expiryDate.HasValue ? (object)expiryDate.Value : DBNull.Value),
                        new NpgsqlParameter("@Id", _productId)
                    };

                    DatabaseHelper.ExecuteNonQuery(sql, parameters);
                    MessageBox.Show("Товар успешно обновлен!");
                }
                else
                {

                    string insertSql = @"INSERT INTO Products (Article, Name, CategoryId, UnitOfMeasure, PurchasePrice, ExpiryDate) 
                    VALUES (@Article, @Name, @CategoryId, @Unit, @Price, @ExpiryDate) RETURNING Id";

                    var insertParams = new[]
                    {
                        new NpgsqlParameter("@Article", txtArticle.Text),
                        new NpgsqlParameter("@Name", txtName.Text),
                        new NpgsqlParameter("@CategoryId", categoryId.HasValue ? (object)categoryId.Value : DBNull.Value),
                        new NpgsqlParameter("@Unit", txtUnit.Text),
                        new NpgsqlParameter("@Price", price),
                       new NpgsqlParameter("@ExpiryDate", expiryDate.HasValue ? (object)expiryDate.Value : DBNull.Value)
                    };

                    int newId = Convert.ToInt32(DatabaseHelper.ExecuteScalar(insertSql, insertParams));

                   
                    string stockSql = "INSERT INTO StockBalances (ProductId, Quantity) VALUES (@ProductId, 0)";
                    var stockParams = new[] { new NpgsqlParameter("@ProductId", newId) };
                    DatabaseHelper.ExecuteNonQuery(stockSql, stockParams);

                    MessageBox.Show("Товар успешно добавлен!");
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении: " + ex.Message);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void txtShelfLife_TextChanged(object sender, EventArgs e)
        {

        }

        private void chkNoExpiry_CheckedChanged(object sender, EventArgs e)
        {
            dtpExpiryDate.Enabled = !chkNoExpiry.Checked;
        }
    }
}
