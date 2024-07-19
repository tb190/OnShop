namespace OnShop.Models
{
    public class ProductModel
    {
        public int ProductId { get; set; }
        public int Rating { get; set; } // kullanýcýlarýn ürüne verdiði puan
        public int Favorites { get; set; } // kaç kiþi bu urunu favoriledi
        public int CompanyID { get; set; }
        public int Stock { get; set; }
        public int Clicked { get; set; }

        public decimal Price { get; set; }

        public string ProductName { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public ICollection<int> ProductReviewsID { get; set; }
        public ICollection<string> ProductReviews { get; set; }
        public ICollection<string> Photos { get; set; }
    }
}