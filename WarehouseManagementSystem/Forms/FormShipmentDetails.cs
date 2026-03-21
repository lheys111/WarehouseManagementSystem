using System;
using System.Data;
using Npgsql;
using System.Windows.Forms;
using WarehouseManagementSystem.Helpers;

namespace WarehouseManagementSystem.Forms
{
    public partial class FormShipmentDetails : Form
    {
        private int shipmentId;

        public FormShipmentDetails(int id, string number)
        {
            InitializeComponent();
            this.btnClose.Click += new EventHandler(this.btnClose_Click);
            shipmentId = id;
            this.Text = $"Отгрузка {number}";
            LoadDetails();
        }

        private void LoadDetails()
        {
            try
            {
                string infoSql = @"SELECT s.ShipmentNumber, s.ShipmentDate, u.FullName, s.Status
                                  FROM Shipments s
                                  JOIN Users u ON s.StorekeeperId = u.Id
                                  WHERE s.Id = @Id";
                DataTable info = DatabaseHelper.ExecuteQuery(infoSql, new[] { new NpgsqlParameter("@Id", shipmentId) });

                if (info.Rows.Count > 0)
                {
                    lblTitle.Text = $"Отгрузка №{info.Rows[0]["ShipmentNumber"]} от {Convert.ToDateTime(info.Rows[0]["ShipmentDate"]):dd.MM.yyyy HH:mm}";
                    lblStorekeeper.Text = $"Кладовщик: {info.Rows[0]["FullName"]}";
                    lblStatus.Text = $"Статус: {info.Rows[0]["Status"]}";
                }

                string detailsSql = @"SELECT p.Article, p.Name, sd.Quantity, sd.PriceAtShipment
                                     FROM ShipmentDetails sd
                                     JOIN Products p ON sd.ProductId = p.Id
                                     WHERE sd.ShipmentId = @Id";
                DataTable details = DatabaseHelper.ExecuteQuery(detailsSql, new[] { new NpgsqlParameter("@Id", shipmentId) });
                dgvDetails.DataSource = details;

                if (dgvDetails.Columns.Contains("Article"))
                    dgvDetails.Columns["Article"].HeaderText = "Артикул";
                if (dgvDetails.Columns.Contains("Name"))
                    dgvDetails.Columns["Name"].HeaderText = "Название";
                if (dgvDetails.Columns.Contains("Quantity"))
                    dgvDetails.Columns["Quantity"].HeaderText = "Количество";
                if (dgvDetails.Columns.Contains("PriceAtShipment"))
                    dgvDetails.Columns["PriceAtShipment"].HeaderText = "Цена";

                int totalItems = details.Rows.Count;
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
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
