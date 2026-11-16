using business_logic.Exceptions;
using Microsoft.AspNetCore.Identity;

namespace business_logic.Helpers
{
    public static class IdentityOperationResultGuard
    {
        public static void EnsureIdentityOperationSucceeded(IdentityResult identityOperationResult, Func<string, string> createFailureMessage)
        {
            if (identityOperationResult.Succeeded)
            {
                return;
            }

            var identityErrorDescriptions = string.Join("; ", identityOperationResult.Errors.Select(identityError => identityError.Description));
            throw new BusinessRuleException(createFailureMessage(identityErrorDescriptions));
        }
    }
}
