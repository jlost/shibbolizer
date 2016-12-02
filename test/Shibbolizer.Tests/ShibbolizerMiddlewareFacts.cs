using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Testing.Abstractions;
using Xunit;

namespace Shibbolizer.Tests
{
    public class ShibbolizerMiddlewareFacts
    {
        private readonly TestServer _testServer;

        public ShibbolizerMiddlewareFacts()
        {
            _testServer = new TestServer(new WebHostBuilder().UseStartup<TestStartup>());
        }

        [Fact]
        public void FailsWhenNoUsernameHeaderConfigured()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                new TestServer(new WebHostBuilder().UseStartup<TestStartupNoUsernameHeaderConfigured>());
            });
        }

        [Fact]
        public async void FailsWhenNoUsername()
        {
            var request = _testServer.CreateRequest("/")
                .AddHeader("userID_with_typo", "jlost")
                .AddHeader("email", "jlost@company.com")
                .AddHeader("groups", "GreatPeople;Illuminati;Company Unit;SMORES_Steering_Committee")
                .AddHeader("roles", "superUsers,managers, space men");

            var response = await request.GetAsync();
            Assert.Equal(401, (int)response.StatusCode);
        }

        [Fact]
        public async void SucceedsWhenHasUsername()
        {
            var request = _testServer.CreateRequest("/")
                .AddHeader("userID", "jlost")
                .AddHeader("email", "jlost@company.com")
                .AddHeader("groups", "GreatPeople;Illuminati;Company Unit;SMORES_Steering_Committee")
                .AddHeader("roles", "superUsers,managers, space men");

            var response = await request.GetAsync();
            
            Assert.Equal(200, (int)response.StatusCode);
            Assert.Equal(
                true.ToString(),
                response.Headers.Single(h => h.Key == "isAuthenticated").Value.Single()
            );
        }

        [Fact]
        public async void GetsSpecifiedClaims()
        {
            var request = _testServer.CreateRequest("/")
                .AddHeader("userID", "jlost")
                .AddHeader("email", "jlost@company.com")
                .AddHeader("groups", "GreatPeople;Illuminati;Company Unit;SMORES_Steering_Committee")
                .AddHeader("roles", "superUsers,managers, space men");

            var response = await request.GetAsync();

            var claims = response.Headers.Single(h => h.Key == "claims").Value;
            Assert.Contains("GreatPeople", claims);
            Assert.Contains("Company Unit", claims);
            Assert.Contains("superUsers", claims);
            Assert.Contains(" space men", claims);
        }

        private class TestStartup
        {
            public TestStartup(IHostingEnvironment env)
            {
            }
            
            public void ConfigureServices(IServiceCollection services)
            {
                services.AddAuthentication();
            }

            public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
            {
                app.UseShibbolizerAuthentication(new ShibbolizerOptions
                {
                    UsernameHeader = "userID",
                    ClaimHeaders = new [] { "email", "employeeType" },
                    MultiClaimHeaders = new []
                    {
                        new MultiClaimHeader { Header = "groups", Parser = s => s.Split(';') },
                        new MultiClaimHeader { Header = "roles", Parser = s => s.Split(',') }
                    },
                    AutomaticAuthenticate = true,
                    AuthenticationScheme = "Shibbolizer",
                    AutomaticChallenge = true,
                });
                app.Use(next =>
                {
                    return async ctx =>
                    {
                        ctx.Response.StatusCode = 200;
                        ctx.Response.Headers.Add("user", ctx.User.Identity.Name);
                        ctx.Response.Headers.Add("isAuthenticated", ctx.User.Identity.IsAuthenticated.ToString());
                        ctx.Response.Headers.Add("claims", ctx.User.Claims.Select(c => c.Value).ToArray());
                        await ctx.Response.WriteAsync("");
                        await next(ctx);
                    };
                });
            }
        }

        private class TestStartupNoUsernameHeaderConfigured
        {
            public TestStartupNoUsernameHeaderConfigured(IHostingEnvironment env)
            {
            }

            public void ConfigureServices(IServiceCollection services)
            {
                services.AddAuthentication();
            }

            public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
            {
                app.UseShibbolizerAuthentication(new ShibbolizerOptions
                {
                    ClaimHeaders = new[] { "email", "employeeType" },
                    MultiClaimHeaders = new[]
                    {
                        new MultiClaimHeader { Header = "groups", Parser = s => s.Split(';') },
                        new MultiClaimHeader { Header = "roles", Parser = s => s.Split(',') }
                    },
                    AutomaticAuthenticate = true,
                    AuthenticationScheme = "Shibbolizer",
                    AutomaticChallenge = true,
                });
                app.Use(next =>
                {
                    return async ctx =>
                    {
                        ctx.Response.StatusCode = 200;
                        ctx.Response.Headers.Add("user", ctx.User.Identity.Name);
                        ctx.Response.Headers.Add("isAuthenticated", ctx.User.Identity.IsAuthenticated.ToString());
                        ctx.Response.Headers.Add("claims", ctx.User.Claims.Select(c => c.Value).ToArray());
                        await next(ctx);
                    };
                });
            }
        }
    }
}
