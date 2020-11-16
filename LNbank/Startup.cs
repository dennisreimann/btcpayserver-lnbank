using System;
using BTCPayServer.Client;
using Hellang.Middleware.ProblemDetails;
using LNbank.Configuration;
using LNbank.Data;
using LNbank.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using LNbank.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

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
            services.AddAppAuthentication();
            services.AddAppAuthorization();

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
            services.AddControllersWithViews()
                    .AddNewtonsoftJson();
            services.AddProblemDetails(options =>
            {
                options.MapToStatusCode<GreenFieldValidationException>(StatusCodes.Status422UnprocessableEntity);
                options.MapToStatusCode<GreenFieldAPIException>(StatusCodes.Status400BadRequest);
            });

            IMvcBuilder builder = services.AddRazorPages(options =>
            {
                options.Conventions.AllowAnonymousToPage("/Settings/New");
            });

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
            app.UseProblemDetails();
            app.UseHttpsRedirection();
            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    // Cache static assets for one year, set asp-append-version="true" on references to update on change.
                    // https://andrewlock.net/adding-cache-control-headers-to-static-files-in-asp-net-core/
                    const int durationInSeconds = 60 * 60 * 24 * 365;
                    ctx.Context.Response.Headers[HeaderNames.CacheControl] = "public,max-age=" + durationInSeconds;
                }
            });
            app.UseStatusCodePagesWithReExecute("/StatusCode/{0}");
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapHub<TransactionHub>("/Hubs/Transaction");
            });
        }
    }
}
