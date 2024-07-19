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
    public class GuestDbFunctions
    {
        private SqlConnection connection;

        public GuestDbFunctions()
        {
            string connection_String = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Casper\Documents\OnShopDB.mdf;Integrated Security=True;Connect Timeout=30";
            connection = new SqlConnection(connection_String);
        }

        // --------------------------------------------------------------------------------------------------------------------------
        // Get Categories from the Database
        public List<CategoryModel> GuestGetCategoriesWithTypes()
        {
            List<CategoryModel> categories = new List<CategoryModel>();

            try
            {
                connection.Open();

                string query = @"
                    SELECT c.Id AS CategoryId, c.CategoryName, t.TypeName
                    FROM Categories c
                    LEFT JOIN Types t ON c.Id = t.CategoryId";

                SqlCommand command = new SqlCommand(query, connection);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    int categoryId = reader.GetInt32(reader.GetOrdinal("CategoryId")); // Getordinal: Bir sütun adýnýn sýrasýný (indeksini) almak için kullanýlan bir yöntemdir
                    string categoryName = reader.GetString(reader.GetOrdinal("CategoryName"));
                    string typeName = reader.IsDBNull(reader.GetOrdinal("TypeName")) ? null : reader.GetString(reader.GetOrdinal("TypeName"));

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

                reader.Close();
            }
            catch (Exception ex)
            {
                // Hata yönetimi
                Debug.WriteLine("Error: " + ex.Message);
            }
            finally
            {
                connection.Close();
            }

            return categories;
        }

        // Get the Product with specified id from the Database
        public ProductModel GuestGetProductById(int productId)
        {
            ProductModel product = null;

            try
            {
                connection.Open();

                string query = @"
                    SELECT *
                    FROM Products
                    WHERE ProductId = @ProductId";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@ProductId", productId);
                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
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
                        ProductReviewsID = new List<int>(), // Assuming these are stored elsewhere and need separate querying
                        ProductReviews = new List<string>(), // Assuming these are stored elsewhere and need separate querying
                        Photos = new List<string>() // Assuming these are stored elsewhere and need separate querying
                    };
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);
            }
            finally
            {
                connection.Close();
            }

            return product;
        }

        // Get all the Product from the Database
        public List<ProductModel> GuestGetProducts()
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
                                ProductReviews = new string[] { "Ben", "sen" }
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


    }
}
