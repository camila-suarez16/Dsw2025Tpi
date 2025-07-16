using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Dtos
{
    public record OrderModel
    {
        public record OrderRequest (Guid CustomerId, string ShippingAddress, string BillingAddress,
            List<OrderItemRequest> OrderItems);

        public record OrderResponse (Guid Id, Guid CustomerId, string ShippingAddress, string BillingAddress,decimal TotalAmount,
            List<OrderItemResponse> OrderItems, string status);

        public record OrderItemRequest (Guid ProductoId, int Quantity, string Name, string? Description, decimal CurrentUnitPrice);
        public record OrderItemResponse (Guid ProductoId, int Quantity, decimal UnitPrice, decimal SubTotal, string Name);



    }
}
