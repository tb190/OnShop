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

    public class LoginController : Controller
    {

        private readonly UserDbFunctions _userDbFunctions;

        public LoginController()
        {
            _userDbFunctions = new UserDbFunctions();
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegisterIndividual(AdminViewModel viewModel)
        {
            try
            {
                if (await _userDbFunctions.RegisterIndividual(viewModel.User, "User"))
                {
                    return RedirectToAction("GuestHome", "Guest");
                }
                return RedirectToAction("Login");


            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Failed to sign up: " + ex.Message;
                return RedirectToAction("Login");
            }
        }


        [HttpPost]
        public async Task<IActionResult> RegisterCompany(IFormFile LogoUrl, IFormFile BannerUrl, AdminViewModel viewModel)
        {
            try
            {
                if (await _userDbFunctions.RegisterCompany(LogoUrl, BannerUrl, viewModel.Company, viewModel.User))
                {
                    return RedirectToAction("GuestHome", "Guest");
                }
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Failed to sign up: " + ex.Message;
                return RedirectToAction("Login");
            }
        }


        [HttpPost]
        public async Task<IActionResult> LoginLogin(string email,string password)
        {
            try
            {
                // Form verilerini al
               /* string email = Request.Form["email"];
                string password = Request.Form["password"];*/

                var result = await _userDbFunctions.ValidateUserCredentials(email, password);
                string validationResult = result.Message;
                string role = result.Role;


                if (validationResult == "User validated successfully.")
                {
                    // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);  bununla al�caks�n diger yerlerde
                    int userId = await _userDbFunctions.GetUserIdByEmail(email);

                    HttpContext.Session.SetInt32("UserId", userId);

                    if (userId == null)
                    {
                        HttpContext.Session.Remove("UserId");
                        return RedirectToAction("Login", "Login"); // Kullanıcı giriş yapmamışsa login sayfasına yönlendir
                    }

                    if (role == "Vendor") return RedirectToAction("VendorHome", "Vendor");
                    else if (role == "Admin") return RedirectToAction("AdminHome", "Admin");
                    return RedirectToAction("UserHome", "User");// Kullan�c� do�ruland�, ba�ar�l� bir �ekilde y�nlendirme
                }
                else
                {
                    TempData["ErrorMessage"] = validationResult;// Kullan�c� do�rulanamad�, hata mesaj�
                    return RedirectToAction("Login");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Failed to log in: " + ex.Message;
                return RedirectToAction("Login");
            }
        }


        public async Task<IActionResult> LogOut()
        {
            try
            {
                // Kullan�c� oturumunu sonland�r
                HttpContext.Session.Remove("UserId");
                // Kullan�c�y� giri� sayfas�na y�nlendir
                return RedirectToAction("GuestHome", "Guest");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Failed to log out: " + ex.Message;
                return RedirectToAction("AdminHome", "Admin");
            }
        }



    }


}