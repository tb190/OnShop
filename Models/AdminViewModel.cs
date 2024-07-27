namespace OnShop.Models
{
    public class AdminViewModel
    {
        public int UnvalidatedCount { get; set; }
        public UserModel User { get; set; }
        public CompanyModel Company { get; set; }
        public string RegistrationType { get; set; } // To determine if it's an individual or company registration


        public List<UserModel> AllUsers { get; set; } 
        public List<ProductModel> AllProducts { get; set; }
        public List<CompanyModel> AllCompanies { get; set; }
        public List<CategoryModel> AllCategories { get; set; }
        public List<PurchasedProductModel> PurchasedProducts { get; set; }
        public List<ProductReviewModel> ProductsReviews { get; set; }

        public decimal TotalRevenue { get; set; } = 0; // Purchased products ile fiyat çarpýlarak hesaplandý
        public int TotalSold { get; set; } = 0;
        public int TotalFavorites { get; set; } = 0;
        public int TotalClicks { get; set; } = 0;
        public int TotalReviews { get; set; } = 0;

    }
}