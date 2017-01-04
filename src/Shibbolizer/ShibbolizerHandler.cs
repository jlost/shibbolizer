using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Authentication;

namespace Shibbolizer
{
    internal class ShibbolizerHandler : AuthenticationHandler<ShibbolizerOptions>
    {
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (string.IsNullOrWhiteSpace(Options.UsernameHeader))
                throw new InvalidOperationException($"No {nameof(Options.UsernameHeader)} configured.");

            var username = Request.Headers[Options.UsernameHeader];
            if (string.IsNullOrWhiteSpace(username))
                return AuthenticateResult.Fail($"Username header: {Options.UsernameHeader} does not exist or contains no value.");

            var claims = Options.ClaimHeaders
                .Where(ch => Request.Headers.Select(h => h.Key).Contains(ch))
                .Select(ch => new Claim(ch, Request.Headers[ch], ClaimValueTypes.String, Options.ClaimsIssuer))
                .Union(Options.MultiClaimHeaders
                    .Where(mch => Request.Headers.Select(h => h.Key).Contains(mch.Header))
                    .SelectMany(mch => mch.Parser(Request.Headers[mch.Header])
                        .Select(s => new Claim(mch.Header, s, ClaimValueTypes.String, Options.ClaimsIssuer))))
                .ToList();

            claims.Add(new Claim(ClaimTypes.Name, username));

            var userIdentity = new ClaimsIdentity(claims, Options.ClaimsIssuer);

            var ticket = new AuthenticationTicket(new ClaimsPrincipal(userIdentity), new AuthenticationProperties(), Options.AuthenticationScheme);
            return AuthenticateResult.Success(ticket);
        }
    }
}