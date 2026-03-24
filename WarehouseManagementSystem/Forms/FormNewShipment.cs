using Npgsql;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using WarehouseManagementSystem.Helpers;
using WarehouseManagementSystem.Models;
using WarehouseManagementSystem.Services;

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
            Text = Constants.FormTitles.NewShipment;
        }

        private void InitializeEvents()
        {
            btnAddItem.Click += btnAddItem_Click;
            btnRemoveItem.Click += btnRemoveItem_Click;
            btnRefreshStock.Click += btnRefreshStock_Click;
            btnConfirm.Click += btnConfirm_Click;
        }

        private void SetupButtons()
        {
            var buttons = new[] { btnAddItem, btnRemoveItem, btnRefreshStock, btnConfirm };
            foreach (var btn in buttons)
            {
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderColor = Color.Black;
                btn.FlatAppearance.BorderSize = 1;
                btn.BackColor = Color.White;
            }
            btnConfirm.Font = new Font("Microsoft Sans Serif", 10, FontStyle.Bold);
            btnAddItem.Text = Constants.ButtonText.AddItem;
            btnRemoveItem.Text = Constants.ButtonText.RemoveItem;
            btnRefreshStock.Text = Constants.ButtonText.Refresh;
            btnConfirm.Text = Constants.ButtonText.Confirm;
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
            dgvCart.Columns["Article"].HeaderText = Constants.GridHeaders.Article;
            dgvCart.Columns["Name"].HeaderText = Constants.GridHeaders.Name;
            dgvCart.Columns["Quantity"].HeaderText = Constants.GridHeaders.Quantity;
            dgvCart.Columns["Price"].HeaderText = Constants.GridHeaders.Price;
            dgvCart.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void LoadStock()
        {
            try
            {
                var sql = @"SELECT Id, Article, Name, UnitOfMeasure, StockQuantity, PurchasePrice
                   FROM vw_ProductsWithStock
                   WHERE StockQuantity > 0
                   ORDER BY Name";

                var data = DatabaseHelper.ExecuteQuery(sql);
                dgvStock.DataSource = data;

                if (dgvStock.Columns.Contains("Id"))
                    dgvStock.Columns["Id"].Visible = false;
                if (dgvStock.Columns.Contains("Article"))
                    dgvStock.Columns["Article"].HeaderText = "Артикул";
                if (dgvStock.Columns.Contains("Name"))
                    dgvStock.Columns["Name"].HeaderText = "Название";
                if (dgvStock.Columns.Contains("UnitOfMeasure"))
                    dgvStock.Columns["UnitOfMeasure"].HeaderText = "Ед. изм.";
                if (dgvStock.Columns.Contains("StockQuantity"))
                    dgvStock.Columns["StockQuantity"].HeaderText = "Остаток";
                if (dgvStock.Columns.Contains("PurchasePrice"))
                    dgvStock.Columns["PurchasePrice"].HeaderText = "Цена";

                dgvStock.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

               
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки остатков: " + ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefreshStock()
        {
            
            dgvStock.DataSource = null;
            LoadStock();
            dgvStock.Update();
            dgvStock.Refresh();
        }
        private void GenerateShipmentNumber()
        {
            try
            {
                _shipmentNumber = DatabaseHelper.ExecuteScalar(Constants.Queries.GenerateShipmentNumber).ToString();
                lblDocNumber.Text = $"Номер документа: {_shipmentNumber}";
            }
            catch
            {
                _shipmentNumber = $"INV-{DateTime.Now:yyyyMMdd}-001";
                lblDocNumber.Text = $"Номер документа: {_shipmentNumber}";
            }
        }

        private void btnAddItem_Click(object sender, EventArgs e)
        {
            

            if (dgvStock.CurrentRow == null)
            {
                MessageBox.Show("Выберите товар из списка остатков", Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var productId = Convert.ToInt32(dgvStock.CurrentRow.Cells["Id"].Value);
            var article = dgvStock.CurrentRow.Cells["Article"].Value.ToString();
            var name = dgvStock.CurrentRow.Cells["Name"].Value.ToString();
            var stockQuantity = Convert.ToDecimal(dgvStock.CurrentRow.Cells["StockQuantity"].Value);
            var price = Convert.ToDecimal(dgvStock.CurrentRow.Cells["PurchasePrice"].Value);

            var selectForm = new FormSelectProduct(productId, article, name, stockQuantity, price);
            if (selectForm.ShowDialog() == DialogResult.OK)
            {
                var selected = selectForm.SelectedProduct;

                var existing = _cartTable.Select($"ProductId = {selected.ProductId}");
                if (existing.Length > 0)
                {
                    MessageBox.Show("Этот товар уже добавлен в отгрузку", Text,
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                _cartTable.Rows.Add(selected.ProductId, selected.Article, selected.Name, selected.Quantity, selected.Price);
            }
        }


        private void btnRemoveItem_Click(object sender, EventArgs e)
        {
            if (dgvCart.CurrentRow == null)
            {
                MessageBox.Show("Выберите позицию для удаления", Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var rowIndex = dgvCart.CurrentRow.Index;

         
                if (rowIndex >= 0 && rowIndex < _cartTable.Rows.Count)
                {
                    var productName = _cartTable.Rows[rowIndex]["Name"].ToString();
                    var confirm = MessageBox.Show($"Удалить товар '{productName}' из отгрузки?",
                        "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (confirm == DialogResult.Yes)
                    {
                        _cartTable.Rows[rowIndex].Delete();
                        _cartTable.AcceptChanges();

                        dgvCart.Refresh();
                    }
                }
                else
                {
                    MessageBox.Show("Ошибка: позиция не найдена", Text,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении: {ex.Message}", Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnRefreshStock_Click(object sender, EventArgs e)
        {
            RefreshStock();
            MessageBox.Show("Список остатков обновлен", "Обновление",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void ForceRefreshStock()
        {
            
            var newData = DatabaseHelper.ExecuteQuery(@"SELECT Id, Article, Name, UnitOfMeasure, StockQuantity, PurchasePrice
                                                FROM vw_ProductsWithStock
                                                WHERE StockQuantity > 0
                                                ORDER BY Name");

            dgvStock.BeginInvoke(new Action(() =>
            {
                dgvStock.DataSource = null;
                dgvStock.DataSource = newData;

                // Настройка колонок
                if (dgvStock.Columns.Contains("Id"))
                    dgvStock.Columns["Id"].Visible = false;
                if (dgvStock.Columns.Contains("Article"))
                    dgvStock.Columns["Article"].HeaderText = "Артикул";
                if (dgvStock.Columns.Contains("Name"))
                    dgvStock.Columns["Name"].HeaderText = "Название";
                if (dgvStock.Columns.Contains("UnitOfMeasure"))
                    dgvStock.Columns["UnitOfMeasure"].HeaderText = "Ед. изм.";
                if (dgvStock.Columns.Contains("StockQuantity"))
                    dgvStock.Columns["StockQuantity"].HeaderText = "Остаток";
                if (dgvStock.Columns.Contains("PurchasePrice"))
                    dgvStock.Columns["PurchasePrice"].HeaderText = "Цена";

                dgvStock.Refresh();
            }));
        }
      
              
          
        private void btnConfirm_Click(object sender, EventArgs e)
        {
            if (_cartTable.Rows.Count == 0)
            {
                MessageBox.Show("Добавьте товары в отгрузку", Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show(Constants.Messages.ShipmentConfirm, "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    using (var tran = conn.BeginTransaction())
                    {
                        var shipmentSql = @"INSERT INTO Shipments (ShipmentNumber, ShipmentDate, StorekeeperId, Status) 
                                   VALUES (@Number, @Date, @StorekeeperId, 'Completed') RETURNING Id";
                        using (var cmd = new NpgsqlCommand(shipmentSql, conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@Number", _shipmentNumber);
                            cmd.Parameters.AddWithValue("@Date", DateTime.Now);
                            cmd.Parameters.AddWithValue("@StorekeeperId", Session.CurrentUser.Id);
                            var shipmentId = Convert.ToInt32(cmd.ExecuteScalar());

                            foreach (DataRow row in _cartTable.Rows)
                            {
                                var productId = Convert.ToInt32(row["ProductId"]);
                                var quantity = Convert.ToDecimal(row["Quantity"]);
                                var price = Convert.ToDecimal(row["Price"]);

                                var checkSql = "SELECT Quantity FROM StockBalances WHERE ProductId = @ProductId";
                                using (var checkCmd = new NpgsqlCommand(checkSql, conn, tran))
                                {
                                    checkCmd.Parameters.AddWithValue("@ProductId", productId);
                                    var stock = Convert.ToDecimal(checkCmd.ExecuteScalar());

                                    if (stock < quantity)
                                    {
                                        throw new Exception($"Недостаточно товара '{row["Name"]}'. Доступно: {stock}");
                                    }
                                }

                                var detailSql = @"INSERT INTO ShipmentDetails (ShipmentId, ProductId, Quantity, PriceAtShipment) 
                                         VALUES (@ShipmentId, @ProductId, @Quantity, @Price)";
                                using (var detailCmd = new NpgsqlCommand(detailSql, conn, tran))
                                {
                                    detailCmd.Parameters.AddWithValue("@ShipmentId", shipmentId);
                                    detailCmd.Parameters.AddWithValue("@ProductId", productId);
                                    detailCmd.Parameters.AddWithValue("@Quantity", quantity);
                                    detailCmd.Parameters.AddWithValue("@Price", price);
                                    detailCmd.ExecuteNonQuery();
                                }

                                var stockSql = "UPDATE StockBalances SET Quantity = Quantity - @Quantity, UpdatedAt = CURRENT_TIMESTAMP WHERE ProductId = @ProductId";
                                using (var stockCmd = new NpgsqlCommand(stockSql, conn, tran))
                                {
                                    stockCmd.Parameters.AddWithValue("@Quantity", quantity);
                                    stockCmd.Parameters.AddWithValue("@ProductId", productId);
                                    stockCmd.ExecuteNonQuery();
                                }
                            }
                            tran.Commit();
                        }
                    }
                }

                AppLogger.Info($"Проведена отгрузка №{_shipmentNumber}");
                MessageBox.Show($"{Constants.Messages.ShipmentSuccess} №{_shipmentNumber}!",
                    "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

                _cartTable.Clear();
                dgvCart.Refresh();
                GenerateShipmentNumber();
                RefreshStock();
                MessageBox.Show("Список остатков обновлен", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                AppLogger.Error(ex, "Ошибка проведения отгрузки");
                MessageBox.Show("Ошибка при проведении отгрузки: " + ex.Message, Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}