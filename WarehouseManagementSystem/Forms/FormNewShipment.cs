using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using static System.Collections.Specialized.BitVector32;
using WarehouseManagementSystem.Helpers;
using WarehouseManagementSystem.Models;

namespace WarehouseManagementSystem.Forms
{
    public partial class FormNewShipment : Form
    {
        private DataTable cartTable;
        private string shipmentNumber;

        public FormNewShipment()
        {
            InitializeComponent();
            InitializeCart();
            LoadStock();
            GenerateShipmentNumber();
            lblDate.Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm");

            // Настройка кнопок
            SetupButtons();
        }

        private void SetupButtons()
        {
            // Кнопка Добавить позицию
            btnAddItem.FlatStyle = FlatStyle.Flat;
            btnAddItem.FlatAppearance.BorderColor = Color.Black;
            btnAddItem.FlatAppearance.BorderSize = 1;
            btnAddItem.BackColor = Color.White;

            // Кнопка Удалить
            btnRemoveItem.FlatStyle = FlatStyle.Flat;
            btnRemoveItem.FlatAppearance.BorderColor = Color.Black;
            btnRemoveItem.FlatAppearance.BorderSize = 1;
            btnRemoveItem.BackColor = Color.White;

            // Кнопка Обновить
            btnRefreshStock.FlatStyle = FlatStyle.Flat;
            btnRefreshStock.FlatAppearance.BorderColor = Color.Black;
            btnRefreshStock.FlatAppearance.BorderSize = 1;
            btnRefreshStock.BackColor = Color.White;

            // Кнопка Провести отгрузку
            btnConfirm.FlatStyle = FlatStyle.Flat;
            btnConfirm.FlatAppearance.BorderColor = Color.Black;
            btnConfirm.FlatAppearance.BorderSize = 1;
            btnConfirm.BackColor = Color.White;
            btnConfirm.Font = new Font("Microsoft Sans Serif", 10, FontStyle.Bold);
        }

