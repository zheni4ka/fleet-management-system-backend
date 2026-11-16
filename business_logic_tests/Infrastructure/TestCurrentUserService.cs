using business_logic.Interfaces;

namespace business_logic_tests.Infrastructure
{
    public sealed class TestCurrentUserService : ICurrentUserService
    {
        private readonly TestCurrentUserScope _testCurrentUserScope;

        public TestCurrentUserService(TestCurrentUserScope testCurrentUserScope)
        {
            _testCurrentUserScope = testCurrentUserScope;
        }

        public string? UserId => _testCurrentUserScope.UserId;
        public bool IsDepotScopeUnrestricted => _testCurrentUserScope.IsDepotScopeUnrestricted;
        public IReadOnlyCollection<int> AccessibleDepotIds => _testCurrentUserScope.AccessibleDepotIds;
    }
}
