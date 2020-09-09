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

namespace LNblitz.Pages
{
    [AllowAnonymous]
    public class BTCPayAccountHandler : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IOptions<LNblitzConfiguration> _config;

        public BTCPayAccountHandler(
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
            return Redirect("https://localhost:14142/api-keys/authorize?applicationName=LNBlitz&applicationIdentifier=lnblitz&permissions=unrestricted&redirect=https://localhost:5001/btcpay-login-callback");
        }

        [HttpPost("~/logout")]
        public async Task<ActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return Redirect("/");
        }

        [AllowAnonymous]
        [HttpPost("~/btcpay-login-callback")]
        public async Task<IActionResult> LoginCallback(string key, string[] permissions, string user)
        {
            var client = new BTCPayServerClient(_config.Value.Endpoint, key);
            var result = await client.GetCurrentUser();

            if (result.Id != user)
            {
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

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // TODO: https://docs.microsoft.com/en-us/aspnet/core/security/authentication/cookie?view=aspnetcore-3.1#create-an-authentication-cookie

            var authProperties = new AuthenticationProperties();

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return RedirectToPage("/Index");
        }
    }
}
