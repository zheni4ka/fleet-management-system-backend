namespace business_logic.Settings
{
    public class ExternalIdentitySettings
    {
        public const string SectionName = "ExternalIdentitySettings";

        public string Authority { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public Dictionary<string, string> GroupRoleMappings { get; set; } = [];
    }
}
