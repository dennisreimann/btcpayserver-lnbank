using System;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using LNbank.Data.Models;
using LNbank.Services.Users;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LNbank.Authentication
{
    public class BTCPayAPIKeyAuthenticationHandler : BaseAuthenticationHandler
    {
        public BTCPayAPIKeyAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, UserService userService) : base(options, logger, encoder, clock, userService)
        {
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            string authHeader = Context.Request.Headers["Authorization"];
            if (authHeader == null || !authHeader.StartsWith("Bearer ", StringComparison.InvariantCultureIgnoreCase))
                return AuthenticateResult.NoResult();

            string apiKey = authHeader.Substring("Bearer ".Length);

            try
            {
                User user = await FindUserByBtcPayApiKey(apiKey);
                return AuthenticateUser(user, AuthenticationSchemes.ApiBTCPayAPIKey);
            }
            catch (Exception exception)
            {
                return AuthenticateResult.Fail($"Authentication failed! {exception}");
            }
        }
    }
}
