using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WarehouseManagementSystem.Helpers;
using WarehouseManagementSystem.Models;

namespace WarehouseManagementSystem.Forms
{
    public partial class FormSupply : Form
    {
        private DataTable itemsTable;
        private int _currentUserId;
        public FormSupply(int userId)
        {
            InitializeComponent();
            _currentUserId = userId;
            dtpSupplyDate.Value = DateTime.Now;
            dtpSupplyDate.ValueChanged += (s, e) => GenerateDocumentNumber();
            CreateItemsTable();
            GenerateDocumentNumber();
        }

        private void FormSupply_Load(object sender, EventArgs e)
        {

        }
        private void GenerateDocumentNumber()
        {
            string datePart = dtpSupplyDate.Value.ToString("yyyyMMdd");

            string sql = "SELECT COUNT(*) FROM Shipments WHERE ShipmentNumber LIKE @pattern";
            var param = new NpgsqlParameter("@pattern", $"INV-{datePart}-%");
            int count = Convert.ToInt32(DatabaseHelper.ExecuteScalar(sql, new[] { param }));

            int nextNumber = count + 1;
            txtDocumentNumber.Text = $"INV-{datePart}-{nextNumber:D3}";
        }
        private void btnGenerateNumber_Click(object sender, EventArgs e)
        {
            GenerateDocumentNumber();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void btnAddItem_Click(object sender, EventArgs e)
        {
            using (var selectForm = new FormChooseProduct()) 
            {
                if (selectForm.ShowDialog() == DialogResult.OK)
                {
                    ProductDto product = selectForm.SelectedProduct;
                    MessageBox.Show($"Добавлен товар: ID={product.Id}, Название={product.Name}");

                    foreach (DataRow row in itemsTable.Rows)
                    {
                        if (Convert.ToInt32(row["ProductId"]) == product.Id)
                        {
                            MessageBox.Show("Этот товар уже добавлен");
                            return;
                        }
                    }

                    itemsTable.Rows.Add(
                        product.Id,
                        product.Article,
                        product.Name,
                        1,
                        product.PurchasePrice,
                        DBNull.Value
                    );
                }
            }
        }
        private void CreateItemsTable()
        {
            itemsTable = new DataTable();

            itemsTable.Columns.Add("ProductId", typeof(int)); 
            itemsTable.Columns.Add("Article", typeof(string)); 
            itemsTable.Columns.Add("ProductName", typeof(string));
            itemsTable.Columns.Add("Quantity", typeof(decimal)); 
            itemsTable.Columns.Add("PurchasePrice", typeof(decimal));
            itemsTable.Columns.Add("ExpiryDate", typeof(DateTime));

            dgvItems.DataSource = itemsTable;

            dgvItems.Columns["ProductId"].Visible = false;      
            dgvItems.Columns["Article"].HeaderText = "Артикул";
            dgvItems.Columns["ProductName"].HeaderText = "Название";
            dgvItems.Columns["Quantity"].HeaderText = "Кол-во";
            dgvItems.Columns["PurchasePrice"].HeaderText = "Цена";
            dgvItems.Columns["ExpiryDate"].HeaderText = "Срок годности";
            dgvItems.Columns["ExpiryDate"].DefaultCellStyle.Format = "dd.MM.yyyy";


        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (itemsTable.Rows.Count == 0)
            {
                MessageBox.Show("Добавьте хотя бы один товар", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            foreach (DataRow row in itemsTable.Rows)
            {
                decimal quantity = Convert.ToDecimal(row["Quantity"]);
                decimal price = Convert.ToDecimal(row["PurchasePrice"]);

                if (quantity <= 0)
                {
                    MessageBox.Show("Количество должно быть больше 0", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (price <= 0)
                {
                    MessageBox.Show("Цена должна быть больше 0", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            GenerateDocumentNumber();

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();

                    using (var transaction = conn.BeginTransaction())
                    {
                        string shipmentNumber = txtDocumentNumber.Text;

                        string sqlShipment = @"
            INSERT INTO Shipments (ShipmentNumber, ShipmentDate, StorekeeperId, Status)
            VALUES (@number, @date, @userId, 'Completed')
            RETURNING Id";

                        int shipmentId;
                        using (var cmd = new NpgsqlCommand(sqlShipment, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@number", shipmentNumber);
                            cmd.Parameters.AddWithValue("@date", dtpSupplyDate.Value);
                            cmd.Parameters.AddWithValue("@userId", Session.CurrentUser.Id);
                            shipmentId = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        foreach (DataRow row in itemsTable.Rows)
                        {
                            int productId = Convert.ToInt32(row["ProductId"]);
                            decimal quantity = Convert.ToDecimal(row["Quantity"]);
                            decimal price = Convert.ToDecimal(row["PurchasePrice"]);

                            
                            string sqlDetail = @"
                INSERT INTO ShipmentDetails (ShipmentId, ProductId, Quantity, PriceAtShipment)
                VALUES (@shipmentId, @productId, @quantity, @price)";
                            using (var cmd = new NpgsqlCommand(sqlDetail, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@shipmentId", shipmentId);
                                cmd.Parameters.AddWithValue("@productId", productId);
                                cmd.Parameters.AddWithValue("@quantity", quantity);
                                cmd.Parameters.AddWithValue("@price", price);
                                cmd.ExecuteNonQuery();
                            }

                            
                            string sqlUpdate = "UPDATE StockBalances SET Quantity = Quantity + @quantity WHERE ProductId = @productId";
                            using (var cmd = new NpgsqlCommand(sqlUpdate, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@productId", productId);
                                cmd.Parameters.AddWithValue("@quantity", quantity);
                                int rowsAffected = cmd.ExecuteNonQuery();

                                if (rowsAffected == 0)
                                {
                                    string sqlInsert = "INSERT INTO StockBalances (ProductId, Quantity) VALUES (@productId, @quantity)";
                                    using (var insertCmd = new NpgsqlCommand(sqlInsert, conn, transaction))
                                    {
                                        insertCmd.Parameters.AddWithValue("@productId", productId);
                                        insertCmd.Parameters.AddWithValue("@quantity", quantity);
                                        insertCmd.ExecuteNonQuery();
                                    }
                                }
                            }

                            /*
                             string sqlBatch = @"
                 INSERT INTO StockBatches (ProductId, Quantity, PurchasePrice, ExpiryDate, ReceivedDate, ShipmentId)
                 VALUES (@productId, @quantity, @price, @expiryDate, @receivedDate, @shipmentId)";
                             using (var cmd = new NpgsqlCommand(sqlBatch, conn, transaction))
                             {
                                 cmd.Parameters.AddWithValue("@productId", productId);
                                 cmd.Parameters.AddWithValue("@quantity", quantity);
                                 cmd.Parameters.AddWithValue("@price", price);
                                 cmd.Parameters.AddWithValue("@expiryDate", GetExpiryDate(productId) ?? (object)DBNull.Value);
                                 cmd.Parameters.AddWithValue("@receivedDate", dtpSupplyDate.Value);
                                 cmd.Parameters.AddWithValue("@shipmentId", shipmentId);
                                 cmd.ExecuteNonQuery(); 
                             }*/
                        }
                        

                        Debug.WriteLine($"ShipmentId: {shipmentId}");
                        Debug.WriteLine($"Items added: {itemsTable.Rows.Count}");
                        /*foreach (DataRow row in itemsTable.Rows)
                        {
                            Debug.WriteLine($"Product: {row["ProductName"]}, Qty: {row["Quantity"]}, Price: {row["PurchasePrice"]}");
                        }*/
                        transaction.Commit();
                        MessageBox.Show("Поставка успешно сохранена!", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.Close();
                    }
                }
            }
            catch (PostgresException ex)
            {
                MessageBox.Show($"Ошибка PostgreSQL: {ex.Message}\nТаблица: {ex.TableName}\nПодробности: {ex.Detail}", "Ошибка");
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка");
            }
        }
        private DateTime? GetExpiryDate(int productId)
        {
            string sql = "SELECT ExpiryDate FROM Products WHERE Id = @productId";
            var param = new[] { new NpgsqlParameter("@productId", productId) };
            var result = DatabaseHelper.ExecuteScalar(sql, param);
            return result != null && result != DBNull.Value ? Convert.ToDateTime(result) : (DateTime?)null;
        }
    }



       
    }

