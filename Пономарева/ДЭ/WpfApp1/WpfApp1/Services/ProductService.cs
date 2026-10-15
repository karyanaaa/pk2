using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using WpfApp1.Models;

namespace WpfApp1.Services
{
    public static class ProductService
    {
        public static List<ProductCard> LoadCatalog(string search, string category)
        {
            using (var ctx = new ChudoObuvEntities())
            {
                var query = ctx.Products
                    .Include(p => p.Categories)
                    .Include(p => p.Manufacturers)
                    .Include(p => p.Subcategories)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(search))
                    query = query.Where(p => p.Name.Contains(search)
                                          || p.Description.Contains(search));

                if (!string.IsNullOrEmpty(category) && category != "Все категории")
                    query = query.Where(p => p.Categories.Name == category);

                var raw = query.ToList();
                var result = new List<ProductCard>();

                foreach (var p in raw)
                {
                    int totalQty = ctx.StockItems
                                      .Where(si => si.ProductId == p.ProductId)
                                      .Select(si => (int?)si.Quantity)
                                      .Sum() ?? 0;

                    bool hasOrdersLastMonth = HasOrdersLastMonth(ctx, p.ProductId);
                    decimal finalPrice = hasOrdersLastMonth ? p.Price : p.Price * 0.75m;

                    result.Add(new ProductCard
                    {
                        ProductId = p.ProductId,
                        Name = p.Name,
                        Description = p.Description,
                        Composition = p.Composition,
                        ImageFile = p.ImagePath,
                        Category = p.Categories.Name,
                        Subcategory = p.Subcategories != null ? p.Subcategories.Name : "",
                        Manufacturer = p.Manufacturers.Name,
                        Price = p.Price,
                        FinalPrice = finalPrice,
                        TotalQuantity = totalQty
                    });
                }
                return result;
            }
        }

        public static List<string> LoadCategories()
        {
            using (var ctx = new ChudoObuvEntities())
            {
                var list = new List<string> { "Все категории" };
                list.AddRange(ctx.Categories.OrderBy(c => c.Name).Select(c => c.Name));
                return list;
            }
        }

        private static bool HasOrdersLastMonth(ChudoObuvEntities ctx, int productId)
        {
            DateTime firstDayPrevMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)
                                            .AddMonths(-1);
            DateTime firstDayCurrMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

            return ctx.OrderItems
                .Any(oi => oi.StockItems.ProductId == productId
                        && oi.Orders.OrderDate >= firstDayPrevMonth
                        && oi.Orders.OrderDate < firstDayCurrMonth);
        }
    }
}