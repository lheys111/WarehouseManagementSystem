using NLog;
using OfficeOpenXml;
using Org.BouncyCastle.Utilities;
using System;
using System.Windows.Forms;
using WarehouseManagementSystem.Forms;

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
                ExcelPackage.License.SetNonCommercialPersonal("Даша");

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new FormLogin());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Критическая ошибка при запуске приложения");
                MessageBox.Show(string.Format(String.CriticalError, ex.Message), String.ErrorTitle,
    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}