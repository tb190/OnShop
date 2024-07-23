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
using System.Security.Cryptography;
using System.Text;

namespace OnShop
{
    public class UserDbFunctions
    {
        private SqlConnection connection;
        string connection_String;

        public UserDbFunctions()
        {
            connection_String = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Casper\Documents\OnShopDB.mdf;Integrated Security=True;Connect Timeout=30";
            connection = new SqlConnection(connection_String);
        }

        // --------------------------------------------------------------------------------------------------------------------------
        public async Task<bool> RegisterIndividual(UserModel user,string role)
        {
            try
            {
                string hashedPassword = HashPassword(user.PasswordHash);

                string query = "INSERT INTO Users (UserName, UserSurName, Email, PasswordHash, Address, Age, PhoneNumber, Role, BirthDate) VALUES (@Name, @SurName, @Email, @PasswordHash, @Address, @Age, @PhoneNumber, @Role, @Birthdate)";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Name", user.Name);
                    command.Parameters.AddWithValue("@SurName", user.SurName);
                    command.Parameters.AddWithValue("@Email", user.Email);
                    command.Parameters.AddWithValue("@PasswordHash", hashedPassword); 
                    command.Parameters.AddWithValue("@Role", role);
                    command.Parameters.AddWithValue("@Address", string.IsNullOrEmpty(user.Address) ? string.Empty : user.Address);
                    command.Parameters.AddWithValue("@PhoneNumber", string.IsNullOrEmpty(user.PhoneNumber) ? string.Empty : user.PhoneNumber);

                    /*
                    // Birthdate kontrol�
                    if (user.BirthDate < new DateTime(1753, 1, 1) || user.BirthDate > new DateTime(9999, 12, 31))
                    {
                        throw new ArgumentOutOfRangeException("BirthDate", "BirthDate must be between 1/1/1753 and 12/31/9999.");
                    }
                    command.Parameters.AddWithValue("@BirthDate", user.BirthDate);

                    // Age hesaplamas�
                    int age = DateTime.Today.Year - user.BirthDate.Year;
                    command.Parameters.AddWithValue("@Age", age);

                */
                    command.Parameters.AddWithValue("@BirthDate", DateTime.Now);
                    command.Parameters.AddWithValue("@Age", 0);

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                    connection.Close();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to register user: {ex.Message}");
                return false;
                throw;
            }
        }

        // --------------------------------------------------------------------------------------------------------------------------
        public async Task<bool> RegisterCompany(IFormFile LogoUrl, IFormFile BannerUrl, CompanyModel company, UserModel user)
        {
            try
            {
                if(await RegisterIndividual(user, "Vendor"))
                {
                    int userId = await GetUserIdByEmail(user.Email);



                    string query = "INSERT INTO Companies (Score, UserId, CompanyName, ContactName, Description, Address, PhoneNumber, CreatedAt, BirthDate, Email, LogoUrl, BannerUrl, TaxIDNumber, IBAN, IsValidatedByAdmin) " +
                                             "VALUES (@Score, @UserId, @CompanyName, @ContactName, @Description, @Address, @PhoneNumber, @CreatedAt, @BirthDate, @Email, @LogoUrl, @BannerUrl, @TaxIDNumber, @IBAN, @IsValidatedByAdmin)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Score", 0);
                        command.Parameters.AddWithValue("@UserId", userId);
                        command.Parameters.AddWithValue("@CompanyName", company.CompanyName);
                        command.Parameters.AddWithValue("@ContactName", user.Name);
                        command.Parameters.AddWithValue("@Description", company.CompanyDescription);
                        command.Parameters.AddWithValue("@Address", user.Address);
                        command.Parameters.AddWithValue("PhoneNumber", user.PhoneNumber);
                        command.Parameters.AddWithValue("CreatedAt", DateTime.Now);
                        command.Parameters.AddWithValue("BirthDate", DateTime.Now);
                        command.Parameters.AddWithValue("Email", user.Email);
                        command.Parameters.AddWithValue("TaxIDNumber", company.taxIDNumber);
                        command.Parameters.AddWithValue("IBAN", company.IBAN);
                        command.Parameters.AddWithValue("IsValidatedByAdmin", 0);


                        // Logo kaydetme
                        if (LogoUrl != null && LogoUrl.Length > 0)
                        {
                            string logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Pictures", "CompanyLogos");
                            if (!Directory.Exists(logoPath)) Directory.CreateDirectory(logoPath);

                            string uniqueFileNameLogo = $"{Guid.NewGuid().ToString()}.jpg";
                            string filePathLogo = Path.Combine(logoPath, uniqueFileNameLogo);
                            using (var fileStream = new FileStream(filePathLogo, FileMode.Create))
                            {
                                await LogoUrl.CopyToAsync(fileStream);
                            }

                            string filePathtoDBLogo = Path.Combine("/Pictures", "CompanyLogos", uniqueFileNameLogo);
                            command.Parameters.AddWithValue("@LogoUrl", filePathtoDBLogo);
                        }
                        // Banner kaydetme
                        if (BannerUrl != null && BannerUrl.Length > 0)
                        {
                            string bannerPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Pictures", "CompanyBanners");
                            if (!Directory.Exists(bannerPath)) Directory.CreateDirectory(bannerPath);

                            string uniqueFileNameBanner = $"{Guid.NewGuid().ToString()}.jpg";
                            string filePathBanner = Path.Combine(bannerPath, uniqueFileNameBanner);
                            using (var fileStream = new FileStream(filePathBanner, FileMode.Create))
                            {
                                await BannerUrl.CopyToAsync(fileStream);
                            }

                            string filePathtoDBBanner = Path.Combine("/Pictures", "CompanyBanners", uniqueFileNameBanner);
                            command.Parameters.AddWithValue("@BannerUrl", filePathtoDBBanner);
                        }

                        await connection.OpenAsync();
                        await command.ExecuteNonQueryAsync();
                        connection.Close();
                        return true;
                    }

                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to register company: {ex.Message}");
                return false;
                throw;
            }
            
        }


