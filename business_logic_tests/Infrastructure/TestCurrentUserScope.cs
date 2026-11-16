namespace business_logic_tests.Infrastructure
{
    public sealed class TestCurrentUserScope
    {
        public string? UserId { get; set; }
        public bool IsDepotScopeUnrestricted { get; set; } = true;
        public IReadOnlyCollection<int> AccessibleDepotIds { get; set; } = [];
    }
}
