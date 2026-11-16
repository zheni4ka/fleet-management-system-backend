namespace business_logic.Entities
{
    public enum RefreshTokenRevocationReason
    {
        Rotated,
        Logout,
        ReuseDetected,
        PasswordChanged,
        PasswordReset,
        UserDeactivated
    }
}
