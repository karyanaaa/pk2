namespace WpfApp1.Models
{
    public class ProductCard
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string Manufacturer { get; set; }
        public string Category { get; set; }
        public string Subcategory { get; set; }
        public string Composition { get; set; }
        public string Description { get; set; }

        // Имя файла из БД (например "IMG_KS_195222.png")
        public string ImageFile { get; set; }

        // Готовый URI для WPF, вычисляется автоматически
        public string ImagePath
        {
            get
            {
                string file = string.IsNullOrWhiteSpace(ImageFile)
                    ? "picture.png"
                    : ImageFile;
                return $"pack://application:,,,/images/{file}";
            }
        }

        public decimal Price { get; set; }
        public decimal FinalPrice { get; set; }
        public int TotalQuantity { get; set; }

        public string QuantityText => TotalQuantity > 5 ? "много" : "мало";
        public bool IsLowStock => TotalQuantity <= 3;
    }
}