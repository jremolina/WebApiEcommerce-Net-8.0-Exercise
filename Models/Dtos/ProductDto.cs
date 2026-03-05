using System;

namespace ApiEcommerce.Models.Dtos;

public class ProductDto
{

    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string ImgUrl { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;// Prdo-001-blk. ejemplo de codigo inventario
    public int Stock { get; set; }
    public DateTime CreationDate { get; set; } = DateTime.Now;
    public DateTime? UpdateDate { get; set; } = null;
    //realacion con category 
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
}