        private void InitializeCart()
        {
            cartTable = new DataTable();
            cartTable.Columns.Add("ProductId", typeof(int));
            cartTable.Columns.Add("Article", typeof(string));
            cartTable.Columns.Add("Name", typeof(string));
            cartTable.Columns.Add("Quantity", typeof(decimal));
            cartTable.Columns.Add("Price", typeof(decimal));
            dgvCart.DataSource = cartTable;

            dgvCart.Columns["ProductId"].Visible = false;
            dgvCart.Columns["Article"].HeaderText = "Артикул";
            dgvCart.Columns["Name"].HeaderText = "Название";
            dgvCart.Columns["Quantity"].HeaderText = "Кол-во";
            dgvCart.Columns["Price"].HeaderText = "Цена";
            dgvCart.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void LoadStock()
        {
            try
            {
                string query = @"SELECT Id, Article, Name, UnitOfMeasure, StockQuantity, PurchasePrice
                                FROM vw_ProductsWithStock
                                WHERE StockQuantity > 0
                                ORDER BY Name";
                DataTable dt = DatabaseHelper.ExecuteQuery(query);
                dgvStock.DataSource = dt;

                dgvStock.Columns["Id"].Visible = false;
                dgvStock.Columns["Article"].HeaderText = "Артикул";
                dgvStock.Columns["Name"].HeaderText = "Название";
                dgvStock.Columns["UnitOfMeasure"].HeaderText = "Ед. изм.";
                dgvStock.Columns["StockQuantity"].HeaderText = "Остаток";
                dgvStock.Columns["PurchasePrice"].HeaderText = "Цена";
                dgvStock.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки остатков: " + ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GenerateShipmentNumber()
        {
            try
            {
                string query = "SELECT generate_shipment_number()";
                shipmentNumber = DatabaseHelper.ExecuteScalar(query).ToString();
                lblDocNumber.Text = $"Номер документа: {shipmentNumber}";
            }
            catch
            {
                shipmentNumber = $"INV-{DateTime.Now:yyyyMMdd}-001";
                lblDocNumber.Text = $"Номер документа: {shipmentNumber}";
            }
        }

        // Кнопка Добавить позицию
        private void btnAddItem_Click(object sender, EventArgs e)
        {
            if (dgvStock.CurrentRow == null)
            {
                MessageBox.Show("Выберите товар из списка остатков", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int productId = Convert.ToInt32(dgvStock.CurrentRow.Cells["Id"].Value);
            string article = dgvStock.CurrentRow.Cells["Article"].Value.ToString();
            string name = dgvStock.CurrentRow.Cells["Name"].Value.ToString();
            decimal stockQuantity = Convert.ToDecimal(dgvStock.CurrentRow.Cells["StockQuantity"].Value);
            decimal price = Convert.ToDecimal(dgvStock.CurrentRow.Cells["PurchasePrice"].Value);

            // Открываем окно выбора количества
            FormSelectProduct selectForm = new FormSelectProduct(productId, article, name, stockQuantity, price);
            if (selectForm.ShowDialog() == DialogResult.OK)
            {
                var selectedItem = selectForm.SelectedProduct;

                // Проверяем, нет ли уже этого товара в корзине
                DataRow[] existing = cartTable.Select($"ProductId = {selectedItem.ProductId}");
                if (existing.Length > 0)
                {
                    MessageBox.Show("Этот товар уже добавлен в отгрузку", "Внимание",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                cartTable.Rows.Add(
                    selectedItem.ProductId,
                    selectedItem.Article,
                    selectedItem.Name,
                    selectedItem.Quantity,
                    selectedItem.Price
                );
            }
        }

        // Кнопка Удалить позицию
        private void btnRemoveItem_Click(object sender, EventArgs e)
        {
            if (dgvCart.CurrentRow != null)
            {
                cartTable.Rows.Remove(dgvCart.CurrentRow.DataBoundItem as DataRow);
            }
            else
            {
                MessageBox.Show("Выберите позицию для удаления", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // Кнопка Обновить остатки
        private void btnRefreshStock_Click(object sender, EventArgs e)
        {
            LoadStock();
            MessageBox.Show("Список остатков обновлен", "Обновление",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // Кнопка Провести отгрузку
        private void btnConfirm_Click(object sender, EventArgs e)
        {
            if (cartTable.Rows.Count == 0)
            {
                MessageBox.Show("Добавьте товары в отгрузку", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Подтвердить отгрузку? Товары будут списаны со склада.",
                "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    using (var transaction = conn.BeginTransaction())
                    {
                        // Создаем отгрузку
                        string shipmentQuery = @"INSERT INTO Shipments (ShipmentNumber, ShipmentDate, StorekeeperId, Status) 
                                                VALUES (@Number, @Date, @StorekeeperId, 'Completed') RETURNING Id";
                        using (var cmd = new NpgsqlCommand(shipmentQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@Number", shipmentNumber);
                            cmd.Parameters.AddWithValue("@Date", DateTime.Now);
                            cmd.Parameters.AddWithValue("@StorekeeperId", Session.CurrentUser.Id);
                            int shipmentId = Convert.ToInt32(cmd.ExecuteScalar());

                            // Добавляем детали и списываем остатки
                            foreach (DataRow row in cartTable.Rows)
                            {
                                int productId = Convert.ToInt32(row["ProductId"]);
                                decimal quantity = Convert.ToDecimal(row["Quantity"]);
                                decimal price = Convert.ToDecimal(row["Price"]);

                                // Проверяем остатки перед списанием
                                string checkStock = "SELECT Quantity FROM StockBalances WHERE ProductId = @ProductId";
                                using (var checkCmd = new NpgsqlCommand(checkStock, conn, transaction))
                                {
                                    checkCmd.Parameters.AddWithValue("@ProductId", productId);
                                    decimal stock = Convert.ToDecimal(checkCmd.ExecuteScalar());

                                    if (stock < quantity)
                                    {
                                        throw new Exception($"Недостаточно товара '{row["Name"]}'. Доступно: {stock}");
                                    }
                                }

                                // Добавляем деталь отгрузки
                                string detailQuery = @"INSERT INTO ShipmentDetails (ShipmentId, ProductId, Quantity, PriceAtShipment) 
                                                      VALUES (@ShipmentId, @ProductId, @Quantity, @Price)";
                                using (var detailCmd = new NpgsqlCommand(detailQuery, conn, transaction))
                                {
                                    detailCmd.Parameters.AddWithValue("@ShipmentId", shipmentId);
                                    detailCmd.Parameters.AddWithValue("@ProductId", productId);
                                    detailCmd.Parameters.AddWithValue("@Quantity", quantity);
                                    detailCmd.Parameters.AddWithValue("@Price", price);
                                    detailCmd.ExecuteNonQuery();
                                }

                                // Списываем остатки
                                string updateStock = "UPDATE StockBalances SET Quantity = Quantity - @Quantity, UpdatedAt = CURRENT_TIMESTAMP WHERE ProductId = @ProductId";
                                using (var stockCmd = new NpgsqlCommand(updateStock, conn, transaction))
                                {
                                    stockCmd.Parameters.AddWithValue("@Quantity", quantity);
                                    stockCmd.Parameters.AddWithValue("@ProductId", productId);
                                    stockCmd.ExecuteNonQuery();
                                }
                            }

                            transaction.Commit();
                        }
                    }
                }

                MessageBox.Show($"Отгрузка №{shipmentNumber} успешно проведена!\nТовары списаны со склада.",
                    "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Очищаем корзину и создаем новый номер
                cartTable.Clear();
                GenerateShipmentNumber();
                LoadStock();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при проведении отгрузки: " + ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
