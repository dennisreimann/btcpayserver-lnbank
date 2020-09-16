using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BTCPayServer.Client;
using BTCPayServer.Client.Models;
using LNblitz.Data;
using LNblitz.Data.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LNblitz.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly ApplicationDbContext _dbContext;
        private readonly IOptions<LNblitzConfiguration> _config;

        private static readonly string AdminRoleName = "ServerAdmin";
        private static readonly string[] RequiredPermissions = {"btcpay.user.canviewprofile"};

        public AccountController(
            ILogger<AccountController> logger,
            ApplicationDbContext dbContext,
            IOptions<LNblitzConfiguration> config)
        {
            _logger = logger;
            _dbContext = dbContext;
            _config = config;
        }

        [AllowAnonymous]
        [HttpGet("~/login")]
        public ActionResult Login()
        {
            var appName = _config.Value.AppName;
            var appIdentifier = appName.ToLower();
            var redirect = $"{Request.Scheme}://{Request.Host}/login-callback";

            UriBuilder uriBuilder = new UriBuilder(_config.Value.Endpoint)
            {
                Path = "api-keys/authorize",
                Query = $"applicationName={appName}&applicationIdentifier={appIdentifier}&redirect={redirect}"
            };
            uriBuilder.Query += RequiredPermissions.Aggregate("", (res, p) => res + $"&permissions={p}");
            return Redirect(uriBuilder.ToString());
        }

        [AllowAnonymous]
        [HttpPost("~/login-callback")]
        public async Task<IActionResult> LoginCallback(string apiKey, string userId, string[] permissions)
        {
            var client = new BTCPayServerClient(_config.Value.Endpoint, apiKey);
            bool authorized = RequiredPermissions.All(p => permissions.Contains(p));

            // TODO: inform user in case of errors
            if (!authorized)
            {
                return Unauthorized();
            }

            ApplicationUserData userData = null;
            try
            {
                userData = await client.GetCurrentUser();
            }
            catch (Exception exception)
            {
                _logger.LogError($"GetCurrentUser failed! {exception}");
            }

            if (userData?.Id != userId)
            {
                return BadRequest();
            }

            var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.BTCPayUserId == userId);
            bool isAdmin = true; // TODO: Use upcoming userData.Roles.Contains(AdminRoleName);

            if (user != null)
            {
                var entry = _dbContext.Entry(user);
                user.BTCPayApiKey = apiKey;
                user.BTCPayIsAdmin = isAdmin;
                entry.State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();
            }
            else
            {
                user = new User
                {
                    BTCPayUserId = userId,
                    BTCPayApiKey = apiKey,
                    BTCPayIsAdmin = isAdmin
                };

                await _dbContext.Users.AddAsync(user);
                await _dbContext.SaveChangesAsync();
            }

            var claims = new List<Claim>
            {
                new Claim("UserId", user.UserId),
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
