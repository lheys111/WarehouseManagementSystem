using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
            string sql = "SELECT COUNT(*) FROM Shipments WHERE ShipmentNumber LIKE '" + datePart + "%'";
            int count = Convert.ToInt32(DatabaseHelper.ExecuteScalar(sql));
            int nextNumber = count + 1;
            txtDocumentNumber.Text = "INV-" + datePart + "-" + nextNumber.ToString("D3");
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
                        product.PurchasePrice
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

            dgvItems.DataSource = itemsTable;

            dgvItems.Columns["ProductId"].Visible = false;      
            dgvItems.Columns["Article"].HeaderText = "Артикул";
            dgvItems.Columns["ProductName"].HeaderText = "Название";
            dgvItems.Columns["Quantity"].HeaderText = "Кол-во";
            dgvItems.Columns["PurchasePrice"].HeaderText = "Цена";


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
                        using (var cmd = new NpgsqlCommand(sqlShipment, conn))
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
                            using (var cmd = new NpgsqlCommand(sqlDetail, conn))
                            {
                                cmd.Parameters.AddWithValue("@shipmentId", shipmentId);
                                cmd.Parameters.AddWithValue("@productId", productId);
                                cmd.Parameters.AddWithValue("@quantity", quantity);
                                cmd.Parameters.AddWithValue("@price", price);
                                cmd.ExecuteNonQuery();
                            }

                            string sqlUpdate = "UPDATE StockBalances SET Quantity = Quantity + @quantity WHERE ProductId = @productId";
                            using (var cmd = new NpgsqlCommand(sqlUpdate, conn))
                            {
                                cmd.Parameters.AddWithValue("@productId", productId);
                                cmd.Parameters.AddWithValue("@quantity", quantity);
                                int rowsAffected = cmd.ExecuteNonQuery();

                                if (rowsAffected == 0)
                                {
                                    string sqlInsert = "INSERT INTO StockBalances (ProductId, Quantity) VALUES (@productId, @quantity)";
                                    using (var insertCmd = new NpgsqlCommand(sqlInsert, conn))
                                    {
                                        insertCmd.Parameters.AddWithValue("@productId", productId);
                                        insertCmd.Parameters.AddWithValue("@quantity", quantity);
                                        insertCmd.ExecuteNonQuery();
                                    }
                                }
                            }
                        }

                        transaction.Commit();
                        MessageBox.Show("Поставка успешно сохранена!", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.Close();
                    }
                }
            }
            catch (PostgresException ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
