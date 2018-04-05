using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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
            var request = GetHappyTestRequest();

            var response = await request.GetAsync();
            
            Assert.Equal(200, (int)response.StatusCode);
            Assert.Equal(
                true.ToString(),
                response.Headers.Single(h => h.Key == "isAuthenticated").Value.Single()
            );
        }

        [Fact]
        public async void UsernameIsSetCorrectly()
        {
            var request = GetHappyTestRequest();

            var response = await request.GetAsync();

            Assert.Equal(
                "jlost",
                response.Headers.Single(h => h.Key == "user").Value.Single()
            );
        }

        [Fact]
        public async void GetsSpecifiedClaims()
        {
            var request = GetHappyTestRequest();

            var response = await request.GetAsync();

            var claims = response.Headers
                .Single(h => h.Key == "claims").Value
                .Select(JsonConvert.DeserializeObject<ClaimTestInfo>);
                
            var claimsValues = claims.Select(cti => cti.Value);

            Assert.Contains("GreatPeople", claimsValues);
            Assert.Contains("Company Unit", claimsValues);
            Assert.Contains("superUsers", claimsValues);
            Assert.Contains(" space men", claimsValues);

            Assert.Equal("roles", claims.First(cti => cti.Value == "superUsers").Type);
            Assert.Equal("groups", claims.First(cti => cti.Value == "GreatPeople").Type);
            
            Assert.Equal("ShibbolizerIssuer", claims.First().Issuer);
        }

        private RequestBuilder GetHappyTestRequest()
        {
            return _testServer.CreateRequest("/")
                .AddHeader("userID", "jlost")
                .AddHeader("email", "jlost@company.com")
                .AddHeader("groups", "GreatPeople;Illuminati;Company Unit;SMORES_Steering_Committee")
                .AddHeader("roles", "superUsers,managers, space men");
        }

        private class ClaimTestInfo
        {
            public string Issuer { get; set; }
            public string OriginalIssuer { get; set; }
            public string Type { get; set; }
            public string Value { get; set; }
            public string ValueType { get; set; }
        }
       
        private class TestStartup
        {
            protected ShibbolizerOptions _shibbolizerOptions;

            public TestStartup(IHostingEnvironment env)
            {
                _shibbolizerOptions = new ShibbolizerOptions
                {
                    UsernameHeader = "userID",
                    ClaimHeaders = new[] {"email", "employeeType"},
                    MultiClaimHeaders = new[]
                    {
                        new MultiClaimHeader {Header = "groups", Parser = s => s.Split(';')},
                        new MultiClaimHeader {Header = "roles", Parser = s => s.Split(',')}
                    },
                    ClaimsIssuer = "ShibbolizerIssuer"
                };
            }

            public void ConfigureServices(IServiceCollection services)
            {

                 services.AddAuthentication( ShibbolizerDefaults.AuthenticationScheme ).AddShibboleth(
                    (options) =>
                    {
                        options.UsernameHeader =_shibbolizerOptions.UsernameHeader;
                        options.ClaimHeaders = _shibbolizerOptions.ClaimHeaders;
                        options.MultiClaimHeaders = _shibbolizerOptions.MultiClaimHeaders;
                        options.ClaimsIssuer = _shibbolizerOptions.ClaimsIssuer;
                    }
                 );
            }
            
            public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
            {
                app.UseAuthentication();
               
                app.Use(next =>
                {
                    return async ctx =>
                    {
                        ctx.Response.StatusCode =
                            ctx.User.Identity.IsAuthenticated
                                ? 200
                                : 401;
                        ctx.Response.Headers.Add("user", ctx.User.Identity.Name);
                        ctx.Response.Headers.Add("isAuthenticated", ctx.User.Identity.IsAuthenticated.ToString());
                        ctx.Response.Headers.Add("claims",
                            ctx.User.Claims.Select(c => new ClaimTestInfo
                            {
                                Issuer = c.Issuer,
                                OriginalIssuer = c.OriginalIssuer,
                                Type = c.Type,
                                Value = c.Value,
                                ValueType = c.ValueType
                            })
                            .Select(JsonConvert.SerializeObject).ToArray()
                        );
                        await ctx.Response.WriteAsync("");
                        await next(ctx);
                    };
                });
            }
        }

        private class TestStartupNoUsernameHeaderConfigured : TestStartup
        {
            public TestStartupNoUsernameHeaderConfigured(IHostingEnvironment env) : base(env)
            {
                _shibbolizerOptions = new ShibbolizerOptions
                {
                    ClaimHeaders = new[] {"email", "employeeType"},
                    MultiClaimHeaders = new[]
                    {
                        new MultiClaimHeader {Header = "groups", Parser = s => s.Split(';')},
                        new MultiClaimHeader {Header = "roles", Parser = s => s.Split(',')}
                    },
                    ClaimsIssuer = "ShibbolizerIssuer"
                };
            }
        }
    }
}
