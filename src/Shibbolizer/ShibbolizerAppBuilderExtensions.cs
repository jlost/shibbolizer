using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;

namespace Shibbolizer
{
    public static class ShibbolizerAppBuilderExtensions
    {
        public static IApplicationBuilder UseShibbolizerAuthentication(this IApplicationBuilder app)
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));

            return app.UseMiddleware<ShibbolizerMiddleware>();
        }

        public static IApplicationBuilder UseShibbolizerAuthentication(this IApplicationBuilder app, ShibbolizerOptions options)
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            return app.UseMiddleware<ShibbolizerMiddleware>(Options.Create(options));
        }
    }
}
