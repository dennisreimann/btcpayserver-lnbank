using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using LNbank.Data.Models;
using LNbank.Services.Settings;
using LNbank.Services.Users;
using LNbank.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LNbank.Controllers
{
    public sealed class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly SettingsService _settingsService;
        private readonly UserService _userService;

        private static readonly string[] RequiredPermissions = {"btcpay.user.canviewprofile"};

        public AccountController(
            ILogger<AccountController> logger,
            SettingsService settingsService,
            UserService userService)
        {
            _logger = logger;
            _userService = userService;
            _settingsService = settingsService;
        }

        [AllowAnonymous]
        [HttpGet("~/Login")]
        public ActionResult Login()
        {
            var appName = _settingsService.App.Name;
            var appIdentifier = appName.GenerateSlug();
            var endpoint = _settingsService.BtcPay.Endpoint;
            var redirect = $"{Request.Scheme}://{Request.Host}/Login";
            UriBuilder uriBuilder = new UriBuilder(endpoint)
            {
                Path = "api-keys/authorize",
                Query = $"applicationName={appName}&applicationIdentifier={appIdentifier}&redirect={redirect}"
            };
            uriBuilder.Query += RequiredPermissions.Aggregate("", (res, p) => res + $"&permissions={p}");

            return Redirect(uriBuilder.ToString());
        }

        [AllowAnonymous]
        [HttpPost("~/Login")]
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

            var scheme = CookieAuthenticationDefaults.AuthenticationScheme;
            var claimsIdentity = new ClaimsIdentity(claims, scheme);
            var principal = new ClaimsPrincipal(claimsIdentity);

            await HttpContext.SignInAsync(scheme, principal);

            return RedirectToPage("/Wallets/Index");
        }

        [HttpPost("~/Logout")]
        public async Task<ActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return Redirect("/");
        }
    }
}
