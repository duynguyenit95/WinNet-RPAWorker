using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RPA.Web.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RPA.Core.Data;

namespace RPA.Web.Common
{
    public class ReportRequestLogging : ActionFilterAttribute
    {

        public ReportRequestLogging()
        {
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var config = filterContext.HttpContext.RequestServices.GetService<IConfiguration>();
            var _dbConStr = config.GetConnectionString("RPAContext");
            var request = filterContext.HttpContext.Request;
            var userID = filterContext.HttpContext.User.Identity.Name;
            Task.Run(() =>
            {
                var controller = request.RouteValues["controller"].ToString();
                var action = request.RouteValues["action"].ToString();
                var queryString = request.QueryString.Value;
                var formStr = string.Empty;
                var path = request.Path;
                foreach (var key in request.Form.Keys)
                {
                    formStr += $"{key}:{request.Form[key]}";
                }

                var newRequestLog = new RequestLog()
                {
                    Action = action,
                    Controller = controller,
                    QueryString = queryString,
                    FormString = formStr,
                    User = userID,
                    QueryURL = path
                };


                using (var context = new RPAContext(RPAContext.UseConnectionString(_dbConStr)))
                {
                    context.RequestLogs.Add(newRequestLog);
                    context.SaveChanges();
                };
            });

        }
    }

}

