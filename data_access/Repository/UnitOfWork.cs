using business_logic.Constants;
using business_logic.Exceptions;
using business_logic.Helpers;
using business_logic.Interfaces;
using data_access.data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace data_access.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private const int SqlServerDuplicateKeyInUniqueIndexErrorNumber = 2601;
        private const int SqlServerUniqueConstraintViolationErrorNumber = 2627;

        private static readonly Regex SqlServerDuplicateKeyMessagePattern = new(
            @"'(?<indexName>[^']+)'\. The duplicate key value is \((?<duplicateKeyValue>.*)\)",
            RegexOptions.Compiled);

        private static readonly IReadOnlyDictionary<string, string> UniqueIndexViolationMessageTemplates = new Dictionary<string, string>
        {
            [UniqueIndexNames.AutoNumber] = ExceptionMessages.AutoNumberAlreadyExistsTemplate,
            [UniqueIndexNames.DriverLicenseNumber] = ExceptionMessages.DriverLicenseNumberAlreadyExistsTemplate,
            [UniqueIndexNames.DepotName] = ExceptionMessages.DepotNameAlreadyExistsTemplate
        };

        private readonly FMS_DbContext _context;

        public UnitOfWork(FMS_DbContext context)
        {
            _context = context;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException concurrencyException)
            {
                var conflictingEntry = concurrencyException.Entries[0];
                var conflictingEntityKeyValues = conflictingEntry.Metadata.FindPrimaryKey()!.Properties
                    .Select(keyProperty => conflictingEntry.Property(keyProperty.Name).CurrentValue);
                throw new ConcurrencyConflictException(string.Format(
                    ExceptionMessages.ConcurrencyConflictTemplate,
                    EntityDisplayNameResolver.GetEntityDisplayName(conflictingEntry.Metadata.ClrType),
                    string.Join(", ", conflictingEntityKeyValues)));
            }
            catch (DbUpdateException updateException) when (TryGetUniqueIndexViolationMessage(updateException, out var uniqueIndexViolationMessage))
            {
                throw new UniqueConstraintViolationException(uniqueIndexViolationMessage, updateException);
            }
        }

        public async Task<IUnitOfWorkTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            var databaseTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            return new UnitOfWorkTransaction(databaseTransaction);
        }

        private static bool TryGetUniqueIndexViolationMessage(DbUpdateException updateException, out string uniqueIndexViolationMessage)
        {
            uniqueIndexViolationMessage = string.Empty;
            if (updateException.InnerException is not SqlException
                {
                    Number: SqlServerDuplicateKeyInUniqueIndexErrorNumber or SqlServerUniqueConstraintViolationErrorNumber
                } uniqueViolationSqlException)
            {
                return false;
            }

            var duplicateKeyMessageMatch = SqlServerDuplicateKeyMessagePattern.Match(uniqueViolationSqlException.Message);
            if (!duplicateKeyMessageMatch.Success
                || !UniqueIndexViolationMessageTemplates.TryGetValue(duplicateKeyMessageMatch.Groups["indexName"].Value, out var uniqueIndexViolationMessageTemplate))
            {
                return false;
            }

            uniqueIndexViolationMessage = string.Format(uniqueIndexViolationMessageTemplate, duplicateKeyMessageMatch.Groups["duplicateKeyValue"].Value);
            return true;
        }
    }
}
