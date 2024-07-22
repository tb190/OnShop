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

        public async Task<IActionResult> UserHome()
        {

            int? userId = HttpContext.Session.GetInt32("UserId");


            // Veritabanýndan kategorileri çek
            var categories = await _guestDbFunctions.GuestGetCategoriesWithTypes();

            var products = await _guestDbFunctions.GuestGetProducts();

            var sortedProducts = products.OrderByDescending(p => p.Clicked).ToList();

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
                }
            };
                          
            return View("UserHome", productviewModel);
        }


        public async Task<IActionResult> ProductDetails(int ProductId)
        {
            try
            {
                var productViewModel = await _userDbFunctions.GuestGetProductDetails(ProductId);

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



    }


}
