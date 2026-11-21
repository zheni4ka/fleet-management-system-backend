using business_logic.DTOs;
using business_logic.Entities;
using business_logic.Helpers;
using business_logic.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Globalization;

namespace data_access.Interceptors
{
    public sealed class AuditLogChangesInterceptor : SaveChangesInterceptor
    {
        private static readonly IReadOnlyDictionary<AuditAction, Type> AuditedEntityTypesByAction = new Dictionary<AuditAction, Type>
        {
            [AuditAction.RouteUpdated] = typeof(Route),
            [AuditAction.AutoUpdated] = typeof(Auto),
            [AuditAction.AutoStatusChanged] = typeof(Auto),
            [AuditAction.AutoArchived] = typeof(Auto),
            [AuditAction.AutoRestored] = typeof(Auto),
            [AuditAction.DriverUpdated] = typeof(Driver),
            [AuditAction.DriverArchived] = typeof(Driver),
            [AuditAction.DriverRestored] = typeof(Driver),
            [AuditAction.LocationArchived] = typeof(Location),
            [AuditAction.LocationRestored] = typeof(Location),
            [AuditAction.LocationCoordinatesFilled] = typeof(Location),
            [AuditAction.MaintenanceUpdated] = typeof(AutoMaintenance),
            [AuditAction.DepotUpdated] = typeof(Depot),
            [AuditAction.UserDeactivated] = typeof(IdentityUser),
            [AuditAction.UserReactivated] = typeof(IdentityUser)
        };

        private static readonly IReadOnlySet<string> ExcludedPropertyNames = new HashSet<string>
        {
            nameof(IHasRowVersion.RowVersion),
            nameof(IdentityUser.ConcurrencyStamp),
            nameof(IdentityUser.SecurityStamp),
            nameof(IdentityUser.PasswordHash)
        };

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            AttachChangesToAddedAuditLogs(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            AttachChangesToAddedAuditLogs(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private static void AttachChangesToAddedAuditLogs(DbContext? dbContext)
        {
            if (dbContext is null)
            {
                return;
            }

            var addedAuditLogs = dbContext.ChangeTracker.Entries<AuditLog>()
                .Where(auditLogEntry => auditLogEntry.State == EntityState.Added
                    && auditLogEntry.Entity.EntityId != null
                    && auditLogEntry.Entity.ChangesJson == null)
                .Select(auditLogEntry => auditLogEntry.Entity)
                .ToList();

            foreach (var addedAuditLog in addedAuditLogs)
            {
                if (!AuditedEntityTypesByAction.TryGetValue(addedAuditLog.Action, out var auditedEntityType))
                {
                    continue;
                }

                var modifiedAuditedEntry = dbContext.ChangeTracker.Entries()
                    .FirstOrDefault(entityEntry => entityEntry.State == EntityState.Modified
                        && entityEntry.Metadata.ClrType == auditedEntityType
                        && GetPrimaryKeyText(entityEntry) == addedAuditLog.EntityId);
                if (modifiedAuditedEntry is null)
                {
                    continue;
                }

                var auditLogPropertyChanges = modifiedAuditedEntry.Properties
                    .Where(propertyEntry => propertyEntry.IsModified
                        && !ExcludedPropertyNames.Contains(propertyEntry.Metadata.Name)
                        && !Equals(propertyEntry.OriginalValue, propertyEntry.CurrentValue))
                    .Select(propertyEntry => new AuditLogPropertyChangeModel(
                        PropertyName: propertyEntry.Metadata.Name,
                        PreviousValue: FormatAuditedValue(propertyEntry.OriginalValue),
                        CurrentValue: FormatAuditedValue(propertyEntry.CurrentValue)))
                    .ToList();
                if (auditLogPropertyChanges.Count > 0)
                {
                    addedAuditLog.ChangesJson = AuditLogChangesSerializer.SerializeAuditLogChanges(auditLogPropertyChanges);
                }
            }
        }

        private static string GetPrimaryKeyText(EntityEntry entityEntry)
        {
            return string.Join(",", entityEntry.Metadata.FindPrimaryKey()!.Properties
                .Select(keyProperty => FormatAuditedValue(entityEntry.Property(keyProperty.Name).CurrentValue)));
        }

        private static string? FormatAuditedValue(object? auditedValue)
        {
            return auditedValue switch
            {
                null => null,
                DateTime auditedDateTime => auditedDateTime.ToString("O", CultureInfo.InvariantCulture),
                DateTimeOffset auditedDateTimeOffset => auditedDateTimeOffset.ToString("O", CultureInfo.InvariantCulture),
                IFormattable auditedFormattableValue => auditedFormattableValue.ToString(null, CultureInfo.InvariantCulture),
                _ => auditedValue.ToString()
            };
        }
    }
}
