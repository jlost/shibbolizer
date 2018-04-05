using System;
using Microsoft.AspNetCore.Authentication;

namespace Shibbolizer
{
    public static class ShibbolizerAppBuilderExtensions
    {

        public static AuthenticationBuilder AddShibboleth(this AuthenticationBuilder builder)
            => builder.AddShibboleth(ShibbolizerDefaults.AuthenticationScheme);

        public static AuthenticationBuilder AddShibboleth(this AuthenticationBuilder builder, string authenticationScheme)
            => builder.AddShibboleth(authenticationScheme, configureOptions: null);

        public static AuthenticationBuilder AddShibboleth(this AuthenticationBuilder builder, Action<ShibbolizerOptions> configureOptions)
            => builder.AddShibboleth(ShibbolizerDefaults.AuthenticationScheme, configureOptions);

        public static AuthenticationBuilder AddShibboleth(this AuthenticationBuilder builder, string authenticationScheme, Action<ShibbolizerOptions> configureOptions)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (configureOptions == null)
                throw new ArgumentNullException(nameof(configureOptions));

            ShibbolizerOptions opts = new ShibbolizerOptions();
            configureOptions(opts);
 
            if (string.IsNullOrWhiteSpace(opts.UsernameHeader))
                throw new ArgumentNullException(opts.UsernameHeader);

            return builder.AddScheme<ShibbolizerOptions, ShibbolizerHandler>(authenticationScheme, configureOptions);
        }

    }
}
