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

            int? userId = HttpContext.Session.GetInt32("UserId");

            // Veritaban�ndan kategorileri �ek
            var categories = await _guestDbFunctions.GuestGetCategoriesWithTypes();

            var products = await _guestDbFunctions.GuestGetProducts();

            var sortedProducts = products.OrderByDescending(p => p.Clicked).ToList();

            var companies = await _userDbFunctions.GetAllCompanies();

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
                Console.WriteLine("id: "+ProductId);

                var productViewModel = await _userDbFunctions.UserGetProductDetails(ProductId);

                var categories = await _guestDbFunctions.GuestGetCategoriesWithTypes();

                productViewModel.Categories = categories;

                var otherproducts = await _guestDbFunctions.GuestGetProducts();

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
        public async Task<IActionResult> ProductAddToCart(int ProductId,int CompanyId,int Quantity)
        {
            try
            {
                int? userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    HttpContext.Session.Remove("UserId");
                    return RedirectToAction("Login", "Login"); // Kullanıcı giriş yapmamışsa login sayfasına yönlendir
                }
                var result = await _userDbFunctions.ProductAddToCartDb(ProductId,CompanyId,userId, Quantity);


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

            var basketCount =  _userDbFunctions.GetNumberOfProductInBasket(userId);
            return Json(basketCount);
        }

        // --------------------------------------------------------------------------------------------------------------------------
        public async Task<IActionResult> RemoveProductFromBasket(int ProductId,int CompanyId)
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
        public async Task<IActionResult> BuyProducts(decimal TotalPrice,int CardId)
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
        [HttpPost]
        public async Task<IActionResult> GetCompanyproducts(int CompanyId)
        {
            Console.WriteLine(CompanyId);
            return RedirectToAction("UserHome");
        }

        // --------------------------------------------------------------------------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> GetProductsByCategoryAndType(string category, string type)
        {
            try
            {
                

                var categories = await _guestDbFunctions.GuestGetCategoriesWithTypes();
                var CategoryName = categories.FirstOrDefault(c => c.CategoryId.ToString() == category)?.CategoryName ?? "All";
                var Allproducts = await _userDbFunctions.GetProductsByCategoryAndType(CategoryName, type);

                Console.WriteLine("category: " + CategoryName + "  type: " + type);

                Console.WriteLine("categoryyy: " + Allproducts[0].Category+ "  type: "+ Allproducts[0].Type);

                var productviewModel = new ProductViewModel
                {
                    Categories = categories,
                    AllProducts = Allproducts,
                };

                Console.WriteLine(productviewModel.AllProducts.Count);
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
            // CompanyDetails metodunu çağır ve dönen ProductViewModel'i al
            var companyInfos = await _userDbFunctions.CompanyDetails(companyId);

            // Kategorileri al
            var categories = await _guestDbFunctions.GuestGetCategoriesWithTypes();

            companyInfos.Categories = categories;


            // Görüntüleme için view'e gönder
            return View(companyInfos);
        }


    }
}
