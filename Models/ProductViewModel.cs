namespace OnShop.Models
{
    public class ProductViewModel
    {
        public CompanyModel Company { get; set; }
        public UserModel User { get; set; }
        public ProductModel Product { get; set; }
        public List<ProductReviewModel> ProductReviews { get; set; }
        public GuestHomeViewModel GuestHomeView { get; set; }
        public List<CategoryModel> Categories { get; set; }
        public List<ProductModel> OtherProducts { get; set; }
    }
}