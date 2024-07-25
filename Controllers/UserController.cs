using Microsoft.AspNetCore.Mvc;
using OnShop.Models;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Text.Json;


namespace OnShop.Controllers
{

    public class UserController : Controller
    {

        private readonly GuestDbFunctions _guestDbFunctions;
        private readonly UserDbFunctions _userDbFunctions;

        public UserController()
        {
            _guestDbFunctions = new GuestDbFunctions();
            _userDbFunctions = new UserDbFunctions();
        }

        // --------------------------------------------------------------------------------------------------------------------------
        public async Task<IActionResult> UserHome()
        {
            try
            {
                int? userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    HttpContext.Session.Remove("UserId");
                    return RedirectToAction("Login", "Login"); // Kullanıcı giriş yapmamışsa login sayfasına yönlendir
                }
                // Veritaban�ndan kategorileri �ek
                var categories = await _guestDbFunctions.GuestGetCategoriesWithTypes();

                var products = await _guestDbFunctions.GuestGetProducts();


                var sortedProducts = products.OrderByDescending(p => p.Clicked).ToList();

                var companies = await _userDbFunctions.GetAllCompanies();


                foreach (var product in products)
                {
                    product.IsFavorited = await _userDbFunctions.IsUserFavoritedProduct(userId, product.ProductId);
                }



                var productviewModel = new ProductViewModel
                {
                    Company = null,
                    User = null,
                    Product = null,
                    ProductReviews = null,
                    Categories = categories,
                    GuestHomeView = new GuestHomeViewModel
                    {
                        Categories = categories,
                        Products = products,
                        MostClickedProducts = sortedProducts
                    },
                    AllCompanies = companies
                };

                return View("UserHome", productviewModel);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);
                return RedirectToAction("UserHome");
            }
        }

        // --------------------------------------------------------------------------------------------------------------------------
        public async Task<IActionResult> ProductDetails(int ProductId)
        {
            try
            {
                int? userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    HttpContext.Session.Remove("UserId");
                    return RedirectToAction("Login", "Login"); // Kullanıcı giriş yapmamışsa login sayfasına yönlendir
                }


                var productViewModel = await _userDbFunctions.UserGetProductDetails(ProductId);

                productViewModel.Product.IsFavorited = await _userDbFunctions.IsUserFavoritedProduct(userId, ProductId);


                var categories = await _guestDbFunctions.GuestGetCategoriesWithTypes();

                productViewModel.Categories = categories;

                var otherproducts = await _guestDbFunctions.GuestGetProducts();


                foreach (var product in otherproducts)
                {
                    product.IsFavorited = await _userDbFunctions.IsUserFavoritedProduct(userId, product.ProductId);
                }

                productViewModel.OtherProducts = otherproducts;

                


                return View(productViewModel);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);

            }

            return RedirectToAction("UserHome");
        }


        // --------------------------------------------------------------------------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> ProductAddToCart(int ProductId, int CompanyId, int Quantity)
        {
            try
            {
                int? userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    HttpContext.Session.Remove("UserId");
                    return RedirectToAction("Login", "Login"); // Kullanıcı giriş yapmamışsa login sayfasına yönlendir
                }
                var result = await _userDbFunctions.ProductAddToCartDb(ProductId, CompanyId, userId, Quantity);


                return RedirectToAction("ProductDetails", new { ProductId = ProductId });
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);
            }
            return RedirectToAction("ProductDetails", new { ProductId = ProductId });
        }

        // --------------------------------------------------------------------------------------------------------------------------
        public async Task<IActionResult> UserBasket()
        {
            try
            {
                int? userId = HttpContext.Session.GetInt32("UserId");

                if (userId == null)
                {
                    HttpContext.Session.Remove("UserId");
                    return RedirectToAction("Login", "Login"); // Kullanıcı giriş yapmamışsa login sayfasına yönlendir
                }

                List<BasketProductModel> BasketProducts = await _userDbFunctions.GetUserBasketProducts(userId);
                List<ProductModel> DeletedProducts = await _userDbFunctions.GetUserDeletedProducts(userId);

                var userModel = await _userDbFunctions.GetUserProfile(userId);

                decimal TotalPrice = 0;


                var categories = await _guestDbFunctions.GuestGetCategoriesWithTypes();


                foreach (var product in BasketProducts)
                {
                    product.IsFavorited = await _userDbFunctions.IsUserFavoritedProduct(userId, product.ProductId);
                }

                foreach (var product in DeletedProducts)
                {
                    product.IsFavorited = await _userDbFunctions.IsUserFavoritedProduct(userId, product.ProductId);
                }

                var productviewModel = new ProductViewModel
                {
                    Company = null,
                    User = null,
                    Product = null,
                    ProductReviews = null,
                    Categories = categories,
                    GuestHomeView = null,
                    BasketProducts = BasketProducts,
                    DeletedProducts = DeletedProducts,
                    userModel = userModel,
                };

                foreach (var product in productviewModel.BasketProducts) TotalPrice += product.Price * product.Count;

                productviewModel.TotalPrice = TotalPrice;

                return View(productviewModel);

            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);

            }

            return RedirectToAction("UserHome");
        }

        // --------------------------------------------------------------------------------------------------------------------------
        public IActionResult GetBasketCount()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                HttpContext.Session.Remove("UserId");
                return RedirectToAction("Login", "Login"); // Kullanıcı giriş yapmamışsa login sayfasına yönlendir
            }
            /*if (userId == null)
            {
                return Json(0); // Kullanıcı giriş yapmamışsa, sepet sayısı 0
            }*/

            var basketCount = _userDbFunctions.GetNumberOfProductInBasket(userId);
            return Json(basketCount);
        }

        // --------------------------------------------------------------------------------------------------------------------------
        public async Task<IActionResult> RemoveProductFromBasket(int ProductId, int CompanyId)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                HttpContext.Session.Remove("UserId");
                return RedirectToAction("Login", "Login"); // Kullanıcı giriş yapmamışsa login sayfasına yönlendir
            }
            var result = await _userDbFunctions.RemoveProductFromBasketDB(ProductId, CompanyId, userId);

            return RedirectToAction("UserBasket");
        }


        // --------------------------------------------------------------------------------------------------------------------------
        public async Task<IActionResult> UpdateBasketProductQuantity(int ProductId, int CompanyId, int Quantity)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                HttpContext.Session.Remove("UserId");
                return RedirectToAction("Login", "Login"); // Kullanıcı giriş yapmamışsa login sayfasına yönlendir
            }
            var result = await _userDbFunctions.UpdateBasketProductQuantityDB(ProductId, CompanyId, userId, Quantity);

            return RedirectToAction("UserBasket");
        }

        // --------------------------------------------------------------------------------------------------------------------------
        public async Task<IActionResult> UserProfile()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                HttpContext.Session.Remove("UserId");
                return RedirectToAction("Login", "Login"); // Kullanıcı giriş yapmamışsa login sayfasına yönlendir
            }
            var userModel = await _userDbFunctions.GetUserProfile(userId);



            var categories = await _guestDbFunctions.GuestGetCategoriesWithTypes();

            var productviewModel = new ProductViewModel
            {
                Company = null,
                User = null,
                Product = null,
                ProductReviews = null,
                Categories = categories,
                GuestHomeView = null,
                BasketProducts = null,
                DeletedProducts = null,
                userModel = userModel,
            };


            return View(productviewModel);

        }

        // --------------------------------------------------------------------------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> AddCard(string CardNumber, string CardHolderName, string ExpirationDate, string CVV)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                HttpContext.Session.Remove("UserId");
                return RedirectToAction("Login", "Login"); // Kullanıcı giriş yapmamışsa login sayfasına yönlendir
            }

            var userModel = await _userDbFunctions.AddCard(userId, CardNumber, CardHolderName, ExpirationDate, CVV);

            return RedirectToAction("UserProfile");
        }

        // --------------------------------------------------------------------------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> BuyProducts(decimal TotalPrice, int CardId)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                HttpContext.Session.Remove("UserId");
                return RedirectToAction("Login", "Login"); // Kullanıcı giriş yapmamışsa login sayfasına yönlendir
            }

            //List<BasketProductModel> BasketProducts = await _userDbFunctions.GetUserBasketProducts(userId);

            var result = await _userDbFunctions.BuyProducts(userId, TotalPrice, CardId);


            return RedirectToAction("UserBasket");

        }


        // --------------------------------------------------------------------------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> AddReview(int ProductId, int Rating, string Review)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                HttpContext.Session.Remove("UserId");
                return RedirectToAction("Login", "Login"); // Kullanıcı giriş yapmamışsa login sayfasına yönlendir
            }


            var productId = await _userDbFunctions.AddReview(userId, ProductId, Rating, Review);

            return RedirectToAction("ProductDetails", new { ProductId = productId });
        }
        // --------------------------------------------------------------------------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> GetProductsByCategoryAndType(string category, string type)
        {
            try
            {
                int? userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    HttpContext.Session.Remove("UserId");
                    return RedirectToAction("Login", "Login"); // Kullanıcı giriş yapmamışsa login sayfasına yönlendir
                }

                var categories = await _guestDbFunctions.GuestGetCategoriesWithTypes();
                var CategoryName = categories.FirstOrDefault(c => c.CategoryId.ToString() == category)?.CategoryName ?? "All";
                var Allproducts = await _userDbFunctions.GetProductsByCategoryAndType(CategoryName, type);

               
                foreach (var product in Allproducts)
                {
                    product.IsFavorited = await _userDbFunctions.IsUserFavoritedProduct(userId, product.ProductId);
                }
                
                
                var productviewModel = new ProductViewModel
                {
                    Categories = categories,
                    AllProducts = Allproducts,
                };

                ViewBag.Category = CategoryName;
                ViewBag.Type = type;

                return View(productviewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return RedirectToAction("Error", "Home");
            }
        }

        // --------------------------------------------------------------------------------------------------------------------------
        public async Task<IActionResult> CompanyDetails(int companyId)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                HttpContext.Session.Remove("UserId");
                return RedirectToAction("Login", "Login"); // Kullanıcı giriş yapmamışsa login sayfasına yönlendir
            }
            // CompanyDetails metodunu çağır ve dönen ProductViewModel'i al
            var companyInfos = await _userDbFunctions.CompanyDetails(companyId);
            // Kategorileri al
            var categories = await _guestDbFunctions.GuestGetCategoriesWithTypes();
            var isFollowing = await _userDbFunctions.IsUserFollowingCompany(userId, companyId);

            companyInfos.Categories = categories;
            companyInfos.IsFollowing = isFollowing;

            // Görüntüleme için view'e gönder
            return View(companyInfos);
        }


        // --------------------------------------------------------------------------------------------------------------------------
        public async Task<IActionResult> FollowCompany(int companyId)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                HttpContext.Session.Remove("UserId");
                return RedirectToAction("Login", "Login"); // Kullanıcı giriş yapmamışsa login sayfasına yönlendir
            }

            var result = await _userDbFunctions.FollowCompany(userId, companyId);


            // Görüntüleme için view'e gönder
            return RedirectToAction("CompanyDetails", new { companyId = companyId });
        }

        // --------------------------------------------------------------------------------------------------------------------------
        public async Task<IActionResult> UnFollowCompany(int companyId)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                HttpContext.Session.Remove("UserId");
                return RedirectToAction("Login", "Login"); // Kullanıcı giriş yapmamışsa login sayfasına yönlendir
            }

            var result = await _userDbFunctions.UnFollowCompany(userId, companyId);


            // Görüntüleme için view'e gönder
            return RedirectToAction("CompanyDetails", new { companyId = companyId });
        }

        // --------------------------------------------------------------------------------------------------------------------------
        public async Task<IActionResult> LikeProduct(int productId)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                HttpContext.Session.Remove("UserId");
                return Json(new { success = false, message = "User not logged in" });
            }

            bool result = await _userDbFunctions.LikeProduct(userId, productId);
            return Json(new { success = result });
        }

        // --------------------------------------------------------------------------------------------------------------------------
        public async Task<IActionResult> UnLikeProduct(int productId)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                HttpContext.Session.Remove("UserId");
                return Json(new { success = false, message = "User not logged in" });
            }

            bool result = await _userDbFunctions.UnLikeProduct(userId, productId);
            return Json(new { success = result });
        }

        // --------------------------------------------------------------------------------------------------------------------------
        public async Task<IActionResult> ToggleFavoriteProduct(int productId, string returnUrl)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                HttpContext.Session.Remove("UserId");
                return RedirectToAction("Login", "Login");
            }

            var isFavorited = await _userDbFunctions.IsUserFavoritedProduct(userId, productId);
            if (isFavorited)
            {
                await _userDbFunctions.UnLikeProduct(userId, productId);
            }
            else
            {
                await _userDbFunctions.LikeProduct(userId, productId);
            }

            // Return to the original view
            if (string.IsNullOrEmpty(returnUrl))
            {
                // If returnUrl is empty or null, redirect to a default action
                return RedirectToAction("ProductDetails", new { ProductId = productId });
            }
            else
            {
                return Redirect(returnUrl);
            }
        }



        // --------------------------------------------------------------------------------------------------------------------------
        public async Task<IActionResult> FavoritedProducts()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                HttpContext.Session.Remove("UserId");
                return RedirectToAction("Login", "Login");
            }

            var Favoritedproducts = await _userDbFunctions.GetFavoritedProducts(userId);
            var categories = await _guestDbFunctions.GuestGetCategoriesWithTypes();

            foreach (var product in Favoritedproducts)
            {
                product.IsFavorited = await _userDbFunctions.IsUserFavoritedProduct(userId, product.ProductId);
            }

            var productviewModel = new ProductViewModel
            {
                Categories = categories,
                AllProducts = Favoritedproducts,
            };
            return View(productviewModel);
        }

        // --------------------------------------------------------------------------------------------------------------------------
        public async Task<IActionResult> AllProducts()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                HttpContext.Session.Remove("UserId");
                return RedirectToAction("Login", "Login");
            }

            var AllProducts = await _userDbFunctions.GetAllProducts();
            var categories = await _guestDbFunctions.GuestGetCategoriesWithTypes();

            foreach (var product in AllProducts)
            {
                product.IsFavorited = await _userDbFunctions.IsUserFavoritedProduct(userId, product.ProductId);
            }

            var productviewModel = new ProductViewModel
            {
                Categories = categories,
                AllProducts = AllProducts,
            };
            return View(productviewModel);
        }
    }
}
