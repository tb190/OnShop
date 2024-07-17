using Microsoft.AspNetCore.Mvc;
using OnShop.Models;
using System.Diagnostics;

namespace OnShop.Controllers
{
    public class VendorController : Controller
    {
        private readonly ILogger<VendorController> _logger;
        private readonly VendorDbFunctions _vendorDbFunctions;


        public VendorController(ILogger<VendorController> logger)
        {
            _vendorDbFunctions = new VendorDbFunctions();
            _logger = logger;
        }

        public IActionResult VendorHome()
        {
            return View();
        }

        // -------------------------------------- Products Page --------------------------------------
        public IActionResult VendorProducts(int page = 1, string searchString = "", string statusFilter = "")
        {
            List<ProductModel> products = new List<ProductModel>();
            ProductModel product;

            for(int i = 0; i < 20; i++)
            {
                product = new ProductModel
                {
                    ProductId = i,
                    Rating = 4,
                    Favorites = i*2,
                    CompanyID = 123,
                    Price = i*3,
                    ProductName = "Men's Jungle Short Sleeve Shirt",
                    Description = "Short sleeve shirt for men, ideal for casual wear.",
                    Category = "Clothing",
                    Status = "Offline",
                    CreatedAt = DateTime.Now, // Example date/time, replace with actual creation date
                    Photos = new List<string>
                    {
                         "https://picsum.photos/id/0/200/300",
                          "https://picsum.photos/id/1/200/300",
                          "https://picsum.photos/id/2/200/300"
                    },
                        ProductReviews = new List<string>
                    {
                         "ürün harika",
                          "çok kullanışlı"
                    }
                };
                products.Add(product);
            }
            

            ProductModel product1 = new ProductModel
            {
                ProductId = 2,
                Rating = 5,
                Favorites = 10,
                CompanyID = 123,
                Price = 58m,
                ProductName = "Computer",
                Description = "Computer on the desk.",
                Category = "Electronic",
                Status = "Online",
                CreatedAt = DateTime.Now, // Example date/time, replace with actual creation date
                Photos = new List<string>
                {
                     "https://picsum.photos/id/1/200/300",
                      "https://picsum.photos/id/2/200/300",
                      "https://picsum.photos/id/2/200/300"
                },
                ProductReviews = new List<string>
                {
                     "berabet",
                      "kötü",
                      "hayatımıkurtardı",
                      "kötü",
                      "kötü",
                      "kötü",
                      "kötü",
                      "kötü",
                      "kötü",
                      "kötü",
                      "kötü",
                      "kötü",
                      "kötü",
                      "kötü"
                }
            };
            products.Add(product1);
            ProductModel product2 = new ProductModel
            {
                ProductId = 2,
                Rating = 5,
                Favorites = 10,
                CompanyID = 123,
                Price = 58m,
                ProductName = "Computer1",
                Description = "Computer on the desk.",
                Category = "Electronic",
                Status = "Online",
                CreatedAt = DateTime.Now, // Example date/time, replace with actual creation date
                Photos = new List<string>
                {
                     "https://picsum.photos/id/1/200/300",
                      "https://picsum.photos/id/2/200/300",
                      "https://picsum.photos/id/2/200/300"
                },
                ProductReviews = new List<string>
                {
                     "berabet",
                      "kötü",
                      "hayatımıkurtardı",
                      "kötü",
                      "kötü",
                      "kötü",
                      "kötü",
                      "kötü",
                      "kötü",
                      "kötü",
                      "kötü",
                      "kötü",
                      "kötü",
                      "kötü"
                }
            };
            products.Add(product2);

            var productsQuery = products.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                productsQuery = productsQuery.Where(p =>
                    p.ProductName.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                    p.Description.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                    p.Category.Contains(searchString, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(statusFilter) && statusFilter.Equals("Online", StringComparison.OrdinalIgnoreCase))
            {
                productsQuery = productsQuery.Where(p => p.Status.Equals("Online", StringComparison.OrdinalIgnoreCase));
            }

            const int pageSize = 5;
            int totalProducts = productsQuery.Count();
            var paginatedProducts = productsQuery.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalProducts / pageSize);
            ViewBag.SearchString = searchString; // Pass searchString to keep it in the input field


            return View(paginatedProducts);
        }


        [HttpGet]
        public IActionResult VendorAddNewProduct()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddNewProduct(ProductModel model)
        {
            try
            {
                int productId = _vendorDbFunctions.VendorAddProduct(model);

                if (productId > 0)
                {
                    TempData["Message"] = "Product added successfully!";
                    return RedirectToAction("VendorProducts");
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to add product.";
                    return View("VendorAddNewProduct", model);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Failed to add product: " + ex.Message;
                return View("VendorAddNewProduct", model);
            }
        }

        public void VendorAddCategory()
        {
            Console.WriteLine("add category");
        }

        public void VendorDeleteCategories()
        {
            Console.WriteLine("DeleteCategories");
        }
    }
}
