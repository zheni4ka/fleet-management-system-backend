using AutoMapper;
using business_logic.Constants;
using business_logic.DTOs;
using business_logic.Entities;
using business_logic.Exceptions;
using business_logic.Helpers;
using business_logic.Interfaces;
using business_logic.Settings;
using business_logic.Specifications;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace business_logic.Services
{
    public class LocationService : ILocationService
    {
        private readonly IRepository<Location> _locationRepository;
        private readonly IRepository<Route> _routeRepository;
        private readonly IAuditLogService _auditLogService;
        private readonly IDatabaseApplicationLockService _databaseApplicationLockService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICityGeocodingClient _cityGeocodingClient;
        private readonly IAddressGeocodingClient _addressGeocodingClient;
        private readonly CityGeocodingSettings _cityGeocodingSettings;
        private readonly IValidator<CreateLocationRequestModel> _createValidator;
        private readonly IValidator<GetCityGeocodingCandidatesRequestModel> _cityGeocodingCandidatesRequestValidator;
        private readonly IValidator<GetAddressCandidatesRequestModel> _addressCandidatesRequestValidator;
        private readonly IValidator<GetPagedListRequestModel<LocationSortField>> _pagedListRequestValidator;
        private readonly IValidator<GetLocationsBySearchFilterRequestModel> _searchFilterValidator;

        public LocationService(
            IRepository<Location> locationRepository,
            IRepository<Route> routeRepository,
            IAuditLogService auditLogService,
            IDatabaseApplicationLockService databaseApplicationLockService,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICityGeocodingClient cityGeocodingClient,
            IAddressGeocodingClient addressGeocodingClient,
            IOptions<CityGeocodingSettings> cityGeocodingSettingsOptions,
            IValidator<CreateLocationRequestModel> createValidator,
            IValidator<GetCityGeocodingCandidatesRequestModel> cityGeocodingCandidatesRequestValidator,
            IValidator<GetAddressCandidatesRequestModel> addressCandidatesRequestValidator,
            IValidator<GetPagedListRequestModel<LocationSortField>> pagedListRequestValidator,
            IValidator<GetLocationsBySearchFilterRequestModel> searchFilterValidator)
        {
            _locationRepository = locationRepository;
            _routeRepository = routeRepository;
            _auditLogService = auditLogService;
            _databaseApplicationLockService = databaseApplicationLockService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cityGeocodingClient = cityGeocodingClient;
            _addressGeocodingClient = addressGeocodingClient;
            _cityGeocodingSettings = cityGeocodingSettingsOptions.Value;
            _createValidator = createValidator;
            _cityGeocodingCandidatesRequestValidator = cityGeocodingCandidatesRequestValidator;
            _addressCandidatesRequestValidator = addressCandidatesRequestValidator;
            _pagedListRequestValidator = pagedListRequestValidator;
            _searchFilterValidator = searchFilterValidator;
        }

        public async Task<IReadOnlyList<GetCityGeocodingCandidateResponseModel>> SearchCityGeocodingCandidatesAsync(
            GetCityGeocodingCandidatesRequestModel cityGeocodingCandidatesRequest)
        {
            await _cityGeocodingCandidatesRequestValidator.ValidateAndThrowAsync(cityGeocodingCandidatesRequest);

            return await _cityGeocodingClient.SearchCitiesAsync(
                cityGeocodingCandidatesRequest.SearchText!.Trim(),
                CurrentUiLanguageResolver.GetCurrentSupportedLanguageCode(),
                CityGeocodingConstraints.SearchResultsMaxCount);
        }

        public async Task<IReadOnlyList<GetAddressCandidateResponseModel>> SearchAddressCandidatesAsync(
            GetAddressCandidatesRequestModel addressCandidatesRequest)
        {
            await _addressCandidatesRequestValidator.ValidateAndThrowAsync(addressCandidatesRequest);

            return await _addressGeocodingClient.SearchAddressesAsync(
                addressCandidatesRequest.SearchText!.Trim(),
                addressCandidatesRequest.Latitude!.Value,
                addressCandidatesRequest.Longitude!.Value,
                CurrentUiLanguageResolver.GetCurrentSupportedLanguageCode(),
                AddressGeocodingConstraints.SearchResultsMaxCount);
        }

        public async Task<FillMissingLocationCoordinatesResponseModel> FillMissingCoordinatesAsync()
        {
            var locationsMissingCoordinates = await _locationRepository.GetListBySpecAsync(new LocationSpecs.MissingCoordinates());
            var usedGeoNamesIds = (await _locationRepository.GetProjectedListBySpecAsync(new LocationSpecs.UsedGeoNamesIds())).ToHashSet();
            var filledLocations = new List<FilledLocationCoordinatesResponseModel>();
            var unresolvedLocations = new List<UnresolvedLocationCoordinatesResponseModel>();

            foreach (var locationMissingCoordinates in locationsMissingCoordinates)
            {
                var cityCandidates = await _cityGeocodingClient.SearchCitiesAsync(
                    locationMissingCoordinates.City,
                    _cityGeocodingSettings.StoredNamesLanguageCode,
                    CityGeocodingConstraints.MissingCoordinatesLookupResultsCount);
                var cityCandidatesInLocationCountry = cityCandidates
                    .Where(cityCandidate => string.Equals(cityCandidate.Country, locationMissingCoordinates.Country, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(cityCandidate => cityCandidate.Population.HasValue)
                    .ThenByDescending(cityCandidate => cityCandidate.Population)
                    .ToList();

                var unresolvedReason = GetUnresolvedLocationCoordinatesReason(cityCandidatesInLocationCountry, usedGeoNamesIds);
                if (unresolvedReason.HasValue)
                {
                    unresolvedLocations.Add(new UnresolvedLocationCoordinatesResponseModel(
                        LocationId: locationMissingCoordinates.Id,
                        City: locationMissingCoordinates.City,
                        Country: locationMissingCoordinates.Country,
                        Reason: unresolvedReason.Value));
                    continue;
                }

                var mostPopulousCityCandidate = cityCandidatesInLocationCountry[0];
                var previousTimeZoneId = locationMissingCoordinates.TimeZoneId;
                ApplyCityGeocodingCandidate(locationMissingCoordinates, mostPopulousCityCandidate);
                usedGeoNamesIds.Add(mostPopulousCityCandidate.GeoNamesId);
                await _auditLogService.LogActionAsync(AuditAction.LocationCoordinatesFilled, locationMissingCoordinates.Id);
                filledLocations.Add(new FilledLocationCoordinatesResponseModel(
                    LocationId: locationMissingCoordinates.Id,
                    City: locationMissingCoordinates.City,
                    Country: locationMissingCoordinates.Country,
                    GeoNamesId: mostPopulousCityCandidate.GeoNamesId,
                    MatchedCity: mostPopulousCityCandidate.City,
                    MatchedRegion: mostPopulousCityCandidate.Region,
                    Latitude: mostPopulousCityCandidate.Latitude,
                    Longitude: mostPopulousCityCandidate.Longitude,
                    TimeZoneId: mostPopulousCityCandidate.TimeZoneId,
                    PreviousTimeZoneId: previousTimeZoneId,
                    OtherCandidatesInCountryCount: cityCandidatesInLocationCountry.Count - 1));
            }

            await _unitOfWork.SaveChangesAsync();
            return new FillMissingLocationCoordinatesResponseModel(filledLocations, unresolvedLocations);
        }

        public async Task<GetPagedListResponseModel<GetLocationResponseModel>> GetPagedListAsync(
            GetLocationsBySearchFilterRequestModel searchFilter,
            GetPagedListRequestModel<LocationSortField> pagedListRequest)
        {
            await _searchFilterValidator.ValidateAndThrowAsync(searchFilter);
            await _pagedListRequestValidator.ValidateAndThrowAsync(pagedListRequest);

            var locationsPagedListSpecification = new LocationSpecs.PagedList(searchFilter, pagedListRequest);
            var locationsPage = await _locationRepository.GetProjectedListBySpecAsync(locationsPagedListSpecification);
            var locationsTotalCount = await _locationRepository.CountBySpecAsync(locationsPagedListSpecification);

            return new GetPagedListResponseModel<GetLocationResponseModel>(
                Items: locationsPage,
                TotalCount: locationsTotalCount);
        }

        public async Task<IReadOnlyList<GetLocationLookupOptionResponseModel>> GetLookupOptionsAsync()
        {
            return await _locationRepository.GetProjectedListBySpecAsync(new LocationSpecs.LookupOptions());
        }

        public async Task<GetLocationResponseModel> GetAsync(int id)
        {
            var location = await GetLocationOrThrowAsync(id);
            return _mapper.Map<GetLocationResponseModel>(location);
        }

        public async Task<GetLocationResponseModel> EnsureCityLocationAsync(CreateLocationRequestModel model)
        {
            await _createValidator.ValidateAndThrowAsync(model);

            var existingLocation = await _locationRepository.GetItemBySpecAsync(new LocationSpecs.ByGeoNamesId(model.GeoNamesId));
            if (existingLocation is not null && !existingLocation.IsArchived)
            {
                return _mapper.Map<GetLocationResponseModel>(existingLocation);
            }

            var cityGeocodingCandidate = existingLocation is null
                ? await GetSupportedCityGeocodingCandidateAsync(model.GeoNamesId)
                : null;

            await using var cityLocationTransaction = await _unitOfWork.BeginTransactionAsync();
            await _databaseApplicationLockService.AcquireTransactionExclusiveLocksAsync(
                [ApplicationLockResourceNameFactory.CreateCityLocationLockResourceName(model.GeoNamesId)]);

            var cityLocation = await _locationRepository.GetItemBySpecAsync(new LocationSpecs.ByGeoNamesId(model.GeoNamesId));
            if (cityLocation is null)
            {
                cityGeocodingCandidate ??= await GetSupportedCityGeocodingCandidateAsync(model.GeoNamesId);
                cityLocation = await AttachToLocationWithoutGeoNamesIdOrCreateAsync(cityGeocodingCandidate);
            }
            else if (cityLocation.IsArchived)
            {
                ArchivingHelper.RestoreArchivedEntity(cityLocation, cityLocation.Id);
                await _auditLogService.LogActionAsync(AuditAction.LocationRestored, cityLocation.Id);
                await _unitOfWork.SaveChangesAsync();
            }

            await cityLocationTransaction.CommitAsync();
            return _mapper.Map<GetLocationResponseModel>(cityLocation);
        }

        private async Task<GetCityGeocodingCandidateResponseModel> GetSupportedCityGeocodingCandidateAsync(int geoNamesId)
        {
            var cityGeocodingCandidate = await _cityGeocodingClient.FindCityByGeoNamesIdAsync(
                geoNamesId,
                _cityGeocodingSettings.StoredNamesLanguageCode)
                ?? throw new NotFoundException(string.Format(ExceptionMessages.CityGeocodingCandidateNotFoundTemplate, geoNamesId));
            if (!IsTimeZoneSupported(cityGeocodingCandidate.TimeZoneId))
            {
                throw new BusinessRuleException(string.Format(
                    ExceptionMessages.CityGeocodingTimeZoneUnsupportedTemplate,
                    cityGeocodingCandidate.City,
                    cityGeocodingCandidate.TimeZoneId));
            }

            return cityGeocodingCandidate;
        }

        private async Task<Location> AttachToLocationWithoutGeoNamesIdOrCreateAsync(GetCityGeocodingCandidateResponseModel cityGeocodingCandidate)
        {
            var locationWithoutGeoNamesId = await _locationRepository.GetItemBySpecAsync(
                new LocationSpecs.WithoutGeoNamesIdByCityAndCountry(cityGeocodingCandidate.City, cityGeocodingCandidate.Country));
            if (locationWithoutGeoNamesId is not null)
            {
                ApplyCityGeocodingCandidate(locationWithoutGeoNamesId, cityGeocodingCandidate);
                if (locationWithoutGeoNamesId.IsArchived)
                {
                    ArchivingHelper.RestoreArchivedEntity(locationWithoutGeoNamesId, locationWithoutGeoNamesId.Id);
                }

                await _auditLogService.LogActionAsync(AuditAction.LocationCoordinatesFilled, locationWithoutGeoNamesId.Id);
                await _unitOfWork.SaveChangesAsync();
                return locationWithoutGeoNamesId;
            }

            var location = new Location
            {
                City = cityGeocodingCandidate.City,
                Country = cityGeocodingCandidate.Country
            };
            ApplyCityGeocodingCandidate(location, cityGeocodingCandidate);
            await _locationRepository.InsertAsync(location);
            await _unitOfWork.SaveChangesAsync();
            await _auditLogService.LogActionAsync(AuditAction.LocationCreated, location.Id);
            await _unitOfWork.SaveChangesAsync();
            return location;
        }

        public async Task<DeleteLocationResponseModel> DeleteAsync(int id)
        {
            var location = await GetLocationOrThrowAsync(id);

            if (await _routeRepository.AnyBySpecAsync(new RouteSpecs.ActiveByLocationId(id)))
            {
                throw new BusinessRuleException(string.Format(ExceptionMessages.LocationHasActiveRoutesTemplate, id));
            }

            var isLocationReferencedByRoutes = await _routeRepository.AnyBySpecAsync(new RouteSpecs.ByLocationId(id));
            if (isLocationReferencedByRoutes)
            {
                ArchivingHelper.ArchiveEntity(location);
                await _auditLogService.LogActionAsync(AuditAction.LocationArchived, id);
            }
            else
            {
                _locationRepository.Delete(location);
                await _auditLogService.LogActionAsync(AuditAction.LocationDeleted, id);
            }

            await _unitOfWork.SaveChangesAsync();
            return new DeleteLocationResponseModel { IsArchivedInsteadOfDeleted = isLocationReferencedByRoutes };
        }

        public async Task RestoreAsync(int id)
        {
            var location = await GetLocationOrThrowAsync(id);
            ArchivingHelper.RestoreArchivedEntity(location, id);
            await _auditLogService.LogActionAsync(AuditAction.LocationRestored, id);
            await _unitOfWork.SaveChangesAsync();
        }

        private static UnresolvedLocationCoordinatesReason? GetUnresolvedLocationCoordinatesReason(
            IReadOnlyList<GetCityGeocodingCandidateResponseModel> cityCandidatesInLocationCountry,
            IReadOnlySet<int> usedGeoNamesIds)
        {
            if (cityCandidatesInLocationCountry.Count == 0)
            {
                return UnresolvedLocationCoordinatesReason.NoCandidateInCountry;
            }

            var mostPopulousCityCandidate = cityCandidatesInLocationCountry[0];
            if (usedGeoNamesIds.Contains(mostPopulousCityCandidate.GeoNamesId))
            {
                return UnresolvedLocationCoordinatesReason.GeoNamesIdUsedByAnotherLocation;
            }

            if (!IsTimeZoneSupported(mostPopulousCityCandidate.TimeZoneId))
            {
                return UnresolvedLocationCoordinatesReason.TimeZoneUnsupported;
            }

            return null;
        }

        private static bool IsTimeZoneSupported(string timeZoneId)
        {
            return TimeZoneInfo.TryFindSystemTimeZoneById(timeZoneId, out _);
        }

        private static void ApplyCityGeocodingCandidate(Location location, GetCityGeocodingCandidateResponseModel cityGeocodingCandidate)
        {
            location.GeoNamesId = cityGeocodingCandidate.GeoNamesId;
            location.Latitude = cityGeocodingCandidate.Latitude;
            location.Longitude = cityGeocodingCandidate.Longitude;
            location.TimeZoneId = cityGeocodingCandidate.TimeZoneId;
        }

        private async Task<Location> GetLocationOrThrowAsync(int id)
        {
            return await _locationRepository.GetItemBySpecAsync(new LocationSpecs.ById(id)) ?? throw NotFoundException.For<Location>(id);
        }
    }
}
