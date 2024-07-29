CREATE TABLE [dbo].[DeletedProducts] (
    [Id]        INT      IDENTITY (1, 1) NOT NULL,
    [UserId]    INT      NOT NULL,
    [ProductId] INT      NOT NULL,
    [CompanyId] INT      NOT NULL,
    [DeletedAt] DATETIME DEFAULT (getdate()) NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]) ON DELETE CASCADE,
    FOREIGN KEY ([CompanyId]) REFERENCES [dbo].[Companies] ([CompanyId]) ON DELETE CASCADE,
    FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products] ([ProductId]) ON DELETE CASCADE
);

