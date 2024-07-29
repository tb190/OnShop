CREATE TABLE [dbo].[FavoritedProducts] (
    [Id]            INT      IDENTITY (1, 1) NOT NULL,
    [UserId]        INT      NOT NULL,
    [ProductId]     INT      NOT NULL,
    [FavoritedDate] DATETIME DEFAULT (getdate()) NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]) ON DELETE CASCADE,
    FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products] ([ProductId]) ON DELETE CASCADE
);

