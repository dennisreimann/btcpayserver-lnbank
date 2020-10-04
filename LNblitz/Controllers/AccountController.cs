using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using LNblitz.Data.Models;
using LNblitz.Services.Users;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LNblitz.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly IOptions<LNblitzConfiguration> _config;
        private readonly UserService _userService;

        private static readonly string[] RequiredPermissions = {"btcpay.user.canviewprofile"};

        public AccountController(
            ILogger<AccountController> logger,
            IOptions<LNblitzConfiguration> config,
            UserService userService)
        {
            _logger = logger;
            _config = config;
            _userService = userService;
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
            bool authorized = RequiredPermissions.All(p => permissions.Contains(p));

            // TODO: inform user in case of errors
            if (!authorized)
            {
                return Unauthorized();
            }

            User user;
            try
            {
                user = await _userService.CreateOrUpdateBtcPayUser(userId, apiKey);
            }
            catch (Exception exception)
            {
                _logger.LogError($"Login failed! {exception}");
                return BadRequest();
            }

            var claims = new List<Claim>
            {
                new Claim("UserId", user.UserId),
                new Claim("IsAdmin", user.BTCPayIsAdmin.ToString()),
            };

            // TODO: https://docs.microsoft.com/en-us/aspnet/core/security/authentication/cookie?view=aspnetcore-3.1#create-an-authentication-cookie
            var authProperties = new AuthenticationProperties();
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

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
