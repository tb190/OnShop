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

        public LoginController()
        {
            
        }

        public IActionResult Login()
        {
            

            return View();
        }
    }


}
