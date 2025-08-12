using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RPA.Web.Services;

namespace RPA.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILoginServices _loginService;
        public AccountController(ILoginServices loginServices)
        {
            _loginService = loginServices;
        }
        [AllowAnonymous]
        public IActionResult Index()
        {
            return RedirectToAction("login", "home");
        }

        [AllowAnonymous]
        [Route("login")]
        public IActionResult Login()
        {
            return View();
        }
        [AllowAnonymous]
        [Route("login")]
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var loginResult = await _loginService.LoginHandler(username, password);
            if (!loginResult.Result)
            {
                TempData["LoginMessage"] = loginResult.Message;
                return RedirectToAction("login", "account");
            }
            return RedirectToAction("index", "home");

        }
        [AllowAnonymous]
        [Route("logout")]
        public async Task<IActionResult> Logout()
        {
            await _loginService.LogOutHandler();
            return RedirectToAction("index", "home");
        }
        public IActionResult SetLanguage(string culture)
        {
            CookieOptions option = new CookieOptions
            {
                Expires = DateTime.Now.AddDays(365)
            };
            Response.Cookies.Append("Language", culture.ToString(), option);

            return Redirect(Request.Headers["Referer"].ToString());
        }
    }
}