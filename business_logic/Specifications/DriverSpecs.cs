using Ardalis.Specification;
using business_logic.Entities;

namespace business_logic.Specifications
{
    public class DriverSpecs
    {
        public class ById : Specification<Driver>
        {
            public ById(int id)
            {
                Query.Where(d => d.Id == id);
            }
        }

        public class ByName : Specification<Driver>
        {
            public ByName(string name)
            {
                Query.Where(d => d.Name == name);
            }
        }

        public class ByLicenseNumber : Specification<Driver>
        {
            public ByLicenseNumber(string licenseNumber)
            {
                Query.Where(d => d.LicenseNumber == licenseNumber);
            }
        }

        public class All : Specification<Driver>
        {
            public All()
            {
                Query.OrderBy(d => d.Id);
            }
        }
    }
}
