namespace business_logic.DTOs
{
    public sealed record GetDriverLookupOptionResponseModel
    {
        public int Id { get; init; }
        public string Name { get; init; }
        public string Surname { get; init; }
        public string LicenseNumber { get; init; }
        public int DepotId { get; init; }
    }
}
