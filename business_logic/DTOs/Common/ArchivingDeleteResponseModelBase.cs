namespace business_logic.DTOs
{
    public abstract record ArchivingDeleteResponseModelBase
    {
        public bool IsArchivedInsteadOfDeleted { get; init; }
    }
}
