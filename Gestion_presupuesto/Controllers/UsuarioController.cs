using DevExpress.Web.Mvc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Gestion_presupuesto.Controllers
{
    public class UsuarioController : Controller
    {
        // GET: Usuario
        public ActionResult Usuario()
        {
            return View();
        }

        Gestion_presupuesto.Models.registro_presupuestoEntities db = new Gestion_presupuesto.Models.registro_presupuestoEntities();
        Gestion_presupuesto.Models.rrhhEntities db2 = new Gestion_presupuesto.Models.rrhhEntities();
        public IEnumerable GetEmpleados()
        {
            try
            {
                var Get = (from e in db.sp_persona_unidad_organizativa()
                           select new
                           {
                               e.id_empleado,
                               empleado = e.nombre_empleado + " (" + e.unidad_organizativa + ")"
                           });
                return Get.ToList();
            }
            catch (Exception)
            {
                return "".ToList();
            }
        }

        public IEnumerable GetRolUsuario()
        {
            try
            {
                var Get = (from r in db.rol_usuario
                           select new
                           {
                               r.id_rol_usuario,
                               r.valor
                           });
                return Get.ToList();
            }
            catch (Exception)
            {
                return "".ToList();
            }
        }

        [ValidateInput(false)]
        public ActionResult GridUsuario()
        {
            var model = db.usuario;
            return PartialView("~/Views/Usuario/_GridUsuario.cshtml", model.ToList());
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult GridUsuarioAddNew([ModelBinder(typeof(DevExpressEditorsBinder))] Gestion_presupuesto.Models.usuario item)
        {
            var model = db.usuario;
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
            return PartialView("~/Views/Usuario/_GridUsuario.cshtml", model.ToList());
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult GridUsuarioUpdate([ModelBinder(typeof(DevExpressEditorsBinder))] Gestion_presupuesto.Models.usuario item)
        {
            var model = db.usuario;
            if (ModelState.IsValid)
            {
                try
                {
                    var modelItem = model.FirstOrDefault(it => it.id_usuario == item.id_usuario);
                    if (modelItem != null)
                    {
                        modelItem.id_empleado = item.id_empleado;
                        modelItem.id_rol_usuario = item.id_rol_usuario;
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
            return PartialView("~/Views/Usuario/_GridUsuario.cshtml", model.ToList());
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult GridUsuarioDelete(System.Int32 id_usuario)
        {
            var model = db.usuario;
            if (id_usuario >= 0)
            {
                try
                {
                    var item = model.FirstOrDefault(it => it.id_usuario == id_usuario);
                    if (item != null)
                        model.Remove(item);
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }
            return PartialView("~/Views/Usuario/_GridUsuario.cshtml", model.ToList());
        }
    }
}