namespace business_logic.DTOs
{
    public sealed record GetPagedListResponseModel<TItem>(
        IReadOnlyList<TItem> Items,
        int TotalCount);
}
