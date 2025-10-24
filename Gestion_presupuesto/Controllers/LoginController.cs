using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json.Linq;
using System.Configuration;
using System.Security.Claims;
using Microsoft.Owin.Security;
using System.Threading.Tasks;
using Gestion_presupuesto.Helpers;

namespace Gestion_presupuesto.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        public ActionResult Index()
        {
            return View();
        }

        Gestion_presupuesto.Models.rrhhEntities db2 = new Models.rrhhEntities();
        public ActionResult Callback()
        {
            var accessToken = HttpContext.Request.Headers["Authorization"];
            //if (accessToken == null) return RedirectToAction("Index", "DenunciaTemp");
            var refreshToken = HttpContext.Request.Headers["RefreshToken"];
            var idToken = HttpContext.Request.Headers["IdToken"];
            var decodedToken = DecodeToken.DecodeTokens(accessToken);
            var username = decodedToken.Claims.FirstOrDefault(x => x.Type == "preferred_username").Value;
            var given_name = decodedToken.Claims.FirstOrDefault(x => x.Type == "given_name").Value;
            var family_name = decodedToken.Claims.FirstOrDefault(x => x.Type == "family_name").Value;
            //var fullname = decodedToken.Claims.FirstOrDefault(x => x.Type == "name").Value;
            var email = decodedToken.Claims.FirstOrDefault(x => x.Type == "email").Value;
            var sub = decodedToken.Claims.FirstOrDefault(x => x.Type == "sub").Value;
            var empleado = db2.Empleado.FirstOrDefault(x => x.correo_institucional == email && x.id_estado_usuario == 1);
            var fullname = empleado.nombres + " " + empleado.apellidos;
            var resourceAccessClaim = decodedToken.Claims.FirstOrDefault(x => x.Type == "resource_access")?.Value;
            List<string> roles = new List<string>();
            if (!string.IsNullOrEmpty(resourceAccessClaim))
            {
                try
                {
                    var resourceAccess = JObject.Parse(resourceAccessClaim);
                    var appRoles = resourceAccess[ConfigurationManager.AppSettings["ClientId"]]?["roles"]?.ToObject<List<string>>();
                    if (appRoles != null)
                    {
                        roles = appRoles;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error al procesar roles: {ex.Message}");
                }
            }
            var claims = new List<Claim>
            {
                new Claim("UserName",username),
                new Claim("FullName",fullname),
                new Claim("GivenName",given_name),
                new Claim("FamilyName",family_name),
                new Claim("email",email),
                new Claim("id_user",sub),
                new Claim("IdToken", idToken?.ToString()),
                new Claim("AccessToken",accessToken.ToString()),
                new Claim("RefreshToken",refreshToken.ToString()),
                new Claim("codigo_empleado",empleado != null? empleado.codigo_empleado.ToString() : ""),
                new Claim("id_empleado",empleado != null? empleado.id_empleado.ToString() : ""),
                new Claim("foto_empleado",empleado != null? empleado.foto_empleado.ToString() : ""),
            };
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            var identity = new ClaimsIdentity(claims, "keycloak_sso_auth");
            Request.GetOwinContext().Authentication.SignIn(new AuthenticationProperties(), identity);
            return RedirectToAction("Presupuesto", "Presupuesto");
        }


        public async Task<ActionResult> SignOut()
        {
            var identity = (ClaimsIdentity)User.Identity;
            var idToken = identity.Claims.FirstOrDefault(c => c.Type == "IdToken")?.Value;
            Request.GetOwinContext().Authentication.SignOut("keycloak_sso_auth");

            var keycloakLogoutUrl = ConfigurationManager.AppSettings["KeycloakUrl"] + "/protocol/openid-connect/logout";
            var redirectUri = ConfigurationManager.AppSettings["Server_local"];

            if (!string.IsNullOrEmpty(idToken))
            {
                var logoutUrl = $"{keycloakLogoutUrl}?id_token_hint={idToken}&post_logout_redirect_uri={Uri.EscapeDataString(redirectUri)}";
                return Redirect(logoutUrl);
            }
            else
            {
                return Redirect(redirectUri);
            }
        }

    }
}