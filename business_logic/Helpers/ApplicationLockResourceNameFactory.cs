using business_logic.Constants;
using System.Globalization;

namespace business_logic.Helpers
{
    public static class ApplicationLockResourceNameFactory
    {
        public static string CreateAutoAssignmentLockResourceName(int autoId)
        {
            return string.Format(CultureInfo.InvariantCulture, ApplicationLockResourceNames.AutoAssignmentTemplate, autoId);
        }

        public static string CreateDriverAssignmentLockResourceName(int driverId)
        {
            return string.Format(CultureInfo.InvariantCulture, ApplicationLockResourceNames.DriverAssignmentTemplate, driverId);
        }
    }
}
