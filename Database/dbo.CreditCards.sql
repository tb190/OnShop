CREATE TABLE [dbo].[CreditCards] (
    [CreditCardId]   INT          IDENTITY (1, 1) NOT NULL,
    [UserId]         INT          NOT NULL,
    [CardNumber]     VARCHAR (20) NOT NULL,
    [CardHolderName] VARCHAR (50) NOT NULL,
    [ExpirationDate] CHAR (5)     NOT NULL,
    [CVV]            CHAR (3)     NOT NULL,
    PRIMARY KEY CLUSTERED ([CreditCardId] ASC),
    FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]) ON DELETE CASCADE
);

