using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OS_40F_PostNL.Components
{
    public class CreateShipmentModel
    {
        public Customer Customer { get; set; } = new Customer();
        public Message Message { get; set; } = new Message();
        public List<Shipment> Shipments { get; set; } = new List<Shipment>();
    }

    public class Customer
    {
        public Address Address { get; set; } = new Address();
        public string CollectionLocation { get; set; }
        public string ContactPerson { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerNumber { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
    }
    public class Address
    {
        public string AddressType { get; set; }
        public string City { get; set; }
        public string CompanyName { get; set; }
        public string Countrycode { get; set; }
        public string HouseNr { get; set; }
        public string HouseNrExt { get; set; }
        public string Street { get; set; }
        public string FirstName { get; set; }
        public string Zipcode { get; set; }
        public string Name { get; set; }
    }

    public class Message
    {
        public string MessageID { get; } = Guid.NewGuid().ToString();
        public DateTime MessageTimeStamp { get; } = DateTime.Now;
        public string PrinterType => "GraphicFile|PDF";
    }

    public class Shipment
    {
        public List<Address> Addresses { get; set; } = new List<Address>();
        public List<Contact> Contacts { get; set; } = new List<Contact>();
        /// <summary>
        /// 3085 - standard shipment
        /// </summary>
        public string ProductCodeDelivery => "3085";
    }

    public class Contact
    {
        public string ContactType { get; set; }
        public string Email { get; set; }
        public string SmsNr { get; set; }
    }
}
