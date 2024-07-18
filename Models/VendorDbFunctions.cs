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
        public List<ProductModel> VendorGetProducts()
        {
            List<ProductModel> products = new List<ProductModel>();

            try
            {
                connection.Open();

                string sql = @"
                            SELECT 
                                p.ProductId, 
                                p.Rating, 
                                p.Favorites, 
                                p.CompanyID, 
                                p.Stock, 
                                p.Price, 
                                p.ProductName, 
                                p.Description, 
                                p.Category, 
                                p.Status, 
                                p.CreatedAt,
                                (SELECT STRING_AGG(PhotoURL, ',') 
                                 FROM Photos ph 
                                 WHERE ph.ProductId = p.ProductId) AS PhotoURLs
                                 FROM 
                                 Products p";


                using (SqlCommand cmd = new SqlCommand(sql, connection))
                {

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ProductModel product = new ProductModel
                            { 
                                ProductId = reader.GetInt32(0),
                                Rating = reader.GetInt32(1),
                                Favorites = reader.GetInt32(2),
                                CompanyID = reader.GetInt32(3),
                                Stock = reader.GetInt32(4),
                                Price = reader.GetDecimal(5),
                                ProductName = reader.GetString(6),
                                Description = reader.GetString(7),
                                Category = reader.GetString(8),
                                Status = reader.GetString(9),
                                CreatedAt = reader.GetDateTime(10),
                                Photos = reader.IsDBNull(11) ? new List<string>() : reader.GetString(11).Split(',').ToList(),
                                ProductReviews = new string[] {"Ben","sen"}
                            };

                            products.Add(product);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error getting products: " + ex.Message);
                throw;
            }
            finally
            {
                connection.Close();
            }

            return products;
        }



        // --------------------------------------------------------------------------------------------------------------------------
        public async Task<int> VendorAddProduct(ProductModel product,List<IFormFile> Photos)
        {
            int productId = 0;

            try
            {
                connection.Open();

                string sql = @"INSERT INTO Products (Rating, Favorites, CompanyID, Stock, Price, ProductName, Description, Category, Status, CreatedAt)
                       OUTPUT INSERTED.ProductId
                       VALUES (@Rating, @Favorites, @CompanyID, @Stock, @Price, @ProductName, @Description, @Category, @Status, @CreatedAt)";

                 using (SqlCommand cmd = new SqlCommand(sql, connection))
                 {
                     cmd.Parameters.AddWithValue("@Rating", 0);
                     cmd.Parameters.AddWithValue("@Favorites", 0);
                     cmd.Parameters.AddWithValue("@CompanyID", 0);
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

    }
}
