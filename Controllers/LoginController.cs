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
using Microsoft.AspNetCore.Authorization;

namespace OnShop.Controllers
{

    public class LoginController : Controller
    {

        private readonly UserDbFunctions _userDbFunctions;

        public LoginController(UserDbFunctions userDbFunctions)
        {
            _userDbFunctions = userDbFunctions;
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
                int id = await _userDbFunctions.GetUserIdByEmail(viewModel.User.Email);
                if (id != -1) // bu email alınmış
                {
                    TempData["ErrorMessage"] = "This email has already been registered!";
                    return RedirectToAction("Login");
                }

                if (await _userDbFunctions.RegisterIndividual(viewModel.User, "User"))
                {
                    return RedirectToAction("Login");
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
                int id = await _userDbFunctions.GetUserIdByEmail(viewModel.User.Email);
                if (id != -1) // bu email alınmış
                {
                    TempData["ErrorMessage"] = "This email has already been registered!";
                    return RedirectToAction("Login");
                }

                if (await _userDbFunctions.RegisterCompany(LogoUrl, BannerUrl, viewModel.Company, viewModel.User))
                {
                    return RedirectToAction("Login");
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

                    if (role == "Vendor") {
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Role, "Vendor")
                        };

                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
                        return RedirectToAction("VendorHome", "Vendor"); 
                    
                    }
                    else if (role == "Admin"){
                        var claims = new List<Claim>
                         {
                             new Claim(ClaimTypes.Role, "Admin")
                         };

                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
                        return RedirectToAction("AdminDashBoard", "Admin"); 
                    }
                    else
                    {
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Role, "User")
                        };

                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
                        return RedirectToAction("UserHome", "User");// Kullan�c� do�ruland�, ba�ar�l� bir �ekilde y�nlendirme
                    }               
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