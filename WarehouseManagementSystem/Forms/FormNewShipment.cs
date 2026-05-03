using Npgsql;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Utilities;
using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using WarehouseManagementSystem.Helpers;
using WarehouseManagementSystem.Models;
using Microsoft.VisualBasic;

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

                MessageBox.Show(string.Format(String.ProductsLoaded, data.Rows.Count));
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(String.LoadError, ex.Message));
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
                MessageBox.Show(string.Format(String.UpdateError, ex.Message));
            }
        }

        private void GenerateShipmentNumber()
        {
            string datePart = DateTime.Now.ToString("yyyyMMdd");

            string sql = "SELECT COUNT(*) FROM Shipments WHERE ShipmentNumber LIKE @pattern";
            var param = new NpgsqlParameter("@pattern", $"SHP-{datePart}-%");
            var count = Convert.ToInt32(DatabaseHelper.ExecuteScalar(sql, new[] { param }));

            var nextNumber = count + 1;
            _shipmentNumber = $"SHP-{datePart}-{nextNumber:D3}";
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
            var totalStock = Convert.ToDecimal(dgvStock.CurrentRow.Cells["StockQuantity"].Value);

            // Получаем все партии товара
            DataTable batches = GetProductBatches(productId);

            // Считаем количество в каждой категории
            decimal expiredQty = 0;
            decimal nearExpiryQty = 0;
            decimal freshQty = 0;
            int discountDays = GetDiscountDays();
            int discountPercent = GetDiscountPercent();
            int markupPercent = GetMarkupPercent();
            decimal purchasePrice = GetProductPrice(productId);

            foreach (DataRow batch in batches.Rows)
            {
                DateTime? expiryDate = null;
                if (batch["ExpiryDate"] != DBNull.Value)
                {
                    expiryDate = Convert.ToDateTime(batch["ExpiryDate"]);
                }

                decimal qty = Convert.ToDecimal(batch["Quantity"]);

                if (expiryDate.HasValue)
                {
                    int daysLeft = (expiryDate.Value - DateTime.Now).Days;
                    if (daysLeft < 0)
                    {
                        expiredQty += qty;  // Просроченные товары
                    }
                    else if (daysLeft <= discountDays && daysLeft >= 0)
                    {
                        nearExpiryQty += qty;  // С истекающим сроком (скидка)
                    }
                    else
                    {
                        freshQty += qty;  // Свежие (без скидки)
                    }
                }
                else
                {
                    freshQty += qty;  // Срок не указан - свежие
                }
            }

            // Проверка: если есть просроченные товары, отгрузка запрещена
            if (expiredQty > 0)
            {
                MessageBox.Show($"Товар '{name}' содержит просроченные партии ({expiredQty} шт.)!\n" +
                                "Отгрузка просроченных товаров невозможна.\n" +
                                "Используйте форму 'Списание просрочки' (доступна администратору).",
                                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Если нет свежих и нет товаров с истекающим сроком
            if (nearExpiryQty == 0 && freshQty == 0)
            {
                MessageBox.Show($"Нет доступных партий для товара '{name}'");
                return;
            }

            // Информация о наличии
            string infoMsg = $"Товар: {name}\n";
            infoMsg += $"Всего на складе: {totalStock} шт.\n";
            if (nearExpiryQty > 0)
            {
                infoMsg += $"С истекающим сроком: {nearExpiryQty} шт. (скидка {discountPercent}%)\n";
            }
            if (freshQty > 0)
            {
                infoMsg += $"Свежих: {freshQty} шт.\n";
            }
            infoMsg += $"Введите количество для отгрузки:";

            // Простое окно ввода количества
            Form inputForm = new Form();
            inputForm.Text = "Отгрузка товара";
            inputForm.Width = 300;
            inputForm.Height = 220;
            inputForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            inputForm.StartPosition = FormStartPosition.CenterParent;
            inputForm.MaximizeBox = false;
            inputForm.MinimizeBox = false;

            Label lblInfo = new Label();
            lblInfo.Text = infoMsg;
            lblInfo.Location = new System.Drawing.Point(10, 10);
            lblInfo.Size = new System.Drawing.Size(260, 110);
            lblInfo.Font = new Font("Arial", 9);

            Label lblQty = new Label();
            lblQty.Text = "Количество:";
            lblQty.Location = new System.Drawing.Point(10, 130);
            lblQty.Size = new System.Drawing.Size(80, 25);

            TextBox txtQty = new TextBox();
            txtQty.Text = "1";
            txtQty.Location = new System.Drawing.Point(90, 130);
            txtQty.Size = new System.Drawing.Size(100, 25);

            Button btnOk = new Button();
            btnOk.Text = "OK";
            btnOk.Location = new System.Drawing.Point(200, 128);
            btnOk.Size = new System.Drawing.Size(70, 30);
            btnOk.DialogResult = DialogResult.OK;

            Button btnCancel = new Button();
            btnCancel.Text = "Отмена";
            btnCancel.Location = new System.Drawing.Point(200, 160);
            btnCancel.Size = new System.Drawing.Size(70, 30);
            btnCancel.DialogResult = DialogResult.Cancel;

            inputForm.Controls.Add(lblInfo);
            inputForm.Controls.Add(lblQty);
            inputForm.Controls.Add(txtQty);
            inputForm.Controls.Add(btnOk);
            inputForm.Controls.Add(btnCancel);

            if (inputForm.ShowDialog() != DialogResult.OK) return;

            if (!decimal.TryParse(txtQty.Text, out decimal quantity) || quantity <= 0)
            {
                MessageBox.Show("Введите корректное количество");
                return;
            }

            if (quantity > totalStock)
            {
                MessageBox.Show($"Недостаточно товара. Доступно: {totalStock} шт.");
                return;
            }

            decimal takeFromNearExpiry = Math.Min(quantity, nearExpiryQty);
            decimal takeFromFresh = quantity - takeFromNearExpiry;

            decimal priceWithMarkup = purchasePrice * (1 + markupPercent / 100m);
            decimal priceWithDiscount = priceWithMarkup * (1 - discountPercent / 100m);

            if (takeFromNearExpiry > 0)
            {
                _cartTable.Rows.Add(productId, article, name + " (скидка)", takeFromNearExpiry, priceWithDiscount);
            }
            if (takeFromFresh > 0)
            {
                _cartTable.Rows.Add(productId, article, name, takeFromFresh, priceWithMarkup);
            }

            string resultMsg = $"Добавлено: {name}\n";
            resultMsg += $"Всего: {quantity} шт.\n";
            if (takeFromNearExpiry > 0)
            {
                resultMsg += $"Со скидкой: {takeFromNearExpiry} шт. x {priceWithDiscount:N2} = {takeFromNearExpiry * priceWithDiscount:N2} руб.\n";
            }
            if (takeFromFresh > 0)
            {
                resultMsg += $"Без скидки: {takeFromFresh} шт. x {priceWithMarkup:N2} = {takeFromFresh * priceWithMarkup:N2} руб.\n";
            }
            resultMsg += $"Итого: {(takeFromNearExpiry * priceWithDiscount) + (takeFromFresh * priceWithMarkup):N2} руб.";

            MessageBox.Show(resultMsg, "Добавлено в отгрузку", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private DataTable GetProductBatches(int productId)
        {
            string sql = @"
        SELECT 
            Id,
            Quantity,
            ExpiryDate
        FROM StockBatches 
        WHERE ProductId = @pid AND Quantity > 0
        ORDER BY ExpiryDate ASC NULLS LAST";

            var param = new NpgsqlParameter("@pid", productId);
            return DatabaseHelper.ExecuteQuery(sql, new[] { param });
        }

        private decimal GetProductPrice(int productId)
        {
            string sql = "SELECT PurchasePrice FROM Products WHERE Id = @id";
            var param = new NpgsqlParameter("@id", productId);
            var result = DatabaseHelper.ExecuteScalar(sql, new[] { param });
            return result != null ? Convert.ToDecimal(result) : 0;
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
                MessageBox.Show(String.SelectItemToDelete);
            }
        }

        private void btnRefreshStock_Click(object sender, EventArgs e)
        {
            RefreshStock();
            MessageBox.Show(String.StockRefreshed);
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
             
            if (Session.CurrentUser == null)
            {
                MessageBox.Show(String.UserNotAuthorized);
                return;
            }

            MessageBox.Show(string.Format(String.UserInfoMessage, Session.CurrentUser.Id, Session.CurrentUser.FullName), String.UserCheckTitle);

            if (_cartTable.Rows.Count == 0)
            {
                MessageBox.Show(String.AddProductsToShipment);
                return;
            }

            string itemsList = "СОСТАВ ОТГРУЗКИ:\n";
            foreach (DataRow row in _cartTable.Rows)
            {
                itemsList += $"- {row["Name"]}: {row["Quantity"]} шт.\n";
            }

            var confirm = MessageBox.Show(string.Format(String.ConfirmShipmentMessage, itemsList),
    String.ConfirmShipmentTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

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
                            MessageBox.Show(string.Format(String.ShipmentCreated, shipmentId));
                        }
 
                        foreach (DataRow row in _cartTable.Rows)
                        {
                            var productId = Convert.ToInt32(row["ProductId"]);
                            var quantity = Convert.ToDecimal(row["Quantity"]);
                            var price = Convert.ToDecimal(row["Price"]);
                            var productName = row["Name"].ToString();



                            string detailSql = @"INSERT INTO ShipmentDetails (ShipmentId, ProductId, Quantity, PriceAtShipment) 
                            VALUES (@ShipmentId, @ProductId, @Quantity, @Price)";
                            using (var detailCmd = new NpgsqlCommand(detailSql, conn, tran))
                            {
                                detailCmd.Parameters.AddWithValue("@ShipmentId", shipmentId);
                                detailCmd.Parameters.AddWithValue("@ProductId", productId);
                                detailCmd.Parameters.AddWithValue("@Quantity", quantity);
                                detailCmd.Parameters.AddWithValue("@Price", price);
                                detailCmd.ExecuteNonQuery();
                            }

                            // FIFO
                            try
                            {
                                ShipProduct(productId, quantity);
                                MessageBox.Show(string.Format(String.ItemsWrittenOffFifo, quantity, productName));
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(string.Format(String.WriteOffError, productName, ex.Message));
                                throw;
                            }
                        }
                        tran.Commit();
                        MessageBox.Show(String.TransactionCompleted);
                    }
                }

                MessageBox.Show(string.Format(String.ShipmentCompleted, _shipmentNumber));

                _cartTable.Clear();
                RefreshStock();
                GenerateShipmentNumber();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(String.ErrorPrefix, ex.Message));
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
        private int GetMarkupPercent()
        {
            try
            {
                string sql = "SELECT SettingValue FROM AppSettings WHERE SettingKey = 'MarkupPercent'";
                var result = DatabaseHelper.ExecuteScalar(sql);
                return result != null ? Convert.ToInt32(result) : 20;
            }
            catch
            {
                return 20;
            }

        }
        // Получить срок годности партии
        private DateTime? GetBatchExpiryDate(int productId)
        {
            string sql = @"
        SELECT ExpiryDate FROM StockBatches 
        WHERE ProductId = @pid AND Quantity > 0
        ORDER BY ExpiryDate ASC NULLS LAST
        LIMIT 1";
            var param = new NpgsqlParameter("@pid", productId);
            var result = DatabaseHelper.ExecuteScalar(sql, new[] { param });
            return result != null && result != DBNull.Value ? Convert.ToDateTime(result) : (DateTime?)null;
        }

        // Получить процент скидки из настроек
        private int GetDiscountPercent()
        {
            try
            {
                string sql = "SELECT SettingValue FROM AppSettings WHERE SettingKey = 'DiscountPercentage'";
                var result = DatabaseHelper.ExecuteScalar(sql);
                return result != null ? Convert.ToInt32(result) : 20;
            }
            catch
            {
                return 20;
            }
        }

        // Получить количество дней для скидки
        private int GetDiscountDays()
        {
            try
            {
                string sql = "SELECT SettingValue FROM AppSettings WHERE SettingKey = 'DiscountDaysBeforeExpiry'";
                var result = DatabaseHelper.ExecuteScalar(sql);
                return result != null ? Convert.ToInt32(result) : 30;
            }
            catch
            {
                return 30;
            }
        }


    }
}
