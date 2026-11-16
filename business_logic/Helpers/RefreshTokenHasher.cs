using System.Security.Cryptography;
using System.Text;

namespace business_logic.Helpers
{
    public static class RefreshTokenHasher
    {
        public static string GetRefreshTokenHash(string refreshToken)
        {
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));
        }
    }
}
