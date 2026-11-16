namespace business_logic.Interfaces
{
    public interface IHasRowVersion
    {
        byte[] RowVersion { get; set; }
    }
}
