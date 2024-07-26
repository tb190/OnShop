namespace OnShop.Models
{
    public class ProductViewModel
    {
        public decimal TotalPrice { get; set; }
        public CompanyModel Company { get; set; }
        public UserModel User { get; set; }
        public ProductModel Product { get; set; }
        public List<ProductReviewModel> ProductReviews { get; set; }
        public GuestHomeViewModel GuestHomeView { get; set; }
        public List<CategoryModel> Categories { get; set; }
        public List<ProductModel> OtherProducts { get; set; }

        public List<BasketProductModel> BasketProducts { get; set; }

        public List<ProductModel> DeletedProducts { get; set; }

        public UserModel userModel { get; set; }
        public List<CompanyModel> AllCompanies { get; set; }

        public List<ProductModel> AllProducts { get; set; }

        public AdminViewModel CompanyDetails { get; set; }

        public bool IsFollowing { get; set; }
    }
}