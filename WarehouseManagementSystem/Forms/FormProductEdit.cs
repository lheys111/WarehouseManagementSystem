using System;
using System.Data;
using Npgsql;
using System.Windows.Forms;
using WarehouseManagementSystem.Helpers;

namespace WarehouseManagementSystem.Forms
{
    public partial class FormProductEdit : Form
    {
        private int productId = -1;
        private bool isEdit = false;

        public FormProductEdit()
        {
            InitializeComponent();
            this.Text = "Добавление товара";
            this.btnSave.Click += new EventHandler(this.btnSave_Click);
            this.btnCancel.Click += new EventHandler(this.btnCancel_Click);
            LoadCategories();
        }

        public FormProductEdit(int id)
        {
            InitializeComponent();
            this.Text = "Редактирование товара";
            productId = id;
            isEdit = true;
            LoadCategories();
            LoadProductData();
        }

        private void LoadCategories()
        {
            try
            {
                string sql = "SELECT Id, Name FROM Categories ORDER BY Name";
                DataTable data = DatabaseHelper.ExecuteQuery(sql);

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
                MessageBox.Show("Ошибка загрузки категорий: " + ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadProductData()
        {
            if (!isEdit) return;

            try
            {
                string sql = "SELECT * FROM Products WHERE Id = @Id";
                NpgsqlParameter[] parameters = { new NpgsqlParameter("@Id", productId) };
                DataTable data = DatabaseHelper.ExecuteQuery(sql, parameters);

                if (data.Rows.Count > 0)
                {
                    DataRow row = data.Rows[0];
                    txtArticle.Text = row["Article"].ToString();
                    txtName.Text = row["Name"].ToString();

                    if (row["CategoryId"] != DBNull.Value)
                    {
                        int catId = Convert.ToInt32(row["CategoryId"]);
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
                MessageBox.Show("Ошибка загрузки данных товара: " + ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtArticle.Text))
            {
                MessageBox.Show("Введите артикул товара!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtArticle.Focus();
                return;
            }

            if (string.IsNullOrEmpty(txtName.Text))
            {
                MessageBox.Show("Введите название товара!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }

            if (string.IsNullOrEmpty(txtUnit.Text))
            {
                MessageBox.Show("Введите единицу измерения!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUnit.Focus();
                return;
            }

            decimal price;
            if (!decimal.TryParse(txtPrice.Text, out price))
            {
                MessageBox.Show("Введите корректную цену закупки!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPrice.Focus();
                return;
            }

            if (price <= 0)
            {
                MessageBox.Show("Цена должна быть больше нуля!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPrice.Focus();
                return;
            }

            int? shelfLife = null;
            if (!string.IsNullOrEmpty(txtShelfLife.Text))
            {
                int sl;
                if (int.TryParse(txtShelfLife.Text, out sl) && sl > 0)
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
                if (isEdit)
                {
                    string sql = @"UPDATE Products 
                                  SET Article = @Article, Name = @Name, CategoryId = @CategoryId,
                                      UnitOfMeasure = @Unit, PurchasePrice = @Price, ShelfLife = @ShelfLife
                                  WHERE Id = @Id";

                    NpgsqlParameter[] parameters = {
                        new NpgsqlParameter("@Article", txtArticle.Text),
                        new NpgsqlParameter("@Name", txtName.Text),
                        new NpgsqlParameter("@CategoryId", categoryId.HasValue ? (object)categoryId.Value : DBNull.Value),
                        new NpgsqlParameter("@Unit", txtUnit.Text),
                        new NpgsqlParameter("@Price", price),
                        new NpgsqlParameter("@ShelfLife", shelfLife.HasValue ? (object)shelfLife.Value : DBNull.Value),
                        new NpgsqlParameter("@Id", productId)
                    };

                    DatabaseHelper.ExecuteNonQuery(sql, parameters);
                    MessageBox.Show("Товар успешно обновлен!", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    string insertSql = @"INSERT INTO Products (Article, Name, CategoryId, UnitOfMeasure, PurchasePrice, ShelfLife) 
                                        VALUES (@Article, @Name, @CategoryId, @Unit, @Price, @ShelfLife)";

                    NpgsqlParameter[] insertParams = {
                        new NpgsqlParameter("@Article", txtArticle.Text),
                        new NpgsqlParameter("@Name", txtName.Text),
                        new NpgsqlParameter("@CategoryId", categoryId.HasValue ? (object)categoryId.Value : DBNull.Value),
                        new NpgsqlParameter("@Unit", txtUnit.Text),
                        new NpgsqlParameter("@Price", price),
                        new NpgsqlParameter("@ShelfLife", shelfLife.HasValue ? (object)shelfLife.Value : DBNull.Value)
                    };

                    DatabaseHelper.ExecuteNonQuery(insertSql, insertParams);

                    string lastIdSql = "SELECT lastval()";
                    int newId = Convert.ToInt32(DatabaseHelper.ExecuteScalar(lastIdSql));

                    string stockSql = "INSERT INTO StockBalances (ProductId, Quantity) VALUES (@ProductId, 0)";
                    NpgsqlParameter[] stockParams = { new NpgsqlParameter("@ProductId", newId) };
                    DatabaseHelper.ExecuteNonQuery(stockSql, stockParams);

                    MessageBox.Show("Товар успешно добавлен!", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении: " + ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
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
