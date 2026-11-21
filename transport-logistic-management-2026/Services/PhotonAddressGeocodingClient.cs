using business_logic.Constants;
using business_logic.DTOs;
using business_logic.Exceptions;
using business_logic.Interfaces;
using System.Globalization;
using System.Text.Json;
using transport_logistic_management_2026.Constants;
using transport_logistic_management_2026.ExternalModels;

namespace transport_logistic_management_2026.Services
{
    public class PhotonAddressGeocodingClient : IAddressGeocodingClient
    {
        private const string SearchPathTemplate =
            "api/?q={0}&lat={1}&lon={2}&bbox={3},{4},{5},{6}&layer=house&layer=street&limit={7}&lang={8}";

        private readonly HttpClient _httpClient;
        private readonly ILogger<PhotonAddressGeocodingClient> _logger;

        public PhotonAddressGeocodingClient(HttpClient httpClient, ILogger<PhotonAddressGeocodingClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<IReadOnlyList<GetAddressCandidateResponseModel>> SearchAddressesAsync(
            string searchText, double latitude, double longitude, string languageCode, int resultsCount)
        {
            var searchPath = string.Format(
                CultureInfo.InvariantCulture,
                SearchPathTemplate,
                Uri.EscapeDataString(searchText),
                latitude,
                longitude,
                RoundCoordinate(longitude - AddressGeocodingConstraints.SearchAreaLongitudeRadiusInDegrees),
                RoundCoordinate(latitude - AddressGeocodingConstraints.SearchAreaLatitudeRadiusInDegrees),
                RoundCoordinate(longitude + AddressGeocodingConstraints.SearchAreaLongitudeRadiusInDegrees),
                RoundCoordinate(latitude + AddressGeocodingConstraints.SearchAreaLatitudeRadiusInDegrees),
                resultsCount * AddressGeocodingConstraints.ProviderResultsOverfetchFactor,
                languageCode == PhotonApi.EnglishLanguageCode ? PhotonApi.EnglishLanguageCode : PhotonApi.DefaultLanguageCode);

            var featureCollection = await SendSearchRequestAsync(searchPath);
            return featureCollection.Features
                .Select(MapToAddressCandidate)
                .OfType<GetAddressCandidateResponseModel>()
                .DistinctBy(addressCandidate => addressCandidate.AddressLine)
                .Take(resultsCount)
                .ToList();
        }

        private static double RoundCoordinate(double coordinate)
        {
            return Math.Round(coordinate, AddressGeocodingConstraints.CoordinateFractionDigits);
        }

        private async Task<PhotonFeatureCollection> SendSearchRequestAsync(string searchPath)
        {
            try
            {
                using var searchResponse = await _httpClient.GetAsync(searchPath);
                searchResponse.EnsureSuccessStatusCode();
                return await searchResponse.Content.ReadFromJsonAsync<PhotonFeatureCollection>() ?? new PhotonFeatureCollection();
            }
            catch (Exception addressSearchException) when (addressSearchException is HttpRequestException or TaskCanceledException or JsonException)
            {
                _logger.LogWarning(addressSearchException, "Address geocoding request '{RequestPath}' failed.", searchPath);
                throw new ExternalServiceUnavailableException(ExceptionMessages.AddressGeocodingServiceUnavailable, addressSearchException);
            }
        }

        private static GetAddressCandidateResponseModel? MapToAddressCandidate(PhotonFeature feature)
        {
            var featureProperties = feature.Properties;
            var featureCoordinates = feature.Geometry?.Coordinates;
            if (featureProperties is null || featureCoordinates is not { Count: 2 })
            {
                return null;
            }

            var addressLine = featureProperties.Type switch
            {
                PhotonApi.HouseResultType when !string.IsNullOrWhiteSpace(featureProperties.Street)
                    && !string.IsNullOrWhiteSpace(featureProperties.HouseNumber)
                    => $"{featureProperties.Street}, {featureProperties.HouseNumber}",
                PhotonApi.StreetResultType when !string.IsNullOrWhiteSpace(featureProperties.Name)
                    => featureProperties.Name,
                _ => null
            };
            if (addressLine is null)
            {
                return null;
            }

            return new GetAddressCandidateResponseModel
            {
                AddressLine = addressLine,
                District = featureProperties.District,
                Postcode = featureProperties.Postcode,
                Latitude = featureCoordinates[1],
                Longitude = featureCoordinates[0]
            };
        }
    }
}
