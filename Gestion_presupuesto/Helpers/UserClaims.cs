using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;

namespace Gestion_presupuesto.Helpers
{
    public class UserClaims
    {
        public static ClaimsPrincipal CurrentUser => (ClaimsPrincipal)HttpContext.Current.User;

        public static string idempleado_key => CurrentUser.FindFirst("id_empleado")?.Value;
        public static string nombre_key => CurrentUser.FindFirst("FullName")?.Value;
        public static string codigoempleado_key => CurrentUser.FindFirst("codigo_empleado")?.Value;
        public static string fotoempleado_key => CurrentUser.FindFirst("foto_empleado")?.Value;
        public static string correo_key => CurrentUser.FindFirst("correo")?.Value;
    }
}