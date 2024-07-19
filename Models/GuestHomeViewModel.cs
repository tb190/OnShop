namespace OnShop.Models
{
    public class GuestHomeViewModel
    {
        public List<CategoryModel> Categories { get; set; }
        public List<ProductModel> Products { get; set; }
        public List<ProductModel> MostClickedProducts { get; set; }
    }
}