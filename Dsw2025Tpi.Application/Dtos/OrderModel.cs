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

        public record OrderItemRequest (Guid ProductId, int Quantity);
        public record OrderItemResponse (Guid ProductId, int Quantity, decimal UnitPrice, decimal SubTotal, string Name);



    }
}
