namespace business_logic.DTOs
{
    public abstract record DriverRequestModelBase
    {
        public string Name { get; init; }
        public string Surname { get; init; }
        public string Patronymic { get; init; }
        public string LicenseNumber { get; init; }
    }
}
