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

    public class AdminController : Controller
    {

        private readonly AdminDbFunctions _adminDbFunctions;

        public AdminController()
        {
            _adminDbFunctions = new AdminDbFunctions();
        }

        public IActionResult AdminHome()
        {
            //int? userId = HttpContext.Session.GetInt32("UserId");

            return View();
        }

        public async Task<IActionResult> AdminCompanies(int page = 1, string searchString = "", string validationFilter = "")
        {
            try
            {
                // Tüm þirket ve kullanýcý bilgilerini al
                List<LoginViewModel> companiesWithUsers = await _adminDbFunctions.GetAllCompaniesWithUsers();

                
                var unvalidatedCount = companiesWithUsers.Count(c => !c.Company.isValidatedbyAdmin);
                ViewBag.UnvalidatedCount = unvalidatedCount;


                var companiesWithUsersQuery = companiesWithUsers.AsQueryable();

                if (!string.IsNullOrEmpty(searchString))
                {
                    companiesWithUsersQuery = companiesWithUsersQuery.Where(p =>
                        p.Company.CompanyName.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                        p.Company.CompanyDescription.Contains(searchString, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrEmpty(validationFilter))
                {
                    if (validationFilter.Equals("validated", StringComparison.OrdinalIgnoreCase))
                    {
                        bool isValidated = true;
                        companiesWithUsersQuery = companiesWithUsersQuery.Where(p => p.Company.isValidatedbyAdmin == isValidated);
                    }
                    else if (validationFilter.Equals("unvalidated", StringComparison.OrdinalIgnoreCase))
                    {
                        bool isValidated = false;
                        companiesWithUsersQuery = companiesWithUsersQuery.Where(p => p.Company.isValidatedbyAdmin == isValidated);
                    }            
                }


                const int pageSize = 5;
                int totalcompaniesWithUsers = companiesWithUsersQuery.Count();
                var paginatedcompaniesWithUsers = companiesWithUsersQuery.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                ViewBag.CurrentFilter = validationFilter;
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = (int)Math.Ceiling((double)totalcompaniesWithUsers / pageSize);
                ViewBag.SearchString = searchString; // Pass searchString to keep it in the input field


                return View(paginatedcompaniesWithUsers);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Failed to retrieve companies: " + ex.Message;
                return View(new List<LoginViewModel>()); // Hata durumunda boþ bir liste gönder
            }
        }
    }
}
