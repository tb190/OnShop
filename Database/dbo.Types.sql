CREATE TABLE [dbo].[Types] (
    [Id]         INT          IDENTITY (1, 1) NOT NULL,
    [CategoryId] INT          NOT NULL,
    [TypeName]   VARCHAR (20) NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    FOREIGN KEY ([CategoryId]) REFERENCES [dbo].[Categories] ([Id]) ON DELETE CASCADE
);

