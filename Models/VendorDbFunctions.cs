using Microsoft.AspNetCore.Mvc;
using OnShop.Models;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Web;


namespace OnShop
{
    public class VendorDbFunctions 
    {
        private SqlConnection connection;

        public VendorDbFunctions(string connectionString)
        {
            connection = new SqlConnection(connectionString);
        }
        public void Dispose()
        {
            connection.Dispose();
        }
        // --------------------------------------------------------------------------------------------------------------------------
        public async Task<List<ProductModel>> VendorGetProducts(int? userId)
        {
            var products = new List<ProductModel>();

            try
            {
                await connection.OpenAsync();

                int CompanyId = 0;

                string queryCompany = @"
                                    SELECT CompanyId
                                    FROM Companies
                                    WHERE UserId = @UserId";

                using (var commandCompany = new SqlCommand(queryCompany, connection))
                {
                    commandCompany.Parameters.AddWithValue("@UserId", userId);

                    using (var readerCompany = await commandCompany.ExecuteReaderAsync())
                    {
                        if (await readerCompany.ReadAsync())
                        {
                            CompanyId = readerCompany.GetInt32(readerCompany.GetOrdinal("CompanyId"));
                        }
                    }
                }


                var queryProducts = @"
                    SELECT * FROM Products
                    WHERE CompanyId = @CompanyId";

                using (SqlCommand cmd = new SqlCommand(queryProducts, connection))
                {
                    cmd.Parameters.AddWithValue("@CompanyId", CompanyId);
                    using (SqlDataReader detailsReader = await cmd.ExecuteReaderAsync())
                    {
                        while (await detailsReader.ReadAsync())
                        {
                            var product_ = new ProductModel();
                            product_.ProductId = detailsReader.GetInt32(detailsReader.GetOrdinal("ProductId"));
                            product_.Rating = detailsReader.GetInt32(detailsReader.GetOrdinal("Rating"));
                            product_.Favorites = detailsReader.GetInt32(detailsReader.GetOrdinal("Favorites"));
                            product_.CompanyID = detailsReader.GetInt32(detailsReader.GetOrdinal("CompanyID"));
                            product_.Stock = detailsReader.GetInt32(detailsReader.GetOrdinal("Stock"));
                            product_.Price = detailsReader.GetDecimal(detailsReader.GetOrdinal("Price"));
                            product_.ProductName = detailsReader.GetString(detailsReader.GetOrdinal("ProductName"));
                            product_.Description = detailsReader.GetString(detailsReader.GetOrdinal("Description"));
                            product_.Category = detailsReader.GetString(detailsReader.GetOrdinal("Category"));
                            product_.Type = detailsReader.GetString(detailsReader.GetOrdinal("Type"));
                            product_.Status = detailsReader.GetString(detailsReader.GetOrdinal("Status"));
                            product_.CreatedAt = detailsReader.GetDateTime(detailsReader.GetOrdinal("CreatedAt"));
                            product_.Clicked = detailsReader.GetInt32(detailsReader.GetOrdinal("Clicked"));
                            product_.Sold = detailsReader.GetInt32(detailsReader.GetOrdinal("Sold"));
                            product_.Photos = new List<string>();
                            products.Add(product_);
                        }
                    }
                }

                for (int i = 0; i < products.Count; i++)
                {
                    string queryPhotos_ = @"
                                            SELECT PhotoURL
                                            FROM Photos
                                            WHERE ProductId = @ProductId";

                    using (var command = new SqlCommand(queryPhotos_, connection))
                    {
                        command.Parameters.AddWithValue("@ProductId", products[i].ProductId);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                products[i].Photos.Add(reader.GetString(reader.GetOrdinal("PhotoURL")));
                            }
                        }
                    }
                }

