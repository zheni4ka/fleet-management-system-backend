namespace business_logic.DTOs
{
    public sealed record UpdateDriverRequestModel
    {
        public int Id { get; init; }
        public string Name { get; init; }
        public string Surname { get; init; }
        public string Patronymic { get; init; }
        public string LicenseNumber { get; init; }
    }
}
