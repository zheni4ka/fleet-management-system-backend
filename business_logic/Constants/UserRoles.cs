namespace business_logic.Constants
{
    public static class UserRoles
    {
        public const string Admin = "Admin";
        public const string Dispatcher = "Dispatcher";
        public const string AdminOrDispatcher = Admin + "," + Dispatcher;

        public static readonly IReadOnlyList<string> All = new[] { Admin, Dispatcher };
    }
}
