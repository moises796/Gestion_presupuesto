using DevExpress.Web.Mvc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Gestion_presupuesto.Controllers
{
    public class PersonalVoboController : Controller
    {
        // GET: PersonalVobo
        public ActionResult PersonalVobo()
        {
            return View();
        }

        Gestion_presupuesto.Models.registro_presupuestoEntities db = new Gestion_presupuesto.Models.registro_presupuestoEntities();
        Gestion_presupuesto.Models.rrhhEntities db2 = new Gestion_presupuesto.Models.rrhhEntities();

        public IEnumerable GetEmpleados()
        {
            try
            {
                var Get = (from e in db2.Empleado
                           select new
                           {
                               e.id_empleado,
                               nombre_empleado = e.nombres + " " + e.apellidos
                           });
                return Get.ToList();
            }
            catch (Exception)
            {
                return "".ToList();
            }
        }

        [ValidateInput(false)]
        public ActionResult GridPersonalVobo()
        {
            var model = db.personal_vobo;
            return PartialView("~/Views/PersonalVobo/_GridPersonalVobo.cshtml", model.ToList());
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult GridPersonalVoboAddNew([ModelBinder(typeof(DevExpressEditorsBinder))] Gestion_presupuesto.Models.personal_vobo item)
        {
            var model = db.personal_vobo;
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
            return PartialView("~/Views/PersonalVobo/_GridPersonalVobo.cshtml", model.ToList());
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult GridPersonalVoboUpdate([ModelBinder(typeof(DevExpressEditorsBinder))] Gestion_presupuesto.Models.personal_vobo item)
        {
            var model = db.personal_vobo;
            if (ModelState.IsValid)
            {
                try
                {
                    var modelItem = model.FirstOrDefault(it => it.id_personal_vobo == item.id_personal_vobo);
                    if (modelItem != null)
                    {
                        modelItem.id_empleado = item.id_empleado;
                        modelItem.reporte = item.reporte;
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
            return PartialView("~/Views/PersonalVobo/_GridPersonalVobo.cshtml", model.ToList());
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult GridPersonalVoboDelete(System.Int32 id_personal_vobo)
        {
            var model = db.personal_vobo;
            if (id_personal_vobo >= 0)
            {
                try
                {
                    var item = model.FirstOrDefault(it => it.id_personal_vobo == id_personal_vobo);
                    if (item != null)
                        model.Remove(item);
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }
            return PartialView("~/Views/PersonalVobo/_GridPersonalVobo.cshtml", model.ToList());
        }
    }
}