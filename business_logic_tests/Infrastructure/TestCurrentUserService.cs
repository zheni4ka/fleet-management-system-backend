using business_logic.Interfaces;

namespace business_logic_tests.Infrastructure
{
    public sealed class TestCurrentUserService : ICurrentUserService
    {
        public string? UserId => null;
    }
}
