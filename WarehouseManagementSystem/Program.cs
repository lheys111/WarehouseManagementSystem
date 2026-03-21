using System;
using System.Windows.Forms;
using WarehouseManagementSystem.Forms;
using NLog;

namespace WarehouseManagementSystem
{
    internal static class Program
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        [STAThread]
        static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new FormLogin());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Критическая ошибка при запуске приложения");
                MessageBox.Show($"Произошла критическая ошибка:\n{ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}