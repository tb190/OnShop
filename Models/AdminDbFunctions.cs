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
        public async Task<List<LoginViewModel>> GetAllCompaniesWithUsers()
        {
            List<LoginViewModel> companiesWithUsers = new List<LoginViewModel>();

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

                            companiesWithUsers.Add(new LoginViewModel
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
    }
}
