using Npgsql;
using OfficeOpenXml;
using Org.BouncyCastle.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
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
                    MessageBox.Show(string.Format(String.ProductAddedInfo, product.Id, product.Name));

                    foreach (DataRow row in itemsTable.Rows)
                    {
                        if (Convert.ToInt32(row["ProductId"]) == product.Id)
                        {
                            MessageBox.Show(String.ProductAlreadyAdded);
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

        private DateTime? GetProductExpiryDate(int productId)
        {
            try
            {
                string sql = "SELECT ExpiryDate FROM Products WHERE Id = @id";
                var param = new NpgsqlParameter("@id", productId);
                var result = DatabaseHelper.ExecuteScalar(sql, new[] { param });

                if (result == null || result == DBNull.Value)
                    return null;

                return Convert.ToDateTime(result);
            }
            catch (Exception ex)
            {
                return null;
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
                MessageBox.Show(String.AddProductsFirst, String.ErrorTitle,
    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            foreach (DataRow row in itemsTable.Rows)
            {
                decimal quantity = Convert.ToDecimal(row["Quantity"]);
                decimal price = Convert.ToDecimal(row["PurchasePrice"]);

                if (quantity <= 0)
                {
                    MessageBox.Show(String.QuantityMustBePositive, String.ErrorTitle,
    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (price <= 0)
                {
                    MessageBox.Show(String.PriceMustBePositive, String.ErrorTitle,
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

                           
                        }
                        

                        Debug.WriteLine($"ShipmentId: {shipmentId}");
                        Debug.WriteLine($"Items added: {itemsTable.Rows.Count}");
                        foreach (DataRow row in itemsTable.Rows)
                        {
                            Debug.WriteLine($"Product: {row["ProductName"]}, Qty: {row["Quantity"]}, Price: {row["PurchasePrice"]}");
                        }
                        transaction.Commit();
                        MessageBox.Show(String.SupplySavedSuccess, String.SuccessTitle,
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.Close();
                    }
                }
            }
            catch (PostgresException ex)
            {
                MessageBox.Show(string.Format(String.PostgresError, ex.Message, ex.TableName, ex.Detail), String.ErrorTitle);
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(String.GenericError, ex.Message), String.ErrorTitle);
            }
        }
        private DateTime? GetExpiryDate(int productId)
        {
            string sql = "SELECT ExpiryDate FROM Products WHERE Id = @productId";
            var param = new[] { new NpgsqlParameter("@productId", productId) };
            var result = DatabaseHelper.ExecuteScalar(sql, param);
            return result != null && result != DBNull.Value ? Convert.ToDateTime(result) : (DateTime?)null;
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "CSV файлы (*.csv)|*.csv|Excel файлы (*.xlsx)|*.xlsx";
            openFileDialog.Title = "Выберите файл";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string file = openFileDialog.FileName;

                if (file.EndsWith(".csv"))
                {
                    ImportFromCsv(file);
                }
                else if (file.EndsWith(".xlsx"))
                {
                    ImportFromExcel(file);
                }
                else
                {
                    MessageBox.Show(String.ImportWrongFormat);
                }
            }
        }

        private void ImportFromCsv(string file)
        {
            string[] lines = File.ReadAllLines(file);
            int lineNumber = 0;
            int added = 0;
            int errors = 0;

            foreach (string line in lines)
            {
                lineNumber++;
                if (string.IsNullOrWhiteSpace(line)) continue;

                string[] parts = line.Split(';');

                if (parts.Length != 3)
                {
                    MessageBox.Show(string.Format(String.ImportCsvWrongColumns, lineNumber));
                    errors++;
                    continue;
                }

                string article = parts[0].Trim();

                decimal quantity;
                if (!decimal.TryParse(parts[1].Trim(), out quantity))
                {
                    MessageBox.Show(string.Format(String.ImportInvalidQuantity, lineNumber, parts[1]));
                    errors++;
                    continue;
                }

                decimal price;
                if (!decimal.TryParse(parts[2].Trim(), out price))
                {
                    MessageBox.Show(string.Format(String.ImportInvalidPrice, lineNumber, parts[2]));
                    errors++;
                    continue;
                }

                string sql = "SELECT Id, Article, Name FROM Products WHERE Article = @art";
                var param = new NpgsqlParameter("@art", article);
                var data = DatabaseHelper.ExecuteQuery(sql, new[] { param });

                if (data.Rows.Count == 0)
                {
                    MessageBox.Show(string.Format(String.ImportArticleNotFound, lineNumber, article));
                    errors++;
                    errors++;
                    continue;
                }

                int productId = Convert.ToInt32(data.Rows[0]["Id"]);
                string productArticle = data.Rows[0]["Article"].ToString();
                string productName = data.Rows[0]["Name"].ToString();

                itemsTable.Rows.Add(productId, productArticle, productName, quantity, price);
                added++;
            }

            MessageBox.Show(string.Format(String.ImportCompleted, added, errors));
        }

        private void ImportFromExcel(string file)
        {
            using (var package = new ExcelPackage(new FileInfo(file)))
            {
                var sheet = package.Workbook.Worksheets[0];
                int rowCount = sheet.Dimension.Rows;

                int added = 0;
                int errors = 0;

                for (int i = 2; i <= rowCount; i++)
                {
                    string article = sheet.Cells[i, 1].Text.Trim();
                    if (string.IsNullOrEmpty(article)) continue;

                    string quantityStr = sheet.Cells[i, 2].Text.Trim();
                    string priceStr = sheet.Cells[i, 3].Text.Trim();

                    decimal quantity;
                    if (!decimal.TryParse(quantityStr, out quantity))
                    {
                        MessageBox.Show(string.Format(String.ImportInvalidQuantity, i, quantityStr));
                        errors++;
                        continue;
                    }

                    decimal price;
                    if (!decimal.TryParse(priceStr, out price))
                    {
                        MessageBox.Show(string.Format(String.ImportInvalidPrice, i, priceStr));
                        errors++;
                        continue;
                    }

                    string sql = "SELECT Id, Article, Name FROM Products WHERE Article = @art";
                    var param = new NpgsqlParameter("@art", article);
                    var data = DatabaseHelper.ExecuteQuery(sql, new[] { param });

                    if (data.Rows.Count == 0)
                    {
                        MessageBox.Show(string.Format(String.ImportArticleNotFound, i, article));
                        errors++;
                        continue;
                    }

                    int productId = Convert.ToInt32(data.Rows[0]["Id"]);
                    string productArticle = data.Rows[0]["Article"].ToString();
                    string productName = data.Rows[0]["Name"].ToString();

                    itemsTable.Rows.Add(productId, productArticle, productName, quantity, price);
                    added++;
                }

                MessageBox.Show(string.Format(String.ImportCompleted, added, errors));
            }
        }
    }



       
    }

