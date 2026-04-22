using Npgsql;
using Org.BouncyCastle.Utilities;
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
using WarehouseManagementSystem.Properties;

namespace WarehouseManagementSystem
{
    public partial class FormWriteOffExpired : Form
    {
        public FormWriteOffExpired()
        {
            InitializeComponent();
            Text = "Списание просроченных товаров";
            LoadExpiredProducts();
            dgvExpiredProducts.SelectionChanged += DgvExpiredProducts_SelectionChanged;
        }

        private void btnWriteOffSelected_Click(object sender, EventArgs e)
        {
            var selectedRows = dgvExpiredProducts.Rows
        .Cast<DataGridViewRow>()
        .Where(row => row.Cells["Select"].Value != null && (bool)row.Cells["Select"].Value == true)
        .ToList();

            if (selectedRows.Count == 0)
            {
                MessageBox.Show(String.SelectItemsToWriteOff);
                return;
            }

            var confirm = MessageBox.Show(string.Format(String.ConfirmWriteOffSelected, selectedRows.Count),
      String.ConfirmTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;

            int successCount = 0;
            foreach (var row in selectedRows)
            {
                int batchId = Convert.ToInt32(row.Cells["BatchId"].Value);
                int productId = Convert.ToInt32(row.Cells["ProductId"].Value);
                decimal quantity = Convert.ToDecimal(row.Cells["Quantity"].Value);
                decimal lossAmount = Convert.ToDecimal(row.Cells["LossAmount"].Value);

                if (WriteOffBatch(batchId, productId, quantity, lossAmount))
                    successCount++;
            }

            MessageBox.Show(string.Format(String.WriteOffResult, successCount, selectedRows.Count));
            LoadExpiredProducts();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadExpiredProducts();
            MessageBox.Show(String.ListRefreshed);
        }

        private void LoadExpiredProducts()
        {
            string sql = @"
        SELECT 
            sb.Id AS BatchId,
            p.Id AS ProductId,
            p.Article,
            p.Name,
            sb.Quantity,
            sb.PurchasePrice,
            sb.ExpiryDate,
            (sb.Quantity * sb.PurchasePrice) AS LossAmount
        FROM StockBatches sb
        JOIN Products p ON sb.ProductId = p.Id
        WHERE sb.ExpiryDate < CURRENT_DATE AND sb.Quantity > 0
        ORDER BY sb.ExpiryDate ASC";

            DataTable data = DatabaseHelper.ExecuteQuery(sql);
            dgvExpiredProducts.DataSource = data;

            dgvExpiredProducts.Columns["BatchId"].Visible = false;
            dgvExpiredProducts.Columns["ProductId"].Visible = false;

            dgvExpiredProducts.Columns["Article"].HeaderText = "Артикул";
            dgvExpiredProducts.Columns["Name"].HeaderText = "Товар";
            dgvExpiredProducts.Columns["Quantity"].HeaderText = "Количество";
            dgvExpiredProducts.Columns["PurchasePrice"].HeaderText = "Цена закупки";
            dgvExpiredProducts.Columns["ExpiryDate"].HeaderText = "Срок годности";
            dgvExpiredProducts.Columns["LossAmount"].HeaderText = "Сумма убытка";

            dgvExpiredProducts.Columns["ExpiryDate"].DefaultCellStyle.Format = "dd.MM.yyyy";
            dgvExpiredProducts.Columns["LossAmount"].DefaultCellStyle.Format = "N2";

            if (dgvExpiredProducts.Columns["Select"] == null)
            {
                DataGridViewCheckBoxColumn checkColumn = new DataGridViewCheckBoxColumn();
                checkColumn.Name = "Select";
                checkColumn.HeaderText = "✓";
                checkColumn.Width = 30;
                dgvExpiredProducts.Columns.Insert(0, checkColumn);
            }

            UpdateSelectedInfo();
        }

        private void UpdateSelectedInfo()
        {
            int selectedCount = 0;
            decimal totalLoss = 0;

            foreach (DataGridViewRow row in dgvExpiredProducts.Rows)
            {
                if (row.Cells["Select"].Value != null && (bool)row.Cells["Select"].Value == true)
                {
                    selectedCount++;
                    totalLoss += Convert.ToDecimal(row.Cells["LossAmount"].Value);
                }
            }

            lblSelectedInfo.Text = $"Выбрано товаров: {selectedCount}, сумма убытка: {totalLoss:N2} руб.";
        }

        private bool WriteOffBatch(int batchId, int productId, decimal quantity, decimal lossAmount)
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    using (var tran = conn.BeginTransaction())
                    {
                        string insertSql = @"
                    INSERT INTO WriteOffs (ProductId, BatchId, Quantity, LossAmount)
                    VALUES (@pid, @bid, @qty, @loss)";
                        using (var cmd = new NpgsqlCommand(insertSql, conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@pid", productId);
                            cmd.Parameters.AddWithValue("@bid", batchId);
                            cmd.Parameters.AddWithValue("@qty", quantity);
                            cmd.Parameters.AddWithValue("@loss", lossAmount);
                            cmd.ExecuteNonQuery();
                        }

                        string deleteSql = "DELETE FROM StockBatches WHERE Id = @bid";
                        using (var cmd = new NpgsqlCommand(deleteSql, conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@bid", batchId);
                            cmd.ExecuteNonQuery();
                        }

                        string updateBalance = "UPDATE StockBalances SET Quantity = Quantity - @qty WHERE ProductId = @pid";
                        using (var cmd = new NpgsqlCommand(updateBalance, conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@qty", quantity);
                            cmd.Parameters.AddWithValue("@pid", productId);
                            cmd.ExecuteNonQuery();
                        }

                        tran.Commit();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(String.WriteOffError, ex.Message), String.ErrorTitle,
    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void dgvExpiredProducts_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            UpdateSelectedInfo();
        }

        private void DgvExpiredProducts_SelectionChanged(object sender, EventArgs e)
        {
            UpdateSelectedInfo();
        }
    }
}
