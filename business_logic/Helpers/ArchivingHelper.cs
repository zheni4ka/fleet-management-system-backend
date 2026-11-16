using business_logic.Constants;
using business_logic.Exceptions;
using business_logic.Interfaces;

namespace business_logic.Helpers
{
    public static class ArchivingHelper
    {
        public static void ArchiveEntity(IArchivable archivableEntity)
        {
            archivableEntity.IsArchived = true;
            archivableEntity.ArchivedAtUtc = DateTime.UtcNow;
        }

        public static void RestoreArchivedEntity<TArchivableEntity>(TArchivableEntity archivableEntity, int entityId)
            where TArchivableEntity : IArchivable
        {
            if (!archivableEntity.IsArchived)
            {
                throw new BusinessRuleException(string.Format(
                    ExceptionMessages.EntityNotArchivedTemplate,
                    EntityDisplayNameResolver.GetEntityDisplayName(typeof(TArchivableEntity)),
                    entityId));
            }

            archivableEntity.IsArchived = false;
            archivableEntity.ArchivedAtUtc = null;
        }
    }
}
