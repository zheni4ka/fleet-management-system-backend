using business_logic.Constants;
using business_logic.DTOs;
using business_logic.Entities;
using business_logic.Exceptions;
using business_logic.Interfaces;
using business_logic.Specifications;
using FluentValidation;

namespace business_logic.Services
{
    public class DriverTripService : IDriverTripService
    {
        private readonly IRepository<DriverTrip> _driverTripRepository;
        private readonly IRepository<Driver> _driverRepository;
        private readonly IRepository<Location> _locationRepository;
        private readonly IValidator<GetDriverTripsRequestModel> _driverTripsRequestValidator;
        private readonly IValidator<GetPagedListRequestModel<DriverTripSortField>> _pagedListRequestValidator;

        public DriverTripService(
            IRepository<DriverTrip> driverTripRepository,
            IRepository<Driver> driverRepository,
            IRepository<Location> locationRepository,
            IValidator<GetDriverTripsRequestModel> driverTripsRequestValidator,
            IValidator<GetPagedListRequestModel<DriverTripSortField>> pagedListRequestValidator)
        {
            _driverTripRepository = driverTripRepository;
            _driverRepository = driverRepository;
            _locationRepository = locationRepository;
            _driverTripsRequestValidator = driverTripsRequestValidator;
            _pagedListRequestValidator = pagedListRequestValidator;
        }

        public async Task<GetPagedListResponseModel<GetDriverTripResponseModel>> GetPagedListAsync(
            GetDriverTripsRequestModel driverTripsRequest,
            GetPagedListRequestModel<DriverTripSortField> pagedListRequest)
        {
            await _driverTripsRequestValidator.ValidateAndThrowAsync(driverTripsRequest);
            await _pagedListRequestValidator.ValidateAndThrowAsync(pagedListRequest);
            await EnsureDriverExistsAsync(driverTripsRequest.DriverId);

            var driverTripsPagedListSpecification = new DriverTripSpecs.PagedListByDriverId(driverTripsRequest.DriverId, pagedListRequest);
            var driverTripsPage = await _driverTripRepository.GetProjectedListBySpecAsync(driverTripsPagedListSpecification);
            var driverTripsTotalCount = await _driverTripRepository.CountBySpecAsync(driverTripsPagedListSpecification);

            return new GetPagedListResponseModel<GetDriverTripResponseModel>(
                Items: driverTripsPage,
                TotalCount: driverTripsTotalCount);
        }

        public async Task<GetDriverTripSummaryResponseModel> GetSummaryAsync(int driverId)
        {
            return await _driverRepository.GetProjectedItemBySpecAsync(new DriverSpecs.TripSummaryById(driverId))
                ?? throw NotFoundException.For<Driver>(driverId);
        }

        public async Task RecordFinishedTripAsync(Route finishedRoute, Auto routeAuto)
        {
            var isFinishedTrip = finishedRoute.ActualDepartureTimeUtc.HasValue
                && RouteStatusGroups.Final.Contains(finishedRoute.Status);
            if (!isFinishedTrip || await _driverTripRepository.AnyBySpecAsync(new DriverTripSpecs.ByRouteId(finishedRoute.Id)))
            {
                return;
            }

            var routeLocations = await _locationRepository.GetProjectedListBySpecAsync(
                new LocationSpecs.LookupOptionsByIds([finishedRoute.StartLocationId, finishedRoute.DestinationLocationId]));
            var startLocation = routeLocations.Single(location => location.Id == finishedRoute.StartLocationId);
            var destinationLocation = routeLocations.Single(location => location.Id == finishedRoute.DestinationLocationId);
            var tripStartedAtUtc = finishedRoute.ActualDepartureTimeUtc!.Value;
            var tripEndedAtUtc = finishedRoute.Status == RouteStatus.Completed
                ? finishedRoute.ActualArrivalTimeUtc ?? finishedRoute.ArrivalTimeUtc
                : DateTime.UtcNow;

            await _driverTripRepository.InsertAsync(new DriverTrip
            {
                DriverId = finishedRoute.DriverId,
                RouteId = finishedRoute.Id,
                AutoId = routeAuto.Id,
                AutoMark = routeAuto.Mark,
                AutoModel = routeAuto.Model,
                AutoNumber = routeAuto.Number,
                StartLocationCity = startLocation.City,
                StartLocationCountry = startLocation.Country,
                StartLocationTimeZoneId = startLocation.TimeZoneId,
                DestinationLocationCity = destinationLocation.City,
                DestinationLocationCountry = destinationLocation.Country,
                DestinationLocationTimeZoneId = destinationLocation.TimeZoneId,
                StartAddressLine = finishedRoute.StartAddressLine,
                DestinationAddressLine = finishedRoute.DestinationAddressLine,
                PlannedDepartureTimeUtc = finishedRoute.DepartureTimeUtc,
                PlannedArrivalTimeUtc = finishedRoute.ArrivalTimeUtc,
                StartedAtUtc = tripStartedAtUtc,
                EndedAtUtc = tripEndedAtUtc,
                DrivingMinutes = Math.Max(0, (int)Math.Round((tripEndedAtUtc - tripStartedAtUtc).TotalMinutes)),
                FinalStatus = finishedRoute.Status,
                DepotId = finishedRoute.DepotId
            });
        }

        private async Task EnsureDriverExistsAsync(int driverId)
        {
            if (!await _driverRepository.AnyBySpecAsync(new DriverSpecs.ById(driverId)))
            {
                throw NotFoundException.For<Driver>(driverId);
            }
        }
    }
}
