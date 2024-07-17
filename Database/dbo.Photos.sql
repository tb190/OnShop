CREATE TABLE [dbo].[Photos]
(
	[Id] INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	[ProductId] INT NOT NULL,
	[PhotoURL] varchar(MAX) NOT NULL,
	FOREIGN KEY([ProductId]) references Products([ProductId])
)
