using DevExpress.Web.Mvc;
using DevExpress.XtraReports.UI;
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
            return View();
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