namespace business_logic.DTOs
{
    public interface IRouteAddressesRequestModel
    {
        string? StartAddressLine { get; }
        double? StartAddressLatitude { get; }
        double? StartAddressLongitude { get; }
        string? DestinationAddressLine { get; }
        double? DestinationAddressLatitude { get; }
        double? DestinationAddressLongitude { get; }
    }
}
