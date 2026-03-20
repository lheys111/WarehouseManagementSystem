using System;
using System.Windows.Forms;
using static System.Collections.Specialized.BitVector32;
using WarehouseManagementSystem.Helpers;
using WarehouseManagementSystem.Models;
namespace WarehouseManagementSystem.Forms
{
    public partial class FormStorekeeperMain : Form
    {
        private Form currentChildForm;

        public FormStorekeeperMain()
        {
            InitializeComponent();
        }

        private void FormStorekeeperMain_Load(object sender, EventArgs e)
        {
            this.Text = "Складская система - Кладовщик";
            //lblUser.Text = $"Кладовщик: {Session.CurrentUser.FullName}";
        }

        private void OpenChildForm(Form childForm)
        {
            if (currentChildForm != null)
                currentChildForm.Close();

            currentChildForm = childForm;
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
            FormStockBalances stockForm = new FormStockBalances();
            OpenChildForm(stockForm);
        }

        private void отгрузкиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormShipmentHistory historyForm = new FormShipmentHistory(true);
            OpenChildForm(historyForm);
        }

        private void новаяОтгрузкаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormNewShipment newShipmentForm = new FormNewShipment();
            OpenChildForm(newShipmentForm);
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}