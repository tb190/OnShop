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
            List<ProductModel> products = _vendorDbFunctions.VendorGetProducts();
            
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

            ViewBag.CurrentFilter = statusFilter;
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
        public async Task<IActionResult> AddNewProduct(List<IFormFile> Photos,ProductModel model)
        {
            try
            {
                if (Photos.Count > 0)
                {              
                    int productId = await _vendorDbFunctions.VendorAddProduct(model,Photos);

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
    }
}
