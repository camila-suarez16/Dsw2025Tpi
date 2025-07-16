using Dsw2025Tpi.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Domain.Entities
{
    public class Order : EntityBase
    {
        public Order() { }
        public DateTime Date { get; set; }
        public required string ShippingAddress { get; set; }
        public required string BillingAddress { get; set; }
        public string? Notes { get; set; }
        public decimal TotalAmount { get; set; }
        //relacion muchos a 1 con Customer
        public Guid CustomerId { get; set; }
        public Customer? Customer { get; set; }

        //relacion con el enum
        public OrderStatus Status { get; set; } = OrderStatus.PENDING;
        
        //relacion 1 a 1..* con OrderItem
        public ICollection<OrderItem> Items { get; set; } = new HashSet<OrderItem>();

    }
}
