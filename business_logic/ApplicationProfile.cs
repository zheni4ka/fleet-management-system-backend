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
            CreateMap<Driver, GetDriverResponseModel>();
            CreateMap<CreateDriverRequestModel, Driver>();
            CreateMap<UpdateDriverRequestModel, Driver>()
                .ForMember(driver => driver.RowVersion, mappingOptions => mappingOptions.Ignore());

            CreateMap<Auto, GetAutoResponseModel>();
            CreateMap<CreateAutoRequestModel, Auto>();
            CreateMap<UpdateAutoRequestModel, Auto>()
                .ForMember(auto => auto.RowVersion, mappingOptions => mappingOptions.Ignore());

            CreateMap<CreateRouteRequestModel, Route>();
            CreateMap<UpdateRouteRequestModel, Route>()
                .ForMember(route => route.RowVersion, mappingOptions => mappingOptions.Ignore());

            CreateMap<Location, GetLocationResponseModel>();
            CreateMap<CreateLocationRequestModel, Location>();

            CreateMap<AutoMaintenance, GetAutoMaintenanceResponseModel>();
            CreateMap<CreateAutoMaintenanceRequestModel, AutoMaintenance>();
            CreateMap<UpdateAutoMaintenanceRequestModel, AutoMaintenance>()
                .ForMember(autoMaintenance => autoMaintenance.RowVersion, mappingOptions => mappingOptions.Ignore());

            CreateMap<IdentityUser, GetDispatcherResponseModel>()
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email));
        }
    }
}
