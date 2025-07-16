using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2025Tpi.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly ProductService _productService;

    public ProductsController(ProductService productService)
    {
        _productService = productService;
    }

    // 1. Crear producto (solo Admin)
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] ProductModel.ProductRequest request)
    {
        try
        {
            var result = await _productService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (InvalidEntityException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (DuplicatedEntityException ex)
        {
            return Conflict(ex.Message);
        }
        catch (Exception)
        {
            return StatusCode(500, "Ocurrió un error al crear el producto.");
        }
    }

    // 2. Obtener todos los productos activos (público)
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var products = await _productService.GetAllAsync();
            if (!products.Any()) return NoContent();
            return Ok(products);
        }
        catch (Exception)
        {
            return StatusCode(500, "Ocurrió un error al obtener los productos.");
        }
    }

    // 3. Obtener producto por ID (público)
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var product = await _productService.GetByIdAsync(id);
            return Ok(product);
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception)
        {
            return StatusCode(500, "Ocurrió un error al buscar el producto.");
        }
    }

    // 4. Actualizar producto (solo Admin)
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ProductModel.ProductRequest request)
    {
        try
        {
            var result = await _productService.UpdateAsync(id, request);
            return Ok(result);
        }
        catch (InvalidEntityException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (DuplicatedEntityException ex)
        {
            return Conflict(ex.Message);
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception)
        {
            return StatusCode(500, "Ocurrió un error al actualizar el producto.");
        }
    }

    // 5. Inhabilitar producto (solo Admin)
    [HttpPatch("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Disable(Guid id)
    {
        try
        {
            await _productService.DisableAsync(id);
            return NoContent();
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception)
        {
            return StatusCode(500, "Ocurrió un error al desactivar el producto.");
        }
    }
}
