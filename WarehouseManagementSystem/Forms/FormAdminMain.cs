using System;
using System.Drawing;
using System.Windows.Forms;
using WarehouseManagementSystem.Helpers;

namespace WarehouseManagementSystem.Forms
{
    public partial class FormAdminMain : Form
    {
        private Form _currentChildForm;

        public FormAdminMain()
        {
            InitializeComponent();
            InitializeEvents();
            SetupForm();
        }

        private void InitializeEvents()
        {
            btnExit.Click += btnExit_Click;
            товарыToolStripMenuItem.Click += товарыToolStripMenuItem_Click;
            категорииToolStripMenuItem.Click += категорииToolStripMenuItem_Click;
            историяОтгрузокToolStripMenuItem.Click += историяОтгрузокToolStripMenuItem_Click;
        }

        private void SetupForm()
        {
            Text = Constants.FormTitles.AdminMain;
            WindowState = FormWindowState.Maximized;

            btnExit.FlatStyle = FlatStyle.Flat;
            btnExit.FlatAppearance.BorderColor = Color.Black;
            btnExit.FlatAppearance.BorderSize = 1;
            btnExit.BackColor = Color.White;
            btnExit.Text = Constants.ButtonText.Exit;

            menuStrip.BackColor = SystemColors.Control;
        }

        private void OpenChildForm(Form childForm)
        {
            if (_currentChildForm != null)
                _currentChildForm.Close();

            _currentChildForm = childForm;
            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;
            panelContent.Controls.Add(childForm);
            panelContent.Tag = childForm;
            childForm.BringToFront();
            childForm.Show();
        }

        private void товарыToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenChildForm(new FormProducts());
        }

        private void категорииToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenChildForm(new FormCategories());
        }

        private void историяОтгрузокToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenChildForm(new FormShipmentHistory());
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            AppLogger.Info("Завершение работы приложения");
            Application.Exit();
        }
    }
}
