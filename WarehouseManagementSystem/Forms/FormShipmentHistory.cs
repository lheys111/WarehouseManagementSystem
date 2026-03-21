using System;
using System.Data;
using Npgsql;
using System.Windows.Forms;
using WarehouseManagementSystem.Helpers;
using WarehouseManagementSystem.Models;

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
                    var sql = Constants.Queries.GetAllShipments + " WHERE StorekeeperName = @Storekeeper";
                    var parameters = new[] { new NpgsqlParameter("@Storekeeper", Session.CurrentUser.FullName) };
                    data = DatabaseHelper.ExecuteQuery(sql, parameters);
                }
                else
                {
                    data = DatabaseHelper.ExecuteQuery(Constants.Queries.GetAllShipments);
                }

                dgvShipments.DataSource = data;
                ConfigureGrid();
            }
            catch (Exception ex)
            {
                AppLogger.Error(ex, "Ошибка загрузки истории отгрузок");
                MessageBox.Show(Constants.Messages.ConnectionError, Text,
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
    }
}