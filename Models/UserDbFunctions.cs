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

        public UserDbFunctions()
        {
            string connection_String = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Casper\Documents\OnShopDB.mdf;Integrated Security=True;Connect Timeout=30";
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
                    // Birthdate kontrolü
                    if (user.BirthDate < new DateTime(1753, 1, 1) || user.BirthDate > new DateTime(9999, 12, 31))
                    {
                        throw new ArgumentOutOfRangeException("BirthDate", "BirthDate must be between 1/1/1753 and 12/31/9999.");
                    }
                    command.Parameters.AddWithValue("@BirthDate", user.BirthDate);

                    // Age hesaplamasý
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
        public async Task<string> ValidateUserCredentials(string email, string password)
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
                                    return "Your account has not been validated by the admin.";
                                }
                                connection.Close();
                                return "User validated successfully.";
                            }
                        }
                        
                    }
                }
                connection.Close();
                return "Invalid email or password.";
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
        public string HashPassword(string password)
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


    }
}
