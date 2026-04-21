using Npgsql;
using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using WarehouseManagementSystem.Helpers;
using WarehouseManagementSystem.Models;

namespace WarehouseManagementSystem.Forms
{
    public partial class FormNewShipment : Form
    {
        private DataTable _cartTable;
        private string _shipmentNumber;

        public FormNewShipment()
        {
            InitializeComponent();
            InitializeEvents();
            InitializeCart();
            LoadStock();
            GenerateShipmentNumber();
            lblDate.Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
            SetupButtons();
            Text = "Оформление новой отгрузки";
        }

        private void InitializeEvents()
        {
            this.btnAddItem.Click += btnAddItem_Click;
            this.btnRemoveItem.Click += btnRemoveItem_Click;
            this.btnRefreshStock.Click += btnRefreshStock_Click;
            this.btnConfirm.Click += btnConfirm_Click;
        }

        private void SetupButtons()
        {
            btnAddItem.FlatStyle = FlatStyle.Flat;
            btnAddItem.FlatAppearance.BorderColor = Color.Black;
            btnAddItem.FlatAppearance.BorderSize = 1;
            btnAddItem.BackColor = Color.White;
            btnAddItem.Text = "Добавить позицию";

            btnRemoveItem.FlatStyle = FlatStyle.Flat;
            btnRemoveItem.FlatAppearance.BorderColor = Color.Black;
            btnRemoveItem.FlatAppearance.BorderSize = 1;
            btnRemoveItem.BackColor = Color.White;
            btnRemoveItem.Text = "Удалить";

            btnRefreshStock.FlatStyle = FlatStyle.Flat;
            btnRefreshStock.FlatAppearance.BorderColor = Color.Black;
            btnRefreshStock.FlatAppearance.BorderSize = 1;
            btnRefreshStock.BackColor = Color.White;
            btnRefreshStock.Text = "Обновить";

            btnConfirm.FlatStyle = FlatStyle.Flat;
            btnConfirm.FlatAppearance.BorderColor = Color.Black;
            btnConfirm.FlatAppearance.BorderSize = 1;
            btnConfirm.BackColor = Color.White;
            btnConfirm.Font = new Font("Microsoft Sans Serif", 10, FontStyle.Bold);
            btnConfirm.Text = "Провести отгрузку";
        }

        private void InitializeCart()
        {
            _cartTable = new DataTable();
            _cartTable.Columns.Add("ProductId", typeof(int));
            _cartTable.Columns.Add("Article", typeof(string));
            _cartTable.Columns.Add("Name", typeof(string));
            _cartTable.Columns.Add("Quantity", typeof(decimal));
            _cartTable.Columns.Add("Price", typeof(decimal));
            dgvCart.DataSource = _cartTable;

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
                string sql = @"SELECT Id, Article, Name, UnitOfMeasure, StockQuantity
                              FROM vw_ProductsWithStock
                              WHERE StockQuantity > 0
                              ORDER BY Name";
                var data = DatabaseHelper.ExecuteQuery(sql);
                dgvStock.DataSource = data;

                dgvStock.Columns["Id"].Visible = false;
                dgvStock.Columns["Article"].HeaderText = "Артикул";
                dgvStock.Columns["Name"].HeaderText = "Название";
                dgvStock.Columns["UnitOfMeasure"].HeaderText = "Ед. изм.";
                dgvStock.Columns["StockQuantity"].HeaderText = "Остаток";
                dgvStock.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                MessageBox.Show($"Загружено товаров с остатками: {data.Rows.Count}");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки: " + ex.Message);
            }
        }

