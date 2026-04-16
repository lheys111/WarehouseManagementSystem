using System;
using System.Drawing;
using System.Windows.Forms;
using WarehouseManagementSystem.Helpers;
using WarehouseManagementSystem.Models;
using WarehouseManagementSystem.Services;

namespace WarehouseManagementSystem.Forms
{
    public partial class FormStorekeeperMain : Form
    {
        private Form _currentChildForm;

        public FormStorekeeperMain()
        {
            InitializeComponent();
            InitializeEvents();
            SetupForm();
        }

        private void InitializeEvents()
        {
            btnExit.Click += btnExit_Click;
            остаткиToolStripMenuItem.Click += остаткиToolStripMenuItem_Click;
            отгрузкиToolStripMenuItem.Click += отгрузкиToolStripMenuItem_Click;
            новаяОтгрузкаToolStripMenuItem.Click += новаяОтгрузкаToolStripMenuItem_Click;
        }

        private void SetupForm()
        {
            Text = Constants.FormTitles.StorekeeperMain;
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

        private void остаткиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenChildForm(new FormStockBalances());
        }

        private void отгрузкиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenChildForm(new FormShipmentHistory(true));
        }

        private void новаяОтгрузкаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenChildForm(new FormNewShipment());
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            AppLogger.Info("Завершение работы приложения");
            Application.Exit();
        }

        private void новаяПоставкаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormSupply form = new FormSupply(Session.CurrentUser.Id);
            form.ShowDialog();
        }

        private void FormStorekeeperMain_Load(object sender, EventArgs e)
        {

        }
    }
}