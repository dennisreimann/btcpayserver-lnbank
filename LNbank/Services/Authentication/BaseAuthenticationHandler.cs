using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using LNbank.Data.Models;
using LNbank.Services.Users;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LNbank.Services.Authentication
{
    public abstract class BaseAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly UserService _userService;

        public BaseAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            UserService userService) : base(options, logger, encoder, clock)
        {
            _userService = userService;
        }

        protected async Task<User> CreateOrUpdateBtcPayUser(string userId, string apiKey)
        {
            return await _userService.CreateOrUpdateBtcPayUser(userId, apiKey);
        }

        protected async Task<User> FindUserByBtcPayApiKey(string apiKey)
        {
            return await _userService.FindUserByBtcPayApiKey(apiKey);
        }

        protected AuthenticateResult AuthenticateUser(User user)
        {
            var claims = new List<Claim>
            {
                new Claim("UserId", user.UserId),
                new Claim("IsAdmin", user.BTCPayIsAdmin.ToString()),
            };

            var scheme = AuthenticationSchemes.ApiBasic;
            var claimsIdentity = new ClaimsIdentity(claims, scheme);
            var principal = new ClaimsPrincipal(claimsIdentity);
            var ticket = new AuthenticationTicket( principal, scheme);

            return AuthenticateResult.Success(ticket);
        }
    }
}
