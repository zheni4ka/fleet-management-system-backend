namespace business_logic.DTOs
{
    public sealed record GetPagedListRequestModel<TSortField>(
        int Page,
        int PageSize,
        TSortField? Sort,
        SortDirection? SortDirection,
        string? Search) where TSortField : struct, Enum;
}
