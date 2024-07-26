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
                                User = user
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


    }
}
