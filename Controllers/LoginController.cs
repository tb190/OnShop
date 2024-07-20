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
                _userDbFunctions.RegisterIndividual(viewModel.User);


                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Failed to sign up: " + ex.Message;
                return RedirectToAction("Login");
            }
        }


        [HttpPost]
        public async Task<IActionResult> RegisterCompany(LoginViewModel viewModel)
        {
            try
            {



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
