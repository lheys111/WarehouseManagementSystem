using System;
using System.Data;
using Npgsql;
using System.Windows.Forms;
using WarehouseManagementSystem.Helpers;

namespace WarehouseManagementSystem.Forms
{
    public partial class FormShipmentDetails : Form
    {
        private int _shipmentId;

        public FormShipmentDetails(int id, string number)
        {
            InitializeComponent();
            InitializeEvents();
            _shipmentId = id;
            Text = $"{Constants.FormTitles.ShipmentDetails} {number}";
            LoadDetails();
        }

        private void InitializeEvents()
        {
            btnClose.Click += btnClose_Click;
        }

        private void LoadDetails()
        {
            try
            {
                var infoSql = @"SELECT s.ShipmentNumber, s.ShipmentDate, u.FullName, s.Status
                               FROM Shipments s
                               JOIN Users u ON s.StorekeeperId = u.Id
                               WHERE s.Id = @Id";
                var info = DatabaseHelper.ExecuteQuery(infoSql, new[] { new NpgsqlParameter("@Id", _shipmentId) });

                if (info.Rows.Count > 0)
                {
                    var shipDate = Convert.ToDateTime(info.Rows[0]["ShipmentDate"]);
                    lblTitle.Text = $"Отгрузка №{info.Rows[0]["ShipmentNumber"]} от {shipDate:dd.MM.yyyy HH:mm}";
                    lblStorekeeper.Text = $"Кладовщик: {info.Rows[0]["FullName"]}";
                    lblStatus.Text = $"Статус: {info.Rows[0]["Status"]}";
                }

                var detailsSql = @"SELECT p.Article, p.Name, sd.Quantity, sd.PriceAtShipment
                                  FROM ShipmentDetails sd
                                  JOIN Products p ON sd.ProductId = p.Id
                                  WHERE sd.ShipmentId = @Id
                                  ORDER BY p.Name";
                var details = DatabaseHelper.ExecuteQuery(detailsSql, new[] { new NpgsqlParameter("@Id", _shipmentId) });
                dgvDetails.DataSource = details;

                ConfigureGrid();

                var totalItems = details.Rows.Count;
                decimal totalSum = 0;
                foreach (DataRow row in details.Rows)
                {
                    totalSum += Convert.ToDecimal(row["Quantity"]) * Convert.ToDecimal(row["PriceAtShipment"]);
                }
                lblTotal.Text = $"Итого: {totalItems} позиций на сумму {totalSum:N2} руб.";

                dgvDetails.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }
            catch (Exception ex)
            {
                AppLogger.Error(ex, "Ошибка загрузки деталей отгрузки");
                MessageBox.Show(Constants.Messages.ConnectionError, Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigureGrid()
        {
            if (dgvDetails.Columns.Contains("Article"))
                dgvDetails.Columns["Article"].HeaderText = Constants.GridHeaders.Article;
            if (dgvDetails.Columns.Contains("Name"))
                dgvDetails.Columns["Name"].HeaderText = Constants.GridHeaders.Name;
            if (dgvDetails.Columns.Contains("Quantity"))
                dgvDetails.Columns["Quantity"].HeaderText = Constants.GridHeaders.Quantity;
            if (dgvDetails.Columns.Contains("PriceAtShipment"))
                dgvDetails.Columns["PriceAtShipment"].HeaderText = Constants.GridHeaders.Price;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

    }
}
