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

        public VendorDbFunctions()
        {
            string connection_String = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Casper\Documents\OnShopDB.mdf;Integrated Security=True;Connect Timeout=30";
            connection = new SqlConnection(connection_String);
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
                    WHERE status = 'Online' and CompanyId = @CompanyId";

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
    }
}
