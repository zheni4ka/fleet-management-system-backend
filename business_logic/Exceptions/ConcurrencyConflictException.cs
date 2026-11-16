namespace business_logic.Exceptions
{
    public class ConcurrencyConflictException : Exception
    {
        public ConcurrencyConflictException(string message) : base(message)
        {
        }
    }
}
