using business_logic.Constants;
using business_logic.Interfaces;
using System.Globalization;
using System.Security.Claims;

namespace transport_logistic_management_2026.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? UserId => _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

        public bool IsDepotScopeUnrestricted
        {
            get
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext is null)
                {
                    return true;
                }

                return httpContext.User.IsInRole(UserRoles.Admin);
            }
        }

        public IReadOnlyCollection<int> AccessibleDepotIds
        {
            get
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext is null)
                {
                    return [];
                }

                return httpContext.User.FindAll(DepotClaimTypes.DepotId)
                    .Select(depotIdClaim => int.Parse(depotIdClaim.Value, CultureInfo.InvariantCulture))
                    .ToList();
            }
        }
    }
}
