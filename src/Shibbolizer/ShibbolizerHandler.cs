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
        const string Issuer = "Shibbolizer";

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (string.IsNullOrWhiteSpace(Options.UsernameHeader))
                throw new InvalidOperationException($"No {nameof(Options.UsernameHeader)} configured.");

            if (string.IsNullOrWhiteSpace(Request.Headers[Options.UsernameHeader]))
                return AuthenticateResult.Fail($"Username header: {Options.UsernameHeader} does not exist or contains no value.");

            var claims = Options.ClaimHeaders
                .Where(ch => Request.Headers.Select(h => h.Key).Contains(ch))
                .Select(ch => new Claim(ch, Request.Headers[ch], ClaimValueTypes.String, Issuer))
                .Union(Options.MultiClaimHeaders
                    .Where(mch => Request.Headers.Select(h => h.Key).Contains(mch.Header))
                    .SelectMany(mch => mch.Parser(Request.Headers[mch.Header])
                        .Select(s => new Claim(mch.Header, s, ClaimValueTypes.String, Issuer))));

            var userIdentity = new ClaimsIdentity(claims, Issuer);

            var ticket = new AuthenticationTicket(new ClaimsPrincipal(userIdentity), new AuthenticationProperties(), Options.AuthenticationScheme);
            return AuthenticateResult.Success(ticket);
        }
    }
}