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
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;

namespace OnShop.Controllers
{
    
    public class GuestController : Controller
    {

        private readonly GuestDbFunctions _guestDbFunctions;
        private readonly UserDbFunctions _userDbFunctions;

        public GuestController(GuestDbFunctions guestDbFunctions, UserDbFunctions userDbFunctions)
        {
            _guestDbFunctions = guestDbFunctions;
            _userDbFunctions = userDbFunctions;
        }

   
        public async Task<IActionResult> GuestHome()
        {
           

            // Veritaban�ndan kategorileri �ek
            var categories = await _guestDbFunctions.GuestGetCategoriesWithTypes();

            var products = await _guestDbFunctions.GuestGetProducts();

            var clickedProducts = products.OrderByDescending(p => p.Clicked).ToList();
            var favoritedSortedProducts = products.OrderByDescending(p => p.Favorites).ToList();
            var recentProducts = products.OrderByDescending(p => p.CreatedAt).ToList();
            var highStarProducts = products.OrderByDescending(p => p.Rating).ToList();
            var lessStockProducts = products.OrderBy(p => p.Stock).ToList();
            var bestsellerProducts = products.OrderByDescending(p => p.Sold).ToList();


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
                    MostClickedProducts = clickedProducts,
                    MostFavoritedProducts = favoritedSortedProducts,
                    RecentProducts = recentProducts,
                    HighStarProducts = highStarProducts,
                    LessStockProducts = lessStockProducts,
                    BestsellerProducts = bestsellerProducts,
                },
                AllCompanies = companies
            };

            return View("GuestHome", productviewModel);
        }






        // --------------------------------------------------------------------------------------------------------------------------
        public async Task<IActionResult> ProductDetails(int ProductId)
        {
            try
            {

                var productViewModel = await _userDbFunctions.UserGetProductDetails(ProductId);
                var categories = await _guestDbFunctions.GuestGetCategoriesWithTypes();
                var otherproducts = await _guestDbFunctions.GuestGetProducts();



                productViewModel.OtherProducts = otherproducts;
                productViewModel.Categories = categories;

                productViewModel.userId = 0;

                return View(productViewModel);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);

            }

            return RedirectToAction("GuestHome");
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

                
                var productviewModel = new ProductViewModel
                {
                    Categories = categories,
                    AllProducts = Allproducts,
                };

                ViewBag.Category = CategoryName;
                ViewBag.Type = type;
                productviewModel.userId = 0;

                return View(productviewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return RedirectToAction("Error", "Home");
            }
        }


        // --------------------------------------------------------------------------------------------------------------------------
        public async Task<IActionResult> AllProducts()
        {

            var AllProducts = await _userDbFunctions.GetAllProducts();
            var categories = await _guestDbFunctions.GuestGetCategoriesWithTypes();


            var productviewModel = new ProductViewModel
            {
                Categories = categories,
                AllProducts = AllProducts,
            };
            productviewModel.userId = 0;
            return View(productviewModel);
        }


        // --------------------------------------------------------------------------------------------------------------------------
        public async Task<IActionResult> CompanyDetails(int companyId)
        {

            // CompanyDetails metodunu çağır ve dönen ProductViewModel'i al
            var companyInfos = await _userDbFunctions.CompanyDetails(companyId);
            // Kategorileri al
            var categories = await _guestDbFunctions.GuestGetCategoriesWithTypes();


            companyInfos.Categories = categories;

            companyInfos.userId = 0;

            // Görüntüleme için view'e gönder
            return View(companyInfos);
        }


    }


}
