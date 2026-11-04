using business_logic.Constants;
using business_logic.Interfaces;
using data_access.data;
using data_access.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace data_access
{
    public static class ServiceExtensions
    {
        public static void AddDataAccessServices(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext(connectionString);
            services.AddRepositories();
            services.AddIdentity();
        }

        private static void AddDbContext(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<FMS_DbContext>(opts => opts.UseSqlServer(connectionString));
        }

        private static void AddRepositories(this IServiceCollection services)
        {
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
        }

        private static void AddIdentity(this IServiceCollection services)
        {
            services.AddIdentityCore<IdentityUser>(opts =>
            {
                opts.Password.RequireDigit = false;
                opts.Password.RequiredLength = AccountConstraints.PasswordMinLength;
                opts.Password.RequireNonAlphanumeric = false;
                opts.Password.RequireUppercase = false;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<FMS_DbContext>();
        }
    }
}
