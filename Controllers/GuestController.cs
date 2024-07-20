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

        public GuestController()
        {
            _guestDbFunctions = new GuestDbFunctions();
        }

        public IActionResult GuestHome()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");


            // Veritabanýndan kategorileri çek
            var categories = _guestDbFunctions.GuestGetCategoriesWithTypes();

            List<ProductModel> products = _guestDbFunctions.GuestGetProducts();

            var sortedProducts = products.OrderByDescending(p => p.Clicked).ToList();

            var viewModel = new GuestHomeViewModel
            {
                Categories = categories,
                Products = products,
                MostClickedProducts = sortedProducts
            };

            return View("GuestHome", viewModel);
        }
    }

  
}
