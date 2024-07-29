

 /*
INSERT INTO [dbo].[Types] 
(
    [CategoryId], 
    [TypeName]
)
VALUES (6, 'Accessories ');



INSERT INTO [dbo].[Users] 
(
    [UserName], 
    [UserSurName], 
    [PasswordHash], 
    [Email], 
    [Role], 
    [Address], 
    [PhoneNumber], 
    [Age], 
    [BirthDate]
)
VALUES
('Bob', 'Smith', HASHBYTES('SHA2_256', 'techmart'), 'bob.smith@example.com', 'User', '456 Tech Road, Innovation City', '555-987-6543', 35, '1989-02-02'),
('Carla', 'Martinez', HASHBYTES('SHA2_256', 'ecomarket'), 'carla.martinez@example.com', 'User', '789 Green Lane, Eco Town', '555-321-7654', 28, '1995-03-03'),
('David', 'Lee', HASHBYTES('SHA2_256', 'homeessentials'), 'david.lee@example.com', 'User', '321 Home Street, Cozy Town', '555-654-3210', 32, '1991-04-04'),
('Emma', 'Brown', HASHBYTES('SHA2_256', 'gourmetgoods'), 'emma.brown@example.com', 'User', '654 Culinary Avenue, Food City', '555-789-1234', 27, '1996-05-05'),
('Frank', 'Wilson', HASHBYTES('SHA2_256', 'kidscorner'), 'frank.wilson@example.com', 'User', '987 Fun Lane, Kid Town', '555-432-1098', 34, '1989-06-06'),
('Grace', 'Davis', HASHBYTES('SHA2_256', 'urbanbazaar'), 'grace.davis@example.com', 'User', '123 Urban Street, Metropolis', '555-567-8901', 29, '1994-07-07'),
('Henry', 'Clark', HASHBYTES('SHA2_256', 'sportsoutdoors'), 'henry.clark@example.com', 'User', '456 Adventure Road, Outdoor City', '555-890-1234', 33, '1990-08-08'),
('Isabel', 'King', HASHBYTES('SHA2_256', 'beautyhaven'), 'isabel.king@example.com', 'User', '789 Beauty Lane, Glamour City', '555-321-9876', 31, '1992-09-09'),
('Jack', 'Wright', HASHBYTES('SHA2_256', 'petemporium'), 'jack.wright@example.com', 'User', '321 Pet Street, Animal Town', '555-123-0987', 36, '1988-10-10');


INSERT INTO [dbo].[Companies]
(
    [Score], 
    [UserId], 
    [CompanyName], 
    [ContactName], 
    [Description], 
    [Address], 
    [PhoneNumber], 
    [Email], 
    [LogoUrl], 
    [BannerUrl], 
    [TaxIDNumber], 
    [IBAN], 
    [IsValidatedByAdmin],
    [BirthDate]
)
VALUES
(0, 3, 'TechMart', 'Bob Smith', 'Your go-to destination for the latest electronics and gadgets.', '456 Tech Road, Innovation City', '555-987-6543', 'info@techmart.com', '/Pictures/CompanyLogos/techmart.png', '/Pictures/CompanyBanners/techmart.png', 'TAX987654321', 'TR330006100519786457841337', 1, '1991-02-02'),
(0, 4, 'EcoMarket', 'Carla Martinez', 'Offering eco-friendly and sustainable products for a greener lifestyle.', '789 Green Lane, Eco Town', '555-321-7654', 'support@ecomarket.com', '/Pictures/CompanyLogos/ecomarket.png', '/Pictures/CompanyBanners/ecomarket.png', 'TAX192837465', 'TR330006100519786457841338', 1, '1992-03-03'),
(0, 5, 'Home Essentials', 'David Lee', 'Quality home decor and essentials for every room.', '321 Home Street, Cozy Town', '555-654-3210', 'sales@homeessentials.com', '/Pictures/CompanyLogos/homeessentials.png', '/Pictures/CompanyBanners/homeessentials.png', 'TAX564738291', 'TR330006100519786457841339', 1, '1993-04-04'),
(0, 6, 'Gourmet Goods', 'Emma Brown', 'Delicious gourmet food and kitchen supplies delivered to your door.', '654 Culinary Avenue, Food City', '555-789-1234', 'orders@gourmetgoods.com', '/Pictures/CompanyLogos/gourmetgoogs.png', '/Pictures/CompanyBanners/gourmetgoods.png', 'TAX837261945', 'TR330006100519786457841340', 1, '1994-05-05'),
(0, 7, 'Kids Corner', 'Frank Wilson', 'Everything your kids need, from clothing to toys.', '987 Fun Lane, Kid Town', '555-432-1098', 'info@kidscorner.com', '/Pictures/CompanyLogos/kidscorner.png', '/Pictures/CompanyBanners/kidscorner.png', 'TAX123789456', 'TR330006100519786457841341', 1, '1995-06-06'),
(0, 8, 'Urban Bazaar', 'Grace Davis', 'Urban essentials from fashion to home decor, all in one place.', '123 Urban Street, Metropolis', '555-567-8901', 'contact@urbanbazaar.com', '/Pictures/CompanyLogos/urbanpazar.png', '/Pictures/CompanyBanners/urbanbazar.png', 'TAX456123789', 'TR330006100519786457841342', 1, '1996-07-07'),
(0, 9, 'Sports & Outdoors', 'Henry Clark', 'Gear up for your adventures with our sports and outdoor equipment.', '456 Adventure Road, Outdoor City', '555-890-1234', 'sales@sportsoutdoors.com', '/Pictures/CompanyLogos/sports.png', '/Pictures/CompanyBanners/sport.png', 'TAX789456123', 'TR330006100519786457841343', 1, '1997-08-08'),
(0, 10, 'Beauty Haven', 'Isabel King', 'Find the best skincare, makeup, and beauty products.', '789 Beauty Lane, Glamour City', '555-321-9876', 'support@beautyhaven.com', '/Pictures/CompanyLogos/beaty.png', '/Pictures/CompanyBanners/beaty.png', 'TAX321789654', 'TR330006100519786457841344', 1, '1998-09-09'),
(0, 11, 'Pet Emporium', 'Jack Wright', 'Quality products and accessories for your pets.', '321 Pet Street, Animal Town', '555-123-0987', 'info@petemporium.com', '/Pictures/CompanyLogos/pet.png', '/Pictures/CompanyBanners/pet.png', 'TAX654123789', 'TR330006100519786457841345', 1, '1999-10-10');*/