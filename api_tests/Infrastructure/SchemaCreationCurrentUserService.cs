using business_logic.Interfaces;

namespace api_tests.Infrastructure
{
    public sealed class SchemaCreationCurrentUserService : ICurrentUserService
    {
        public string? UserId => null;
        public bool IsDepotScopeUnrestricted => true;
        public IReadOnlyCollection<int> AccessibleDepotIds => [];
    }
}
