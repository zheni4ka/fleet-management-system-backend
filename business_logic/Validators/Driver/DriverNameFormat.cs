namespace business_logic.Validators
{
    internal static class DriverNameFormat
    {
        public const string Pattern = @"^[А-ЩЬЮЯҐЄІЇ][а-щьюяґєії'’ʼ]*(-[А-ЩЬЮЯҐЄІЇ][а-щьюяґєії'’ʼ]*)?$";
    }
}
