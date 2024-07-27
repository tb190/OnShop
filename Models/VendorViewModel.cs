namespace OnShop.Models
{
    public class VendorViewModel
    {
        public UserModel VendorUserInfos { get; set; }
        public CompanyModel VendorCompanyInfos { get; set; }

        public ProductModel productModel = new ProductModel(); 

        public List<ProductModel> AllProducts = new List<ProductModel>();
        public List<ProductModel> OnlineProducts = new List<ProductModel>();
        public List<ProductModel> OfflineProducts = new List<ProductModel>();
        public List<CategoryModel> AllCategoriesWithTypes = new List<CategoryModel>();
        public List<UserModel> AllUsers = new List<UserModel>();
        public List<UserModel> AllFollowers = new List<UserModel>();
        public Dictionary<string, Dictionary<string, int>> categoryTypeCount = new Dictionary<string, Dictionary<string, int>>();


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