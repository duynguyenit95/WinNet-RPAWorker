using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.DataProtection;
using RPA.Core.Data;
using RPA.Web.Context;
using RPA.Web.Common;
using RPA.Web.Data;
using RPA.Web.Hubs;
using RPA.Web.Services;
using PCenter.APIServer.Exceptions;
using Utility;

namespace RPA.Web
{
    public class Startup
    {
        private readonly IWebHostEnvironment webHostEnvironment;

        public Startup(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            Configuration = configuration;
            this.webHostEnvironment = webHostEnvironment;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<RPAContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("RPAContext")));
            services.AddDbContext<ORPContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("ORPContext")));
            services.AddDbContext<UserContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("UserConnection")));
            services.AddDbContext<RPAInvoiceContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("RPAInvoiceContext")));

            // cookie Authen
            var authFolder = Configuration["Authentication"];// "E:\\KeyAuth\\";
            services.AddDataProtection().PersistKeysToFileSystem(new System.IO.DirectoryInfo(authFolder)).SetApplicationName("NETCORE");
            services.AddAuthentication(AuthenticationTypes.ApplicationCookie).AddCookie(AuthenticationTypes.ApplicationCookie, options =>
            {
                options.Cookie = new CookieBuilder
                {
                    HttpOnly = true,
                    Name = ".ROSAuth.Cookie",
                    Path = "/",
                    IsEssential = true
                };
                options.LoginPath = "/login";
                options.ExpireTimeSpan = TimeSpan.FromDays(7);
                options.SlidingExpiration = true;
                options.ReturnUrlParameter = "path";
            });


            services.ConfigureApplicationCookie(options =>
            {
                options.AccessDeniedPath = new PathString("/403");
                options.LoginPath = new PathString("/login");
                options.LogoutPath = new PathString("/logout");
            });

            //keep format json
            services.AddControllersWithViews(options =>
            {
                options.Filters.Add<HttpResponseExceptionFilter>();
            });

            // session
            services.AddSession(options =>
            {
                options.Cookie.Name = ".RPAAuth.Session";
                options.IdleTimeout = TimeSpan.FromDays(7);
                options.Cookie.IsEssential = true;
            });

            // Hub 
            services.AddSingleton<EmailSender>();
            services.AddSingleton<WorkerConnectionFactory>();



            services.AddSingleton<IRegexParserServices, RegexParserServices>();
            services.AddScoped<IUserServices, UserServices>();

            services.AddTransient<ILoginServices, LoginServices>();
            services.AddTransient<IFormRecognizerServices, FormRecognizerServices>();

            services.AddSignalR(o => o.EnableDetailedErrors = true);
            services.AddHttpContextAccessor();
            services.AddCors();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDeveloperExceptionPage();
            app.UseStaticFiles();
            app.UseSession();
            app.UseRouting();
            app.UseCors(x => x.AllowAnyHeader().AllowAnyOrigin().AllowAnyHeader());


            app.UseAuthentication(); //  use authentication
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Hub}/{action=Index}/{id?}");
                endpoints.MapHub<WorkerHubBase>("/rpahub");
            });
        }
    }
}
