using DevExpress.Web.Mvc;
using Gestion_presupuesto.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Gestion_presupuesto.Controllers
{
    public class PresupuestoController : Controller
    {
        // GET: Presupuesto
        public ActionResult Presupuesto()
        {
            return View();
        }

        Gestion_presupuesto.Models.registro_presupuestoEntities db = new Gestion_presupuesto.Models.registro_presupuestoEntities();
        Gestion_presupuesto.Models.rrhhEntities db2 = new Models.rrhhEntities();

        public IEnumerable GetEstructuras()
        {
            try
            {
                var Get = (from e in db2.Estructura
                                      join p in db2.Periodo on e.id_periodo equals p.id_periodo
                                      where p.estado == 1
                                      select e);
                return Get.ToList();
            }
            catch (Exception)
            {
                return "".ToList();
            }
        }

        public IEnumerable GetFuenteFinanciamiento()
        {
            try
            {
                var Get = (from ff in db.fuente_financiamiento
                                      where ff.estado == 1
                                      select ff);
                return Get.ToList();
            }
            catch (Exception)
            {
                return "".ToList();
            }
        }

        public IEnumerable GetMetodoContratacion()
        {
            try
            {
                var Get = (from mc in db.metodo_contratacion
                                      where mc.estado == 1
                                      select mc);
                return Get.ToList();
            }
            catch (Exception)
            {
                return "".ToList();
            }
        }

        [ValidateInput(false)]
        public ActionResult GridDetallePresupuesto()
        {
            var model = db.detalle_presupuesto;
            return PartialView("~/Views/Presupuesto/_GridDetallePresupuesto.cshtml", model.ToList());
        }


        public ActionResult IniciarProceso(int? id_detalle_presupuesto)
        {
            var presupuesto = db.detalle_presupuesto.FirstOrDefault(x => x.id_detalle_presupuesto == id_detalle_presupuesto);
            if (presupuesto != null)
            {
                //VALIDAMOS QUE NO HAYA INICIADO EL PROCESO ANTERIORMENTE O NO ESTE OBSERVADO
                var validacion_proceso = db.vobo.FirstOrDefault(x => x.id_etapa_vobo == 1 || x.id_etapa_vobo == 2);
                if (validacion_proceso!= null)
                {
                    return Json(new { data = -1 }, JsonRequestBehavior.AllowGet);
                }
                //OBTENEMOS LOS VOBOS
                var persona_vobo = db.personal_vobo.Where(x=>x.estado == 1).ToList();
                //VAMOS A INICIAR EL PROCESO DE VISTOS BUENOS
                foreach (var item in persona_vobo)
                {
                    //VAMOS A VALIDAR QUE SI HAY UNO EN PROCESO LOS DEMÁS SEAN PENDIENTES
                    var validacion = db.vobo.FirstOrDefault(x => x.id_detalle_presupuesto == id_detalle_presupuesto && x.estado == 1 && x.id_etapa_vobo == 3);
                    vobo clase = new vobo();
                    clase.id_personal_vobo = item.id_personal_vobo;
                    clase.id_detalle_presupuesto = id_detalle_presupuesto;
                    clase.estado = 1;
                    if (validacion != null)
                    {
                        clase.id_etapa_vobo = 2;
                    }
                    else
                    {
                        clase.id_etapa_vobo = 3;
                    }

                    db.vobo.Add(clase);
                    db.SaveChanges();
                }
                
            }

            return Json(new { data = 1 }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult GridDetallePresupuestoAddNew([ModelBinder(typeof(DevExpressEditorsBinder))] Gestion_presupuesto.Models.detalle_presupuesto item)
        {
            var model = db.detalle_presupuesto;
            if (ModelState.IsValid)
            {
                try
                {

                    item.estado = 1;
                    model.Add(item);
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }
            else
                ViewData["EditError"] = "Please, correct all errors.";
            return PartialView("~/Views/Presupuesto/_GridDetallePresupuesto.cshtml", model.ToList());
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult GridDetallePresupuestoUpdate([ModelBinder(typeof(DevExpressEditorsBinder))] Gestion_presupuesto.Models.detalle_presupuesto item)
        {
            var model = db.detalle_presupuesto;
            if (ModelState.IsValid)
            {
                try
                {
                    var modelItem = model.FirstOrDefault(it => it.id_detalle_presupuesto == item.id_detalle_presupuesto);
                    if (modelItem != null)
                    {
                        modelItem.nombre_proceso = item.nombre_proceso;
                        modelItem.id_metodo_contratacion = item.id_metodo_contratacion;
                        modelItem.fecha_inicio = item.fecha_inicio;
                        modelItem.fecha_fin = item.fecha_fin;
                        modelItem.monto = item.monto;
                        modelItem.id_fuente_financiamiento = item.id_fuente_financiamiento;
                        modelItem.id_unidad_organizativa = item.id_unidad_organizativa;
                        db.SaveChanges();
                    }
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }
            else
                ViewData["EditError"] = "Please, correct all errors.";
            return PartialView("~/Views/Presupuesto/_GridDetallePresupuesto.cshtml", model.ToList());
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult GridDetallePresupuestoDelete(System.Int32 id_detalle_presupuesto)
        {
            var model = db.detalle_presupuesto;
            if (id_detalle_presupuesto >= 0)
            {
                try
                {
                    var item = model.FirstOrDefault(it => it.id_detalle_presupuesto == id_detalle_presupuesto);
                    if (item != null)
                        model.Remove(item);
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }
            return PartialView("~/Views/Presupuesto/_GridDetallePresupuesto.cshtml", model.ToList());
        }
    }
}