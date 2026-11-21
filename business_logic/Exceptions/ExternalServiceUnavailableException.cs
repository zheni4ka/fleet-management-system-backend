namespace business_logic.Exceptions
{
    public class ExternalServiceUnavailableException : Exception
    {
        public ExternalServiceUnavailableException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
