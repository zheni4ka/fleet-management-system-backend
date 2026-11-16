namespace business_logic.Exceptions
{
    public class PasswordChangeRequiredException : Exception
    {
        public PasswordChangeRequiredException(string message) : base(message)
        {
        }
    }
}
