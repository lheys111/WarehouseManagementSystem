using System;
using System.Windows.Forms;
using WarehouseManagementSystem.Forms;

namespace WarehouseManagementSystem
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormLogin());  // ← ДОЛЖНО БЫТЬ ТАК
        }
    }
}