using Microsoft.Owin;
using Microsoft.Owin.Security;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace Gestion_presupuesto.Middleware
{
    public class KeycloakSessionValidatorMiddleware: OwinMiddleware
    {
        // Caché para almacenar tokens validados y evitar llamadas innecesarias a Keycloak
        private static readonly ConcurrentDictionary<string, (bool isValid, DateTime expiresAt)> TokenCache =
            new ConcurrentDictionary<string, (bool, DateTime)>();

        public KeycloakSessionValidatorMiddleware(OwinMiddleware next) : base(next) { }

        public static string keyclo = ConfigurationManager.AppSettings["KeycloakUrl"];
        string keycloakbase = keyclo + "/protocol/openid-connect/";
        string client_id = ConfigurationManager.AppSettings["ClientId"];
        string client_secret = ConfigurationManager.AppSettings["ClientSecret"];




        public async override Task Invoke(IOwinContext context)
        {
            var authentication = context.Authentication;
            var user = authentication.User;

            //para ajax
            var request = context.Request;
            var response = context.Response;
            bool isAjaxRequest =
            request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
            (request.Headers.ContainsKey("Accept") && request.Headers["Accept"].Contains("application/json"));
            //fin

            // Obtener los tokens desde los claims del usuario autenticado
            //try
            //{
            var accessToken = user?.Claims.FirstOrDefault(c => c.Type == "AccessToken")?.Value;
            var refreshToken = user?.Claims.FirstOrDefault(c => c.Type == "RefreshToken")?.Value;

            if (!string.IsNullOrEmpty(accessToken) && !string.IsNullOrEmpty(refreshToken))
            {
                bool isTokenValid = await ValidateTokenWithCache(accessToken, refreshToken, authentication);

                if (!isTokenValid)
                {
                    authentication.SignOut("keycloak_sso_auth");
                    if (isAjaxRequest)
                    {
                        // Para peticiones AJAX devolvemos 401 para que el cliente lo maneje
                        response.StatusCode = 401;
                        response.ReasonPhrase = "Unauthorized";
                        await response.WriteAsync("Sesión expirada");
                    }
                    else
                    {
                        // Para navegación normal redirigimos al inicio de sesión
                        response.Redirect("/");
                    }
                    return;
                }
            }
            else
            {
                if (isAjaxRequest)
                {
                    authentication.SignOut("keycloak_sso_auth");
                    response.StatusCode = 401;
                    response.ReasonPhrase = "Unauthorized";
                    await response.WriteAsync("Token no encontrado");
                    return;
                }

            }

            //}
            //catch (Exception)
            //{
            //    response.Redirect("/");
            //}
            // 🔹 Continuar con la ejecución normal del middleware si el usuario sigue autenticado
            await Next.Invoke(context);
        }

        private async Task<bool> ValidateTokenWithCache(string accessToken, string refreshToken, IAuthenticationManager authentication)
        {
            // 🔹 1️⃣ Verificar si el token está en caché y sigue válido
            if (TokenCache.TryGetValue(accessToken, out var cachedToken))
            {
                if (cachedToken.expiresAt > DateTime.UtcNow)
                {
                    //Debug.WriteLine($" Token encontrado en caché. Sigue válido hasta {cachedToken.expiresAt}");
                    return cachedToken.isValid;
                }
            }

            // 🔹 2️⃣ Verificar si el access token sigue válido en Keycloak
            var isValid = await ValidateTokenWithKeycloak(accessToken);

            if (!isValid)
            {
                // 🔹 3️⃣ Si el access token ha expirado, intentar refrescarlo
                var newAccessToken = await RefreshAccessToken(refreshToken);

                if (!string.IsNullOrEmpty(newAccessToken))
                {
                    // Debug.WriteLine($"Nuevo access token generado y guardado en sesión.");
                    TokenCache[newAccessToken] = (true, GetTokenExpiration(newAccessToken));

                    // 🔹 4️⃣ Actualizar la identidad del usuario con el nuevo access token
                    var identity = new ClaimsIdentity(authentication.User.Claims, "keycloak_sso_auth");
                    identity.RemoveClaim(identity.FindFirst("AccessToken"));
                    identity.AddClaim(new Claim("AccessToken", newAccessToken));

                    authentication.SignIn(new AuthenticationProperties(), identity);
                    return true;
                }
                else
                {
                    //Debug.WriteLine("El refresh token también expiró o fue revocado.");
                    return false;
                }
            }

            // 🔹 5️⃣ Si el token es válido, almacenarlo en caché para evitar consultas futuras
            TokenCache[accessToken] = (true, GetTokenExpiration(accessToken));
            return true;
        }

        private async Task<bool> ValidateTokenWithKeycloak(string token)
        {
            var keycloakUrlF = keycloakbase + "userinfo";

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var response = await client.GetAsync(keycloakUrlF);

                bool isValid = response.IsSuccessStatusCode;
                // Debug.WriteLine($"Validación de Keycloak: {isValid}");
                return isValid;
            }
        }

        private async Task<string> RefreshAccessToken(string refreshToken)
        {
            var keycloakTokenUrl = keycloakbase + "token";

            using (var client = new HttpClient())
            {
                var content = new FormUrlEncodedContent(new[]
                {
            new KeyValuePair<string, string>("client_id", client_id),
            new KeyValuePair<string, string>("client_secret", client_secret),
            new KeyValuePair<string, string>("grant_type", "refresh_token"),
            new KeyValuePair<string, string>("refresh_token", refreshToken)
        });

                var response = await client.PostAsync(keycloakTokenUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var tokenResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonString);
                    return tokenResponse.ContainsKey("access_token") ? tokenResponse["access_token"] : null;
                }
            }

            return null;
        }

        private DateTime GetTokenExpiration(string token)
        {
            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as System.IdentityModel.Tokens.Jwt.JwtSecurityToken;

            if (jwtToken != null)
            {
                var expClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "exp")?.Value;
                if (!string.IsNullOrEmpty(expClaim) && long.TryParse(expClaim, out long exp))
                {
                    return DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime;
                }
            }

            return DateTime.UtcNow.AddMinutes(5); // 🔹 Valor por defecto si no se encuentra la expiración
        }
    }
}