using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Gestion_presupuesto
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            string keycloakUrl = ConfigurationManager.AppSettings["KeycloakUrl"];
            string clientId = ConfigurationManager.AppSettings["ClientId"];
            string clientSecret = ConfigurationManager.AppSettings["ClientSecret"];
            string cookiedomain = ConfigurationManager.AppSettings["Domain"];
            string server = ConfigurationManager.AppSettings["Server"];


            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "keycloak_sso_auth",
                CookieHttpOnly = false,
                CookieName = "keycloak_cookie_sicor_interno",
                //CookieSameSite = SameSiteMode.None,
                //CookieSecure = CookieSecureOption.Always,
                CookieSameSite = Microsoft.Owin.SameSiteMode.Lax, // Compatible con HTTP servidor
                CookieSecure = CookieSecureOption.Never, // Importante para HTTP servidor
                                                         //CookieDomain = cookiedomain,
                CookiePath = "/"
            });
            app.SetDefaultSignInAsAuthenticationType("keycloak_sso_auth");
            app.Use<Middleware.KeycloakSessionValidatorMiddleware>();
            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions()
            {
                Authority = keycloakUrl,
                ClientId = clientId,
                ClientSecret = clientSecret,
                ResponseType = "code",
                SaveTokens = true,
                Scope = "openid",
                RedirectUri = server + "Login/callback",
                RedeemCode = true,
                RequireHttpsMetadata = false,
                Notifications = new OpenIdConnectAuthenticationNotifications()
                {

                    RedirectToIdentityProvider = async (context) =>
                    {
                        context.ProtocolMessage.Parameters["code_challenge"] = "0aspHXI8ZJCXmq0XejAvl8LE9qu5FTFDeQ0aGyY4iDs";
                        context.ProtocolMessage.Parameters["code_challenge_method"] = "plain";
                    },
                    AuthorizationCodeReceived = async (context) =>
                    {
                        context.TokenEndpointRequest.Parameters["code_verifier"] = "0aspHXI8ZJCXmq0XejAvl8LE9qu5FTFDeQ0aGyY4iDs";
                    },
                    TokenResponseReceived = async (responseToken) =>
                    {
                        responseToken.Request.Headers.Add("Authorization", new[] { responseToken.TokenEndpointResponse.AccessToken });
                        responseToken.Request.Headers.Add("RefreshToken", new[] { responseToken.TokenEndpointResponse.RefreshToken });
                        responseToken.Request.Headers.Add("IdToken", new[] { responseToken.TokenEndpointResponse.IdToken });
                        responseToken.SkipToNextMiddleware();
                    },
                }
            });


        }
    }
}