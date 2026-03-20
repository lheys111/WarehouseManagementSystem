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

        // Конструктор для добавления нового товара
        public FormProductEdit()
        {
            InitializeComponent();
            this.Text = "Добавление товара";
            LoadCategories();
        }

        // Конструктор для редактирования существующего товара
        public FormProductEdit(int id)
        {
            InitializeComponent();
            this.Text = "Редактирование товара";
            productId = id;
            isEdit = true;
            LoadCategories();
            LoadProductData();
        }

        // Загрузка категорий в ComboBox
        private void LoadCategories()
        {
            try
            {
                string query = "SELECT Id, Name FROM Categories ORDER BY Name";
                DataTable dt = DatabaseHelper.ExecuteQuery(query);

                cmbCategory.DataSource = dt;
                cmbCategory.DisplayMember = "Name";
                cmbCategory.ValueMember = "Id";

                // Добавляем пустую категорию, если нет данных
                if (dt.Rows.Count == 0)
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

        // Загрузка данных товара для редактирования
        private void LoadProductData()
        {
            if (!isEdit) return;

            try
            {
                string query = "SELECT * FROM Products WHERE Id = @Id";
                NpgsqlParameter[] parameters = { new NpgsqlParameter("@Id", productId) };
                DataTable dt = DatabaseHelper.ExecuteQuery(query, parameters);

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    txtArticle.Text = row["Article"].ToString();
                    txtName.Text = row["Name"].ToString();

                    // Устанавливаем категорию
                    if (row["CategoryId"] != DBNull.Value)
                    {
                        int categoryId = Convert.ToInt32(row["CategoryId"]);
                        cmbCategory.SelectedValue = categoryId;
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

        // Сохранение товара
        private void btnSave_Click(object sender, EventArgs e)
        {
            // Проверка обязательных полей
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

            // Проверка цены
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

            // Проверка срока годности (необязательное поле)
            int? shelfLife = null;
            if (!string.IsNullOrEmpty(txtShelfLife.Text))
            {
                int sl;
                if (int.TryParse(txtShelfLife.Text, out sl))
                {
                    if (sl > 0)
                        shelfLife = sl;
                }
            }

            // Проверка выбора категории
            int? categoryId = null;
            if (cmbCategory.SelectedValue != null && cmbCategory.SelectedValue is int)
            {
                categoryId = (int)cmbCategory.SelectedValue;
            }

            try
            {
                if (isEdit)
                {
                    // Обновление существующего товара
                    string query = @"UPDATE Products 
                                    SET Article = @Article, 
                                        Name = @Name, 
                                        CategoryId = @CategoryId,
                                        UnitOfMeasure = @Unit, 
                                        PurchasePrice = @Price, 
                                        ShelfLife = @ShelfLife
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

                    DatabaseHelper.ExecuteNonQuery(query, parameters);
                    MessageBox.Show("Товар успешно обновлен!", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // Добавление нового товара
                    string query = @"INSERT INTO Products (Article, Name, CategoryId, UnitOfMeasure, PurchasePrice, ShelfLife) 
                                    VALUES (@Article, @Name, @CategoryId, @Unit, @Price, @ShelfLife)";

                    NpgsqlParameter[] parameters = {
                        new NpgsqlParameter("@Article", txtArticle.Text),
                        new NpgsqlParameter("@Name", txtName.Text),
                        new NpgsqlParameter("@CategoryId", categoryId.HasValue ? (object)categoryId.Value : DBNull.Value),
                        new NpgsqlParameter("@Unit", txtUnit.Text),
                        new NpgsqlParameter("@Price", price),
                        new NpgsqlParameter("@ShelfLife", shelfLife.HasValue ? (object)shelfLife.Value : DBNull.Value)
                    };

                    DatabaseHelper.ExecuteNonQuery(query, parameters);

                    // Получаем ID нового товара
                    string lastIdQuery = "SELECT lastval()";
                    int newId = Convert.ToInt32(DatabaseHelper.ExecuteScalar(lastIdQuery));

                    // Добавляем запись об остатках (0)
                    string stockQuery = "INSERT INTO StockBalances (ProductId, Quantity) VALUES (@ProductId, 0)";
                    NpgsqlParameter[] stockParams = { new NpgsqlParameter("@ProductId", newId) };
                    DatabaseHelper.ExecuteNonQuery(stockQuery, stockParams);

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

        // Отмена
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        // Валидация цены при вводе
        private void txtPrice_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Разрешаем цифры, запятую, точку и Backspace
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
                e.KeyChar != ',' && e.KeyChar != '.')
            {
                e.Handled = true;
            }
        }

        // Валидация срока годности
        private void txtShelfLife_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Разрешаем только цифры и Backspace
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }
    }
}
