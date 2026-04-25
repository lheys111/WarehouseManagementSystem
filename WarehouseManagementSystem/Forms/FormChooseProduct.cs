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
using WarehouseManagementSystem.Models;

namespace WarehouseManagementSystem.Forms
{
    public partial class FormChooseProduct : Form
    {
        public ProductDto SelectedProduct { get; private set; }
        public FormChooseProduct()
        {
            InitializeComponent();
            LoadProducts();
        }

        /// <summary>
        /// Загружает список товаров из базы данных.
        /// Отображает общий остаток и разбивку по партиям.
        /// </summary>
        private void LoadProducts()
        {
            string sql = @"
        SELECT 
            p.Id,
            p.Article,
            p.Name,
            p.PurchasePrice,
            COALESCE(SUM(sb.Quantity), 0) AS TotalStock,
            COALESCE(
                STRING_AGG(
                    CONCAT(sb.Quantity, ' кг (поступление: ', COALESCE(sb.ReceivedDate::text, 'не указана'), ')'), 
                    '; ' 
                    ORDER BY sb.ReceivedDate ASC
                ), 
                'нет партий'
            ) AS BatchesInfo
        FROM Products p
        LEFT JOIN StockBatches sb ON p.Id = sb.ProductId AND sb.Quantity > 0
        GROUP BY p.Id, p.Article, p.Name, p.PurchasePrice
        ORDER BY p.Name";

            var data = DatabaseHelper.ExecuteQuery(sql);

            dgvProducts.AutoGenerateColumns = false;
            dgvProducts.Columns.Clear();

            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "Id", DataPropertyName = "Id", Visible = false });
            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { Name = "Article", HeaderText = "Артикул", DataPropertyName = "Article", Width = 80 });
            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Название", DataPropertyName = "Name", Width = 200 });
            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { Name = "PurchasePrice", HeaderText = "Цена", DataPropertyName = "PurchasePrice", Width = 80 });
            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { Name = "TotalStock", HeaderText = "Общий остаток", DataPropertyName = "TotalStock", Width = 100 });
            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { Name = "BatchesInfo", HeaderText = "Остатки по партиям", DataPropertyName = "BatchesInfo", Width = 350 });

            dgvProducts.DataSource = data;

            dgvProducts.Columns["PurchasePrice"].DefaultCellStyle.Format = "N2";
            dgvProducts.Columns["TotalStock"].DefaultCellStyle.Format = "N2";
        }

        /// <summary>
        /// Обработчик нажатия кнопки "Выбрать".
        /// Сохраняет выбранный товар и закрывает форму.
        /// </summary>
        private void btnSelect_Click(object sender, EventArgs e)
        {
            if (dgvProducts.CurrentRow != null)
            {
                SelectedProduct = new ProductDto
                {
                    Id = Convert.ToInt32(dgvProducts.CurrentRow.Cells["Id"].Value),
                    Article = dgvProducts.CurrentRow.Cells["Article"].Value.ToString(),
                    Name = dgvProducts.CurrentRow.Cells["Name"].Value.ToString(),
                    PurchasePrice = Convert.ToDecimal(dgvProducts.CurrentRow.Cells["PurchasePrice"].Value)
                };
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
