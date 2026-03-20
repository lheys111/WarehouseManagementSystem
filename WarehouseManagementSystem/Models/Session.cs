namespace WarehouseManagementSystem.Models
{
    public static class Session
    {
        public static User CurrentUser { get; set; }

        public static bool IsAuthenticated
        {
            get { return CurrentUser != null; }
        }

        public static bool IsAdmin
        {
            get { return CurrentUser != null && CurrentUser.Role == "Admin"; }
        }

        public static bool IsStorekeeper
        {
            get { return CurrentUser != null && CurrentUser.Role == "Storekeeper"; }
        }

        public static void Clear()
        {
            CurrentUser = null;
        }
    }
}