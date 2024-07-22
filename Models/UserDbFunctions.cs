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
        public async Task<bool> ProductAddToCartDb(int ProductId,int CompanyId,int? UserId)
        {
                var query = "INSERT INTO BasketProducts (UserId, ProductId, CompanyId) VALUES (@UserId, @ProductId, @CompanyId)";

                try
                {
                    await connection.OpenAsync();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserId", UserId);
                        command.Parameters.AddWithValue("@ProductId", ProductId);
                        command.Parameters.AddWithValue("@CompanyId", CompanyId);
                        await command.ExecuteNonQueryAsync();
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
                SELECT COUNT(*)
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
        public async Task<List<ProductModel>> GetUserBasketProducts(int? userId)
        {
            var products = new List<ProductModel>(); 
            var productIds = new List<int>();

            try
            {
                connection.Open();
                string queryProductIds = @"
                    SELECT ProductId
                    FROM BasketProducts
                    WHERE UserId = @UserId";

                using (SqlCommand cmd = new SqlCommand(queryProductIds, connection))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            productIds.Add(reader.GetInt32(0));
                        }
                    }
                }

                for (int i = 0; i < productIds.Count; i++)
                {
                    string queryProducts = @"
                    SELECT *
                    FROM Products
                    WHERE ProductId = @ProductId";


                    using (SqlCommand cmd = new SqlCommand(queryProducts, connection))
                    {
                        cmd.Parameters.AddWithValue("@ProductId", productIds[i]);
                        using (SqlDataReader detailsReader = cmd.ExecuteReader())
                        {
                            while (detailsReader.Read())
                            {
                                ProductModel product = new ProductModel
                                {
                                    ProductId = detailsReader.GetInt32(detailsReader.GetOrdinal("ProductId")),
                                    Rating = detailsReader.GetInt32(detailsReader.GetOrdinal("Rating")),
                                    Favorites = detailsReader.GetInt32(detailsReader.GetOrdinal("Favorites")),
                                    CompanyID = detailsReader.GetInt32(detailsReader.GetOrdinal("CompanyID")),
                                    Stock = detailsReader.GetInt32(detailsReader.GetOrdinal("Stock")),
                                    Price = detailsReader.GetDecimal(detailsReader.GetOrdinal("Price")),
                                    ProductName = detailsReader.GetString(detailsReader.GetOrdinal("ProductName")),
                                    Description = detailsReader.GetString(detailsReader.GetOrdinal("Description")),
                                    Category = detailsReader.GetString(detailsReader.GetOrdinal("Category")),
                                    Status = detailsReader.GetString(detailsReader.GetOrdinal("Status")),
                                    CreatedAt = detailsReader.GetDateTime(detailsReader.GetOrdinal("CreatedAt")),
                                    Clicked = detailsReader.GetInt32(detailsReader.GetOrdinal("Clicked")),
                                    Sold = detailsReader.GetInt32(detailsReader.GetOrdinal("Sold"))
                                };
                                products.Add(product);
                            }
                        }
                    }
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

    }
}
