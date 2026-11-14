using Ardalis.Specification;
using business_logic.DTOs;
using business_logic.Entities;
using System.Linq.Expressions;

namespace business_logic.Specifications
{
    public class AuditLogSpecs
    {
        public class ById : Specification<AuditLog>
        {
            public ById(int id)
            {
                Query.Where(x => x.Id == id);
            }
        }

        public class All : Specification<AuditLog>
        {
            public All()
            {
                Query.Include(x => x.Dispatcher)
                    .OrderByDescending(x => x.Time);
            }
        }

        public class PagedList : Specification<AuditLog, AuditLogDTO>
        {
            public PagedList(
                GetAuditLogsBySearchFilterRequestModel searchFilter,
                GetPagedListRequestModel<AuditLogSortField> pagedListRequest)
            {
                if (searchFilter.Action.HasValue)
                {
                    var searchedAuditAction = searchFilter.Action.Value;
                    Query.Where(x => x.Action == searchedAuditAction);
                }

                if (searchFilter.TimeFromDate.HasValue)
                {
                    var timeFromDateTime = searchFilter.TimeFromDate.Value.ToDateTime(TimeOnly.MinValue);
                    Query.Where(x => x.Time >= timeFromDateTime);
                }

                if (searchFilter.TimeToDate.HasValue)
                {
                    var timeToExclusiveDateTime = searchFilter.TimeToDate.Value.AddDays(1).ToDateTime(TimeOnly.MinValue);
                    Query.Where(x => x.Time < timeToExclusiveDateTime);
                }

                if (!string.IsNullOrWhiteSpace(pagedListRequest.Search))
                {
                    var trimmedSearch = pagedListRequest.Search.Trim();
                    Query.Where(x => (x.EntityId != null && x.EntityId.Contains(trimmedSearch))
                        || (x.Dispatcher != null && x.Dispatcher.UserName != null && x.Dispatcher.UserName.Contains(trimmedSearch)));
                }

                var sortField = pagedListRequest.Sort ?? AuditLogSortField.Time;
                Expression<Func<AuditLog, object?>> sortKeySelector = sortField switch
                {
                    AuditLogSortField.Time => x => x.Time,
                    AuditLogSortField.Action => x => x.Action,
                    AuditLogSortField.DispatcherUsername => x => x.Dispatcher != null ? x.Dispatcher.UserName : null,
                    AuditLogSortField.EntityId => x => x.EntityId,
                    _ => throw new Exception($"Audit log sort field '{sortField}' is not supported.")
                };
                var defaultSortDirection = pagedListRequest.Sort.HasValue ? SortDirection.Ascending : SortDirection.Descending;

                Query.OrderBySortDirection(sortKeySelector, x => x.Id, pagedListRequest.SortDirection ?? defaultSortDirection);
                Query.ApplyPage(pagedListRequest.Page, pagedListRequest.PageSize);
                Query.Select(x => new AuditLogDTO
                {
                    Id = x.Id,
                    DispatcherId = x.DispatcherId,
                    DispatcherUsername = x.Dispatcher != null ? x.Dispatcher.UserName : null,
                    Time = x.Time,
                    Action = x.Action,
                    EntityId = x.EntityId
                });
            }
        }
    }
}
