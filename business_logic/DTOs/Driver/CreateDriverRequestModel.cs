namespace business_logic.DTOs
{
    public sealed record CreateDriverRequestModel
    {
        public string Name { get; init; }
        public string Surname { get; init; }
        public string Patronymic { get; init; }
        public string LicenseNumber { get; init; }
    }
}
