using iTextSharp.text;
using iTextSharp.text.pdf;
using Npgsql;
using Npgsql.Internal;
using OfficeOpenXml;
using Org.BouncyCastle.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
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
                    MessageBox.Show(String.NoShipmentsFound, String.InfoTitle,
      MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(String.ReportGenerationError, ex.Message), String.ErrorTitle,
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
            MessageBox.Show(String.ExportNotImplemented, String.InfoTitle,
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnExport_Click_1(object sender, EventArgs e)
        {
            if (dgvReport.DataSource == null || dgvReport.Rows.Count == 0)
            {
                MessageBox.Show(String.ExportNoData, String.WarningTitle,
    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = MessageBox.Show(String.ExportSelectFormat, String.ExportReportTitle,
      MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                ExportToExcel();
            }
            else if (result == DialogResult.No)
            {
                ExportToPdf();
            }
        }

        private void ExportToExcel()
        {
            using (SaveFileDialog saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "Excel файлы (*.xlsx)|*.xlsx";
                saveDialog.FileName = $"Отчёт_по_отгрузкам_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                saveDialog.Title = "Сохранить отчёт";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    using (var package = new ExcelPackage())
                    {
                        var worksheet = package.Workbook.Worksheets.Add("Отчёт по отгрузкам");

                        worksheet.Cells[1, 1].Value = "ОТЧЁТ ПО ОТГРУЗКАМ";
                        worksheet.Cells[1, 1, 1, dgvReport.Columns.Count].Merge = true;
                        worksheet.Cells[1, 1].Style.Font.Bold = true;
                        worksheet.Cells[1, 1].Style.Font.Size = 14;
                        worksheet.Cells[1, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                        worksheet.Cells[2, 1].Value = $"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}";
                        worksheet.Cells[2, 1, 2, dgvReport.Columns.Count].Merge = true;

                        int row = 4;
                        for (int i = 0; i < dgvReport.Columns.Count; i++)
                        {
                            worksheet.Cells[row, i + 1].Value = dgvReport.Columns[i].HeaderText;
                            worksheet.Cells[row, i + 1].Style.Font.Bold = true;
                        }

                        row++;
                        for (int i = 0; i < dgvReport.Rows.Count; i++)
                        {
                            for (int j = 0; j < dgvReport.Columns.Count; j++)
                            {
                                if (dgvReport.Rows[i].Cells[j].Value != null)
                                {
                                    worksheet.Cells[row + i, j + 1].Value = dgvReport.Rows[i].Cells[j].Value.ToString();
                                }
                            }
                        }

                        worksheet.Cells.AutoFitColumns();
                        package.SaveAs(new FileInfo(saveDialog.FileName));
                    }

                    MessageBox.Show(String.ExportSuccessExcel, String.SuccessTitle,
    MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void ExportToPdf()
        {

            using (SaveFileDialog saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "PDF файлы (*.pdf)|*.pdf";
                saveDialog.FileName = $"Отчёт_по_отгрузкам_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                saveDialog.Title = "Сохранить отчёт";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    iTextSharp.text.Document document = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4.Rotate());
                    iTextSharp.text.pdf.PdfWriter.GetInstance(document, new FileStream(saveDialog.FileName, FileMode.Create));
                    document.Open();

                    iTextSharp.text.Font titleFont = iTextSharp.text.FontFactory.GetFont("Arial", 16, iTextSharp.text.Font.BOLD);
                    iTextSharp.text.Paragraph title = new iTextSharp.text.Paragraph("ОТЧЁТ ПО ОТГРУЗКАМ", titleFont);
                    title.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
                    document.Add(title);

                    document.Add(new iTextSharp.text.Paragraph(" "));

                    iTextSharp.text.Font dateFont = iTextSharp.text.FontFactory.GetFont("Arial", 10, iTextSharp.text.Font.ITALIC);
                    iTextSharp.text.Paragraph datePara = new iTextSharp.text.Paragraph($"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}", dateFont);
                    datePara.Alignment = iTextSharp.text.Element.ALIGN_RIGHT;
                    document.Add(datePara);

                    document.Add(new iTextSharp.text.Paragraph(" "));

                    iTextSharp.text.pdf.PdfPTable table = new iTextSharp.text.pdf.PdfPTable(dgvReport.Columns.Count);
                    table.WidthPercentage = 100;

                    iTextSharp.text.Font headerFont = iTextSharp.text.FontFactory.GetFont("Arial", 10, iTextSharp.text.Font.BOLD);
                    foreach (DataGridViewColumn column in dgvReport.Columns)
                    {
                        iTextSharp.text.pdf.PdfPCell cell = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase(column.HeaderText, headerFont));
                        cell.BackgroundColor = iTextSharp.text.BaseColor.LIGHT_GRAY;
                        table.AddCell(cell);
                    }

                    iTextSharp.text.Font cellFont = iTextSharp.text.FontFactory.GetFont("Arial", 9);
                    foreach (DataGridViewRow row in dgvReport.Rows)
                    {
                        foreach (DataGridViewCell cell in row.Cells)
                        {
                            string value = cell.Value != null ? cell.Value.ToString() : "";
                            table.AddCell(new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase(value, cellFont)));
                        }
                    }

                    document.Add(table);
                    document.Close();

                    MessageBox.Show(String.ExportSuccessPdf, String.SuccessTitle,
      MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
    }
}
    

