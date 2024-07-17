public class ProductReviewModel
{
    public int ReviewId { get; set; } // primary key
    public int ProductId { get; set; } // foreign key
    public int UserId { get; set; } // foreign key
    public int Rating { get; set; }
    public string Comment { get; set; }

    public DateTime CreatedAt { get; set; }
}