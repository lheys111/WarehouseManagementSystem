using Npgsql;
using System;
using System.Data;
using System.Windows.Forms;
using WarehouseManagementSystem.Helpers;

namespace WarehouseManagementSystem
{
    public partial class FormBatchDetails : Form
    {
        public FormBatchDetails(int productId, string productName)
        {
            InitializeComponent();
            Text = $"Разбивка по партиям: {productName}";
            LoadBatches(productId);
        }

        private void LoadBatches(int productId)
        {
            string sql = @"
        SELECT 
        Id AS Id,
        Quantity AS Quantity,
        PurchasePrice AS PurchasePrice,
        ExpiryDate AS ExpiryDate,
        ReceivedDate AS ReceivedDate
        FROM StockBatches
        WHERE ProductId = @pid AND Quantity > 0
         ORDER BY ExpiryDate ASC NULLS LAST, ReceivedDate ASC";

            var param = new NpgsqlParameter("@pid", productId);
            DataTable data = DatabaseHelper.ExecuteQuery(sql, new[] { param });
            dgvBatches.DataSource = data;

            if (dgvBatches.Columns.Contains("Id"))
                dgvBatches.Columns["Id"].HeaderText = "ID партии";
            if (dgvBatches.Columns.Contains("Quantity"))
                dgvBatches.Columns["Quantity"].HeaderText = "Количество";
            if (dgvBatches.Columns.Contains("PurchasePrice"))
                dgvBatches.Columns["PurchasePrice"].HeaderText = "Цена закупки";
            if (dgvBatches.Columns.Contains("ExpiryDate"))
            {
                dgvBatches.Columns["ExpiryDate"].HeaderText = "Срок годности";
                dgvBatches.Columns["ExpiryDate"].DefaultCellStyle.Format = "dd.MM.yyyy";
            }
            if (dgvBatches.Columns.Contains("ReceivedDate"))
            {
                dgvBatches.Columns["ReceivedDate"].HeaderText = "Дата поступления";
                dgvBatches.Columns["ReceivedDate"].DefaultCellStyle.Format = "dd.MM.yyyy";
            }

            dgvBatches.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvBatches.ReadOnly = true;
            dgvBatches.AllowUserToAddRows = false;
        }

        private void FormBatchDetails_Load(object sender, EventArgs e)
        {

        }
    }
}