using DevExpress.Web.Mvc;
using Gestion_presupuesto.Helpers;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static Gestion_presupuesto.Helpers.Clases;

namespace Gestion_presupuesto.Controllers
{
    [Authorize]
    public class VoboController : Controller
    {
        // GET: Vobo
        public ActionResult Vobo()
        {
            return View();
        }

        Gestion_presupuesto.Models.registro_presupuestoEntities db = new Gestion_presupuesto.Models.registro_presupuestoEntities();
        Gestion_presupuesto.Models.rrhhEntities db2 = new Models.rrhhEntities();

        [ValidateInput(false)]
        public ActionResult GridVobo()
        {
            string codEmp = (string)UserClaims.codigoempleado_key;
            var id = Convert.ToInt32(UserClaims.idempleado_key);
            var model = db.consulta_bandeja_vobo(id);
            List<BandejaVobo> clase = new List<BandejaVobo>();
            model.ForEach(x =>
            {
                BandejaVobo bv = new BandejaVobo();
                bv.id_vobo = x.id_vobo;
                bv.id_detalle_presupuesto = x.id_detalle_presupuesto;
                bv.codigo = x.codigo;
                bv.nombre_proceso = x.nombre_proceso;
                bv.id_metodo_contratacion = x.id_metodo_contratacion;
                bv.fecha_inicio = x.fecha_inicio;
                bv.fecha_fin = x.fecha_fin;
                bv.monto = x.monto;
                bv.id_fuente_financiamiento = x.id_fuente_financiamiento;
                bv.id_unidad_organizativa = x.id_unidad_organizativa;
                bv.estado = x.estado;
                clase.Add(bv);
            });

            return PartialView("~/Views/Vobo/_GridVobo.cshtml", clase.ToList());
        }


        [HttpPost, ValidateInput(false)]
        public ActionResult GridVoboUpdate([ModelBinder(typeof(DevExpressEditorsBinder))] Gestion_presupuesto.Models.detalle_presupuesto item)
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
            return GridVobo();
        }

        public ActionResult Observar(int? id_vobo, int? id_detalle_presupuesto,string instruccion)
        {
            var vobo = db.vobo.FirstOrDefault(x => x.id_vobo == id_vobo);
            if (vobo == null)
            {
                return Json(new { data = -1 }, JsonRequestBehavior.AllowGet);
            }
            vobo.instruccion = instruccion;
            db.SaveChanges();
            //VAMOS A PASAR TODO A LA ETAPA 4 DE OBSERVADO
            var lista_vobo = db.vobo.Where(x => x.id_detalle_presupuesto == id_detalle_presupuesto).ToList();
            lista_vobo.ForEach(x => { 
                x.id_etapa_vobo = 4;
                db.SaveChanges(); 
            });

            return Json(new { data = 1 }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Aprobar(int? id_vobo)
        {
            //VAMOS A PASAR TODO A LA ETAPA 4 DE OBSERVADO
            var lista_vobo = db.vobo.FirstOrDefault(x=>x.id_vobo == id_vobo);
            lista_vobo.id_etapa_vobo = 1;
            db.SaveChanges();
            var siguiente_vobo = db.vobo.FirstOrDefault(x=>x.id_etapa_vobo == 2 && x.id_detalle_presupuesto == lista_vobo.id_detalle_presupuesto);
            if (siguiente_vobo != null)
            {
                siguiente_vobo.id_etapa_vobo = 3;
                db.SaveChanges();
            }
            return Json(new { data = 1 }, JsonRequestBehavior.AllowGet);
        }

        [ValidateInput(false)]
        public ActionResult GridListadoVobo(int? id_detalle_presupuesto, int? id_movimiento_detalle_presupuesto)
        {
            if (id_detalle_presupuesto != null)
            {
                var model = db.vobo.Where(x => x.id_detalle_presupuesto == id_detalle_presupuesto && x.estado != 5);

                return PartialView("~/Views/Vobo/_GridListadoVobo.cshtml", model.ToList());
            }
            else
            {
                var model = db.vobo.Where(x => x.id_movimiento_detalle_presupuesto == id_movimiento_detalle_presupuesto && x.estado != 5);

                return PartialView("~/Views/Vobo/_GridListadoVobo.cshtml", model.ToList());
            }
            
        }


        public IEnumerable GetVobos()
        {
            var vobos = db.personal_vobo.Where(x => x.estado == 1).ToList();
            List<EmpleadoVobo> lista = new List<EmpleadoVobo>();

            for (int i = 0; i < vobos.Count; i++)
            {
                int? id_empleado = vobos[i].id_empleado;
                var nombreEmpleado = db2.Empleado.FirstOrDefault(x => x.id_empleado == id_empleado);
                EmpleadoVobo clase = new EmpleadoVobo();
                clase.id_personal_vobo = Convert.ToInt32(vobos[i].id_personal_vobo);
                clase.nombre_empleado = nombreEmpleado.nombres + " " + nombreEmpleado.apellidos;

                lista.Add(clase);
            }
            return lista;
        }


    }
}