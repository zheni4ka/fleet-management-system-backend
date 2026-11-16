namespace business_logic.Validators.Auto
{
    internal static class AutoNumberFormat
    {
        public const string Pattern = @"^[А-ЩЬЮЯҐЄІЇ][А-ЩЬЮЯҐЄІЇ][0-9]{4}[А-ЩЬЮЯҐЄІЇ][А-ЩЬЮЯҐЄІЇ]$";

        public const string InvalidFormatMessage = "Номерний знак має формат АА1234АА (великі українські літери та 4 цифри).";
    }
}
