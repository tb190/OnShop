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
        public async Task RegisterIndividual(UserModel user)
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
                    command.Parameters.AddWithValue("@Role", "User");
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
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to register user: {ex.Message}");
                throw;
            }
        }
        // --------------------------------------------------------------------------------------------------------------------------
        public async Task<bool> ValidateUserCredentials(string email, string password)
        {
            try
            {
                string hashedPassword = HashPassword(password);

                string query = "SELECT PasswordHash FROM Users WHERE Email = @Email";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);

                    connection.Open();
                    var result = command.ExecuteScalar(); // SQL sorgusunun sonucunda dönen ilk satýrýn ilk sütunundaki deðeri döndürür.
                    connection.Close();

                    if (result != null)
                    {
                        string storedHash = result.ToString();
                        return storedHash.Equals(hashedPassword);
                    }

                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to validate user credentials: {ex.Message}");
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