        private void RefreshStock()
        {
            try
            {
                string sql = @"SELECT Id, Article, Name, UnitOfMeasure, StockQuantity
                              FROM vw_ProductsWithStock
                              WHERE StockQuantity > 0
                              ORDER BY Name";
                var data = DatabaseHelper.ExecuteQuery(sql);
                dgvStock.DataSource = null;
                dgvStock.DataSource = data;

                dgvStock.Columns["Id"].Visible = false;
                dgvStock.Columns["Article"].HeaderText = "Артикул";
                dgvStock.Columns["Name"].HeaderText = "Название";
                dgvStock.Columns["UnitOfMeasure"].HeaderText = "Ед. изм.";
                dgvStock.Columns["StockQuantity"].HeaderText = "Остаток";
                dgvStock.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dgvStock.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка обновления: " + ex.Message);
            }
        }

        private void GenerateShipmentNumber()
        {
            string datePart = DateTime.Now.ToString("yyyyMMdd");
            _shipmentNumber = $"SHP-{datePart}-001";
            lblDocNumber.Text = $"Номер документа: {_shipmentNumber}";
        }

        private void btnAddItem_Click(object sender, EventArgs e)
        {
            if (dgvStock.CurrentRow == null)
            {
                MessageBox.Show("Выберите товар из списка остатков");
                return;
            }

            var productId = Convert.ToInt32(dgvStock.CurrentRow.Cells["Id"].Value);
            var article = dgvStock.CurrentRow.Cells["Article"].Value.ToString();
            var name = dgvStock.CurrentRow.Cells["Name"].Value.ToString();
            var stockQuantity = Convert.ToDecimal(dgvStock.CurrentRow.Cells["StockQuantity"].Value);

            var selectForm = new FormSelectProduct(productId, article, name, stockQuantity, 0);
            if (selectForm.ShowDialog() == DialogResult.OK)
            {
                var selected = selectForm.SelectedProduct;

                var existing = _cartTable.Select($"ProductId = {selected.ProductId}");
                if (existing.Length > 0)
                {
                    MessageBox.Show("Этот товар уже добавлен в отгрузку");
                    return;
                }

                _cartTable.Rows.Add(selected.ProductId, selected.Article, selected.Name, selected.Quantity, 0);
                MessageBox.Show($"Добавлено: {selected.Name} - {selected.Quantity} шт.");
            }
        }

        private void btnRemoveItem_Click(object sender, EventArgs e)
        {
            if (dgvCart.CurrentRow != null)
            {
                var row = dgvCart.CurrentRow.DataBoundItem as DataRowView;
                if (row != null)
                {
                    _cartTable.Rows.Remove(row.Row);
                }
            }
            else
            {
                MessageBox.Show("Выберите позицию для удаления");
            }
        }

