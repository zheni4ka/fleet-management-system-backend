namespace business_logic.Interfaces
{
    public interface IArchivable
    {
        bool IsArchived { get; set; }
        DateTime? ArchivedAtUtc { get; set; }
    }
}
