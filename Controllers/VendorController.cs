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
    public class VendorController : Controller
    {
        private readonly ILogger<VendorController> _logger;
        private readonly VendorDbFunctions _vendorDbFunctions;
        private readonly GuestDbFunctions _guestDbFunctions;
        private VendorViewModel vendor;



        public VendorController(ILogger<VendorController> logger)
        {
            _vendorDbFunctions = new VendorDbFunctions();
            _guestDbFunctions = new GuestDbFunctions();
            _logger = logger;

        }


        public async Task<IActionResult> VendorHome()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                HttpContext.Session.Remove("UserId");
                return RedirectToAction("Login", "Login"); // Kullanıcı giriş yapmamışsa login sayfasına yönlendir
            }

            vendor = await _vendorDbFunctions.GetVendor(userId);

            VendorViewModel modelDashboard = await _vendorDbFunctions.GetVendorDashBoard(userId);
            modelDashboard.VendorUserInfos = vendor.VendorUserInfos;
            modelDashboard.VendorCompanyInfos = vendor.VendorCompanyInfos;

            return View(modelDashboard);
        }

        // -------------------------------------- Products Page --------------------------------------
        public async Task<IActionResult> VendorProducts(int page = 1, string searchString = "", string statusFilter = "")
        {

            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                HttpContext.Session.Remove("UserId");
                return RedirectToAction("Login", "Login"); // Kullanıcı giriş yapmamışsa login sayfasına yönlendir
            }

            List<ProductModel> products = await _vendorDbFunctions.VendorGetProducts(userId);
            
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
            else if(!string.IsNullOrEmpty(statusFilter) && statusFilter.Equals("Offline", StringComparison.OrdinalIgnoreCase))
            {
                productsQuery = productsQuery.Where(p => p.Status.Equals("Offline", StringComparison.OrdinalIgnoreCase));
            }


            const int pageSize = 5;
            int totalProducts = productsQuery.Count();
            var paginatedProducts = productsQuery.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.CurrentFilter = statusFilter;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalProducts / pageSize);
            ViewBag.SearchString = searchString; // Pass searchString to keep it in the input field

            VendorViewModel model = new VendorViewModel();

            model.AllProducts = paginatedProducts;

            vendor = await _vendorDbFunctions.GetVendor(userId);
            model.VendorUserInfos = vendor.VendorUserInfos;
            model.VendorCompanyInfos = vendor.VendorCompanyInfos;

            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> VendorAddNewProduct()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                HttpContext.Session.Remove("UserId");
                return RedirectToAction("Login", "Login"); // Kullanıcı giriş yapmamışsa login sayfasına yönlendir
            }

            VendorViewModel model = new VendorViewModel();

            var categories = await _guestDbFunctions.GuestGetCategoriesWithTypes();
            model.AllCategoriesWithTypes = categories;
            vendor = await _vendorDbFunctions.GetVendor(userId);
            model.VendorUserInfos = vendor.VendorUserInfos;
            model.VendorCompanyInfos = vendor.VendorCompanyInfos;
            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> AddNewProduct(List<IFormFile> Photos,ProductModel model)
        {
            try
            {
                if (Photos.Count > 0)
                {
                    int? userId = HttpContext.Session.GetInt32("UserId");

                    if (userId == null)
                    {
                        HttpContext.Session.Remove("UserId");
                        return RedirectToAction("Login", "Login"); // Kullanıcı giriş yapmamışsa login sayfasına yönlendir
                    }


                    int productId = await _vendorDbFunctions.VendorAddProduct(userId,model, Photos);

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
                return View("VendorAddNewProduct", model);
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


        // -------------------------------------- Order Page --------------------------------------
        public async Task<IActionResult> VendorOrders()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                HttpContext.Session.Remove("UserId");
                return RedirectToAction("Login", "Login"); // Kullanıcı giriş yapmamışsa login sayfasına yönlendir
            }

            List<ProductModel> products = await _vendorDbFunctions.GetAllProducts(userId);

            VendorViewModel model = new VendorViewModel();

            model.AllProducts = products;
            vendor = await _vendorDbFunctions.GetVendor(userId);
            model.VendorUserInfos = vendor.VendorUserInfos;
            model.VendorCompanyInfos = vendor.VendorCompanyInfos;
            return View(model);
            
        }


        // -------------------------------------- Customers Page --------------------------------------
        public async Task<IActionResult> VendorCustomers()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                HttpContext.Session.Remove("UserId");
                return RedirectToAction("Login", "Login"); // Kullanıcı giriş yapmamışsa login sayfasına yönlendir
            }

            List<UserModel> users = await _vendorDbFunctions.GetAllUsers(userId);

            VendorViewModel model = new VendorViewModel();

            model.AllUsers = users;
            vendor = await _vendorDbFunctions.GetVendor(userId);
            model.VendorUserInfos = vendor.VendorUserInfos;
            model.VendorCompanyInfos = vendor.VendorCompanyInfos;
            return View(model);

        }


        // -------------------------------------- Followers Page --------------------------------------
        public async Task<IActionResult> VendorFollowers()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                HttpContext.Session.Remove("UserId");
                return RedirectToAction("Login", "Login"); // Kullanıcı giriş yapmamışsa login sayfasına yönlendir
            }

            List<UserModel> Followers = await _vendorDbFunctions.GetAllFollowers(userId);

            VendorViewModel model = new VendorViewModel();

            model.AllFollowers = Followers;
            vendor = await _vendorDbFunctions.GetVendor(userId);
            model.VendorUserInfos = vendor.VendorUserInfos;
            model.VendorCompanyInfos = vendor.VendorCompanyInfos;
            return View(model);

        }
    }
}
