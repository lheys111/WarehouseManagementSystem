using System;
using System.Drawing;
using System.Windows.Forms;
using WarehouseManagementSystem.Helpers;
using WarehouseManagementSystem.Models;
namespace WarehouseManagementSystem.Forms
{
    public partial class FormAdminMain : Form
    {
        private Form currentChildForm;

        public FormAdminMain()
        {
            InitializeComponent();
        }

        private void FormAdminMain_Load(object sender, EventArgs e)
        {
            this.Text = "Складская система - Администратор";
            this.WindowState = FormWindowState.Maximized;

            // Настройка кнопки ВЫХОД
            btnExit.FlatStyle = FlatStyle.Flat;
            btnExit.FlatAppearance.BorderColor = Color.Black;
            btnExit.FlatAppearance.BorderSize = 1;
            btnExit.BackColor = Color.White;
            btnExit.Text = "ВЫХОД";

            // Настройка MenuStrip
            menuStrip.BackColor = SystemColors.Control;
        }

        private void OpenChildForm(Form childForm)
        {
            if (currentChildForm != null)
            {
                currentChildForm.Close();
            }

            currentChildForm = childForm;
            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;
            panelContent.Controls.Add(childForm);
            panelContent.Tag = childForm;
            childForm.BringToFront();
            childForm.Show();
        }

        // Справочники -> Товары
        private void товарыToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormProducts productsForm = new FormProducts();
            OpenChildForm(productsForm);
        }

        // Справочники -> Категории
        private void категорииToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormCategories categoriesForm = new FormCategories();
            OpenChildForm(categoriesForm);
        }

        // Администрирование -> История отгрузок
        private void историяОтгрузокToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormShipmentHistory historyForm = new FormShipmentHistory();
            OpenChildForm(historyForm);
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}