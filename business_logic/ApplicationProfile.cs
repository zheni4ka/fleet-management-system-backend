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
            CreateMap<UpdateDriverRequestModel, Driver>();

            CreateMap<Auto, GetAutoResponseModel>();
            CreateMap<CreateAutoRequestModel, Auto>();
            CreateMap<UpdateAutoRequestModel, Auto>();

            CreateMap<CreateRouteRequestModel, Route>();
            CreateMap<UpdateRouteRequestModel, Route>();

            CreateMap<Location, GetLocationResponseModel>();
            CreateMap<CreateLocationRequestModel, Location>();

            CreateMap<AutoMaintenance, GetAutoMaintenanceResponseModel>();
            CreateMap<CreateAutoMaintenanceRequestModel, AutoMaintenance>();
            CreateMap<UpdateAutoMaintenanceRequestModel, AutoMaintenance>();

            CreateMap<IdentityUser, GetDispatcherResponseModel>()
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email));
        }
    }
}
