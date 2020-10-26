using System;
using LNbank.Configuration;
using LNbank.Data;
using LNbank.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using LNbank.Extensions;

namespace LNbank
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Env { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Env = env;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var appOptions = new AppOptions(Configuration);

            services.AddAppServices(appOptions);
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                    {
                        options.LoginPath = "/";
                        options.LogoutPath = "/logout";
                        options.Cookie.Name = "LNbank";
                    });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireAdmin", policy => policy.RequireClaim("IsAdmin"));
                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
            });
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                switch (appOptions.DatabaseType)
                {
                    case DatabaseType.Sqlite:
                        options.UseSqlite(appOptions.DatabaseConnectionString);
                        break;
                    case DatabaseType.Postgres:
                        options.UseNpgsql(appOptions.DatabaseConnectionString);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            });
            services.AddControllersWithViews();

            IMvcBuilder builder = services.AddRazorPages();

            if (Env.IsDevelopment())
            {
                builder.AddRazorRuntimeCompilation();
            }

            services.AddSignalR();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            if (Env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days.
                // You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseStatusCodePagesWithReExecute("/StatusCode/{0}");
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapHub<InvoiceHub>("/InvoiceHub");
            });
        }
    }
}
