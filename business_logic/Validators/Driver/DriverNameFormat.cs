namespace business_logic.Validators
{
    internal static class DriverNameFormat
    {
        public const string Pattern = @"^[А-ЩЬЮЯҐЄІЇ][а-щьюяґєії'’ʼ]*(-[А-ЩЬЮЯҐЄІЇ][а-щьюяґєії'’ʼ]*)?$";

        public const string InvalidFormatMessage = "Ім'я, прізвище та по батькові мають починатися з великої української літери й містити лише українські літери, апостроф або дефіс.";
    }
}
