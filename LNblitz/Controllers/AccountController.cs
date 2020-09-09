using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using BTCPayServer.Client;
using LNblitz.Data;
using LNblitz.Data.Models;
using LNblitz.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace LNblitz.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IOptions<LNblitzConfiguration> _config;

        public AccountController(
            ApplicationDbContext dbContext,
            IOptions<LNblitzConfiguration> config)
        {
            _dbContext = dbContext;
            _config = config;
        }

        [AllowAnonymous]
        [HttpGet("~/login")]
        public ActionResult Login()
        {
            var appName = _config.Value.AppName;
            var appIdentifier = appName.ToLower();
            var permissions = "unrestricted"; // TODO: Restrict permissions
            var redirect = $"{Request.Scheme}://{Request.Host}/login-callback";

            UriBuilder uriBuilder = new UriBuilder(_config.Value.Endpoint)
            {
                Path = "api-keys/authorize",
                Query = $"applicationName={appName}&applicationIdentifier={appIdentifier}&permissions={permissions}&redirect={redirect}"
            };
            return Redirect(uriBuilder.ToString());
        }

        [AllowAnonymous]
        [HttpPost("~/login-callback")]
        public async Task<IActionResult> LoginCallback(string key, string user)
        {
            var client = new BTCPayServerClient(_config.Value.Endpoint, key);
            var result = await client.GetCurrentUser();

            if (result.Id != user)
            {
                // TODO: Inform user about what's wrong
                return BadRequest();
            }

            // TODO: Check permission suffice

            var ourUser = await _dbContext.Users.SingleOrDefaultAsync(u => u.UserId == user);

            if (ourUser != null)
            {
                if (ourUser.AccessToken != key)
                {
                    ourUser.AccessToken = key;
                    await _dbContext.SaveChangesAsync();
                }
            }
            else
            {
                ourUser = new User
                {
                    BTCPayUserId = user,
                    AccessToken = key
                };

                await _dbContext.Users.AddAsync(ourUser);
                await _dbContext.SaveChangesAsync();
            }

            var claims = new List<Claim>
            {
                new Claim("UserId", ourUser.UserId),
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // TODO: https://docs.microsoft.com/en-us/aspnet/core/security/authentication/cookie?view=aspnetcore-3.1#create-an-authentication-cookie
            var authProperties = new AuthenticationProperties();

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return RedirectToPage("/Wallets/Index");
        }

        [HttpPost("~/logout")]
        public async Task<ActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return Redirect("/");
        }
    }
}
