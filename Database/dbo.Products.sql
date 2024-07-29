CREATE TABLE [dbo].[Products] (
    [ProductId]   INT             IDENTITY (1, 1) NOT NULL,
    [Rating]      INT             NOT NULL,
    [Favorites]   INT             NOT NULL,
    [CompanyID]   INT             NOT NULL,
    [Stock]       INT             NOT NULL,
    [Price]       DECIMAL (18, 2) NOT NULL,
    [ProductName] NVARCHAR (100)  NOT NULL,
    [Description] NVARCHAR (MAX)  NOT NULL,
    [Category]    NVARCHAR (50)   NOT NULL,
    [Type]        NVARCHAR (50)   DEFAULT ('Furniture') NOT NULL,
    [Status]      NVARCHAR (20)   NOT NULL,
    [CreatedAt]   DATETIME        NOT NULL,
    [Clicked]     INT             DEFAULT ((0)) NOT NULL,
    [Sold]        INT             DEFAULT ((0)) NOT NULL,
    PRIMARY KEY CLUSTERED ([ProductId] ASC)
);

