using System.Globalization;

namespace business_logic.Specifications
{
    public static class SearchQueryIdParser
    {
        public static int? ParseSearchedId(string trimmedSearch)
        {
            return int.TryParse(trimmedSearch, NumberStyles.None, CultureInfo.InvariantCulture, out var searchedId)
                ? searchedId
                : null;
        }
    }
}
