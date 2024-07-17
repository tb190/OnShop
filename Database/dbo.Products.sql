CREATE TABLE [dbo].[Product]
(
    [ProductId] INT NOT NULL PRIMARY KEY,
    [Rating] INT NOT NULL,
    [Favorites] INT NOT NULL,
    [CompanyID] INT NOT NULL,
    [Stock] INT NOT NULL,
    [Price] DECIMAL(18, 2) NOT NULL,
    [ProductName] NVARCHAR(100) NOT NULL,
    [Description] NVARCHAR(MAX) NOT NULL,
    [Category] NVARCHAR(50) NOT NULL,
    [Status] NVARCHAR(20) NOT NULL,
    [CreatedAt] DATETIME NOT NULL,
    -- Assuming ProductReviewsID is a related table's foreign key
    -- [ProductReviewsID] INT FOREIGN KEY REFERENCES ProductReviews(Id),
    -- Assuming ProductReviews and Photos are stored in related tables
);