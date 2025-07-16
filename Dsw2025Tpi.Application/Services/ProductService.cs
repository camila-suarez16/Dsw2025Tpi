using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Interfaces;

namespace Dsw2025Tpi.Application.Services;

public class ProductService
{
    private readonly IRepository _repository;

    public ProductService(IRepository repository)
    {
        _repository = repository;
    }

    // 1. Crear producto
    public async Task<ProductModel.ProductResponse> CreateAsync(ProductModel.ProductRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Sku))
            throw new InvalidEntityException("El SKU es obligatorio.");

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new InvalidEntityException("El nombre es obligatorio.");

        if (request.CurrentUnitPrice <= 0)
            throw new InvalidEntityException("El precio debe ser mayor a 0.");

        if (request.StockQuantity < 0)
            throw new InvalidEntityException("El stock no puede ser negativo.");

        var existing = await _repository.First<Product>(p => p.Sku == request.Sku);
        if (existing != null)
            throw new DuplicatedEntityException($"Ya existe un producto con el SKU '{request.Sku}'.");

        var product = new Product(
            request.Sku,
            request.InternalCode,
            request.Name,
            request.Description,
            request.CurrentUnitPrice,
            request.StockQuantity
        );

        await _repository.Add(product);
        await _repository.SaveChangesAsync();

        return new ProductModel.ProductResponse(
            product.Id,
            product.Sku,
            product.InternalCode,
            product.Name,
            product.Description,
            product.CurrentUnitPrice,
            product.StockQuantity,
            product.IsActive
        );
    }

    // 2. Obtener todos los productos activos
    public async Task<IEnumerable<ProductModel.ProductResponse>> GetAllAsync()
    {
        var products = await _repository.GetFiltered<Product>(p => p.IsActive);

        if (products == null || !products.Any())
            return Enumerable.Empty<ProductModel.ProductResponse>();

        return products.Select(p => new ProductModel.ProductResponse(
            p.Id,
            p.Sku,
            p.InternalCode,
            p.Name,
            p.Description,
            p.CurrentUnitPrice,
            p.StockQuantity,
            p.IsActive
        ));
    }

    // 3. Obtener producto por ID
    public async Task<ProductModel.ProductResponse> GetByIdAsync(Guid id)
    {
        var product = await _repository.GetById<Product>(id)
            ?? throw new EntityNotFoundException($"Producto con ID {id} no encontrado.");

        return new ProductModel.ProductResponse(
            product.Id,
            product.Sku,
            product.InternalCode,
            product.Name,
            product.Description,
            product.CurrentUnitPrice,
            product.StockQuantity,
            product.IsActive
        );
    }

    // 4. Actualizar producto
    public async Task<ProductModel.ProductResponse> UpdateAsync(Guid id, ProductModel.ProductRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Sku))
            throw new InvalidEntityException("El SKU es obligatorio.");

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new InvalidEntityException("El nombre es obligatorio.");

        if (request.CurrentUnitPrice <= 0)
            throw new InvalidEntityException("El precio debe ser mayor a 0.");

        if (request.StockQuantity < 0)
            throw new InvalidEntityException("El stock no puede ser negativo.");

        var product = await _repository.GetById<Product>(id)
            ?? throw new EntityNotFoundException($"Producto con ID {id} no encontrado.");

        if (product.Sku != request.Sku)
        {
            var duplicate = await _repository.First<Product>(p => p.Sku == request.Sku && p.Id != id);
            if (duplicate != null)
                throw new DuplicatedEntityException($"Ya existe otro producto con el SKU '{request.Sku}'.");
        }

        product.Sku = request.Sku.Trim();
        product.InternalCode = request.InternalCode?.Trim();
        product.Name = request.Name.Trim();
        product.Description = request.Description?.Trim();
        product.CurrentUnitPrice = request.CurrentUnitPrice;
        product.StockQuantity = request.StockQuantity;

        await _repository.Update(product);
        await _repository.SaveChangesAsync();

        return new ProductModel.ProductResponse(
            product.Id,
            product.Sku,
            product.InternalCode,
            product.Name,
            product.Description,
            product.CurrentUnitPrice,
            product.StockQuantity,
            product.IsActive
        );
    }

    // 5. Inhabilitar producto
    public async Task DisableAsync(Guid id)
    {
        var product = await _repository.GetById<Product>(id)
            ?? throw new EntityNotFoundException($"Producto con ID {id} no encontrado.");

        product.IsActive = false;

        await _repository.Update(product);
        await _repository.SaveChangesAsync();
    }
}


