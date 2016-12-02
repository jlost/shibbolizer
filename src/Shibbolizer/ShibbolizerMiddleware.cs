using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Shibbolizer
{
    public class ShibbolizerMiddleware : AuthenticationMiddleware<ShibbolizerOptions>
    {
        public ShibbolizerMiddleware(RequestDelegate next, IOptions<ShibbolizerOptions> options, ILoggerFactory loggerFactory, UrlEncoder encoder) : base(next, options, loggerFactory, encoder)
        {
        }

        protected override AuthenticationHandler<ShibbolizerOptions> CreateHandler()
        {
            return new ShibbolizerHandler();
        }
    }
}
