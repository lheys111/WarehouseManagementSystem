using Npgsql;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using WarehouseManagementSystem.Helpers;
namespace WarehouseManagementSystem.Forms
{
    public partial class FormStockBalances : Form
    {
        public FormStockBalances()
        {
            InitializeComponent();
            this.btnSearch.Click += new EventHandler(this.btnSearch_Click);
            LoadStock();
            SetupButtons();
        }

        private void SetupButtons()
        {
            btnSearch.FlatStyle = FlatStyle.Flat;
            btnSearch.FlatAppearance.BorderColor = Color.Black;
            btnSearch.FlatAppearance.BorderSize = 1;
            btnSearch.BackColor = Color.White;
        }

        private void LoadStock(string searchText = "")
        {
            try
            {
                string query = @"SELECT Article, Name, Category, UnitOfMeasure, StockQuantity, PurchasePrice
                                FROM vw_StockForStorekeeper";

                if (!string.IsNullOrEmpty(searchText))
                {
                    query += " WHERE Article ILIKE @Search OR Name ILIKE @Search";
                }

                query += " ORDER BY Name";

                NpgsqlParameter[] parameters = null;
                if (!string.IsNullOrEmpty(searchText))
                {
                    parameters = new NpgsqlParameter[] {
                        new NpgsqlParameter("@Search", "%" + searchText + "%")
                    };
                }

                DataTable dt = DatabaseHelper.ExecuteQuery(query, parameters);
                dgvStock.DataSource = dt;

                dgvStock.Columns["Article"].HeaderText = "Артикул";
                dgvStock.Columns["Name"].HeaderText = "Название";
                dgvStock.Columns["Category"].HeaderText = "Категория";
                dgvStock.Columns["UnitOfMeasure"].HeaderText = "Ед. изм.";
                dgvStock.Columns["StockQuantity"].HeaderText = "Остаток";
                dgvStock.Columns["PurchasePrice"].HeaderText = "Цена закупки";

                dgvStock.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки: " + ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            LoadStock(txtSearch.Text);
        }
    }
}