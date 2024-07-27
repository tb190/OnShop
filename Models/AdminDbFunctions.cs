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
    public class AdminDbFunctions
    {
        private SqlConnection connection;

        public AdminDbFunctions()
        {
            string connection_String = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Casper\Documents\OnShopDB.mdf;Integrated Security=True;Connect Timeout=30";
            connection = new SqlConnection(connection_String);
        }

        // --------------------------------------------------------------------------------------------------------------------------
        public async Task<List<AdminViewModel>> GetAllCompaniesWithUsers()
        {
            List<AdminViewModel> companiesWithUsers = new List<AdminViewModel>();

            string query = @"
                SELECT c.CompanyId, c.Score, c.UserId, c.CompanyName, c.ContactName, c.Description, 
                       c.Address AS CompanyAddress, c.PhoneNumber AS CompanyPhoneNumber, c.Email AS CompanyEmail, 
                       c.LogoUrl, c.BannerUrl, c.TaxIDNumber, c.IBAN, c.IsValidatedByAdmin, c.CreatedAt AS CompanyCreatedAt, c.BirthDate AS CompanyBirthDate,
                       u.UserId, u.UserName, u.UserSurName, u.PasswordHash, u.Email AS UserEmail, u.Role, 
                       u.Address AS UserAddress, u.PhoneNumber AS UserPhoneNumber, u.Age, u.BirthDate AS UserBirthDate, u.CreatedAt AS UserCreatedAt
                FROM Companies c
                INNER JOIN Users u ON c.UserId = u.UserId";

            try
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    await connection.OpenAsync();
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            CompanyModel company = new CompanyModel
                            {
                                CompanyId = reader.GetInt32(reader.GetOrdinal("CompanyId")),
                                Score = reader.GetInt32(reader.GetOrdinal("Score")),
                                UserID = reader.GetInt32(reader.GetOrdinal("UserId")),
                                CompanyName = reader.GetString(reader.GetOrdinal("CompanyName")),
                                ContactName = reader.GetString(reader.GetOrdinal("ContactName")),
                                CompanyDescription = reader.GetString(reader.GetOrdinal("Description")),
                                CompanyAddress = reader.GetString(reader.GetOrdinal("CompanyAddress")),
                                CompanyPhoneNumber = reader.GetString(reader.GetOrdinal("CompanyPhoneNumber")),
                                Email = reader.GetString(reader.GetOrdinal("CompanyEmail")),
                                LogoUrl = reader.GetString(reader.GetOrdinal("LogoUrl")),
                                BannerUrl = reader.GetString(reader.GetOrdinal("BannerUrl")),
                                taxIDNumber = reader.GetString(reader.GetOrdinal("TaxIDNumber")),
                                IBAN = reader.GetString(reader.GetOrdinal("IBAN")),
                                isValidatedbyAdmin = reader.GetBoolean(reader.GetOrdinal("IsValidatedByAdmin")),
                                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CompanyCreatedAt")),
                                BirthDate = reader.GetDateTime(reader.GetOrdinal("CompanyBirthDate"))
                            };

                            UserModel user = new UserModel
                            {
                                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                                Name = reader.GetString(reader.GetOrdinal("UserName")),
                                SurName = reader.GetString(reader.GetOrdinal("UserSurName")),
                                PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                                Email = reader.GetString(reader.GetOrdinal("UserEmail")),
                                Role = reader.GetString(reader.GetOrdinal("Role")),
                                Address = reader.GetString(reader.GetOrdinal("UserAddress")),
                                PhoneNumber = reader.GetString(reader.GetOrdinal("UserPhoneNumber")),
                                Age = reader.GetInt32(reader.GetOrdinal("Age")),
                                BirthDate = reader.GetDateTime(reader.GetOrdinal("UserBirthDate")),
                                CreatedAt = reader.GetDateTime(reader.GetOrdinal("UserCreatedAt"))
                            };

                            companiesWithUsers.Add(new AdminViewModel
                            {
                                Company = company,
                                User = user,
                                UnvalidatedCount = 0
                            });
                        }
                    }
                    await connection.CloseAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to retrieve companies with users: {ex.Message}");
                throw;
            }

            return companiesWithUsers;
        }


        // --------------------------------------------------------------------------------------------------------------------------
        public async Task<bool> ToggleCompanyValidationAsync(int companyId)
        {
            try
            {
                string query = @"
                   UPDATE Companies
                    SET isValidatedbyAdmin = CASE 
                        WHEN isValidatedbyAdmin = 1 THEN 0
                        ELSE 1
                    END
                    WHERE companyId = @companyId";

                
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@companyId", companyId);

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                    connection.Close();
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while toggling the validation status.", ex);
            }
        }


        // --------------------------------------------------------------------------------------------------------------------------
        public async Task<AdminViewModel> GetAllUsers()
        {
            List<UserModel> AllUsers = new List<UserModel>();

            AdminViewModel model = new AdminViewModel();

            string query = @"
                SELECT 
                       u.UserId, u.UserName, u.UserSurName, u.PasswordHash, u.Email AS UserEmail, u.Role, 
                       u.Address AS UserAddress, u.PhoneNumber AS UserPhoneNumber, u.Age, u.BirthDate AS UserBirthDate, u.CreatedAt AS UserCreatedAt
                FROM Users u";

            try
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    await connection.OpenAsync();
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {                           
                            UserModel user = new UserModel
                            {
                                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                                Name = reader.GetString(reader.GetOrdinal("UserName")),
                                SurName = reader.GetString(reader.GetOrdinal("UserSurName")),
                                PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                                Email = reader.GetString(reader.GetOrdinal("UserEmail")),
                                Role = reader.GetString(reader.GetOrdinal("Role")),
                                Address = reader.GetString(reader.GetOrdinal("UserAddress")),
                                PhoneNumber = reader.GetString(reader.GetOrdinal("UserPhoneNumber")),
                                Age = reader.GetInt32(reader.GetOrdinal("Age")),
                                BirthDate = reader.GetDateTime(reader.GetOrdinal("UserBirthDate")),
                                CreatedAt = reader.GetDateTime(reader.GetOrdinal("UserCreatedAt"))
                            };

                            AllUsers.Add(user);
                        }
                    }
                }
                model.AllUsers = AllUsers;
                await connection.CloseAsync();
                return model;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to retrieve companies with users: {ex.Message}");
                return model;
                throw;
            }
        }


        // --------------------------------------------------------------------------------------------------------------------------
        public async Task<bool> DeleteUser(int userId)
        {
            string query = "DELETE FROM Users WHERE UserId = @UserId";

            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserId", userId);

            try
            {
                connection.Open();
                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    Console.WriteLine("User deleted successfully.");
                    return true;
                }
                else
                {
                    Console.WriteLine("No user found with the provided UserId.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
            return false;
        }


        // --------------------------------------------------------------------------------------------------------------------------
        public async Task<List<ProductModel>> GetAllProducts()
        {
            var products = new List<ProductModel>();

            try
            {
                await connection.OpenAsync();

                var queryProducts = @"
                    SELECT * FROM Products
                    WHERE status = 'Online'";

                using (SqlCommand cmd = new SqlCommand(queryProducts, connection))
                {
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
        public async Task<bool> DeleteProduct(int ProductId)
        {
            string query = "DELETE FROM Products WHERE ProductId = @ProductId";

            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@ProductId", ProductId);

            try
            {
                connection.Open();
                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    Console.WriteLine("Product deleted successfully.");
                    return true;
                }
                else
                {
                    Console.WriteLine("No product found with the provided ProductId.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
            return false;
        }

        // --------------------------------------------------------------------------------------------------------------------------
        public async Task<AdminViewModel> GetAdminDashBoard()
        {
            AdminViewModel model = new AdminViewModel();

            await connection.OpenAsync();

            try
            {

                // Get All Users
                List<UserModel> AllUsers = new List<UserModel>();
                string queryUsers = @"
                    SELECT 
                           u.UserId, u.UserName, u.UserSurName, u.PasswordHash, u.Email AS UserEmail, u.Role, 
                           u.Address AS UserAddress, u.PhoneNumber AS UserPhoneNumber, u.Age, u.BirthDate AS UserBirthDate, u.CreatedAt AS UserCreatedAt
                    FROM Users u";

            
                using (SqlCommand commandUsers = new SqlCommand(queryUsers, connection))
                {
                    
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
                model.AllUsers = AllUsers.OrderBy(p => p.CreatedAt).ToList(); 

                // Get All Products
                var products = new List<ProductModel>();

                var queryProducts = @"
                    SELECT * FROM Products
                    WHERE status = 'Online'";

                using (SqlCommand cmd = new SqlCommand(queryProducts, connection))
                {
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

                            model.TotalSold += product_.Sold;
                            model.TotalFavorites += product_.Favorites;
                            model.TotalClicks += product_.Clicked;
                            model.TotalRevenue += product_.Sold*product_.Price;
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

                    using (var commandPhotos = new SqlCommand(queryPhotos_, connection))
                    {
                        commandPhotos.Parameters.AddWithValue("@ProductId", products[i].ProductId);

                        using (var readerPhotos = await commandPhotos.ExecuteReaderAsync())
                        {
                            while (await readerPhotos.ReadAsync())
                            {
                                products[i].Photos.Add(readerPhotos.GetString(readerPhotos.GetOrdinal("PhotoURL")));
                            }
                        }
                    }
                }

                model.AllProducts = products;


                // Get All Products Reviews
                List<ProductReviewModel> reviews = new List<ProductReviewModel>();

                string queryReviews = @"
                    SELECT ReviewId, ProductId,  CompanyId, Rating, Review, CreatedAt
                    FROM Reviews;";

                using (var commandReviews = new SqlCommand(queryReviews, connection))
                {
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


                // Get All Companies
                List<CompanyModel> allCompanies = new List<CompanyModel>();

                string queryCompany = @"
                SELECT CompanyId, Score, UserId, CompanyName, ContactName, Description, Address, PhoneNumber, Email, LogoUrl, BannerUrl, TaxIDNumber, IBAN, IsValidatedByAdmin, CreatedAt, BirthDate
                FROM Companies";
                using (SqlCommand companyCmd = new SqlCommand(queryCompany, connection))
                {
                    using (SqlDataReader companyReader = await companyCmd.ExecuteReaderAsync())
                    {
                        while (await companyReader.ReadAsync())
                        {
                            var company = new CompanyModel
                            {
                                CompanyId = companyReader.GetInt32(companyReader.GetOrdinal("CompanyId")),
                                Score = companyReader.GetInt32(companyReader.GetOrdinal("Score")),
                                UserID = companyReader.GetInt32(companyReader.GetOrdinal("UserId")),
                                CompanyName = companyReader.GetString(companyReader.GetOrdinal("CompanyName")),
                                ContactName = companyReader.GetString(companyReader.GetOrdinal("ContactName")),
                                CompanyDescription = companyReader.GetString(companyReader.GetOrdinal("Description")),
                                CompanyAddress = companyReader.GetString(companyReader.GetOrdinal("Address")),
                                CompanyPhoneNumber = companyReader.GetString(companyReader.GetOrdinal("PhoneNumber")),
                                Email = companyReader.GetString(companyReader.GetOrdinal("Email")),
                                LogoUrl = companyReader.GetString(companyReader.GetOrdinal("LogoUrl")),
                                BannerUrl = companyReader.GetString(companyReader.GetOrdinal("BannerUrl")),
                                taxIDNumber = companyReader.GetString(companyReader.GetOrdinal("TaxIDNumber")),
                                IBAN = companyReader.GetString(companyReader.GetOrdinal("IBAN")),
                                isValidatedbyAdmin = companyReader.GetBoolean(companyReader.GetOrdinal("IsValidatedByAdmin")),
                                CreatedAt = companyReader.GetDateTime(companyReader.GetOrdinal("CreatedAt")),
                                BirthDate = companyReader.GetDateTime(companyReader.GetOrdinal("BirthDate"))
                            };

                            allCompanies.Add(company);
                        }
                    }
                }
                model.AllCompanies = allCompanies;



                // Get All Categories
                List<CategoryModel> categories = new List<CategoryModel>();
                string queryCategory = @"
                    SELECT c.Id AS CategoryId, c.CategoryName, t.TypeName
                    FROM Categories c
                    LEFT JOIN Types t ON c.Id = t.CategoryId";

                using (SqlCommand commandCategory = new SqlCommand(queryCategory, connection))
                {
                    using (SqlDataReader readerCategory = await commandCategory.ExecuteReaderAsync())
                    {
                        while (readerCategory.Read())
                        {
                            int categoryId = readerCategory.GetInt32(readerCategory.GetOrdinal("CategoryId"));
                            string categoryName = readerCategory.GetString(readerCategory.GetOrdinal("CategoryName"));
                            string typeName = readerCategory.IsDBNull(readerCategory.GetOrdinal("TypeName")) ? null : readerCategory.GetString(readerCategory.GetOrdinal("TypeName"));

                            // Mevcut bir kategoriyi bul veya yeni bir kategori ekle
                            CategoryModel category = categories.FirstOrDefault(c => c.CategoryId == categoryId);
                            if (category == null)
                            {
                                category = new CategoryModel
                                {
                                    CategoryId = categoryId,
                                    CategoryName = categoryName,
                                    Types = new List<string>()
                                };
                                categories.Add(category);
                            }

                            // Tür adý varsa, listeye ekle
                            if (!string.IsNullOrEmpty(typeName))
                            {
                                category.Types.Add(typeName);
                            }
                        }
                    }
                }
                model.AllCategories = categories;




                // Get All PurchasedProducts
                List<PurchasedProductModel> PurchasedProducts = new List<PurchasedProductModel>();
                List<int> PurchasedProductsIds = new List<int>();

                string queryPurchasedProductsId = @"
                    SELECT ProductId
                    FROM PurchasedProducts;";

                using (SqlCommand cmdPR = new SqlCommand(queryPurchasedProductsId, connection))
                {
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

    }
}
