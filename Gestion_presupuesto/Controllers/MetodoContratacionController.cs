using DevExpress.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Gestion_presupuesto.Controllers
{
    public class MetodoContratacionController : Controller
    {
        // GET: MetodoContratacion
        public ActionResult MetodoContratacion()
        {
            return View();
        }

        Gestion_presupuesto.Models.registro_presupuestoEntities db = new Gestion_presupuesto.Models.registro_presupuestoEntities();

        [ValidateInput(false)]
        public ActionResult GridMetodoContratacion()
        {
            var model = db.metodo_contratacion;
            return PartialView("~/Views/MetodoContratacion/_GridMetodoContratacion.cshtml", model.ToList());
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult GridMetodoContratacionAddNew([ModelBinder(typeof(DevExpressEditorsBinder))] Gestion_presupuesto.Models.metodo_contratacion item)
        {
            var model = db.metodo_contratacion;
            if (ModelState.IsValid)
            {
                try
                {
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
            return PartialView("~/Views/MetodoContratacion/_GridMetodoContratacion.cshtml", model.ToList());
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult GridMetodoContratacionUpdate([ModelBinder(typeof(DevExpressEditorsBinder))] Gestion_presupuesto.Models.metodo_contratacion item)
        {
            var model = db.metodo_contratacion;
            if (ModelState.IsValid)
            {
                try
                {
                    var modelItem = model.FirstOrDefault(it => it.id_metodo_contratacion == item.id_metodo_contratacion);
                    if (modelItem != null)
                    {
                        modelItem.valor = item.valor;
                        modelItem.estado = item.estado;
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
            return PartialView("~/Views/MetodoContratacion/_GridMetodoContratacion.cshtml", model.ToList());
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult GridMetodoContratacionDelete(System.Int32 id_metodo_contratacion)
        {
            var model = db.metodo_contratacion;
            if (id_metodo_contratacion >= 0)
            {
                try
                {
                    var item = model.FirstOrDefault(it => it.id_metodo_contratacion == id_metodo_contratacion);
                    if (item != null)
                        model.Remove(item);
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }
            return PartialView("~/Views/MetodoContratacion/_GridMetodoContratacion.cshtml", model.ToList());
        }
    }
}