namespace OnShop.Models
{
    public class VendorViewModel
    {

        public UserModel VendorUserInfos { get; set; }
        public CompanyModel VendorCompanyInfos { get; set; }

        public ProductModel productModel = new ProductModel(); 

        public List<ProductModel> AllProducts = new List<ProductModel>();
        public List<CategoryModel> AllCategoriesWithTypes = new List<CategoryModel>();
        public List<UserModel> AllUsers = new List<UserModel>();
        public List<UserModel> AllFollowers = new List<UserModel>();
    }
}