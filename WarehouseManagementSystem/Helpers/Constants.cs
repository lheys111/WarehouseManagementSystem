namespace WarehouseManagementSystem.Helpers
{
    public static class Constants
    {
        public static class Messages
        {
            public const string LoginError = "Неверный email или пароль";
            public const string UserNotFound = "Пользователь не найден";
            public const string FillAllFields = "Заполните все поля";
            public const string PasswordMismatch = "Пароли не совпадают";
            public const string EmailExists = "Пользователь с таким email уже существует";
            public const string RegistrationSuccess = "Регистрация успешно завершена!";
            public const string ConnectionError = "Ошибка подключения к базе данных";
            public const string SaveSuccess = "Сохранено успешно";
            public const string DeleteConfirm = "Вы уверены, что хотите удалить?";
            public const string SelectItem = "Выберите элемент";
            public const string ArticleExists = "Товар с таким артикулом уже существует";
            public const string PriceInvalid = "Введите корректную цену";
            public const string QuantityInvalid = "Введите корректное количество";
            public const string StockInsufficient = "Недостаточно товара на складе";
            public const string ShipmentConfirm = "Подтвердить отгрузку? Товары будут списаны";
            public const string ShipmentSuccess = "Отгрузка успешно проведена";
            public const string ProductNotFound = "Товар не найден";
            public const string CategoryNotFound = "Категория не найдена";
            public const string CategoryHasProducts = "Нельзя удалить категорию, так как в ней есть товары";
            public const string ProductHasShipments = "Нельзя удалить товар, так как есть отгрузки с этим товаром";
        }

        /*public static class Roles
        {
            public const string Admin = "Admin";
            public const string Storekeeper = "Storekeeper";
        }*/

        public static class ShipmentStatus
        {
            public const string Completed = "Completed";
            public const string Draft = "Draft";
        }

        public static class FormTitles
        {
            public const string AdminMain = "Складская система - Администратор";
            public const string StorekeeperMain = "Складская система - Кладовщик";
            public const string Login = "Авторизация";
            public const string Registration = "Регистрация кладовщика";
            public const string AddProduct = "Добавление товара";
            public const string EditProduct = "Редактирование товара";
            public const string AddCategory = "Добавление категории";
            public const string EditCategory = "Редактирование категории";
            public const string Categories = "Управление категориями";
            public const string Products = "Список товаров";
            public const string NewShipment = "Оформление новой отгрузки";
            public const string SelectProduct = "Выбор товара и количества";
            public const string ShipmentHistory = "История отгрузок";
            public const string ShipmentDetails = "Детали отгрузки";
            public const string StockBalances = "Остатки товаров";
        }

        public static class GridHeaders
        {
            public const string Article = "Артикул";
            public const string Name = "Название";
            public const string Category = "Категория";
            public const string Unit = "Ед. изм.";
            public const string Price = "Цена закупки";
            public const string Stock = "Остаток";
            public const string Quantity = "Количество";
            public const string ShipmentNumber = "Номер отгрузки";
            public const string Date = "Дата";
            public const string Storekeeper = "Кладовщик";
            public const string ItemsCount = "Количество позиций";
            public const string TotalSum = "Общая сумма";
            public const string Description = "Описание";
            public const string ProductsCount = "Товаров";
            public const string Actions = "Действия";
        }

        public static class Queries
        {
            public const string GetUserByEmail = "SELECT Id, FullName, Role FROM Users WHERE Email = @Email AND PasswordHash = @Password";
            public const string GetCategories = "SELECT Id, Name FROM Categories ORDER BY Name";
            public const string GetProductsWithStock = @"SELECT Id, Article, Name, CategoryName, UnitOfMeasure, PurchasePrice, StockQuantity FROM vw_ProductsWithStock";
            public const string GetAllShipments = @"SELECT Id, ShipmentNumber, ShipmentDate, StorekeeperName, ItemsCount, TotalSum FROM vw_ShipmentsHistory ORDER BY ShipmentDate DESC";
            public const string GenerateShipmentNumber = "SELECT generate_shipment_number()";
            public const string GetCategoriesWithCount = @"SELECT c.Id, c.Name, c.Description, COUNT(p.Id) as ProductsCount
                                                          FROM Categories c
                                                          LEFT JOIN Products p ON c.Id = p.CategoryId
                                                          GROUP BY c.Id, c.Name, c.Description
                                                          ORDER BY c.Name";
        }

        public static class ButtonText
        {
            public const string Add = "Добавить";
            public const string Edit = "Изменить";
            public const string Delete = "Удалить";
            public const string Save = "Сохранить";
            public const string Cancel = "Отмена";
            public const string Search = "Найти";
            public const string Exit = "ВЫХОД";
            public const string Refresh = "Обновить";
            public const string AddItem = "Добавить позицию";
            public const string RemoveItem = "Удалить";
            public const string Confirm = "Провести отгрузку";
            public const string ViewDetails = "Просмотреть детали";
            public const string Close = "Закрыть";
            public const string Register = "Зарегистрироваться";
            public const string Login = "Войти";
        }
    }
}