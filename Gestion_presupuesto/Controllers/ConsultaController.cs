using DevExpress.Web.Mvc;
using DevExpress.XtraReports.UI;
using Gestion_presupuesto.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Gestion_presupuesto.Controllers
{
    public class ConsultaController : Controller
    {
        // GET: Consulta
        public ActionResult Consulta()
        {
            var id_emp = Convert.ToInt32(UserClaims.idempleado_key);
            var user = db.usuario.FirstOrDefault(x => x.id_empleado == id_emp && x.estado == 1);
            if (user != null)
            {
                var rol_user = user.id_rol_usuario;
                var acceso = db.menu.FirstOrDefault(x => x.accion == "Consulta" && x.controlador == "Consulta" && x.estado == 1);
                if (acceso != null)
                {
                    var rol_acceso = acceso.id_rol;
                    if (rol_acceso.Split(',').Contains(rol_user.ToString()))
                    {

                        return View();
                    }
                    return View("~/Views/Home/Index.cshtml");
                }
                else
                {
                    //BUSCAMOS EN SUB MENU
                    var acceso_submenu = db.sub_menu.FirstOrDefault(x => x.accion == "Consulta" && x.controlador == "Consulta" && x.estado == 1);
                    if (acceso_submenu != null)
                    {
                        var rol_acceso = acceso_submenu.id_rol;
                        if (rol_acceso.Split(',').Contains(rol_user.ToString()))
                        {
                            return View();
                        }
                    }
                    return View("~/Views/Home/Index.cshtml");
                }
            }
            else
            {
                return View("~/Views/Home/Index.cshtml");
            }
        }

        Gestion_presupuesto.Models.registro_presupuestoEntities db = new Models.registro_presupuestoEntities();

        public ActionResult ViewerReporteConsulta(int? anio)
        {

            if (anio == null) { return PartialView("~/Views/Consulta/repConsulta.cshtml"); }

            object[] data = new object[2];
            XtraReport rp = new Gestion_presupuesto.Reportes.Consulta();
            rp.Parameters["anio"].Value = anio;

            var consultaRegistro = db.sp_general_proceso_compra(anio).ToList();
            if (consultaRegistro.Count > 0)
            {
                rp.CreateDocument();
                data[0] = rp;
                return PartialView("~/Views/Consulta/repConsulta.cshtml", data);
            }
            else
            {
                return PartialView("~/Views/Consulta/repConsulta.cshtml");
            }

            
        }
    }
}