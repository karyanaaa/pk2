using System;
using System.Collections.Generic;
using System.Linq;
using WpfApp1.Models;

namespace WpfApp1.Services
{
    public class SizeOption
    {
        public int StockItemId { get; set; }
        public decimal Size { get; set; }
        public int Quantity { get; set; }
        public override string ToString() => $"Размер {Size} (в наличии {Quantity})";
    }

    public static class OrderService
    {
        public static List<OrderRow> LoadOrders()
        {
            using (var ctx = new ChudoObuvEntities())
            {
                return ctx.Orders
                    .Select(o => new OrderRow
                    {
                        OrderId = o.OrderId,
                        OrderDate = o.OrderDate,
                        CustomerName = o.CustomerName,
                        Total = o.OrderItems.Sum(oi => (decimal?)(oi.Quantity * oi.UnitPrice)) ?? 0
                    })
                    .OrderByDescending(o => o.OrderDate)
                    .ToList();
            }
        }

        public static List<SizeOption> LoadSizes(int productId)
        {
            using (var ctx = new ChudoObuvEntities())
            {
                return ctx.StockItems
                    .Where(si => si.ProductId == productId && si.Quantity > 0)
                    .Select(si => new SizeOption
                    {
                        StockItemId = si.StockItemId,
                        Size = si.Sizes.SizeValue,
                        Quantity = si.Quantity
                    })
                    .OrderBy(si => si.Size)
                    .ToList();
            }
        }

        public static List<OrderItemRow> LoadOrderItems(int orderId)
        {
            using (var ctx = new ChudoObuvEntities())
            {
                return ctx.OrderItems
                    .Where(oi => oi.OrderId == orderId)
                    .Select(oi => new OrderItemRow
                    {
                        Description = oi.StockItems.Products.Name + " ("
                                    + oi.StockItems.Products.Manufacturers.Name + "), р."
                                    + oi.StockItems.Sizes.SizeValue,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice
                    })
                    .ToList();
            }
        }

        public static int CreateOrder(string customerName, int userId,
                                      int stockItemId, int quantity, decimal unitPrice)
        {
            using (var ctx = new ChudoObuvEntities())
            {
                var order = new Orders
                {
                    OrderDate = DateTime.Now,
                    CustomerName = customerName,
                    UserId = userId
                };
                ctx.Orders.Add(order);
                ctx.SaveChanges();

                var item = new OrderItems
                {
                    OrderId = order.OrderId,
                    StockItemId = stockItemId,
                    Quantity = quantity,
                    UnitPrice = unitPrice
                };
                ctx.OrderItems.Add(item);

                var stock = ctx.StockItems.First(s => s.StockItemId == stockItemId);
                stock.Quantity -= quantity;

                ctx.SaveChanges();
                return order.OrderId;
            }
        }

        public static void DeleteOrder(int orderId)
        {
            using (var ctx = new ChudoObuvEntities())
            {
                var order = ctx.Orders.FirstOrDefault(o => o.OrderId == orderId);
                if (order == null) return;
                ctx.Orders.Remove(order);
                ctx.SaveChanges();
            }
        }
    }
}