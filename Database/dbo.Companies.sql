CREATE TABLE [dbo].[Companies] (
    [CompanyId]          INT           IDENTITY (1, 1) NOT NULL,
    [Score]              INT           DEFAULT ((0)) NOT NULL,
    [UserId]             INT           NOT NULL,
    [CompanyName]        VARCHAR (255) NOT NULL,
    [ContactName]        VARCHAR (50)  NOT NULL,
    [Description]        VARCHAR (MAX) NOT NULL,
    [Address]            VARCHAR (255) NOT NULL,
    [PhoneNumber]        VARCHAR (20)  NOT NULL,
    [Email]              VARCHAR (100) NOT NULL,
    [LogoUrl]            VARCHAR (MAX) NULL,
    [BannerUrl]          VARCHAR (MAX) NULL,
    [TaxIDNumber]        VARCHAR (50)  NOT NULL,
    [IBAN]               VARCHAR (34)  NOT NULL,
    [IsValidatedByAdmin] BIT           DEFAULT ((0)) NOT NULL,
    [CreatedAt]          DATETIME      DEFAULT (getdate()) NOT NULL,
    [BirthDate]          DATETIME      DEFAULT (getdate()) NOT NULL,
    PRIMARY KEY CLUSTERED ([CompanyId] ASC),
    UNIQUE NONCLUSTERED ([Email] ASC)
);