        // --------------------------------------------------------------------------------------------------------------------------
        public async Task<(string Message, string Role)> ValidateUserCredentials(string email, string password)
        {
            try
            {
                string hashedPassword = HashPassword(password);
                await connection.OpenAsync();
                string query = @"
                        SELECT Users.PasswordHash, Users.Role, Companies.IsValidatedByAdmin
                        FROM Users
                        LEFT JOIN Companies ON Users.Email = Companies.Email
                        WHERE Users.Email = @Email";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);

                    
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string storedHash = reader["PasswordHash"].ToString();
                            string role = reader["Role"].ToString();
                            bool? isValidatedByAdmin = reader["IsValidatedByAdmin"] as bool?;

                            
                            if (storedHash.Equals(hashedPassword))
                            {                              
                                if (role == "Vendor" && (isValidatedByAdmin == false))
                                {                                   
                                    connection.Close();
                                    return ("Your account has not been validated by the admin.",role);
                                }
                                connection.Close();
                                return ("User validated successfully.",role);
                            }
                        }
                        
                    }
                }
                connection.Close();
                return ("Invalid email or password.","empty");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to validate user credentials: {ex.Message}");
                throw;
            }
        }

        // --------------------------------------------------------------------------------------------------------------------------
        public async Task<int> GetUserIdByEmail(string email)
        {
            try
            {
                string query = "SELECT UserId FROM Users WHERE Email = @Email";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);

                    await connection.OpenAsync();
                    var result = await command.ExecuteScalarAsync();
                    connection.Close();

                    if (result != null && int.TryParse(result.ToString(), out int userId))
                    {
                        return userId;
                    }
                    else
                    {
                        return -1;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to get user ID: {ex.Message}");
                throw;
            }
        }
        // --------------------------------------------------------------------------------------------------------------------------
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }


        // --------------------------------------------------------------------------------------------------------------------------
        public async Task<ProductViewModel> UserGetProductDetails(int productId)
        {
            ProductModel product = null;
            CompanyModel company = null;
            UserModel user = null;
            List<ProductReviewModel> reviews = null;

            int CompanyId;
            int UserId;

            try
            {

                //  Product detaylar� i�in query 
                await connection.OpenAsync();

                // Update Clicked field
                string updateQuery = @"
                UPDATE Products
                SET Clicked = Clicked + 1
                WHERE ProductId = @ProductId";

                using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection))
                {
                    updateCommand.Parameters.AddWithValue("@ProductId", productId);
                    updateCommand.ExecuteNonQuery();
                }

                string query = @"
                SELECT 
                    ProductId,
                    Rating,
                    Favorites,
                    CompanyID,
                    Stock,
                    Clicked,
                    Price,
                    ProductName,
                    Description,
                    Category,
                    Status,
                    CreatedAt
                FROM Products
                WHERE ProductId = @ProductId";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ProductId", productId);
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            product = new ProductModel
                            {
                                ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                                Rating = reader.GetInt32(reader.GetOrdinal("Rating")),
                                Favorites = reader.GetInt32(reader.GetOrdinal("Favorites")),
                                CompanyID = reader.GetInt32(reader.GetOrdinal("CompanyID")),
                                Stock = reader.GetInt32(reader.GetOrdinal("Stock")),
                                Clicked = reader.GetInt32(reader.GetOrdinal("Clicked")),
                                Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                                ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
                                Description = reader.GetString(reader.GetOrdinal("Description")),
                                Category = reader.GetString(reader.GetOrdinal("Category")),
                                Status = reader.GetString(reader.GetOrdinal("Status")),
                                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                                ProductReviewsID = new List<int>(), // Separate query needed
                                ProductReviews = new List<string>(), // Separate query needed
                                Photos = new List<string>() // Separate query needed
                            };  
                        }
                    }
                }
                
                CompanyId = product.CompanyID;

                           
                if (product == null)
                {
                    Console.WriteLine("Product is null");
                    throw new NullReferenceException("Product not found");
                    
                }


                //  Company detaylar� i�in query 
                string query1 = @"
                SELECT CompanyId, Score, UserId, CompanyName, ContactName, Description, 
                      Address, PhoneNumber, Email, LogoUrl, BannerUrl, TaxIDNumber, IBAN, IsValidatedByAdmin, CreatedAt, BirthDate
                        FROM Companies where CompanyId = @CompanyId";

                       
                using (SqlCommand command = new SqlCommand(query1, connection))
                {
        
                    command.Parameters.AddWithValue("@CompanyId", CompanyId);
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {                      
                        if (await reader.ReadAsync())
                        {
                            company = new CompanyModel
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
                        }
                    }                
                }

                
                UserId = company.UserID;

             
                if (company == null)
                {
                    Console.WriteLine("Company is null");
                    throw new NullReferenceException("Company not found");
                    
                }


                //  User detaylar� i�in query 
                
                query = @"
                SELECT UserId,UserName, UserSurName, PasswordHash, Email, Role, Address, PhoneNumber, Age, BirthDate , CreatedAt
                       FROM Users where Users.UserId = @UserId";


                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserId", UserId);
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            user = new UserModel
                            {
                                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                                Name = reader.GetString(reader.GetOrdinal("UserName")),
                                SurName = reader.GetString(reader.GetOrdinal("UserSurName")),
                                PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                                Email = reader.GetString(reader.GetOrdinal("Email")),
                                Role = reader.GetString(reader.GetOrdinal("Role")),
                                Address = reader.GetString(reader.GetOrdinal("Address")),
                                PhoneNumber = reader.GetString(reader.GetOrdinal("PhoneNumber")),
                                Age = reader.GetInt32(reader.GetOrdinal("Age")),
                                BirthDate = reader.GetDateTime(reader.GetOrdinal("BirthDate")),
                                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
                            };

                        }
                    }

                }

               
                if (user == null)
                {
                    Console.WriteLine("User is null");
                    throw new NullReferenceException("User not found");
                    
                }



                // Product Reviews i�in
                reviews = new List<ProductReviewModel>();
                
                query = @"
                    SELECT ReviewId, ProductId,  CompanyId, Rating, Review, CreatedAt
                    FROM Reviews
                    WHERE ProductId = @ProductId";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ProductId", productId);
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
                product.ProductReviewsID = reviews.Select(r => r.ReviewId).ToList();
                product.ProductReviews = reviews.Select(r => r.Review).ToList();
               
                if (reviews == null)
                {
                    throw new NullReferenceException("Reviews not found");
                    Console.WriteLine("Reviews is null");
                }

                // Product Photos i�in
                
                query = @"
                    SELECT PhotoURL
                    FROM Photos
                    WHERE ProductId = @ProductId";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ProductId", productId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            product.Photos.Add(reader.GetString(reader.GetOrdinal("PhotoURL")));
                        }
                    }
                }
                await connection.CloseAsync();

                if (product.Photos == null)
                {
                    throw new NullReferenceException("Photos not found");
                }


                if (product == null) Console.WriteLine("Product is null");
                if (company == null) Console.WriteLine("Company is null");
                if (user == null) Console.WriteLine("User is null");
                if (product.Photos == null) Console.WriteLine("Photos is null");

                ProductViewModel productViewModel = new ProductViewModel
                {
                    Company = company,
                    User = user,
                    Product = product,
                    ProductReviews = reviews
                };

                return productViewModel;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);
                connection.Close();
                throw;
            } 
        }

        // --------------------------------------------------------------------------------------------------------------------------
        public async Task<bool> ProductAddToCartDb(int ProductId, int CompanyId, int? UserId, int Quantity)
        {
            var queryCheck = "SELECT Count FROM BasketProducts WHERE UserId = @UserId AND ProductId = @ProductId AND CompanyId = @CompanyId";
            var queryUpdate = "UPDATE BasketProducts SET Count = Count + @Quantity WHERE UserId = @UserId AND ProductId = @ProductId AND CompanyId = @CompanyId";
            var queryInsert = "INSERT INTO BasketProducts (UserId, ProductId, CompanyId, Count) VALUES (@UserId, @ProductId, @CompanyId, @Quantity)";

            try
            {
                await connection.OpenAsync();

                // İlk olarak mevcut ürünü kontrol edelim
                using (SqlCommand command = new SqlCommand(queryCheck, connection))
                {
                    command.Parameters.AddWithValue("@UserId", UserId);
                    command.Parameters.AddWithValue("@ProductId", ProductId);
                    command.Parameters.AddWithValue("@CompanyId", CompanyId);

                    var result = await command.ExecuteScalarAsync();

                    if (result != null) // Ürün mevcutsa
                    {
                        // Count değerini artır
                        command.CommandText = queryUpdate;
                        command.Parameters.AddWithValue("@Quantity", Quantity);
                        await command.ExecuteNonQueryAsync();
                    }
                    else // Ürün mevcut değilse
                    {
                        // Yeni kayıt ekle
                        command.CommandText = queryInsert;
                        command.Parameters.AddWithValue("@Quantity", Quantity);
                        await command.ExecuteNonQueryAsync();
                    }
                }

                connection.Close();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);
                connection.Close();
                return false;
            }
        }



        // --------------------------------------------------------------------------------------------------------------------------
        public int GetNumberOfProductInBasket(int? userId)
        {
            int productCount = 0;

            string query = @"
                SELECT SUM(Count)
                FROM BasketProducts
                WHERE UserId = @UserId";

            try
            {
                connection.Open();

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);

                    productCount = (int)command.ExecuteScalar();
                }
                connection.Close();
            }
            catch (Exception ex)
            {
                // Log the exception (if you have logging in place)
                // Handle the exception if necessary
                Debug.WriteLine("Error: " + ex.Message);
                connection.Close();
            }

            return productCount;
        }


        // --------------------------------------------------------------------------------------------------------------------------
        public async Task<List<BasketProductModel>> GetUserBasketProducts(int? userId)
        {
            var products = new List<BasketProductModel>();
            var productIds = new List<int>();

            try
            {
                await connection.OpenAsync();
                string queryProductIds = @"
                    SELECT ProductId, Count
                    FROM BasketProducts
                    WHERE UserId = @UserId";

                using (SqlCommand cmd = new SqlCommand(queryProductIds, connection))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            productIds.Add(reader.GetInt32(0));
                        }
                    }
                }

                for (int i = 0; i < productIds.Count; i++)
                {
                    var product = new BasketProductModel();
                    string queryProducts = @"
                        SELECT *
                        FROM Products
                        WHERE ProductId = @ProductId";

                    using (SqlCommand cmd = new SqlCommand(queryProducts, connection))
                    {
                        cmd.Parameters.AddWithValue("@ProductId", productIds[i]);

                        using (SqlDataReader detailsReader = await cmd.ExecuteReaderAsync())
                        {
                            if (await detailsReader.ReadAsync())
                            {
                                product.ProductId = detailsReader.GetInt32(detailsReader.GetOrdinal("ProductId"));
                                product.Rating = detailsReader.GetInt32(detailsReader.GetOrdinal("Rating"));
                                product.Favorites = detailsReader.GetInt32(detailsReader.GetOrdinal("Favorites"));
                                product.CompanyID = detailsReader.GetInt32(detailsReader.GetOrdinal("CompanyID"));
                                product.Stock = detailsReader.GetInt32(detailsReader.GetOrdinal("Stock"));
                                product.Price = detailsReader.GetDecimal(detailsReader.GetOrdinal("Price"));
                                product.ProductName = detailsReader.GetString(detailsReader.GetOrdinal("ProductName"));
                                product.Description = detailsReader.GetString(detailsReader.GetOrdinal("Description"));
                                product.Category = detailsReader.GetString(detailsReader.GetOrdinal("Category"));
                                product.Status = detailsReader.GetString(detailsReader.GetOrdinal("Status"));
                                product.CreatedAt = detailsReader.GetDateTime(detailsReader.GetOrdinal("CreatedAt"));
                                product.Clicked = detailsReader.GetInt32(detailsReader.GetOrdinal("Clicked"));
                                product.Sold = detailsReader.GetInt32(detailsReader.GetOrdinal("Sold"));
                            }
                        }
                    }

                    // Sepet ürünü için adet bilgisi al
                    string queryCount = @"
                        SELECT Count
                        FROM BasketProducts
                        WHERE UserId = @UserId AND ProductId = @ProductId";

                    using (SqlCommand cmd = new SqlCommand(queryCount, connection))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.Parameters.AddWithValue("@ProductId", product.ProductId);

                        product.Count = (int)await cmd.ExecuteScalarAsync();
                    }

                    product.Photos = new List<string>();
                    string queryPhotos = @"
                        SELECT PhotoURL
                        FROM Photos
                        WHERE ProductId = @ProductId";

                    using (var command = new SqlCommand(queryPhotos, connection))
                    {
                        command.Parameters.AddWithValue("@ProductId", productIds[i]);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                product.Photos.Add(reader.GetString(reader.GetOrdinal("PhotoURL")));
                            }
                        }
                    }

                    products.Add(product);
                }

                connection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                connection.Close();
                throw;
            }

            return products;
        }
        // --------------------------------------------------------------------------------------------------------------------------
        public async Task<List<ProductModel>> GetUserDeletedProducts(int? userId)
        {
            var products = new List<ProductModel>();
            var productIds = new List<int>();

            try
            {
                await connection.OpenAsync();
                string queryProductIds = @"
                    SELECT ProductId
                    FROM DeletedProducts
                    WHERE UserId = @UserId";

                using (SqlCommand cmd = new SqlCommand(queryProductIds, connection))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            productIds.Add(reader.GetInt32(0));
                        }
                    }
                }

                for (int i = 0; i < productIds.Count; i++)
                {
                    var product = new ProductModel();
                    string queryProducts = @"
                        SELECT *
                        FROM Products
                        WHERE ProductId = @ProductId";

                    using (SqlCommand cmd = new SqlCommand(queryProducts, connection))
                    {
                        cmd.Parameters.AddWithValue("@ProductId", productIds[i]);

                        using (SqlDataReader detailsReader = await cmd.ExecuteReaderAsync())
                        {
                            if (await detailsReader.ReadAsync())
                            {
                                product.ProductId = detailsReader.GetInt32(detailsReader.GetOrdinal("ProductId"));
                                product.Rating = detailsReader.GetInt32(detailsReader.GetOrdinal("Rating"));
                                product.Favorites = detailsReader.GetInt32(detailsReader.GetOrdinal("Favorites"));
                                product.CompanyID = detailsReader.GetInt32(detailsReader.GetOrdinal("CompanyID"));
                                product.Stock = detailsReader.GetInt32(detailsReader.GetOrdinal("Stock"));
                                product.Price = detailsReader.GetDecimal(detailsReader.GetOrdinal("Price"));
                                product.ProductName = detailsReader.GetString(detailsReader.GetOrdinal("ProductName"));
                                product.Description = detailsReader.GetString(detailsReader.GetOrdinal("Description"));
                                product.Category = detailsReader.GetString(detailsReader.GetOrdinal("Category"));
                                product.Status = detailsReader.GetString(detailsReader.GetOrdinal("Status"));
                                product.CreatedAt = detailsReader.GetDateTime(detailsReader.GetOrdinal("CreatedAt"));
                                product.Clicked = detailsReader.GetInt32(detailsReader.GetOrdinal("Clicked"));
                                product.Sold = detailsReader.GetInt32(detailsReader.GetOrdinal("Sold"));
                            }
                        }
                    }

                    product.Photos = new List<string>();
                    string queryPhotos = @"
                        SELECT PhotoURL
                        FROM Photos
                        WHERE ProductId = @ProductId";

                    using (var command = new SqlCommand(queryPhotos, connection))
                    {
                        command.Parameters.AddWithValue("@ProductId", productIds[i]);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                product.Photos.Add(reader.GetString(reader.GetOrdinal("PhotoURL")));
                            }
                        }
                    }

                    products.Add(product);
                }

                connection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                connection.Close();
                throw;
            }

            return products;
        }




        // --------------------------------------------------------------------------------------------------------------------------
        public async Task<bool> RemoveProductFromBasketDB(int ProductId, int CompanyId, int? UserId)
        {
            await connection.OpenAsync();

            // Transaction başlat
            //SqlTransaction transaction = connection.BeginTransaction();

            try
            {
                // Önce Duplicate Kontrolü Yap
                string checkQuery = @"
                SELECT COUNT(*)
                FROM DeletedProducts
                WHERE UserId = @UserId AND ProductId = @ProductId AND CompanyId = @CompanyId";

                using (SqlCommand checkCommand = new SqlCommand(checkQuery, connection))
                {
                    checkCommand.Parameters.AddWithValue("@UserId", UserId);
                    checkCommand.Parameters.AddWithValue("@ProductId", ProductId);
                    checkCommand.Parameters.AddWithValue("@CompanyId", CompanyId);

                    int count = (int)await checkCommand.ExecuteScalarAsync();
                    if (count == 0)// Eğer kayıt zaten varsa, ekleme işlemini yapma
                    {
                        // Kayıt yoksa, yeni kayıt ekle
                        var insertQuery = "INSERT INTO DeletedProducts (UserId, ProductId, CompanyId) VALUES (@UserId, @ProductId, @CompanyId)";

                        using (SqlCommand insertCommand = new SqlCommand(insertQuery, connection))
                        {
                            insertCommand.Parameters.AddWithValue("@UserId", UserId);
                            insertCommand.Parameters.AddWithValue("@ProductId", ProductId);
                            insertCommand.Parameters.AddWithValue("@CompanyId", CompanyId);
                            await insertCommand.ExecuteNonQueryAsync();
                        }

                    }
                }
                
                // Sepetten ürünü sil
                var deleteQuery = "DELETE FROM BasketProducts WHERE UserId = @UserId AND ProductId = @ProductId AND CompanyId = @CompanyId";

                using (SqlCommand deleteCommand = new SqlCommand(deleteQuery, connection))
                {
                    deleteCommand.Parameters.AddWithValue("@UserId", UserId);
                    deleteCommand.Parameters.AddWithValue("@ProductId", ProductId);
                    deleteCommand.Parameters.AddWithValue("@CompanyId", CompanyId);
                    await deleteCommand.ExecuteNonQueryAsync();
                }

                // Tüm sorgular başarılı ise transaction'ı commit et
                //transaction.Commit();

                connection.Close();
                return true;
            }
            catch (Exception ex)
            {
                // Hata oluşursa transaction'ı rollback yap
                //transaction.Rollback();
                Debug.WriteLine("Error: " + ex.Message);
                connection.Close();
                return false;
            }
        }


        // --------------------------------------------------------------------------------------------------------------------------
        public async Task<bool> UpdateBasketProductQuantityDB(int ProductId, int CompanyId, int? UserId, int Quantity)
        {
            await connection.OpenAsync();

            // Transaction başlat
            //SqlTransaction transaction = connection.BeginTransaction();
            Console.WriteLine(Quantity);
            try
            {
                var query = "UPDATE BasketProducts SET Count = @Quantity WHERE UserId = @UserId AND ProductId = @ProductId AND CompanyId = @CompanyId";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Quantity", Quantity);
                    command.Parameters.AddWithValue("@UserId", UserId);
                    command.Parameters.AddWithValue("@ProductId", ProductId);
                    command.Parameters.AddWithValue("@CompanyId", CompanyId);
                    await command.ExecuteNonQueryAsync();
                }

                // Tüm sorgular başarılı ise transaction'ı commit et
                //transaction.Commit();

                connection.Close();
                return true;
            }
            catch (Exception ex)
            {
                // Hata oluşursa transaction'ı rollback yap
                //transaction.Rollback();
                Debug.WriteLine("Error: " + ex.Message);
                connection.Close();
                return false;
            }
        }


        // --------------------------------------------------------------------------------------------------------------------------
        public async Task<UserModel> GetUserProfile(int? userId)
        {
            var user = new UserModel();

            try
            {
                await connection.OpenAsync();

                //-------------------------------------------------------------------------------------------------
                // Kullanıcı bilgilerini al
                string queryUser = @"
                SELECT *
                FROM Users
                WHERE UserId = @UserId";

                using (SqlCommand cmd = new SqlCommand(queryUser, connection))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            user.UserId = reader.GetInt32(reader.GetOrdinal("UserId"));
                            user.Name = reader.GetString(reader.GetOrdinal("UserName"));
                            user.SurName = reader.GetString(reader.GetOrdinal("UserSurName"));
                            user.PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash"));
                            user.Email = reader.GetString(reader.GetOrdinal("Email"));
                            user.Role = reader.GetString(reader.GetOrdinal("Role"));
                            user.Address = reader.GetString(reader.GetOrdinal("Address"));
                            user.PhoneNumber = reader.GetString(reader.GetOrdinal("PhoneNumber"));
                            user.Age = reader.GetInt32(reader.GetOrdinal("Age"));
                            user.BirthDate = reader.GetDateTime(reader.GetOrdinal("BirthDate"));
                            user.CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"));
                        }
                    }
                }

                //-------------------------------------------------------------------------------------------------
                // Fetch product reviews
                string queryReviews = @"
                    SELECT ReviewId, ProductId, CompanyId, Rating, Review, CreatedAt
                    FROM Reviews
                    WHERE UserId = @UserId";

                user.ProductReviews = new List<ProductReviewModel>();

                using (SqlCommand cmd = new SqlCommand(queryReviews, connection))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var review = new ProductReviewModel
                            {
                                ReviewId = reader.GetInt32(reader.GetOrdinal("ReviewId")),
                                ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                                CompanyId = reader.GetInt32(reader.GetOrdinal("CompanyId")),
                                Rating = reader.GetInt32(reader.GetOrdinal("Rating")),
                                Review = reader.GetString(reader.GetOrdinal("Review")),
                                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
                            };

                            // Fetch product details for each review
                            string queryProduct = @"
                                SELECT ProductId, Rating, Favorites, CompanyID, Stock, Price, ProductName, Description, Category, Status, CreatedAt, Clicked, Sold
                                FROM Products
                                WHERE ProductId = @ProductId";

                            using (SqlCommand productCmd = new SqlCommand(queryProduct, connection))
                            {
                                productCmd.Parameters.AddWithValue("@ProductId", review.ProductId);

                                using (SqlDataReader productReader = await productCmd.ExecuteReaderAsync())
                                {
                                    if (await productReader.ReadAsync())
                                    {
                                        var product = new ProductModel
                                        {
                                            ProductId = productReader.GetInt32(productReader.GetOrdinal("ProductId")),
                                            Rating = productReader.GetInt32(productReader.GetOrdinal("Rating")),
                                            Favorites = productReader.GetInt32(productReader.GetOrdinal("Favorites")),
                                            CompanyID = productReader.GetInt32(productReader.GetOrdinal("CompanyID")),
                                            Stock = productReader.GetInt32(productReader.GetOrdinal("Stock")),
                                            Price = productReader.GetDecimal(productReader.GetOrdinal("Price")),
                                            ProductName = productReader.GetString(productReader.GetOrdinal("ProductName")),
                                            Description = productReader.GetString(productReader.GetOrdinal("Description")),
                                            Category = productReader.GetString(productReader.GetOrdinal("Category")),
                                            Status = productReader.GetString(productReader.GetOrdinal("Status")),
                                            CreatedAt = productReader.GetDateTime(productReader.GetOrdinal("CreatedAt")),
                                            Clicked = productReader.GetInt32(productReader.GetOrdinal("Clicked")),
                                            Sold = productReader.GetInt32(productReader.GetOrdinal("Sold")),
                                        };

                                        // Fetch product photos
                                        product.Photos = new List<string>();
                                        string queryPhotos = @"
                                    SELECT PhotoURL
                                    FROM Photos
                                    WHERE ProductId = @ProductId";

                                        using (SqlCommand photoCmd = new SqlCommand(queryPhotos, connection))
                                        {
                                            photoCmd.Parameters.AddWithValue("@ProductId", product.ProductId);

                                            using (SqlDataReader photoReader = await photoCmd.ExecuteReaderAsync())
                                            {
                                                while (await photoReader.ReadAsync())
                                                {
                                                    product.Photos.Add(photoReader.GetString(photoReader.GetOrdinal("PhotoURL")));
                                                }
                                            }
                                        }

                                        review.product = product;
                                    }
                                }
                            }

                            user.ProductReviews.Add(review);
                        }
                    }
                }
                //-------------------------------------------------------------------------------------------------
                // Fetch company details
                string queryFollowedCompanies = @"
                SELECT CompanyId
                FROM FollowedCompanies
                WHERE UserId = @UserId";

                user.FollowedCompanies = new List<CompanyModel>();

                using (SqlCommand cmd = new SqlCommand(queryFollowedCompanies, connection))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        var companyIds = new List<int>();

                        while (await reader.ReadAsync())
                        {
                            companyIds.Add(reader.GetInt32(reader.GetOrdinal("CompanyId")));
                        }

                        // Fetch company details for each followed company
                        foreach (var companyId in companyIds)
                        {
                            var company = new CompanyModel();
                            string queryCompany = @"
                            SELECT CompanyId, Score, UserId, CompanyName, ContactName, Description, Address, PhoneNumber, Email, LogoUrl, BannerUrl, TaxIDNumber, IBAN, IsValidatedByAdmin, CreatedAt, BirthDate
                            FROM Companies
                            WHERE CompanyId = @CompanyId";

                            using (SqlCommand companyCmd = new SqlCommand(queryCompany, connection))
                            {
                                companyCmd.Parameters.AddWithValue("@CompanyId", companyId);

                                using (SqlDataReader companyReader = await companyCmd.ExecuteReaderAsync())
                                {
                                    if (await companyReader.ReadAsync())
                                    {
                                        company.CompanyId = companyReader.GetInt32(companyReader.GetOrdinal("CompanyId"));
                                        company.Score = companyReader.GetInt32(companyReader.GetOrdinal("Score"));
                                        company.UserID = companyReader.GetInt32(companyReader.GetOrdinal("UserId"));
                                        company.CompanyName = companyReader.GetString(companyReader.GetOrdinal("CompanyName"));
                                        company.ContactName = companyReader.GetString(companyReader.GetOrdinal("ContactName"));
                                        company.CompanyDescription = companyReader.GetString(companyReader.GetOrdinal("Description"));
                                        company.CompanyAddress = companyReader.GetString(companyReader.GetOrdinal("Address"));
                                        company.CompanyPhoneNumber = companyReader.GetString(companyReader.GetOrdinal("PhoneNumber"));
                                        company.Email = companyReader.GetString(companyReader.GetOrdinal("Email"));
                                        company.LogoUrl = companyReader.GetString(companyReader.GetOrdinal("LogoUrl"));
                                        company.BannerUrl = companyReader.GetString(companyReader.GetOrdinal("BannerUrl"));
                                        company.taxIDNumber = companyReader.GetString(companyReader.GetOrdinal("TaxIDNumber"));
                                        company.IBAN = companyReader.GetString(companyReader.GetOrdinal("IBAN"));
                                        company.isValidatedbyAdmin = companyReader.GetBoolean(companyReader.GetOrdinal("IsValidatedByAdmin"));
                                        company.CreatedAt = companyReader.GetDateTime(companyReader.GetOrdinal("CreatedAt"));
                                        company.BirthDate = companyReader.GetDateTime(companyReader.GetOrdinal("BirthDate"));
                                    }
                                }
                            }

                            user.FollowedCompanies.Add(company);
                        }
                    }
                }
                //-------------------------------------------------------------------------------------------------
                // Fetch credit cards
                string queryCreditCards = @"
                    SELECT CreditCardId, UserId, CardNumber, CardHolderName, ExpirationDate, CVV
                    FROM CreditCards
                    WHERE UserId = @UserId";

                user.CreditCards = new List<CreditCardModel>();

                using (SqlCommand cmd = new SqlCommand(queryCreditCards, connection))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var card = new CreditCardModel
                            {
                                CardInfoId = reader.GetInt32(reader.GetOrdinal("CreditCardId")),
                                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                                CardNumber = reader.GetString(reader.GetOrdinal("CardNumber")),
                                CardHolderName = reader.GetString(reader.GetOrdinal("CardHolderName")),
                                ExpirationDate = reader.GetString(reader.GetOrdinal("ExpirationDate")),
                                CVV = reader.GetString(reader.GetOrdinal("CVV"))
                            };
                            user.CreditCards.Add(card);
                        }
                    }
                }

                //-------------------------------------------------------------------------------------------------
                // Fetch purchased products
                var productIds = new List<int>();
                string queryProductIds = @"
                SELECT ProductId
                FROM PurchasedProducts
                WHERE UserId = @UserId";

                using (SqlCommand cmd = new SqlCommand(queryProductIds, connection))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            productIds.Add(reader.GetInt32(0));
                        }
                    }
                }

                for (int i = 0; i < productIds.Count; i++)
                {
                    var product = new ProductModel();
                    string queryProducts = @"
                    SELECT *
                    FROM Products
                    WHERE ProductId = @ProductId";

                    using (SqlCommand cmd = new SqlCommand(queryProducts, connection))
                    {
                        cmd.Parameters.AddWithValue("@ProductId", productIds[i]);

                        using (SqlDataReader detailsReader = await cmd.ExecuteReaderAsync())
                        {
                            if (await detailsReader.ReadAsync())
                            {
                                product.ProductId = detailsReader.GetInt32(detailsReader.GetOrdinal("ProductId"));
                                product.Rating = detailsReader.GetInt32(detailsReader.GetOrdinal("Rating"));
                                product.Favorites = detailsReader.GetInt32(detailsReader.GetOrdinal("Favorites"));
                                product.CompanyID = detailsReader.GetInt32(detailsReader.GetOrdinal("CompanyID"));
                                product.Stock = detailsReader.GetInt32(detailsReader.GetOrdinal("Stock"));
                                product.Price = detailsReader.GetDecimal(detailsReader.GetOrdinal("Price"));
                                product.ProductName = detailsReader.GetString(detailsReader.GetOrdinal("ProductName"));
                                product.Description = detailsReader.GetString(detailsReader.GetOrdinal("Description"));
                                product.Category = detailsReader.GetString(detailsReader.GetOrdinal("Category"));
                                product.Status = detailsReader.GetString(detailsReader.GetOrdinal("Status"));
                                product.CreatedAt = detailsReader.GetDateTime(detailsReader.GetOrdinal("CreatedAt"));
                                product.Clicked = detailsReader.GetInt32(detailsReader.GetOrdinal("Clicked"));
                                product.Sold = detailsReader.GetInt32(detailsReader.GetOrdinal("Sold"));
                            }
                        }
                    }

                    product.Photos = new List<string>();
                    string queryPhotos = @"
                    SELECT PhotoURL
                    FROM Photos
                    WHERE ProductId = @ProductId";

                    using (var command = new SqlCommand(queryPhotos, connection))
                    {
                        command.Parameters.AddWithValue("@ProductId", productIds[i]);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                product.Photos.Add(reader.GetString(reader.GetOrdinal("PhotoURL")));
                            }
                        }
                    }

                    user.PurchasedProducts.Add(product);

                    connection.Close();
                    
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                throw;
            }
            finally
            {
                connection.Close();
            }

            return user;
        }
    }
}
