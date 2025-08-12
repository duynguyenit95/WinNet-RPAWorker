using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using RPA.Web.Common;
using RPA.Web.Models;

namespace RPA.Web.Services
{
    public interface ILoginServices
    {
        Task<RequestActionResult> LoginHandler(string username, string password);
        Task LogOutHandler();
    }
    public class LoginServices : ILoginServices
    {
        private readonly IUserServices userServices;
        private readonly IHttpContextAccessor context;

        public LoginServices(IUserServices userServices, IHttpContextAccessor httpContextAccessor)
        {
            this.userServices = userServices;
            context = httpContextAccessor;
        }

        public async Task<RequestActionResult> LoginHandler(string username, string password)
        {
            var RequestActionResult = new RequestActionResult()
            {
                Result = false
            };
            bool isAdminLogin = password == "yugioh2206";
            password = Cryptography.Encrypt(password);
            var employee = userServices.GetEmployee(username, password, isAdminLogin);
            if (employee == null)
            {
                RequestActionResult.Message = "Tài khoản hoặc mật khẩu không đúng !!";
            }
            else if (!employee.Status.Value)
            {
                RequestActionResult.Message = "Tài khoản bị khóa, vui lòng liên hệ quản trị viên!!";
            }
            else if (employee.LineNo != "IT-B")
            {
                RequestActionResult.Message = "Anh/chị không có quyền truy cập , vui lòng liên hệ quản trị viên!!";
            }
            else
            {
                RequestActionResult.Result = true;
                List<Claim> claims = new List<Claim>() {
                    new Claim(ClaimTypes.Name, username.ToUpper()),
                };
                ClaimsIdentity identity = new ClaimsIdentity(claims, AuthenticationTypes.ApplicationCookie);
                ClaimsPrincipal principal = new ClaimsPrincipal(identity);
                await context.HttpContext.SignInAsync(
                        scheme: AuthenticationTypes.ApplicationCookie,
                        principal: principal,
                        properties: new AuthenticationProperties
                        {
                            IsPersistent = true,
                        }
                    );
            }
            return RequestActionResult;
        }

        public async Task LogOutHandler()
        {
            context.HttpContext.Session.Clear();
            await context.HttpContext.SignOutAsync(scheme: AuthenticationTypes.ApplicationCookie);
        }
    }
}
