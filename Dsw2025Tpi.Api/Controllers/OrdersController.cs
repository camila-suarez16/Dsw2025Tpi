using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Services;
using Dsw2025Tpi.Application.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2025Tpi.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly OrderService _orderService;

    public OrdersController(OrderService orderService)
    {
        _orderService = orderService;
    }

    // 6. Crear una orden
    [HttpPost]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> CreateOrder([FromBody] OrderModel.OrderRequest request)
    {
        try
        {
            var result = await _orderService.CreateAsync(request);
            return CreatedAtAction(nameof(GetOrderById), new { id = result.Id }, result);
        }
        catch (InvalidEntityException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InsufficientStockException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return StatusCode(500, "Ocurrió un error inesperado al crear la orden.");
        }
    }

    // 7. Obtener todas las órdenes (con filtros opcionales)
    [HttpGet]
    [Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> GetOrders(
        [FromQuery] string? status,
        [FromQuery] Guid? customerId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var result = await _orderService.GetAllAsync(status, customerId, pageNumber, pageSize);
            if (!result.Any()) return NoContent();
            return Ok(result);
        }
        catch (Exception)
        {
            return StatusCode(500, "Ocurrió un error al obtener las órdenes.");
        }
    }

    // 8. Obtener orden por ID
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> GetOrderById(Guid id)
    {
        try
        {
            var order = await _orderService.GetByIdAsync(id);
            return Ok(order);
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception)
        {
            return StatusCode(500, "Ocurrió un error al obtener la orden.");
        }
    }

    // 9. Actualizar estado de una orden
    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] OrderStatusUpdateModel request)
    {
        try
        {
            var result = await _orderService.UpdateStatusAsync(id, request.NewStatus);
            return Ok(result);
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidEntityException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return StatusCode(500, "Ocurrió un error al actualizar el estado.");
        }
    }
}
