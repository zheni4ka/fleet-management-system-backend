using Ardalis.Specification;
using business_logic.DTOs;
using business_logic.Entities;

namespace business_logic.Specifications
{
    public class DepotSpecs
    {
        public class ById : Specification<Depot>
        {
            public ById(int id)
            {
                Query.Where(depot => depot.Id == id);
            }
        }

        public class ByName : Specification<Depot>
        {
            public ByName(string name, int? excludedDepotId = null)
            {
                Query.Where(depot => depot.Name == name);

                if (excludedDepotId.HasValue)
                {
                    var excludedDepotIdValue = excludedDepotId.Value;
                    Query.Where(depot => depot.Id != excludedDepotIdValue);
                }
            }
        }

        public class ExistingIdsByIds : Specification<Depot, int>
        {
            public ExistingIdsByIds(IReadOnlyCollection<int> depotIds)
            {
                Query.Where(depot => depotIds.Contains(depot.Id));
                Query.Select(depot => depot.Id);
            }
        }

        public class ResponseModels : Specification<Depot, GetDepotResponseModel>
        {
            public ResponseModels()
            {
                Query.OrderBy(depot => depot.Name);
                Query.Select(depot => new GetDepotResponseModel
                {
                    Id = depot.Id,
                    Name = depot.Name
                });
            }
        }
    }
}
