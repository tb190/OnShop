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

namespace OnShop.Controllers
{
    
    public class GuestController : Controller
    {

        private readonly GuestDbFunctions _guestDbFunctions;
        private readonly UserDbFunctions _userDbFunctions;

        public GuestController()
        {
            _guestDbFunctions = new GuestDbFunctions();
            _userDbFunctions = new UserDbFunctions();
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
    }

  
}
