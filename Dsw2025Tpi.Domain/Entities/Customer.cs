using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Domain.Entities
{
    public class Customer : EntityBase
    {
        public Customer() { }
        public required string Email { get; set; }
        public required string Name { get; set; }
        public required string PhoneNumber { get; set; }

        //relacion 1 a * con Order
        public ICollection<Order> Orders { get; set; } = new HashSet<Order>();
    }
}
