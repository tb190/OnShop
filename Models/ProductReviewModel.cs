public class ProductReviewModel
{
    public int ReviewId { get; set; } // primary key
    public int ProductId { get; set; } // foreign key
    public int CompanyId { get; set; }
    public int Rating { get; set; }
    public string Review { get; set; }

    public DateTime CreatedAt { get; set; }
}