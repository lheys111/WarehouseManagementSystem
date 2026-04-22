using Npgsql;
using Org.BouncyCastle.Utilities;
using System;
using System.Data;
using System.Windows.Forms;
using WarehouseManagementSystem.Helpers;
using WarehouseManagementSystem.Models;
using WarehouseManagementSystem.Services;

namespace WarehouseManagementSystem.Forms
{
    public partial class FormShipmentHistory : Form
    {
        private bool _isStorekeeper;

        public FormShipmentHistory(bool forStorekeeper = false)
        {
            InitializeComponent();
            InitializeEvents();
            _isStorekeeper = forStorekeeper;
            Text = Constants.FormTitles.ShipmentHistory;
            LoadShipments();
        }

        private void InitializeEvents()
        {
            btnViewDetails.Click += btnViewDetails_Click;
        }
        private void LoadShipments()
        {
            try
            {
                DataTable data;

                if (_isStorekeeper)
                {
                    var sql = @"
                SELECT 
                    s.Id,
                    s.ShipmentNumber,
                    s.ShipmentDate,
                    u.FullName AS StorekeeperName,
                    COUNT(sd.Id) AS ItemsCount,
                    COALESCE(SUM(sd.Quantity * sd.PriceAtShipment), 0) AS TotalSum
                FROM Shipments s
                JOIN Users u ON s.StorekeeperId = u.Id
                LEFT JOIN ShipmentDetails sd ON s.Id = sd.ShipmentId
                WHERE s.StorekeeperId = @StorekeeperId
                AND s.ShipmentNumber LIKE 'SHP-%'
                GROUP BY s.Id, s.ShipmentNumber, s.ShipmentDate, u.FullName
                ORDER BY s.ShipmentDate DESC";

                    var parameters = new[] { new NpgsqlParameter("@StorekeeperId", Session.CurrentUser.Id) };
                    data = DatabaseHelper.ExecuteQuery(sql, parameters);
                }
                else
                {
                    var sql = @"
                SELECT 
                    s.Id,
                    s.ShipmentNumber,
                    s.ShipmentDate,
                    u.FullName AS StorekeeperName,
                    COUNT(sd.Id) AS ItemsCount,
                    COALESCE(SUM(sd.Quantity * sd.PriceAtShipment), 0) AS TotalSum
                FROM Shipments s
                JOIN Users u ON s.StorekeeperId = u.Id
                LEFT JOIN ShipmentDetails sd ON s.Id = sd.ShipmentId
                WHERE s.ShipmentNumber LIKE 'SHP-%'
                GROUP BY s.Id, s.ShipmentNumber, s.ShipmentDate, u.FullName
                ORDER BY s.ShipmentDate DESC";

                    data = DatabaseHelper.ExecuteQuery(sql);
                }

                dgvShipments.DataSource = data;

             
                if (dgvShipments.Columns.Contains("Id"))
                    dgvShipments.Columns["Id"].Visible = false;
                if (dgvShipments.Columns.Contains("ShipmentNumber"))
                    dgvShipments.Columns["ShipmentNumber"].HeaderText = "Номер отгрузки";
                if (dgvShipments.Columns.Contains("ShipmentDate"))
                    dgvShipments.Columns["ShipmentDate"].HeaderText = "Дата";
                if (dgvShipments.Columns.Contains("StorekeeperName"))
                    dgvShipments.Columns["StorekeeperName"].HeaderText = "Кладовщик";
                if (dgvShipments.Columns.Contains("ItemsCount"))
                    dgvShipments.Columns["ItemsCount"].HeaderText = "Количество позиций";
                if (dgvShipments.Columns.Contains("TotalSum"))
                    dgvShipments.Columns["TotalSum"].HeaderText = "Общая сумма";

                dgvShipments.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(String.LoadError, ex.Message), String.ErrorTitle,
    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigureGrid()
        {
            if (dgvShipments.Columns.Contains("Id"))
                dgvShipments.Columns["Id"].Visible = false;
            if (dgvShipments.Columns.Contains("ShipmentNumber"))
                dgvShipments.Columns["ShipmentNumber"].HeaderText = Constants.GridHeaders.ShipmentNumber;
            if (dgvShipments.Columns.Contains("ShipmentDate"))
                dgvShipments.Columns["ShipmentDate"].HeaderText = Constants.GridHeaders.Date;
            if (dgvShipments.Columns.Contains("StorekeeperName"))
                dgvShipments.Columns["StorekeeperName"].HeaderText = Constants.GridHeaders.Storekeeper;
            if (dgvShipments.Columns.Contains("ItemsCount"))
                dgvShipments.Columns["ItemsCount"].HeaderText = Constants.GridHeaders.ItemsCount;
            if (dgvShipments.Columns.Contains("TotalSum"))
                dgvShipments.Columns["TotalSum"].HeaderText = Constants.GridHeaders.TotalSum;

            dgvShipments.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void btnViewDetails_Click(object sender, EventArgs e)
        {
            if (dgvShipments.CurrentRow == null)
            {
                MessageBox.Show(Constants.Messages.SelectItem, Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var shipmentId = Convert.ToInt32(dgvShipments.CurrentRow.Cells["Id"].Value);
            var shipmentNumber = dgvShipments.CurrentRow.Cells["ShipmentNumber"].Value.ToString();
            var detailsForm = new FormShipmentDetails(shipmentId, shipmentNumber);
            detailsForm.ShowDialog();
        }

        private void FormShipmentHistory_Load(object sender, EventArgs e)
        {

        }
    }
}