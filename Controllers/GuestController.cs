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
            // Veritabanýndan kategorileri çek
            var categories = _guestDbFunctions.GuestGetCategoriesWithTypes();

            var products = _guestDbFunctions.GuestGetProducts();
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
