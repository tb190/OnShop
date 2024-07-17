using Microsoft.AspNetCore.Mvc;
using OnShop.Models;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


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

        public int VendorAddProduct(ProductModel product)
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
                if (productId > 0 && product.Photos != null && product.Photos.Count > 0)
                {
                    foreach (var photoUrl in product.Photos)
                    {
                        sql = @"INSERT INTO Photos (ProductId, PhotoURL)
                                        VALUES (@ProductId, @PhotoURL)";

                        using (SqlCommand cmd = new SqlCommand(sql, connection))
                        {
                            cmd.Parameters.AddWithValue("@ProductId", productId);
                            cmd.Parameters.AddWithValue("@PhotoURL", photoUrl);

                            cmd.ExecuteNonQuery();
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
