using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System;
using Utility;

namespace PCenter.APIServer.Exceptions
{
    public class HttpResponseException : Exception
    {
#pragma warning disable CS8618,CS8600 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public HttpResponseException(int statusCode, object? value = null)
        {
            (StatusCode, Value) = (statusCode, value);
        }
#pragma warning restore CS8600,CS8618 // Converting null literal or possible null value to non-nullable type.
        public int StatusCode { get; }

        public object Value { get; }
    }


    public class HttpResponseExceptionFilter : IActionFilter, IOrderedFilter
    {
        private readonly EmailSender _emailSender;

        //private readonly ILocalizer localizer;

        public HttpResponseExceptionFilter(EmailSender emailSender) //ILocalizer localizer
        {
            this._emailSender = emailSender;
            //  this.localizer = localizer;
        }
        public int Order => int.MaxValue - 10;

        public void OnActionExecuting(ActionExecutingContext context) { }

        public void OnActionExecuted(ActionExecutedContext context)
        {
#if DEBUG
            return;
#endif
#pragma warning disable CS0162 // Unreachable code detected
            if (context.Exception == null) return;



            if (context.Exception is ValidationException validation)
            {
                context.Result = new BadRequestObjectResult(validation.Message); //localizer[].Value
                context.ExceptionHandled = true;
            }

            //if (context.Exception is HttpResponseException httpResponseException)
            //{
            //    context.Result = new ObjectResult(httpResponseException.Value)
            //    {
            //        StatusCode = httpResponseException.StatusCode
            //    };
            //    context.ExceptionHandled = true;
            //}
            if (!context.ExceptionHandled)
            {
                var request = context.HttpContext.Request;
                var queryString = request.QueryString.Value;
                var formStr = string.Empty;
                if(request.HasFormContentType)
                {
                    foreach (var key in request.Form.Keys)
                    {
                        formStr += $"{key}:{request.Form[key]}";
                    }
                }


                _emailSender.BasicEmail(new EmailOptions()
                {
                    Receivers = new[] { "darius.nguyen@reginamiracle.com" },
                    Content = $"<p>Action: {request.Path}</p>" +
                              $"<p>{queryString}</p>" +
                              $"<p>{formStr}</p>" +
                              $"<p>{context.Exception}</p>",
                    Subject = "[RPA.Web] Internal Server Error",
                });

                return;
            }
        }
    }
}
