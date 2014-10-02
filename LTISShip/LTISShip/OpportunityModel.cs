using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LTISShip
{
    public class OpportunityModel
    {
        public long OrderNumber { get; set; }
        public string Name { get; set; }
        public List<OpportuityProductModel> products { get; set; }
        public ContactModel Contact { get; set; }
    }

    public class OpportuityProductModel
    {
        public string Name { get; set; }
        public decimal Quantity { get; set; }
        public decimal Cost { get; set; }
        public decimal Price { get; set; }
        public decimal Discount { get; set; }
    }

    public class ContactModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Organization { get; set; }
        public string EmailAddress { get; set; }
        public string StreetAddress { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Phone { get; set; }
    }

}
