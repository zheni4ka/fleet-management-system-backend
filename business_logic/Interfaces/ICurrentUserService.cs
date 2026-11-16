namespace business_logic.Interfaces
{
    public interface ICurrentUserService
    {
        string? UserId { get; }
        bool IsDepotScopeUnrestricted { get; }
        IReadOnlyCollection<int> AccessibleDepotIds { get; }
    }
}
