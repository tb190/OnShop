namespace OnShop.Models
{
    public class CompanyModel
    {
        public int CompanyId { get; set; } // primary key
        public int Score { get; set; }
        public int UserID { get; set; } // foreign key

        public string CompanyName { get; set; }
        public string ContactName { get; set; }
        public string CompanyDescription { get; set; }
        public string CompanyAddress { get; set; }
        public string CompanyPhoneNumber { get; set; }
        public string Email { get; set; }
        public string LogoUrl { get; set; }
        public string BannerUrl { get; set; }
        public string taxIDNumber { get; set; }
        public string IBAN { get; set; }

        public bool isValidatedbyAdmin = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime BirthDate { get; set; } = DateTime.Now;

        public ICollection<int> ProductsID { get; set; } // foreign keys
        public ICollection<int> FollowersID { get; set; } // foreign keys
    }
}