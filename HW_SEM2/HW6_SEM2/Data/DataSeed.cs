using EfCoreHomework.Models;

namespace EfCoreHomework.Data;

public static class DataSeed
{
    public static readonly Category[] Categories =
    {
        new Category { Id = 1, Name = "Ноутбуки" },
        new Category { Id = 2, Name = "Смартфоны" },
        new Category { Id = 3, Name = "Аксессуары" }
    };

    public static readonly Product[] Products =
    {
        new Product { Id = 1, Name = "Lenovo IdeaPad 3", Price = 55000m, CategoryId = 1 },
        new Product { Id = 2, Name = "Apple MacBook Air", Price = 120000m, CategoryId = 1 },
        new Product { Id = 3, Name = "Samsung Galaxy S23", Price = 85000m, CategoryId = 2 },
        new Product { Id = 4, Name = "iPhone 14", Price = 95000m, CategoryId = 2 },
        new Product { Id = 5, Name = "Logitech Mouse", Price = 2500m, CategoryId = 3 },
        new Product { Id = 6, Name = "USB-C Cable", Price = 900m, CategoryId = 3 }
    };
}
