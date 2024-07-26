namespace OnShop.Models
{
    public class AdminViewModel
    {
        public UserModel User { get; set; }
        public CompanyModel Company { get; set; }
        public string RegistrationType { get; set; } // To determine if it's an individual or company registration


        public List<UserModel> AllUsers { get; set; } // Ensure this property is present
    }
}