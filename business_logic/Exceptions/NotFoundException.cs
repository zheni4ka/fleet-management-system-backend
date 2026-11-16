using business_logic.Constants;
using business_logic.Helpers;

namespace business_logic.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message)
        {
        }

        public static NotFoundException For<TEntity>(object id) => new(string.Format(
            ExceptionMessages.EntityNotFoundTemplate,
            EntityDisplayNameResolver.GetEntityDisplayName(typeof(TEntity)),
            id));
    }
}
