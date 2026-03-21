using Npgsql;
using System;
using System.Data;
using System.Windows.Forms;
using WarehouseManagementSystem.Helpers;
using WarehouseManagementSystem.Models;

namespace WarehouseManagementSystem.Forms
{
    public partial class FormShipmentHistory : Form
    {
        private bool isStorekeeper = false;

        public FormShipmentHistory(bool forStorekeeper = false)
        {
            InitializeComponent();
            this.btnViewDetails.Click += new EventHandler(this.btnViewDetails_Click);
            isStorekeeper = forStorekeeper;
            LoadShipments();
        }

        private void LoadShipments()
        {
            try
            {
                string sql = @"SELECT Id, ShipmentNumber, ShipmentDate, StorekeeperName, 
                                      ItemsCount, TotalSum
                              FROM vw_ShipmentsHistory";

                if (isStorekeeper)
                {
                    sql += " WHERE StorekeeperName = @Storekeeper";
                    NpgsqlParameter[] parameters = { new NpgsqlParameter("@Storekeeper", Session.CurrentUser.FullName) };
                    DataTable data = DatabaseHelper.ExecuteQuery(sql, parameters);
                    dgvShipments.DataSource = data;
                }
                else
                {
                    DataTable data = DatabaseHelper.ExecuteQuery(sql);
                    dgvShipments.DataSource = data;
                }

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
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnViewDetails_Click(object sender, EventArgs e)
        {
            if (dgvShipments.CurrentRow != null)
            {
                int shipmentId = Convert.ToInt32(dgvShipments.CurrentRow.Cells["Id"].Value);
                string shipmentNumber = dgvShipments.CurrentRow.Cells["ShipmentNumber"].Value.ToString();
                FormShipmentDetails detailsForm = new FormShipmentDetails(shipmentId, shipmentNumber);
                detailsForm.ShowDialog();
            }
            else
            {
                MessageBox.Show("Выберите отгрузку", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
