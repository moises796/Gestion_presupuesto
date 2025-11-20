using DevExpress.Web.Mvc;
using Gestion_presupuesto.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Gestion_presupuesto.Controllers
{
    public class FuenteFinanciamientoController : Controller
    {
        // GET: FuenteFinanciamiento
        public ActionResult FuenteFinanciamiento()
        {
            var id_emp = Convert.ToInt32(UserClaims.idempleado_key);
            var user = db.usuario.FirstOrDefault(x => x.id_empleado == id_emp && x.estado == 1);
            if (user != null)
            {
                var rol_user = user.id_rol_usuario;
                var acceso = db.menu.FirstOrDefault(x => x.accion == "FuenteFinanciamiento" && x.controlador == "FuenteFinanciamiento" && x.estado == 1);
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
                    var acceso_submenu = db.sub_menu.FirstOrDefault(x => x.accion == "FuenteFinanciamiento" && x.controlador == "FuenteFinanciamiento" && x.estado == 1);
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

        Gestion_presupuesto.Models.registro_presupuestoEntities db = new Gestion_presupuesto.Models.registro_presupuestoEntities();

        [ValidateInput(false)]
        public ActionResult GridFuenteFinanciamiento()
        {
            var model = db.fuente_financiamiento;
            return PartialView("~/Views/FuenteFinanciamiento/_GridFuenteFinanciamiento.cshtml", model.ToList());
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult GridFuenteFinanciamientoAddNew([ModelBinder(typeof(DevExpressEditorsBinder))] Gestion_presupuesto.Models.fuente_financiamiento item)
        {
            var model = db.fuente_financiamiento;
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
            return PartialView("~/Views/FuenteFinanciamiento/_GridFuenteFinanciamiento.cshtml", model.ToList());
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult GridFuenteFinanciamientoUpdate([ModelBinder(typeof(DevExpressEditorsBinder))] Gestion_presupuesto.Models.fuente_financiamiento item)
        {
            var model = db.fuente_financiamiento;
            if (ModelState.IsValid)
            {
                try
                {
                    var modelItem = model.FirstOrDefault(it => it.id_fuente_financiamiento == item.id_fuente_financiamiento);
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
            return PartialView("~/Views/FuenteFinanciamiento/_GridFuenteFinanciamiento.cshtml", model.ToList());
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult GridFuenteFinanciamientoDelete(System.Int32 id_fuente_financiamiento)
        {
            var model = db.fuente_financiamiento;
            if (id_fuente_financiamiento >= 0)
            {
                try
                {
                    var item = model.FirstOrDefault(it => it.id_fuente_financiamiento == id_fuente_financiamiento);
                    if (item != null)
                        model.Remove(item);
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }
            return PartialView("~/Views/FuenteFinanciamiento/_GridFuenteFinanciamiento.cshtml", model.ToList());
        }
    }
}