namespace business_logic.DTOs
{
    public sealed record GetDriverResponseModel
    {
        public int Id { get; init; }
        public string Name { get; init; }
        public string Surname { get; init; }
        public string Patronymic { get; init; }
        public string LicenseNumber { get; init; }
        public int DepotId { get; init; }
        public string DepotName { get; init; } = string.Empty;
        public bool IsArchived { get; init; }
        public byte[] RowVersion { get; init; } = [];
    }
}
