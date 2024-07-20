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
        public async Task<IActionResult> RegisterIndividual(LoginViewModel viewModel)
        {
            try
            {
                _userDbFunctions.RegisterIndividual(viewModel.User,"User");


                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Failed to sign up: " + ex.Message;
                return RedirectToAction("Login");
            }
        }


        [HttpPost]
        public async Task<IActionResult> RegisterCompany(IFormFile companyLogo, IFormFile companyBanner, LoginViewModel viewModel)
        {
            try
            {
                _userDbFunctions.RegisterCompany(companyLogo, companyBanner, viewModel.Company, viewModel.User);


                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Failed to sign up: " + ex.Message;
                return RedirectToAction("Login");
            }
        }


        [HttpPost]
        public async Task<IActionResult> LoginLogin()
        {
            try
            {
                // Form verilerini al
                string email = Request.Form["email"];
                string password = Request.Form["password"];

                bool isValidUser = await _userDbFunctions.ValidateUserCredentials(email, password);

                if (isValidUser)
                {
                    // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);  bununla alýcaksýn diger yerlerde
                    int userId = await _userDbFunctions.GetUserIdByEmail(email);

                    HttpContext.Session.SetInt32("UserId", userId);

                    return RedirectToAction("GuestHome", "Guest");// Kullanýcý doðrulandý, baþarýlý bir þekilde yönlendirme
                }
                else
                {
                    TempData["ErrorMessage"] = "Invalid email or password.";// Kullanýcý doðrulanamadý, hata mesajý
                    return RedirectToAction("Login");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Failed to log in: " + ex.Message;
                return RedirectToAction("Login");
            }
        }
    }


}
