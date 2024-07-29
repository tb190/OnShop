CREATE TABLE [dbo].[Photos] (
    [Id]        INT           IDENTITY (1, 1) NOT NULL,
    [ProductId] INT           NOT NULL,
    [PhotoURL]  VARCHAR (MAX) NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products] ([ProductId]) ON DELETE CASCADE
);

