using NLog;
using OfficeOpenXml;
using Org.BouncyCastle.Utilities;
using System;
using System.Windows.Forms;
using WarehouseManagementSystem.Forms;

namespace WarehouseManagementSystem
{
    /// <summary>
   
    /// 1: ПОСТАВКИ
    /// 
    /// 1.1 - Форма ручного ввода поставки
    /// - Номер документа генерируется автоматически
    /// - Таблица для добавления позиций (выбор товара из каталога, количество, цена)
    /// - После сохранения остатки увеличиваются, история сохраняется
    /// 
    /// 1.2 - Импорт поставки из файла
    /// - Кнопка "Импорт из файла" на форме поставки
    /// - Поддержка CSV (разделитель ";") и Excel (.xlsx)
    /// - При ошибке (артикул не найден) - сообщение с номером строки
    /// 
    /// 2: ОТЧЁТНОСТЬ
    /// 
    /// 2.1 - Доработка истории отгрузок 
    /// - Добавлена колонка "Общая сумма" в FormShipmentHistory
    /// - Сумма = сумма по позициям (количество × цена)
    /// - В детальном просмотре (FormShipmentDetails) отображается итоговая сумма
    /// 
    /// 2.2 - Отчёт по отгрузкам за период
    /// - Создана форма FormShipmentReport
    /// - Выбор периода, кнопка "Сформировать"
    /// - Таблица: дата, номер, кладовщик, сумма, себестоимость, прибыль
    /// 
    /// 2.3 - Экспорт отчёта в Excel
    /// - Кнопка "Экспорт" в форме отчёта
    /// - Выгрузка в Excel (.xlsx) с заголовками и форматированием
    ///
    /// 3: СРОКИ ГОДНОСТИ
    /// 
    /// 3.1 - Добавление срока годности в карточку товара 
    /// - Поле "Срок годности" в карточке товара (опционально)
    /// - Колонка "Статус" в списке товаров (Годен/Просрочен/Скоро истекает)
    /// 
    /// 3.2 - Партионный учёт (разные сроки годности) 
    /// - Создана таблица StockBatches
    /// - При отгрузке списание FIFO (сначала партии с ближайшим сроком)
    /// - При детализации остатков - разбивка по партиям
    /// 
    /// 3.3 - Автоматическая скидка
    /// - В настройках: процент скидки и количество дней
    /// - При отгрузке проверяется срок годности, применяется скидка
    /// - В форме отгрузки отображается цена со скидкой
    /// 
    /// 3.4 - Списание просроченных товаров 
    /// - Форма FormWriteOffExpired со списком просроченных товаров
    /// - Кнопка "Списать выбранные"
    /// - При списании: уменьшение остатка, запись в историю списаний    /// </summary>
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