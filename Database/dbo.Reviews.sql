CREATE TABLE [dbo].[Reviews] (
    [ReviewId]  INT            IDENTITY (1, 1) NOT NULL,
    [ProductId] INT            NOT NULL,
    [CompanyId] INT            NOT NULL,
    [UserId]    INT            DEFAULT ((2)) NOT NULL,
    [Rating]    INT            DEFAULT ((0)) NOT NULL,
    [Review]    NVARCHAR (MAX) NOT NULL,
    [CreatedAt] DATETIME       DEFAULT (getdate()) NOT NULL,
    PRIMARY KEY CLUSTERED ([ReviewId] ASC),
    FOREIGN KEY ([CompanyId]) REFERENCES [dbo].[Companies] ([CompanyId]) ON DELETE CASCADE,
    FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products] ([ProductId]) ON DELETE CASCADE,
    FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]) ON DELETE CASCADE
);

