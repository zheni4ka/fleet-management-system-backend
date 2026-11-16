using business_logic.DTOs;
using System.Text.Json;

namespace business_logic.Helpers
{
    public static class AuditLogChangesSerializer
    {
        private static readonly JsonSerializerOptions AuditLogChangesJsonSerializerOptions = new(JsonSerializerDefaults.Web);

        public static string SerializeAuditLogChanges(IReadOnlyList<AuditLogPropertyChangeModel> auditLogPropertyChanges)
        {
            return JsonSerializer.Serialize(auditLogPropertyChanges, AuditLogChangesJsonSerializerOptions);
        }

        public static IReadOnlyList<AuditLogPropertyChangeModel> DeserializeAuditLogChanges(string? auditLogChangesJson)
        {
            if (auditLogChangesJson is null)
            {
                return [];
            }

            return JsonSerializer.Deserialize<List<AuditLogPropertyChangeModel>>(auditLogChangesJson, AuditLogChangesJsonSerializerOptions)
                ?? throw new InvalidOperationException($"Audit log changes JSON '{auditLogChangesJson}' is not a list of property changes.");
        }
    }
}
