CREATE TABLE [dbo].[Users] (
    [UserId]       INT           IDENTITY (1, 1) NOT NULL,
    [UserName]     VARCHAR (20)  NOT NULL,
    [UserSurName]  VARCHAR (30)  NOT NULL,
    [PasswordHash] VARCHAR (300) NOT NULL,
    [Email]        VARCHAR (100) NOT NULL,
    [Role]         VARCHAR (20)  NOT NULL,
    [Address]      VARCHAR (255) NOT NULL,
    [PhoneNumber]  VARCHAR (25)  NOT NULL,
    [Age]          INT           NULL,
    [BirthDate]    DATETIME      DEFAULT (getdate()) NULL,
    [CreatedAt]    DATETIME      DEFAULT (getdate()) NOT NULL,
    PRIMARY KEY CLUSTERED ([UserId] ASC),
    UNIQUE NONCLUSTERED ([Email] ASC)
);

