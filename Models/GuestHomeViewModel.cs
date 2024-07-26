namespace OnShop.Models
{
    public class GuestHomeViewModel
    {
        public List<CategoryModel> Categories { get; set; }
        public List<ProductModel> Products { get; set; }
        public List<ProductModel> MostClickedProducts { get; set; }
        public List<ProductModel> MostFavoritedProducts { get; set; }
        public List<ProductModel> RecentProducts { get; set; }
        public List<ProductModel> HighStarProducts { get; set; }
        public List<ProductModel> LessStockProducts { get; set; }
        public List<ProductModel> BestsellerProducts { get; set; }
        public List<ProductModel> AllProducts { get; set; }
        public List<CompanyModel> AllCompanies { get; set; }
    }
}