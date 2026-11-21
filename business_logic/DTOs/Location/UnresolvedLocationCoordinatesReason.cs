namespace business_logic.DTOs
{
    public enum UnresolvedLocationCoordinatesReason
    {
        NoCandidateInCountry,
        GeoNamesIdUsedByAnotherLocation,
        TimeZoneUnsupported
    }
}
