using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Domain.Entities
{
    public class Product : EntityBase
    {
        public Product() { }
        public Product(string sku, string internalcode, string name, string? descripcion, decimal price, int stock) 
        {
            Sku = sku;
            InternalCode = internalcode;
            Name = name;
            Description = descripcion;
            CurrentUnitPrice = price;
            StockQuantity = stock;
            IsActive = true;
        }
        public string Sku {  get; set; }
        public string? InternalCode {  get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal CurrentUnitPrice { get; set; }
        public int StockQuantity { get; set; }
        public bool IsActive { get; set; }

        //relacion 1 a * con OrderItem
        public ICollection<OrderItem>? OrderItems { get; set; }



    }
}
