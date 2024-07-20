namespace OnShop.Models
{
    public class LoginViewModel
    {
        public UserModel User { get; set; }
        public CompanyModel Company { get; set; }
        public string RegistrationType { get; set; } // To determine if it's an individual or company registration
    }
}