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
            CreateMap<Driver, DriverDTO>();
            CreateMap<CreateDriverModel, Driver>();
            CreateMap<EditDriverModel, Driver>();

            CreateMap<Auto, AutoDTO>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
            CreateMap<CreateAutoModel, Auto>();
            CreateMap<EditAutoModel, Auto>();

            CreateMap<Route, RouteDTO>();
            CreateMap<CreateRouteModel, Route>();
            CreateMap<EditRouteModel, Route>();

            CreateMap<Location, LocationDTO>();
            CreateMap<CreateLocationModel, Location>();

            CreateMap<AutoMaintenance, AutoMaintenanceDTO>();
            CreateMap<CreateAutoMaintenanceModel, AutoMaintenance>();
            CreateMap<EditAutoMaintenanceModel, AutoMaintenance>();

            CreateMap<AuditLog, AuditLogDTO>();

            CreateMap<IdentityUser, UserDTO>()
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email));
        }
    }
}
