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
            services.AddAccountIdentity();
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

        public static void AddAccountIdentity(this IServiceCollection services)
        {
            services.AddIdentityCore<IdentityUser>(identityOptions =>
            {
                identityOptions.Password.RequiredLength = AccountConstraints.PasswordMinLength;
                identityOptions.Password.RequireDigit = true;
                identityOptions.Password.RequireUppercase = true;
                identityOptions.Password.RequireLowercase = true;
                identityOptions.Password.RequireNonAlphanumeric = false;
                identityOptions.Lockout.AllowedForNewUsers = true;
                identityOptions.Lockout.MaxFailedAccessAttempts = AccountConstraints.LockoutMaxFailedAccessAttempts;
                identityOptions.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(AccountConstraints.LockoutDurationInMinutes);
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<FMS_DbContext>()
            .AddSignInManager();
        }
    }
}
