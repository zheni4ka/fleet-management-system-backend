using AutoMapper;
using business_logic.DTOs;
using business_logic.Entities;
using Microsoft.AspNetCore.Identity;

namespace business_logic
{
    public class ApplicationProfile : Profile
    {
        public ApplicationProfile()
        {
            CreateMap<DriverRequestModelBase, Driver>()
                .ForMember(driver => driver.PhoneNumber, mappingOptions => mappingOptions.MapFrom(model => NormalizeOptionalText(model.PhoneNumber)))
                .ForMember(driver => driver.Email, mappingOptions => mappingOptions.MapFrom(model => NormalizeOptionalText(model.Email)))
                .IncludeAllDerived();
            CreateMap<CreateDriverRequestModel, Driver>();
            CreateMap<UpdateDriverRequestModel, Driver>()
                .ForMember(driver => driver.RowVersion, mappingOptions => mappingOptions.Ignore());

            CreateMap<CreateAutoRequestModel, Auto>();
            CreateMap<UpdateAutoRequestModel, Auto>()
                .ForMember(auto => auto.RowVersion, mappingOptions => mappingOptions.Ignore());

            CreateMap<CreateRouteRequestModel, Route>();
            CreateMap<UpdateRouteRequestModel, Route>()
                .ForMember(route => route.RowVersion, mappingOptions => mappingOptions.Ignore())
                .ForMember(route => route.DepartureTimeUtc, mappingOptions => mappingOptions.Ignore())
                .ForMember(route => route.ArrivalTimeUtc, mappingOptions => mappingOptions.Ignore());

            CreateMap<Location, GetLocationResponseModel>();

            CreateMap<AutoMaintenance, GetAutoMaintenanceResponseModel>();
            CreateMap<CreateAutoMaintenanceRequestModel, AutoMaintenance>();
            CreateMap<UpdateAutoMaintenanceRequestModel, AutoMaintenance>()
                .ForMember(autoMaintenance => autoMaintenance.RowVersion, mappingOptions => mappingOptions.Ignore());

            CreateMap<IdentityUser, GetDispatcherResponseModel>()
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email));
        }

        private static string? NormalizeOptionalText(string? optionalText)
        {
            return string.IsNullOrWhiteSpace(optionalText) ? null : optionalText.Trim();
        }
    }
}
