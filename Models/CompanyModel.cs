namespace OnShop.Models
{
    public class CompanyModel
    {
        public int CompanyId { get; set; } // primary key
        public int Score { get; set; }
        public int UserID { get; set; } // foreign key

        public string Name { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string LogoUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<int> ProductsID { get; set; } // foreign keys
        public ICollection<int> FollowersID { get; set; } // foreign keys
    }
}