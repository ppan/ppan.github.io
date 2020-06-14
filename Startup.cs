using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Security.Jwt;
using Microsoft.Owin.Security.OAuth;
using Owin;
using Swashbuckle.Application;
using Swashbuckle.Swagger;

namespace MyWebAPI
{
    public class Startup
    {
        public static string WellKnownURL = "https://<my-tenant>.b2clogin.com/<my-tenant>.onmicrosoft.com/<my-signin-signup-policy>/v2.0/.well-known/openid-configuration";
        public static string OAuth2AuthorizeURL = "https://<my-tenant>.b2clogin.com/<my-tenant>.onmicrosoft.com/<my-signin-signup-policy>/oauth2/v2.0/authorize";
        public static string ScopeURL = "https://<my-tenant>.onmicrosoft.com/myapp/my_api";
        public static string AppClientID = "<myapp-client-id>";  // Client ID of Azure B2C App "myapp"
        public static string SwaggerClientID = "<myappswagger-client-id>";  // Client ID of Azure B2C App "myappswagger"
        public static string DefaultPolicy = "<my-signin-signup-policy>";

        public class AuthorizeCheckOperationFilter : IOperationFilter
        {
            public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
            {
                if (operation.security == null)
                {
                    operation.security = new List<IDictionary<string, IEnumerable<string>>>();
                }

                // Add the appropriate security definition to the operation
                var scopes = new List<string>()
                {
                    ScopeURL
                };

                var openAuthRequirements = new Dictionary<string, IEnumerable<string>> { { "oauth2", scopes } };
                operation.security.Add(openAuthRequirements);
            }
        }

        public void ConfigureAuth(IAppBuilder app)
        {
            TokenValidationParameters tvps = new TokenValidationParameters
            {
                // Accept only those tokens where the audience of the token is equal to the client ID of this app
                ValidAudience = AppClientID,
                AuthenticationType = DefaultPolicy
            };

            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions
            {
                // This SecurityTokenProvider fetches the Azure AD B2C metadata & signing keys from the OpenIDConnect metadata endpoint
                AccessTokenFormat = new JwtFormat(tvps, new OpenIdConnectCachingSecurityTokenProvider(WellKnownURL))
            });
        }

        public void Configuration(IAppBuilder app)
        {
            // Configure Web API for self-host. 
            try
            {
                Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;
                HttpConfiguration config = new HttpConfiguration();
                config.Routes.MapHttpRoute(
            	    name: "MyWebApi",
            	    routeTemplate: "api/{controller}/{id}",
            	    defaults: new { id = RouteParameter.Optional }
                );
                config.EnableSwagger(c =>
                {
                    //c.RootUrl(req => SwaggerDocsConfig.DefaultRootUrlResolver(req) + "/MyApp");
                    c.RootUrl(req => req.RequestUri.GetLeftPart(UriPartial.Authority) + req.GetRequestContext().VirtualPathRoot.TrimEnd('/'));
            	    c.SingleApiVersion("v1", "My Web API");
            	    c.OAuth2("oauth2")
            	    .Description("OAuth2 Implicit Grant")
            	    .Flow("implicit")
            	    .AuthorizationUrl(OAuth2AuthorizeURL)
            	    .Scopes(scopes =>
            	    {
            		    scopes.Add(ScopeURL, "Read Write Access to Web API");
            	    });
            	    c.OperationFilter<AuthorizeCheckOperationFilter>();
                }).EnableSwaggerUi(c =>
                {
            	    c.EnableOAuth2Support(SwaggerClientID, null, null, null, null, additionalQueryStringParams: new Dictionary<string, string>() { { "p", DefaultPolicy } });
                });

                app.UseCors(CorsOptions.AllowAll);
                ConfigureAuth(app);

                app.UseWebApi(config);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Program.Configuration()", ex);
            }
        }
    }
}
