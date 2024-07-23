namespace OnShop.Models
{
    public class UserModel
    {
        public int UserId { get; set; } // primary key

        public string Name { get; set; }
        public string SurName { get; set; }
        public string PasswordHash { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }

        public int Age { get; set; }
        public DateTime BirthDate { get; set; }


        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<ProductReviewModel> ProductReviews { get; set; } // foreign keys
        public ICollection<CompanyModel> FollowedCompanies { get; set; } // foreign keys
        public ICollection<CreditCardModel> CreditCards { get; set; } // foreign keys
        public ICollection<ProductModel> PurchasedProducts { get; set; } // foreign keys

        public ICollection<int> BasketProductsID { get; set; } // foreign keys
    }
}