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
        private void LoadProducts()
        {
            string sql = "SELECT Id, Article, Name, PurchasePrice FROM Products ORDER BY Name";
            DataTable products = DatabaseHelper.ExecuteQuery(sql);
            dgvProducts.DataSource = products;

            dgvProducts.Columns["Id"].Visible = false;
            dgvProducts.Columns["Article"].HeaderText = "Артикул";
            dgvProducts.Columns["Name"].HeaderText = "Название";
            dgvProducts.Columns["PurchasePrice"].HeaderText = "Цена";
        }

        private void FormChooseProduct_Load(object sender, EventArgs e)
        {

        }

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
