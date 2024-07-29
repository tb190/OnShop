CREATE TABLE [dbo].[FollowedCompanies] (
    [Id]        INT      IDENTITY (1, 1) NOT NULL,
    [UserId]    INT      NOT NULL,
    [CompanyId] INT      NOT NULL,
    [CreatedAt] DATETIME DEFAULT (getdate()) NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]) ON DELETE CASCADE,
    FOREIGN KEY ([CompanyId]) REFERENCES [dbo].[Companies] ([CompanyId]) ON DELETE CASCADE
);