                for (int i = 0; i < products.Count; i++)
                {
                    List<ProductReviewModel> reviews = new List<ProductReviewModel>();

                    string query = @"
                        SELECT ReviewId, ProductId,  CompanyId, Rating, Review, CreatedAt
                        FROM Reviews
                        WHERE ProductId = @ProductId";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ProductId", products[i].ProductId);
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                reviews.Add(new ProductReviewModel
                                {
                                    ReviewId = reader.GetInt32(0),
                                    ProductId = reader.GetInt32(1),
                                    CompanyId = reader.GetInt32(2),
                                    Rating = reader.GetInt32(3),
                                    Review = reader.GetString(4),
                                    CreatedAt = reader.GetDateTime(5)
                                });
                            }

                        }
                    }
                    products[i].ProductReviewsModel = reviews;
                }

                connection.Close();
                return products;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                connection.Close();
                return products; // Optionally return an empty list on error
            }
        }



        // --------------------------------------------------------------------------------------------------------------------------
        public async Task<int> VendorAddProduct(int? userId,ProductModel product,List<IFormFile> Photos)
        {
            int productId = 0;

            try
            {
                connection.Open();
                int CompanyId = 0;

                string queryCompany = @"
                                    SELECT CompanyId
                                    FROM Companies
                                    WHERE UserId = @UserId";

                using (var commandCompany = new SqlCommand(queryCompany, connection))
                {
                    commandCompany.Parameters.AddWithValue("@UserId", userId);

                    using (var readerCompany = await commandCompany.ExecuteReaderAsync())
                    {
                        if (await readerCompany.ReadAsync())
                        {
                            CompanyId = readerCompany.GetInt32(readerCompany.GetOrdinal("CompanyId"));
                        }
                    }
                }



                string sql = @"INSERT INTO Products (Rating, Favorites, CompanyID, Stock, Price, ProductName, Description, Category, Status, CreatedAt)
                       OUTPUT INSERTED.ProductId
                       VALUES (@Rating, @Favorites, @CompanyID, @Stock, @Price, @ProductName, @Description, @Category, @Status, @CreatedAt)";

                 using (SqlCommand cmd = new SqlCommand(sql, connection))
                 {
                     cmd.Parameters.AddWithValue("@Rating", 0);
                     cmd.Parameters.AddWithValue("@Favorites", 0);
                     cmd.Parameters.AddWithValue("@CompanyID", CompanyId);
                     cmd.Parameters.AddWithValue("@Stock", product.Stock);
                     cmd.Parameters.AddWithValue("@Price", product.Price);
                     cmd.Parameters.AddWithValue("@ProductName", product.ProductName);
                     cmd.Parameters.AddWithValue("@Description", product.Description);
                     cmd.Parameters.AddWithValue("@Category", product.Category);
                     cmd.Parameters.AddWithValue("@Status", product.Status);
                     cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

                     // Execute the command and get the inserted ProductId
                     productId = (int)cmd.ExecuteScalar();     
                 }
                 
                // Insert photos into the Photos table       
                if (productId > 0 && Photos != null && Photos.Count > 0)
                {
                    foreach (var photoName in Photos)
                    {

                        string categoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Pictures", product.Category);
                        // Ensure the category directory exists
                        if (!Directory.Exists(categoryPath)){Directory.CreateDirectory(categoryPath);}


                        // Create a unique file name
                        string uniqueFileName = $"{Guid.NewGuid().ToString()}.jpg";
                        string filePath = Path.Combine(categoryPath, uniqueFileName);

                        Console.WriteLine("filepath: "+filePath);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await photoName.CopyToAsync(fileStream);
                        }

                        string filePathtoDB = Path.Combine("/Pictures",product.Category,uniqueFileName);

                        // Insert into the Photos table
                        sql = @"INSERT INTO Photos (ProductId, PhotoURL)
                                VALUES (@ProductId, @PhotoURL)";

                        using (SqlCommand cmd = new SqlCommand(sql, connection))
                        {
                            cmd.Parameters.AddWithValue("@ProductId", productId);
                            cmd.Parameters.AddWithValue("@PhotoURL", filePathtoDB);

                            await cmd.ExecuteNonQueryAsync();
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error adding product: " + ex.Message);
                throw;
            }
            finally
            {
                connection.Close();
            }

            return productId;
        }



        // --------------------------------------------------------------------------------------------------------------------------
        public async Task<List<ProductModel>> GetAllProducts(int? userId)
        {
            var products = new List<ProductModel>();

            try
            {
                await connection.OpenAsync();
                int CompanyId = 0;

                string queryCompany= @"
                                    SELECT CompanyId
                                    FROM Companies
                                    WHERE UserId = @UserId";

                using (var commandCompany = new SqlCommand(queryCompany, connection))
                {
                    commandCompany.Parameters.AddWithValue("@UserId", userId);

                    using (var readerCompany = await commandCompany.ExecuteReaderAsync())
                    {
                        if (await readerCompany.ReadAsync())
                        {
                            CompanyId = readerCompany.GetInt32(readerCompany.GetOrdinal("CompanyId"));
                        }
                    }
                }


                List<int> productIds = new List<int>();

                var queryProductsIds = @"
                    SELECT ProductId FROM PurchasedProducts
                    WHERE CompanyId = @CompanyId";

                using (SqlCommand cmdd = new SqlCommand(queryProductsIds, connection))
                {
                    cmdd.Parameters.AddWithValue("@CompanyId", CompanyId);
                    using (SqlDataReader PRReader = await cmdd.ExecuteReaderAsync())
                    {
                        while (await PRReader.ReadAsync())
                        {
                            int productId = PRReader.GetInt32(PRReader.GetOrdinal("ProductId"));
                            productIds.Add(productId);
                        }
                    }
                }

                for(int i = 0; i < productIds.Count; i++)
                {
                    var queryProducts = @"
                        SELECT * FROM Products
                        WHERE ProductId = @ProductId";

                    using (SqlCommand cmd = new SqlCommand(queryProducts, connection))
                    {
                        cmd.Parameters.AddWithValue("@ProductId", productIds[i]);
                        using (SqlDataReader detailsReader = await cmd.ExecuteReaderAsync())
                        {
                            while (await detailsReader.ReadAsync())
                            {
                                var product_ = new ProductModel();
                                product_.ProductId = detailsReader.GetInt32(detailsReader.GetOrdinal("ProductId"));
                                product_.Rating = detailsReader.GetInt32(detailsReader.GetOrdinal("Rating"));
                                product_.Favorites = detailsReader.GetInt32(detailsReader.GetOrdinal("Favorites"));
                                product_.CompanyID = detailsReader.GetInt32(detailsReader.GetOrdinal("CompanyID"));
                                product_.Stock = detailsReader.GetInt32(detailsReader.GetOrdinal("Stock"));
                                product_.Price = detailsReader.GetDecimal(detailsReader.GetOrdinal("Price"));
                                product_.ProductName = detailsReader.GetString(detailsReader.GetOrdinal("ProductName"));
                                product_.Description = detailsReader.GetString(detailsReader.GetOrdinal("Description"));
                                product_.Category = detailsReader.GetString(detailsReader.GetOrdinal("Category"));
                                product_.Type = detailsReader.GetString(detailsReader.GetOrdinal("Type"));
                                product_.Status = detailsReader.GetString(detailsReader.GetOrdinal("Status"));
                                product_.CreatedAt = detailsReader.GetDateTime(detailsReader.GetOrdinal("CreatedAt"));
                                product_.Clicked = detailsReader.GetInt32(detailsReader.GetOrdinal("Clicked"));
                                product_.Sold = detailsReader.GetInt32(detailsReader.GetOrdinal("Sold"));
                                product_.Photos = new List<string>();

                                products.Add(product_);
                            }
                        }
                    }
                }

                 for (int i = 0; i < products.Count; i++)
                {
                    string queryPhotos_ = @"
                                            SELECT PhotoURL
                                            FROM Photos
                                            WHERE ProductId = @ProductId";

                    using (var command = new SqlCommand(queryPhotos_, connection))
                    {
                        command.Parameters.AddWithValue("@ProductId", products[i].ProductId);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                products[i].Photos.Add(reader.GetString(reader.GetOrdinal("PhotoURL")));
                            }
                        }
                    }
                }

                for (int i = 0; i < products.Count; i++)
                {
                    List<ProductReviewModel> reviews = new List<ProductReviewModel>();

                    string query = @"
                        SELECT ReviewId, ProductId,  CompanyId, Rating, Review, CreatedAt
                        FROM Reviews
                        WHERE ProductId = @ProductId";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ProductId", products[i].ProductId);
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                reviews.Add(new ProductReviewModel
                                {
                                    ReviewId = reader.GetInt32(0),
                                    ProductId = reader.GetInt32(1),
                                    CompanyId = reader.GetInt32(2),
                                    Rating = reader.GetInt32(3),
                                    Review = reader.GetString(4),
                                    CreatedAt = reader.GetDateTime(5)
                                });
                            }

                        }
                    }
                    products[i].ProductReviewsModel = reviews;
                }

                connection.Close();
                return products;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                connection.Close();
                return products; // Optionally return an empty list on error
            }
            connection.Close();
        }




        // --------------------------------------------------------------------------------------------------------------------------
        public async Task<List<UserModel>> GetAllUsers(int? userId)
        {

            // Get All Users
            List<UserModel> AllUsers = new List<UserModel>();
            try
            {
                await connection.OpenAsync();

                int CompanyId = 0;

                string queryCompany = @"
                                    SELECT CompanyId
                                    FROM Companies
                                    WHERE UserId = @UserId";

                using (var commandCompany = new SqlCommand(queryCompany, connection))
                {
                    commandCompany.Parameters.AddWithValue("@UserId", userId);

                    using (var readerCompany = await commandCompany.ExecuteReaderAsync())
                    {
                        if (await readerCompany.ReadAsync())
                        {
                            CompanyId = readerCompany.GetInt32(readerCompany.GetOrdinal("CompanyId"));
                        }
                    }
                }




                List<int> PurchasedProductsUserIds = new List<int>();

                string queryPurchasedProductUserId = @"
                        SELECT UserId
                        FROM PurchasedProducts where CompanyId = @CompanyId;";

                using (SqlCommand cmdPR = new SqlCommand(queryPurchasedProductUserId, connection))
                {
                    cmdPR.Parameters.AddWithValue("@CompanyId", CompanyId);
                    using (SqlDataReader PRReader = await cmdPR.ExecuteReaderAsync())
                    {
                        while (await PRReader.ReadAsync())
                        {
                            int UserId = PRReader.GetInt32(PRReader.GetOrdinal("UserId"));
                            PurchasedProductsUserIds.Add(UserId);
                        }
                    }
                }




                string queryUsers = @"
                        SELECT 
                               u.UserId, u.UserName, u.UserSurName, u.PasswordHash, u.Email AS UserEmail, u.Role, 
                               u.Address AS UserAddress, u.PhoneNumber AS UserPhoneNumber, u.Age, u.BirthDate AS UserBirthDate, u.CreatedAt AS UserCreatedAt
                        FROM Users u where UserId = @UserId";

                for(int i=0;i< PurchasedProductsUserIds.Count; i++)
                {
                    using (SqlCommand commandUsers = new SqlCommand(queryUsers, connection))
                    {
                        commandUsers.Parameters.AddWithValue("@UserId", PurchasedProductsUserIds[i]);
                        using (SqlDataReader readerUsers = await commandUsers.ExecuteReaderAsync())
                        {
                            while (await readerUsers.ReadAsync())
                            {
                                UserModel user = new UserModel
                                {
                                    UserId = readerUsers.GetInt32(readerUsers.GetOrdinal("UserId")),
                                    Name = readerUsers.GetString(readerUsers.GetOrdinal("UserName")),
                                    SurName = readerUsers.GetString(readerUsers.GetOrdinal("UserSurName")),
                                    PasswordHash = readerUsers.GetString(readerUsers.GetOrdinal("PasswordHash")),
                                    Email = readerUsers.GetString(readerUsers.GetOrdinal("UserEmail")),
                                    Role = readerUsers.GetString(readerUsers.GetOrdinal("Role")),
                                    Address = readerUsers.GetString(readerUsers.GetOrdinal("UserAddress")),
                                    PhoneNumber = readerUsers.GetString(readerUsers.GetOrdinal("UserPhoneNumber")),
                                    Age = readerUsers.GetInt32(readerUsers.GetOrdinal("Age")),
                                    BirthDate = readerUsers.GetDateTime(readerUsers.GetOrdinal("UserBirthDate")),
                                    CreatedAt = readerUsers.GetDateTime(readerUsers.GetOrdinal("UserCreatedAt"))
                                };
                                AllUsers.Add(user);
                            }
                        }
                    }
                }
                connection.Close();
                return AllUsers;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                connection.Close();
                return AllUsers; // Optionally return an empty list on error
            }
            connection.Close();

        }


        // --------------------------------------------------------------------------------------------------------------------------
        public async Task<List<UserModel>> GetAllFollowers(int? userId)
        {
            // Get All Followers
            List<UserModel> AllFollowers = new List<UserModel>();
            try
            {
                await connection.OpenAsync();

                int CompanyId = 0;

                string queryCompany = @"
                                    SELECT CompanyId
                                    FROM Companies
                                    WHERE UserId = @UserId";

                using (var commandCompany = new SqlCommand(queryCompany, connection))
                {
                    commandCompany.Parameters.AddWithValue("@UserId", userId);

                    using (var readerCompany = await commandCompany.ExecuteReaderAsync())
                    {
                        if (await readerCompany.ReadAsync())
                        {
                            CompanyId = readerCompany.GetInt32(readerCompany.GetOrdinal("CompanyId"));
                        }
                    }
                }




                List<int> UserIds = new List<int>();

                string queryProductUserId = @"
                        SELECT UserId
                        FROM FollowedCompanies where CompanyId = @CompanyId;";

                using (SqlCommand cmdPR = new SqlCommand(queryProductUserId, connection))
                {
                    cmdPR.Parameters.AddWithValue("@CompanyId", CompanyId);
                    using (SqlDataReader PRReader = await cmdPR.ExecuteReaderAsync())
                    {
                        while (await PRReader.ReadAsync())
                        {
                            int UserId = PRReader.GetInt32(PRReader.GetOrdinal("UserId"));
                            UserIds.Add(UserId);
                        }
                    }
                }




                string queryUsers = @"
                        SELECT 
                               u.UserId, u.UserName, u.UserSurName, u.PasswordHash, u.Email AS UserEmail, u.Role, 
                               u.Address AS UserAddress, u.PhoneNumber AS UserPhoneNumber, u.Age, u.BirthDate AS UserBirthDate, u.CreatedAt AS UserCreatedAt
                        FROM Users u where UserId = @UserId";

                for (int i = 0; i < UserIds.Count; i++)
                {
                    using (SqlCommand commandUsers = new SqlCommand(queryUsers, connection))
                    {
                        commandUsers.Parameters.AddWithValue("@UserId", UserIds[i]);
                        using (SqlDataReader readerUsers = await commandUsers.ExecuteReaderAsync())
                        {
                            while (await readerUsers.ReadAsync())
                            {
                                UserModel user = new UserModel
                                {
                                    UserId = readerUsers.GetInt32(readerUsers.GetOrdinal("UserId")),
                                    Name = readerUsers.GetString(readerUsers.GetOrdinal("UserName")),
                                    SurName = readerUsers.GetString(readerUsers.GetOrdinal("UserSurName")),
                                    PasswordHash = readerUsers.GetString(readerUsers.GetOrdinal("PasswordHash")),
                                    Email = readerUsers.GetString(readerUsers.GetOrdinal("UserEmail")),
                                    Role = readerUsers.GetString(readerUsers.GetOrdinal("Role")),
                                    Address = readerUsers.GetString(readerUsers.GetOrdinal("UserAddress")),
                                    PhoneNumber = readerUsers.GetString(readerUsers.GetOrdinal("UserPhoneNumber")),
                                    Age = readerUsers.GetInt32(readerUsers.GetOrdinal("Age")),
                                    BirthDate = readerUsers.GetDateTime(readerUsers.GetOrdinal("UserBirthDate")),
                                    CreatedAt = readerUsers.GetDateTime(readerUsers.GetOrdinal("UserCreatedAt"))
                                };
                                AllFollowers.Add(user);
                            }
                        }
                    }
                }
                connection.Close();
                return AllFollowers;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                connection.Close();
                return AllFollowers; // Optionally return an empty list on error
            }
            connection.Close();

        }
        // --------------------------------------------------------------------------------------------------------------------------
        public async Task<VendorViewModel> GetVendor(int? userId)
        {
            VendorViewModel model = new VendorViewModel();
            try
            {
                await connection.OpenAsync();

               

                string query1 = @"
                SELECT CompanyId, Score, UserId, CompanyName, ContactName, Description, 
                    Address, PhoneNumber, Email, LogoUrl, BannerUrl, TaxIDNumber, IBAN, IsValidatedByAdmin, CreatedAt, BirthDate
                    FROM Companies where UserId = @UserId";


                using (SqlCommand command = new SqlCommand(query1, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            CompanyModel company = new CompanyModel
                            {
                                CompanyId = reader.GetInt32(reader.GetOrdinal("CompanyId")),
                                Score = reader.GetInt32(reader.GetOrdinal("Score")),
                                UserID = reader.GetInt32(reader.GetOrdinal("UserId")),
                                CompanyName = reader.GetString(reader.GetOrdinal("CompanyName")),
                                ContactName = reader.GetString(reader.GetOrdinal("ContactName")),
                                CompanyDescription = reader.GetString(reader.GetOrdinal("Description")),
                                CompanyAddress = reader.GetString(reader.GetOrdinal("Address")),
                                CompanyPhoneNumber = reader.GetString(reader.GetOrdinal("PhoneNumber")),
                                Email = reader.GetString(reader.GetOrdinal("Email")),
                                LogoUrl = reader.GetString(reader.GetOrdinal("LogoUrl")),
                                BannerUrl = reader.GetString(reader.GetOrdinal("BannerUrl")),
                                taxIDNumber = reader.GetString(reader.GetOrdinal("TaxIDNumber")),
                                IBAN = reader.GetString(reader.GetOrdinal("IBAN")),
                                isValidatedbyAdmin = reader.GetBoolean(reader.GetOrdinal("IsValidatedByAdmin")),
                                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                                BirthDate = reader.GetDateTime(reader.GetOrdinal("BirthDate"))
                            };
                            model.VendorCompanyInfos = company;
                        }
                    }
                }


                string queryUsers = @"
                SELECT 
                        u.UserId, u.UserName, u.UserSurName, u.PasswordHash, u.Email AS UserEmail, u.Role, 
                        u.Address AS UserAddress, u.PhoneNumber AS UserPhoneNumber, u.Age, u.BirthDate AS UserBirthDate, u.CreatedAt AS UserCreatedAt
                FROM Users u where UserId = @UserId";

                using (SqlCommand commandUsers = new SqlCommand(queryUsers, connection))
                {
                    commandUsers.Parameters.AddWithValue("@UserId", userId);
                    using (SqlDataReader readerUsers = await commandUsers.ExecuteReaderAsync())
                    {
                        if (await readerUsers.ReadAsync())
                        {

                            UserModel user = new UserModel
                            {
                                UserId = readerUsers.GetInt32(readerUsers.GetOrdinal("UserId")),
                                Name = readerUsers.GetString(readerUsers.GetOrdinal("UserName")),
                                SurName = readerUsers.GetString(readerUsers.GetOrdinal("UserSurName")),
                                PasswordHash = readerUsers.GetString(readerUsers.GetOrdinal("PasswordHash")),
                                Email = readerUsers.GetString(readerUsers.GetOrdinal("UserEmail")),
                                Role = readerUsers.GetString(readerUsers.GetOrdinal("Role")),
                                Address = readerUsers.GetString(readerUsers.GetOrdinal("UserAddress")),
                                PhoneNumber = readerUsers.GetString(readerUsers.GetOrdinal("UserPhoneNumber")),
                                Age = readerUsers.GetInt32(readerUsers.GetOrdinal("Age")),
                                BirthDate = readerUsers.GetDateTime(readerUsers.GetOrdinal("UserBirthDate")),
                                CreatedAt = readerUsers.GetDateTime(readerUsers.GetOrdinal("UserCreatedAt"))
                            };
                            model.VendorUserInfos = user;
                        }

                    }
                }

                connection.Close();
                return model;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                connection.Close();
                return model; // Optionally return an empty list on error
            }
            connection.Close();

        }



        // --------------------------------------------------------------------------------------------------------------------------
        public async Task<VendorViewModel> GetVendorDashBoard(int? userId)
        {
            VendorViewModel model = new VendorViewModel();

            await connection.OpenAsync();

            try
            {

                int CompanyId = 0;

                string queryCompany = @"
                                    SELECT CompanyId
                                    FROM Companies
                                    WHERE UserId = @UserId";

                using (var commandCompany = new SqlCommand(queryCompany, connection))
                {
                    commandCompany.Parameters.AddWithValue("@UserId", userId);

                    using (var readerCompany = await commandCompany.ExecuteReaderAsync())
                    {
                        if (await readerCompany.ReadAsync())
                        {
                            CompanyId = readerCompany.GetInt32(readerCompany.GetOrdinal("CompanyId"));
                        }
                    }
                }




                // Get All Users
                List<UserModel> AllFollowers = new List<UserModel>();
                string queryUsers = @"
                    SELECT 
                           u.UserId, u.UserName, u.UserSurName, u.PasswordHash, u.Email AS UserEmail, u.Role, 
                           u.Address AS UserAddress, u.PhoneNumber AS UserPhoneNumber, u.Age, u.BirthDate AS UserBirthDate, u.CreatedAt AS UserCreatedAt
                    FROM Users u join FollowedCompanies fc on u.UserId = fc.UserId where fc.CompanyId = @CompanyId";


                using (SqlCommand commandUsers = new SqlCommand(queryUsers, connection))
                {
                    commandUsers.Parameters.AddWithValue("@CompanyId", CompanyId);
                    using (SqlDataReader readerUsers = await commandUsers.ExecuteReaderAsync())
                    {
                        while (await readerUsers.ReadAsync())
                        {
                            UserModel user = new UserModel
                            {
                                UserId = readerUsers.GetInt32(readerUsers.GetOrdinal("UserId")),
                                Name = readerUsers.GetString(readerUsers.GetOrdinal("UserName")),
                                SurName = readerUsers.GetString(readerUsers.GetOrdinal("UserSurName")),
                                PasswordHash = readerUsers.GetString(readerUsers.GetOrdinal("PasswordHash")),
                                Email = readerUsers.GetString(readerUsers.GetOrdinal("UserEmail")),
                                Role = readerUsers.GetString(readerUsers.GetOrdinal("Role")),
                                Address = readerUsers.GetString(readerUsers.GetOrdinal("UserAddress")),
                                PhoneNumber = readerUsers.GetString(readerUsers.GetOrdinal("UserPhoneNumber")),
                                Age = readerUsers.GetInt32(readerUsers.GetOrdinal("Age")),
                                BirthDate = readerUsers.GetDateTime(readerUsers.GetOrdinal("UserBirthDate")),
                                CreatedAt = readerUsers.GetDateTime(readerUsers.GetOrdinal("UserCreatedAt"))
                            };
                            AllFollowers.Add(user);
                        }
                    }
                }
                model.AllFollowers = AllFollowers.OrderBy(p => p.CreatedAt).ToList();



                // onnline productss
                var products = new List<ProductModel>();

                var queryProducts = @"
                    SELECT * FROM Products
                    WHERE status = 'Online' and CompanyID = @CompanyId";

                using (SqlCommand cmd = new SqlCommand(queryProducts, connection))
                {
                    cmd.Parameters.AddWithValue("@CompanyId", CompanyId);
                    using (SqlDataReader detailsReader = await cmd.ExecuteReaderAsync())
                    {
                        while (await detailsReader.ReadAsync())
                        {
                            var product_ = new ProductModel();
                            product_.ProductId = detailsReader.GetInt32(detailsReader.GetOrdinal("ProductId"));
                            product_.Rating = detailsReader.GetInt32(detailsReader.GetOrdinal("Rating"));
                            product_.Favorites = detailsReader.GetInt32(detailsReader.GetOrdinal("Favorites"));
                            product_.CompanyID = detailsReader.GetInt32(detailsReader.GetOrdinal("CompanyID"));
                            product_.Stock = detailsReader.GetInt32(detailsReader.GetOrdinal("Stock"));
                            product_.Price = detailsReader.GetDecimal(detailsReader.GetOrdinal("Price"));
                            product_.ProductName = detailsReader.GetString(detailsReader.GetOrdinal("ProductName"));
                            product_.Description = detailsReader.GetString(detailsReader.GetOrdinal("Description"));
                            product_.Category = detailsReader.GetString(detailsReader.GetOrdinal("Category"));
                            product_.Type = detailsReader.GetString(detailsReader.GetOrdinal("Type"));
                            product_.Status = detailsReader.GetString(detailsReader.GetOrdinal("Status"));
                            product_.CreatedAt = detailsReader.GetDateTime(detailsReader.GetOrdinal("CreatedAt"));
                            product_.Clicked = detailsReader.GetInt32(detailsReader.GetOrdinal("Clicked"));
                            product_.Sold = detailsReader.GetInt32(detailsReader.GetOrdinal("Sold"));
                            product_.Photos = new List<string>();

                            model.TotalSold += product_.Sold;
                            model.TotalFavorites += product_.Favorites;
                            model.TotalClicks += product_.Clicked;
                            model.TotalRevenue += product_.Sold * product_.Price;
                            products.Add(product_);
                        }
                    }
                }

                model.OnlineProducts = products;


                // offline products
                products = new List<ProductModel>();
                queryProducts = @"
                    SELECT * FROM Products
                    WHERE status = 'Offline' and CompanyID = @CompanyId";

                using (SqlCommand cmd = new SqlCommand(queryProducts, connection))
                {
                    cmd.Parameters.AddWithValue("@CompanyId", CompanyId);
                    using (SqlDataReader detailsReader = await cmd.ExecuteReaderAsync())
                    {
                        while (await detailsReader.ReadAsync())
                        {
                            var product_ = new ProductModel();
                            product_.ProductId = detailsReader.GetInt32(detailsReader.GetOrdinal("ProductId"));
                            product_.Rating = detailsReader.GetInt32(detailsReader.GetOrdinal("Rating"));
                            product_.Favorites = detailsReader.GetInt32(detailsReader.GetOrdinal("Favorites"));
                            product_.CompanyID = detailsReader.GetInt32(detailsReader.GetOrdinal("CompanyID"));
                            product_.Stock = detailsReader.GetInt32(detailsReader.GetOrdinal("Stock"));
                            product_.Price = detailsReader.GetDecimal(detailsReader.GetOrdinal("Price"));
                            product_.ProductName = detailsReader.GetString(detailsReader.GetOrdinal("ProductName"));
                            product_.Description = detailsReader.GetString(detailsReader.GetOrdinal("Description"));
                            product_.Category = detailsReader.GetString(detailsReader.GetOrdinal("Category"));
                            product_.Type = detailsReader.GetString(detailsReader.GetOrdinal("Type"));
                            product_.Status = detailsReader.GetString(detailsReader.GetOrdinal("Status"));
                            product_.CreatedAt = detailsReader.GetDateTime(detailsReader.GetOrdinal("CreatedAt"));
                            product_.Clicked = detailsReader.GetInt32(detailsReader.GetOrdinal("Clicked"));
                            product_.Sold = detailsReader.GetInt32(detailsReader.GetOrdinal("Sold"));
                            product_.Photos = new List<string>();

                            model.TotalSold += product_.Sold;
                            model.TotalFavorites += product_.Favorites;
                            model.TotalClicks += product_.Clicked;
                            model.TotalRevenue += product_.Sold * product_.Price;
                            products.Add(product_);
                        }
                    }
                }
                model.OfflineProducts = products;

                // all products
                products = new List<ProductModel>();
                queryProducts = @"
                    SELECT * FROM Products
                    WHERE CompanyID = @CompanyId";

                using (SqlCommand cmd = new SqlCommand(queryProducts, connection))
                {
                    cmd.Parameters.AddWithValue("@CompanyId", CompanyId);
                    using (SqlDataReader detailsReader = await cmd.ExecuteReaderAsync())
                    {
                        while (await detailsReader.ReadAsync())
                        {
                            var product_ = new ProductModel();
                            product_.ProductId = detailsReader.GetInt32(detailsReader.GetOrdinal("ProductId"));
                            product_.Rating = detailsReader.GetInt32(detailsReader.GetOrdinal("Rating"));
                            product_.Favorites = detailsReader.GetInt32(detailsReader.GetOrdinal("Favorites"));
                            product_.CompanyID = detailsReader.GetInt32(detailsReader.GetOrdinal("CompanyID"));
                            product_.Stock = detailsReader.GetInt32(detailsReader.GetOrdinal("Stock"));
                            product_.Price = detailsReader.GetDecimal(detailsReader.GetOrdinal("Price"));
                            product_.ProductName = detailsReader.GetString(detailsReader.GetOrdinal("ProductName"));
                            product_.Description = detailsReader.GetString(detailsReader.GetOrdinal("Description"));
                            product_.Category = detailsReader.GetString(detailsReader.GetOrdinal("Category"));
                            product_.Type = detailsReader.GetString(detailsReader.GetOrdinal("Type"));
                            product_.Status = detailsReader.GetString(detailsReader.GetOrdinal("Status"));
                            product_.CreatedAt = detailsReader.GetDateTime(detailsReader.GetOrdinal("CreatedAt"));
                            product_.Clicked = detailsReader.GetInt32(detailsReader.GetOrdinal("Clicked"));
                            product_.Sold = detailsReader.GetInt32(detailsReader.GetOrdinal("Sold"));
                            product_.Photos = new List<string>();

                            model.TotalSold += product_.Sold;
                            model.TotalFavorites += product_.Favorites;
                            model.TotalClicks += product_.Clicked;
                            model.TotalRevenue += product_.Sold * product_.Price;
                            products.Add(product_);
                        }
                    }
                }
                model.AllProducts = products;



                // Kategori ve türlere göre ürün sayýsýný hesaplamak için bir sözlük
                var categoryTypeCount = new Dictionary<string, Dictionary<string, int>>();


                foreach (var product in model.AllProducts)
                {
                    // Eðer category veya type alaný null ya da boþ ise bu durumu loglayýn
                    if (string.IsNullOrWhiteSpace(product.Category) || string.IsNullOrWhiteSpace(product.Type))
                    {
                        continue; // Null veya boþ deðerler için iþlemi atla
                    }

                    if (!categoryTypeCount.ContainsKey(product.Category))
                    {
                        categoryTypeCount[product.Category] = new Dictionary<string, int>();
                    }

                    if (!categoryTypeCount[product.Category].ContainsKey(product.Type))
                    {
                        categoryTypeCount[product.Category][product.Type] = 0;
                    }
                    categoryTypeCount[product.Category][product.Type]++;
                }

                model.categoryTypeCount = categoryTypeCount;

                // Get All Products Reviews
                List<ProductReviewModel> reviews = new List<ProductReviewModel>();

                string queryReviews = @"
                    SELECT ReviewId, ProductId,  CompanyId, Rating, Review, CreatedAt
                    FROM Reviews WHERE CompanyId = @CompanyId;";

                using (var commandReviews = new SqlCommand(queryReviews, connection))
                {
                    commandReviews.Parameters.AddWithValue("@CompanyId", CompanyId);
                    using (SqlDataReader readerReviews = await commandReviews.ExecuteReaderAsync())
                    {
                        while (await readerReviews.ReadAsync())
                        {
                            reviews.Add(new ProductReviewModel
                            {
                                ReviewId = readerReviews.GetInt32(0),
                                ProductId = readerReviews.GetInt32(1),
                                CompanyId = readerReviews.GetInt32(2),
                                Rating = readerReviews.GetInt32(3),
                                Review = readerReviews.GetString(4),
                                CreatedAt = readerReviews.GetDateTime(5)
                            });
                        }

                    }
                }
                model.ProductsReviews = reviews;
                model.TotalReviews = reviews.Count;



              
                // Get All PurchasedProducts
                List<PurchasedProductModel> PurchasedProducts = new List<PurchasedProductModel>();
                List<int> PurchasedProductsIds = new List<int>();

                string queryPurchasedProductsId = @"
                    SELECT ProductId
                    FROM PurchasedProducts WHERE CompanyId = @CompanyId;";

                using (SqlCommand cmdPR = new SqlCommand(queryPurchasedProductsId, connection))
                {
                    cmdPR.Parameters.AddWithValue("@CompanyId", CompanyId);
                    using (SqlDataReader PRReader = await cmdPR.ExecuteReaderAsync())
                    {
                        while (await PRReader.ReadAsync())
                        {
                            int productId = PRReader.GetInt32(PRReader.GetOrdinal("ProductId"));
                            PurchasedProductsIds.Add(productId);
                        }
                    }
                }

                for (int i = 0; i < PurchasedProductsIds.Count; i++)
                {
                    string queryPurchasedProducts = @"
                            SELECT p.*, pp.PurchasedDate
                            FROM Products p
                            JOIN PurchasedProducts pp ON p.ProductId = pp.ProductId
                            WHERE pp.ProductId = @ProductId;";

                    using (SqlCommand cmdPR = new SqlCommand(queryPurchasedProducts, connection))
                    {
                        cmdPR.Parameters.AddWithValue("@ProductId", PurchasedProductsIds[i]);

                        using (SqlDataReader PRReader = await cmdPR.ExecuteReaderAsync())
                        {
                            while (await PRReader.ReadAsync())
                            {
                                var productPR = new PurchasedProductModel
                                {
                                    ProductId = PRReader.GetInt32(PRReader.GetOrdinal("ProductId")),
                                    Rating = PRReader.GetInt32(PRReader.GetOrdinal("Rating")),
                                    Favorites = PRReader.GetInt32(PRReader.GetOrdinal("Favorites")),
                                    CompanyID = PRReader.GetInt32(PRReader.GetOrdinal("CompanyID")),
                                    Stock = PRReader.GetInt32(PRReader.GetOrdinal("Stock")),
                                    Price = PRReader.GetDecimal(PRReader.GetOrdinal("Price")),
                                    ProductName = PRReader.GetString(PRReader.GetOrdinal("ProductName")),
                                    Description = PRReader.GetString(PRReader.GetOrdinal("Description")),
                                    Category = PRReader.GetString(PRReader.GetOrdinal("Category")),
                                    Status = PRReader.GetString(PRReader.GetOrdinal("Status")),
                                    CreatedAt = PRReader.GetDateTime(PRReader.GetOrdinal("CreatedAt")),
                                    Clicked = PRReader.GetInt32(PRReader.GetOrdinal("Clicked")),
                                    Sold = PRReader.GetInt32(PRReader.GetOrdinal("Sold")),
                                    PurchasedDate = PRReader.GetDateTime(PRReader.GetOrdinal("PurchasedDate")),
                                    Photos = new List<string>() // Photos listesine nasýl veri ekleneceði daha sonra belirlenmelidir
                                };
                                PurchasedProducts.Add(productPR);
                            }
                        }
                    }
                }
                model.PurchasedProducts = PurchasedProducts.OrderBy(p => p.PurchasedDate).ToList();


                await connection.CloseAsync();
                return model;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to retrieve Dashboard: {ex.Message}");
                await connection.CloseAsync();
                return model;
                throw;
            }
        }

        // --------------------------------------------------------------------------------------------------------------------------
        public async Task<bool> VendorUpdateProduct(int? userId, ProductModel product, List<IFormFile> Photos, int productId)
        {
            bool isUpdated = false;

            try
            {
                connection.Open();

                // Retrieve the existing product details
                string query = "SELECT * FROM Products WHERE ProductId = @ProductId";
                ProductModel existingProduct = null;

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ProductId", productId);

                    using (var PRReader = await command.ExecuteReaderAsync())
                    {
                        if (await PRReader.ReadAsync())
                        {
                            existingProduct = new ProductModel
                            {
                                ProductId = PRReader.GetInt32(PRReader.GetOrdinal("ProductId")),
                                Rating = PRReader.GetInt32(PRReader.GetOrdinal("Rating")),
                                Favorites = PRReader.GetInt32(PRReader.GetOrdinal("Favorites")),
                                CompanyID = PRReader.GetInt32(PRReader.GetOrdinal("CompanyID")),
                                Stock = PRReader.GetInt32(PRReader.GetOrdinal("Stock")),
                                Price = PRReader.GetDecimal(PRReader.GetOrdinal("Price")),
                                ProductName = PRReader.GetString(PRReader.GetOrdinal("ProductName")),
                                Description = PRReader.GetString(PRReader.GetOrdinal("Description")),
                                Category = PRReader.GetString(PRReader.GetOrdinal("Category")),
                                Status = PRReader.GetString(PRReader.GetOrdinal("Status")),
                                CreatedAt = PRReader.GetDateTime(PRReader.GetOrdinal("CreatedAt")),
                                Clicked = PRReader.GetInt32(PRReader.GetOrdinal("Clicked")),
                                Sold = PRReader.GetInt32(PRReader.GetOrdinal("Sold"))
                            };
                        }
                    }
                }

                if (existingProduct == null)
                {
                    connection.Close();
                    throw new Exception("Product not found");
                }

                // Güncelleme iþlemi
                string updateSql = @"UPDATE Products
                             SET Stock = @Stock,
                                 Price = @Price,
                                 ProductName = @ProductName,
                                 Description = @Description,
                                 Category = @Category,
                                 Status = @Status,
                                 CreatedAt = @CreatedAt
                             WHERE ProductId = @ProductId";

                using (var cmd = new SqlCommand(updateSql, connection))
                {
                    cmd.Parameters.AddWithValue("@ProductId", productId);
                    cmd.Parameters.AddWithValue("@Stock", product.Stock != 0 ? product.Stock : existingProduct.Stock);
                    cmd.Parameters.AddWithValue("@Price", product.Price != 0 ? product.Price : existingProduct.Price);

                    cmd.Parameters.AddWithValue("@ProductName", product.ProductName ?? existingProduct.ProductName);
                    cmd.Parameters.AddWithValue("@Description", product.Description ?? existingProduct.Description);
                    cmd.Parameters.AddWithValue("@Category", product.Category ?? existingProduct.Category);
                    cmd.Parameters.AddWithValue("@Status", product.Status ?? existingProduct.Status);
                    cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

                    int rowsAffected = await cmd.ExecuteNonQueryAsync();
                    isUpdated = rowsAffected > 0;
                }
                
                // Fotoðraflarý güncelleme iþlemi
                if (isUpdated && Photos != null && Photos.Count > 0)
                {
                    // Eski fotoðraflarý silin
                    string deletePhotosSql = @"DELETE FROM Photos WHERE ProductId = @ProductId";
                    using (var deleteCmd = new SqlCommand(deletePhotosSql, connection))
                    {
                        deleteCmd.Parameters.AddWithValue("@ProductId", productId);
                        await deleteCmd.ExecuteNonQueryAsync();
                    }

                    foreach (var photoName in Photos)
                    {
                        string categoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Pictures", product.Category ?? existingProduct.Category);
                        if (!Directory.Exists(categoryPath)) Directory.CreateDirectory(categoryPath);

                        string uniqueFileName = $"{Guid.NewGuid().ToString()}.jpg";
                        string filePath = Path.Combine(categoryPath, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await photoName.CopyToAsync(fileStream);
                        }

                        string filePathtoDB = Path.Combine("/Pictures", product.Category ?? existingProduct.Category, uniqueFileName);

                        string insertPhotoSql = @"INSERT INTO Photos (ProductId, PhotoURL)
                                          VALUES (@ProductId, @PhotoURL)";
                        using (var insertCmd = new SqlCommand(insertPhotoSql, connection))
                        {
                            insertCmd.Parameters.AddWithValue("@ProductId", productId);
                            insertCmd.Parameters.AddWithValue("@PhotoURL", filePathtoDB);

                            await insertCmd.ExecuteNonQueryAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating product: " + ex.Message);
                throw;
            }
            finally
            {
                connection.Close();
            }

            return isUpdated;
        }




    }
}
