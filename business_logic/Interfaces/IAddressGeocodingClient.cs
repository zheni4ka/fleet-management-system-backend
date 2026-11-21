using business_logic.DTOs;

namespace business_logic.Interfaces
{
    public interface IAddressGeocodingClient
    {
        Task<IReadOnlyList<GetAddressCandidateResponseModel>> SearchAddressesAsync(
            string searchText, double latitude, double longitude, string languageCode, int resultsCount);
    }
}
