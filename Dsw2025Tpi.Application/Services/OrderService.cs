using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Enums;
using Dsw2025Tpi.Domain.Interfaces;
using Dsw2025Tpi.Application.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Services;

    public class OrderService
    {
        private readonly IRepository _repository;
        public OrderService(IRepository repository)
        {
            _repository = repository;
        }

    //6.Crear una orden 
    public async Task<OrderModel.OrderResponse> CreateAsync(OrderModel.OrderRequest request)
    {
        
        if (string.IsNullOrWhiteSpace(request.ShippingAddress))
            throw new InvalidEntityException("La dirección de envío es obligatoria.");

        if (string.IsNullOrWhiteSpace(request.BillingAddress))
            throw new InvalidEntityException("La dirección de facturación es obligatoria.");

        if (request.OrderItems is null || !request.OrderItems.Any())
            throw new InvalidEntityException("La orden debe contener al menos un producto.");

        
        var customer = await _repository.GetById<Customer>(request.CustomerId)
            ?? throw new EntityNotFoundException($"Cliente con ID {request.CustomerId} no encontrado.");

        
        var productIds = request.OrderItems.Select(i => i.ProductId).Distinct().ToList();
        var products = await _repository.GetFiltered<Product>(p => productIds.Contains(p.Id) && p.IsActive)
            ?? throw new EntityNotFoundException("Uno o más productos no existen o están inactivos.");

        var orderItems = new List<OrderItem>();

        foreach (var item in request.OrderItems)
        {
            var product = products.FirstOrDefault(p => p.Id == item.ProductId)
                ?? throw new EntityNotFoundException($"Producto con ID {item.ProductId} no encontrado.");

            if (item.Quantity <= 0)
                throw new InvalidEntityException("La cantidad debe ser mayor a 0.");

            if (item.Quantity > product.StockQuantity)
                throw new InsufficientStockException($"Stock insuficiente para '{product.Name}'. Disponibles: {product.StockQuantity}, solicitados: {item.Quantity}.");

            
            var orderItem = new OrderItem
            {
                ProductId = product.Id,
                Quantity = item.Quantity,
                UnitPrice = product.CurrentUnitPrice,
                SubTotal = product.CurrentUnitPrice * item.Quantity
            };

            orderItems.Add(orderItem);

            
            product.StockQuantity -= item.Quantity;
            await _repository.Update(product);
        }

        
        var total = orderItems.Sum(i => i.SubTotal);

        
        var order = new Order
        {
            CustomerId = customer.Id,
            ShippingAddress = request.ShippingAddress.Trim(),
            BillingAddress = request.BillingAddress.Trim(),
            Status = OrderStatus.PENDING,
            Date = DateTime.UtcNow,
            Items = orderItems,
            TotalAmount = total
        };

        await _repository.Add(order);
        await _repository.SaveChangesAsync();

        
        var responseItems = orderItems.Select(oi =>
        {
            var productName = products.First(p => p.Id == oi.ProductId).Name;

            return new OrderModel.OrderItemResponse(
                oi.ProductId,
                oi.Quantity,
                oi.UnitPrice,
                oi.SubTotal,
                productName
            );
        }).ToList();

        return new OrderModel.OrderResponse(
            order.Id,
            order.CustomerId,
            order.ShippingAddress,
            order.BillingAddress,
            order.TotalAmount,
            responseItems,
            order.Status.ToString()
        );
    }


    // 7.Obtener todas las órdenes
    public async Task<IEnumerable<OrderModel.OrderResponse>> GetAllAsync(
        string? status = null,
        Guid? customerId = null,
        int pageNumber = 1,
        int pageSize = 10)
    {
        var query = await _repository.GetAll<Order>("Items");

        if (query is null)
            return Enumerable.Empty<OrderModel.OrderResponse>();

        var filtered = query.AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<OrderStatus>(status, true, out var parsedStatus))
            filtered = filtered.Where(o => o.Status == parsedStatus);

        if (customerId.HasValue)
            filtered = filtered.Where(o => o.CustomerId == customerId.Value);

        var pagedOrders = filtered
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

     
        var productIds = pagedOrders
        .SelectMany(o => o.Items.Select(i => i.ProductId)).Distinct().ToList();

        var products = await _repository.GetFiltered<Product>(p => productIds.Contains(p.Id));

        return pagedOrders.Select(o => new OrderModel.OrderResponse(
        o.Id,
        o.CustomerId,
        o.ShippingAddress,
        o.BillingAddress,
        o.TotalAmount,
        o.Items.Select(i =>
       {
        var productName = products?.FirstOrDefault(p => p.Id == i.ProductId)?.Name ?? "";
        return new OrderModel.OrderItemResponse(
            i.ProductId,
            i.Quantity,
            i.UnitPrice,
            i.SubTotal,
            productName
        );
      }).ToList(),
     o.Status.ToString()
   ));
  }


    // 8.Obtener una orden por ID
    public async Task<OrderModel.OrderResponse> GetByIdAsync(Guid id)
    {
        var order = await _repository.GetById<Order>(id, "Items")
        ?? throw new EntityNotFoundException($"Orden con ID {id} no encontrada.");

        var productIds = order.Items.Select(i => i.ProductId).Distinct().ToList();

        var products = await _repository.GetFiltered<Product>(p => productIds.Contains(p.Id));

        return new OrderModel.OrderResponse(
            order.Id,
            order.CustomerId,
            order.ShippingAddress,
            order.BillingAddress,
            order.TotalAmount,
            order.Items.Select(i =>
            {
                var productName = products?.FirstOrDefault(p => p.Id == i.ProductId)?.Name ?? "";
                return new OrderModel.OrderItemResponse(
                    i.ProductId,
                    i.Quantity,
                    i.UnitPrice,
                    i.SubTotal,
                    productName
                );
            }).ToList(),
            order.Status.ToString()
        );
    }

    // 9.Actualizar el estado de una orden
    public async Task<OrderModel.OrderResponse> UpdateStatusAsync(Guid orderId, string newStatus)
    {
        var order = await _repository.GetById<Order>(orderId, "Items")
            ?? throw new EntityNotFoundException($"Orden con ID {orderId} no encontrada.");

        if (!Enum.TryParse<OrderStatus>(newStatus, true, out var parsedStatus))
            throw new InvalidEntityException($"Estado '{newStatus}' no válido.");


        bool IsValidTransition(OrderStatus current, OrderStatus next) => current switch
        {
            OrderStatus.PENDING => next == OrderStatus.PROCESSING || next == OrderStatus.CANCELLED,
            OrderStatus.PROCESSING => next == OrderStatus.SHIPPED || next == OrderStatus.CANCELLED,
            OrderStatus.SHIPPED => next == OrderStatus.DELIVERED,
            OrderStatus.DELIVERED => false, 
            OrderStatus.CANCELLED => false, 
            _ => false
        };

        if (!IsValidTransition(order.Status, parsedStatus))
        {
            throw new InvalidEntityException($"Transición inválida de estado '{order.Status}' a '{parsedStatus}'.");
        }

        order.Status = parsedStatus;
        await _repository.Update(order);

        return new OrderModel.OrderResponse(
            order.Id,
            order.CustomerId,
            order.ShippingAddress,
            order.BillingAddress,
            order.TotalAmount,
            order.Items.Select(i => new OrderModel.OrderItemResponse(
                i.ProductId,
                i.Quantity,
                i.UnitPrice,
                i.SubTotal,
                ""
            )).ToList(),
            order.Status.ToString()
        );
    }

}

