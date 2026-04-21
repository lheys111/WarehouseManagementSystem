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

namespace WarehouseManagementSystem.Forms
{
    public partial class FormShipmentReport : Form
    {
        public FormShipmentReport()
        {
            InitializeComponent();
            dateStart.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            dateEnd.Value = DateTime.Now;
        }

        private void FormShipmentReport_Load(object sender, EventArgs e)
        {

        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            GenerateReport();
        }
        private void GenerateReport()
        {
            try
            {
                string sql = @"
                    SELECT 
                        s.ShipmentDate AS Дата,
                        s.ShipmentNumber AS НомерОтгрузки,
                        u.FullName AS Кладовщик,
                        COALESCE(SUM(sd.Quantity * sd.PriceAtShipment), 0) AS Сумма,
                        COALESCE(SUM(sd.Quantity * p.PurchasePrice), 0) AS Себестоимость,
                        COALESCE(SUM(sd.Quantity * sd.PriceAtShipment), 0) - 
                        COALESCE(SUM(sd.Quantity * p.PurchasePrice), 0) AS Прибыль
                    FROM Shipments s
                    JOIN Users u ON s.StorekeeperId = u.Id
                    JOIN ShipmentDetails sd ON s.Id = sd.ShipmentId
                    JOIN Products p ON sd.ProductId = p.Id
                    WHERE s.ShipmentDate BETWEEN @startDate AND @endDate
                    GROUP BY s.Id, s.ShipmentNumber, s.ShipmentDate, u.FullName
                    ORDER BY s.ShipmentDate DESC";

                var parameters = new[]
                {
                    new NpgsqlParameter("@startDate",  dateStart.Value.Date),
                    new NpgsqlParameter("@endDate",  dateEnd.Value.Date)
                };

                DataTable data = DatabaseHelper.ExecuteQuery(sql, parameters);
                dgvReport.DataSource = data;

                ConfigureGrid();

                if (data.Rows.Count == 0)
                {
                    MessageBox.Show("За выбранный период отгрузок не найдено.", "Информация",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка формирования отчёта: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

             private void ConfigureGrid()
        {
            if (dgvReport.Columns.Contains("Дата"))
            {
                dgvReport.Columns["Дата"].DefaultCellStyle.Format = "dd.MM.yyyy";
                dgvReport.Columns["Дата"].Width = 100;
            }

            if (dgvReport.Columns.Contains("НомерОтгрузки"))
            {
                dgvReport.Columns["НомерОтгрузки"].HeaderText = "Номер отгрузки";
                dgvReport.Columns["НомерОтгрузки"].Width = 150;
            }

            if (dgvReport.Columns.Contains("Кладовщик"))
            {
                dgvReport.Columns["Кладовщик"].Width = 180;
            }

            if (dgvReport.Columns.Contains("Сумма"))
            {
                dgvReport.Columns["Сумма"].DefaultCellStyle.Format = "N2";
                dgvReport.Columns["Сумма"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                dgvReport.Columns["Сумма"].Width = 120;
            }

            if (dgvReport.Columns.Contains("Себестоимость"))
            {
                dgvReport.Columns["Себестоимость"].DefaultCellStyle.Format = "N2";
                dgvReport.Columns["Себестоимость"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                dgvReport.Columns["Себестоимость"].Width = 120;
            }

            if (dgvReport.Columns.Contains("Прибыль"))
            {
                dgvReport.Columns["Прибыль"].DefaultCellStyle.Format = "N2";
                dgvReport.Columns["Прибыль"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                dgvReport.Columns["Прибыль"].Width = 120;
            }

            dgvReport.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvReport.AllowUserToAddRows = false;
            dgvReport.ReadOnly = true;
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Функция экспорта будет добавлена позже.", "Информация",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    
    }
}
    

