using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Auth
{
    // Simple header-based auth for demo/testing.
    // Expects headers:
    // - X-User: username
    // - X-User-Role: role (e.g., "Admin" or "User")
    public class HeaderAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public HeaderAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Check for header
            if (!Request.Headers.TryGetValue("X-User", out var username) || string.IsNullOrWhiteSpace(username))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var role = "User";
            if (Request.Headers.TryGetValue("X-User-Role", out var roleHeader) && !string.IsNullOrWhiteSpace(roleHeader))
            {
                role = roleHeader.ToString();
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username.ToString()),
                new Claim(ClaimTypes.Role, role)
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
