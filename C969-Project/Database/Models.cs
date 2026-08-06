using System;
using System.Collections.Generic;
using System.Text;

namespace C969_Project.Database
{
    public abstract class AuditableModel
    {
        public DateTime CreateDate { get; set; }

        public string CreatedBy { get; set; } = string.Empty;

        public DateTime UpdatedDate { get; set; }

        public string LastUpdatedBy { get; set; } = string.Empty;
    }

    public class Customer : AuditableModel
    {
        public int CustomerId { get; set; }

        public string CustomerName { get; set; } = string.Empty;

        public int AddressId { get; set; }

        public bool Active { get; set; }
    }

    //A User authenticates into the application. A User is never a Customer -
    //Customer is a managed record that never logs in - so the two models stay
    //separate even though both are audited rows.
    public class User : AuditableModel
    {
        public int UserId { get; set; }

        public string UserName { get; set; } = string.Empty;

        public bool Active { get; set; }
    }

    public class Country : AuditableModel
    {
        public int CountryId { get; set; }

        public string CountryName { get; set; } = String.Empty;
    }

    public class City : AuditableModel
    {
        public int CityId { get; set; }

        public string CityName { get; set; } = string.Empty;

        public int CountryId { get; set; }
    }
}
