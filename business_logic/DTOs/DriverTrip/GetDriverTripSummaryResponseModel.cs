namespace business_logic.DTOs
{
    public sealed record GetDriverTripSummaryResponseModel
    {
        public int DriverId { get; init; }
        public int TripsCount { get; init; }
        public int CompletedTripsCount { get; init; }
        public int CancelledTripsCount { get; init; }
        public int DrivingMinutes { get; init; }
        public DateTime? LastTripEndedAtUtc { get; init; }
    }
}