        private void btnRefreshStock_Click(object sender, EventArgs e)
        {
            RefreshStock();
            MessageBox.Show("Список остатков обновлен");
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
             
            if (Session.CurrentUser == null)
            {
                MessageBox.Show("Ошибка: пользователь не авторизован!");
                return;
            }

            MessageBox.Show($"ID пользователя: {Session.CurrentUser.Id}\nИмя: {Session.CurrentUser.FullName}", "Проверка");

            if (_cartTable.Rows.Count == 0)
            {
                MessageBox.Show("Добавьте товары в отгрузку");
                return;
            }

            string itemsList = "СОСТАВ ОТГРУЗКИ:\n";
            foreach (DataRow row in _cartTable.Rows)
            {
                itemsList += $"- {row["Name"]}: {row["Quantity"]} шт.\n";
            }

            var confirm = MessageBox.Show($"{itemsList}\nПодтвердить отгрузку? Товары будут списаны.",
                "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    using (var tran = conn.BeginTransaction())
                    {
                        
                        string shipmentSql = @"INSERT INTO Shipments (ShipmentNumber, ShipmentDate, StorekeeperId, Status) 
                                              VALUES (@Number, @Date, @StorekeeperId, 'Completed') RETURNING Id";
                        int shipmentId;
                        using (var cmd = new NpgsqlCommand(shipmentSql, conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@Number", _shipmentNumber);
                            cmd.Parameters.AddWithValue("@Date", DateTime.Now);
                            cmd.Parameters.AddWithValue("@StorekeeperId", Session.CurrentUser.Id);
                            shipmentId = Convert.ToInt32(cmd.ExecuteScalar());
                            MessageBox.Show($"Создана отгрузка ID={shipmentId}");
                        }
 
                        foreach (DataRow row in _cartTable.Rows)
                        {
                            var productId = Convert.ToInt32(row["ProductId"]);
                            var quantity = Convert.ToDecimal(row["Quantity"]);
                            var productName = row["Name"].ToString();

                        
                            string detailSql = @"INSERT INTO ShipmentDetails (ShipmentId, ProductId, Quantity, PriceAtShipment) 
                                                VALUES (@ShipmentId, @ProductId, @Quantity, 0)";
                            using (var detailCmd = new NpgsqlCommand(detailSql, conn, tran))
                            {
                                detailCmd.Parameters.AddWithValue("@ShipmentId", shipmentId);
                                detailCmd.Parameters.AddWithValue("@ProductId", productId);
                                detailCmd.Parameters.AddWithValue("@Quantity", quantity);
                                detailCmd.ExecuteNonQuery();
                            }

                            // FIFO
                            try
                            {
                                ShipProduct(productId, quantity);
                                MessageBox.Show($"Списано {quantity} шт. товара {productName} (FIFO)");
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Ошибка списания {productName}: {ex.Message}");
                                throw;
                            }
                        }
                        tran.Commit();
                        MessageBox.Show("Транзакция завершена!");
                    }
                }

                MessageBox.Show($"Отгрузка №{_shipmentNumber} проведена!");

                _cartTable.Clear();
                RefreshStock();
                GenerateShipmentNumber();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Ошибка: {ex.Message}");
            }


        }

        private void ShipProduct(int productId, decimal quantityToShip)
        {
            decimal remaining = quantityToShip;

            string sql = @"
    SELECT Id, Quantity, ExpiryDate, ReceivedDate FROM StockBatches 
    WHERE ProductId = @productId AND Quantity > 0
    ORDER BY ExpiryDate ASC NULLS LAST, ReceivedDate ASC";

            var parameters = new[] { new NpgsqlParameter("@productId", productId) };
            var batches = DatabaseHelper.ExecuteQuery(sql, parameters);

            if (batches.Rows.Count == 0)
            {
                throw new Exception($"Нет партий товара для списания");
            }

            foreach (DataRow batch in batches.Rows)
            {
                if (remaining <= 0) break;

                int batchId = Convert.ToInt32(batch["Id"]);
                decimal batchQty = Convert.ToDecimal(batch["Quantity"]);
                decimal take = Math.Min(remaining, batchQty);
                Debug.WriteLine($"Списано {take} кг из партии {batchId}");

                if (take >= batchQty)
                {
                    string deleteSql = "DELETE FROM StockBatches WHERE Id = @batchId";
                    DatabaseHelper.ExecuteNonQuery(deleteSql, new[]
                    {
                new NpgsqlParameter("@batchId", batchId)
            });
                }
                else
                {
                    string updateSql = "UPDATE StockBatches SET Quantity = Quantity - @take WHERE Id = @batchId";
                    DatabaseHelper.ExecuteNonQuery(updateSql, new[]
                    {
                new NpgsqlParameter("@take", take),
                new NpgsqlParameter("@batchId", batchId)
            });
                }

                remaining -= take;
            }

            if (remaining > 0)
            {
                throw new Exception($"Недостаточно товара на складе. Не хватает: {remaining}");
            }

            string updateBalance = "UPDATE StockBalances SET Quantity = Quantity - @qty WHERE ProductId = @pid";
            DatabaseHelper.ExecuteNonQuery(updateBalance, new[]
            {
        new NpgsqlParameter("@qty", quantityToShip),
        new NpgsqlParameter("@pid", productId)
    });
        }
    }
}
