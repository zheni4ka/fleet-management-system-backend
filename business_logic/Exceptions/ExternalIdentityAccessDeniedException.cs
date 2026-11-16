namespace business_logic.Exceptions
{
    public class ExternalIdentityAccessDeniedException : Exception
    {
        public ExternalIdentityAccessDeniedException(string message) : base(message)
        {
        }
    }
}
