using DevExpress.ClipboardSource.SpreadsheetML;
using DevExpress.Utils.Serializing;
using DevExpress.Web.Internal;
using DevExpress.Web.Mvc;
using DevExpress.XtraReports.UI;
using Gestion_presupuesto.Helpers;
using Gestion_presupuesto.Models;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using static Gestion_presupuesto.Helpers.Clases;

namespace Gestion_presupuesto.Controllers
{
    [Authorize]
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

        public ActionResult GetVerificarStatus(int? id_detalle_presupuesto)
        {
            var status = db.vobo.Where(x => x.id_detalle_presupuesto == id_detalle_presupuesto && x.id_etapa_vobo != 4);
            if (status.Count() > 0)
            {
                var conteo = 0;
                foreach (var item in status)
                {
                    var finalizado = db.vobo.FirstOrDefault(x => x.id_vobo == item.id_vobo && x.id_etapa_vobo == 1);
                    if (finalizado != null)
                    {
                        conteo++;
                    }
                }
                if (conteo == status.Count())
                {
                    return Json(new { data = 1 }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { data = -1 }, JsonRequestBehavior.AllowGet);
                }
                
            }
            else
            {
                return Json(new { data = -1 }, JsonRequestBehavior.AllowGet);
            }
                
        }

        public ActionResult IniciarModificativa(int? id_detalle_presupuesto)
        {
            var presupuesto_original = db.detalle_presupuesto.FirstOrDefault(x => x.id_detalle_presupuesto == id_detalle_presupuesto);
            if (presupuesto_original != null)
            {


                //AHORA ANULAMOS TODAS LA SOLICITUDES ANTERIORES QUE NO ESTEN FINALIZADAS
                var solicitudes = db.movimiento_detalle_presupuesto.Where(x => x.id_detalle_presupuesto == id_detalle_presupuesto).ToList();
                solicitudes.ForEach(x => {
                    //if (!GetVerificarStatusMovimiento_backoffice(x.id_movimiento_detalle_presupuesto))
                    //{
                    //    x.estado = 0;
                    //    db.SaveChanges();
                    //}
                    x.estado = 0;
                    db.SaveChanges();

                });

                movimiento_detalle_presupuesto clase = new movimiento_detalle_presupuesto();
                clase.id_detalle_presupuesto = id_detalle_presupuesto;
                clase.nombre_proceso = presupuesto_original.nombre_proceso;
                clase.id_metodo_contratacion = presupuesto_original.id_metodo_contratacion;
                clase.fecha_inicio = presupuesto_original.fecha_inicio;
                clase.fecha_fin = presupuesto_original.fecha_fin;
                clase.monto = presupuesto_original.monto;
                clase.id_fuente_financiamiento = presupuesto_original.id_fuente_financiamiento;
                clase.id_unidad_organizativa = presupuesto_original.id_unidad_organizativa;
                clase.estado = presupuesto_original.estado;
                clase.fecha_movimiento = DateTime.Now;
                db.movimiento_detalle_presupuesto.Add(clase);
                db.SaveChanges();

                
                return Json(new { data = 1 }, JsonRequestBehavior.AllowGet);

            }

            return Json(new { data = -1 }, JsonRequestBehavior.AllowGet);
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
            //var model = db.detalle_presupuesto;
            string codEmp = (string)UserClaims.codigoempleado_key;
            var id = Convert.ToInt32(UserClaims.idempleado_key);
            var model = db.sp_consulta_procesos_compra(id).ToList();
            List<BandejaProcesoCompra> clase = new List<BandejaProcesoCompra>();
            model.ForEach(x =>
            {
                BandejaProcesoCompra bpc = new BandejaProcesoCompra();
                bpc.id_detalle_presupuesto = x.id_detalle_presupuesto;
                bpc.codigo = x.codigo;
                bpc.nombre_proceso = x.nombre_proceso;
                bpc.id_metodo_contratacion = x.id_metodo_contratacion;
                bpc.fecha_inicio = x.fecha_inicio;
                bpc.fecha_fin = x.fecha_fin;
                bpc.monto = x.monto;
                bpc.id_fuente_financiamiento = x.id_fuente_financiamiento;
                bpc.id_unidad_organizativa = x.id_unidad_organizativa;
                bpc.estado = x.estado;
                bpc.metodo_contratacion = x.metodo_contratacion;
                bpc.fuente_financiamiento = x.fuente_financiamiento;
                bpc.estatus_proceso = x.estatus_proceso;
                bpc.estatus_general = x.estatus_general;
                clase.Add(bpc);
            });

            return PartialView("~/Views/Presupuesto/_GridDetallePresupuesto.cshtml", model.ToList().OrderByDescending(x=>x.id_detalle_presupuesto));
        }

        public ActionResult ValidarProceso(int? id_detalle_presupuesto)
        {
            var validacion_proceso = db.vobo.FirstOrDefault(x => (x.id_etapa_vobo == 1 || x.id_etapa_vobo == 2) && x.id_detalle_presupuesto == id_detalle_presupuesto);
            if (validacion_proceso != null)
            {
                return Json(new { data = -1 }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { data = 1 }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult ValidarAnulacion(int? id_detalle_presupuesto)
        {
            var validacion_proceso = db.detalle_presupuesto.FirstOrDefault(x => x.id_detalle_presupuesto == id_detalle_presupuesto && x.estado == 1);
            if (validacion_proceso != null)
            {
                return Json(new { data = 1 }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { data = -1 }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult AnulacionProceso(int? id_detalle_presupuesto)
        {
            if (id_detalle_presupuesto != null)
            {
                var detalle_presupuesto = db.detalle_presupuesto.FirstOrDefault(x => x.id_detalle_presupuesto == id_detalle_presupuesto);
                if (detalle_presupuesto != null)
                {
                    detalle_presupuesto.estado = 0;
                    db.SaveChanges();

                    //AHORA VAMOS A VALIDAR QUE SI POSEE MODIFICACIONES LAS ANULE TODAS TAMBIEN
                    var listaMovimiento = db.movimiento_detalle_presupuesto.Where(x => x.id_detalle_presupuesto == id_detalle_presupuesto).ToList();
                    listaMovimiento.ForEach(x =>
                    {
                        x.estado = 0;
                        db.SaveChanges();
                    });
                    return Json(new { data = 1 }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { data = -1 }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(new { data = -1 }, JsonRequestBehavior.AllowGet);
            }

        }


        public ActionResult ValidarProcesoMovimiento(int? id_movimiento_detalle_presupuesto)
        {
            var validacion_proceso = db.vobo.FirstOrDefault(x => (x.id_etapa_vobo == 1 || x.id_etapa_vobo == 2) && x.id_movimiento_detalle_presupuesto == id_movimiento_detalle_presupuesto);
            if (validacion_proceso != null)
            {
                return Json(new { data = -1 }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { data = 1 }, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult ValidarAnulacionMovimiento(int? id_movimiento_detalle_presupuesto)
        {
            var validacion_proceso = db.movimiento_detalle_presupuesto.FirstOrDefault(x => x.id_movimiento_detalle_presupuesto == id_movimiento_detalle_presupuesto && x.estado == 1);
            if (validacion_proceso != null)
            {
                return Json(new { data = 1 }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { data = -1 }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult AnulacionProcesoMovimiento(int? id_movimiento_detalle_presupuesto)
        {
            if (id_movimiento_detalle_presupuesto != null)
            {
                var detalle_presupuesto = db.movimiento_detalle_presupuesto.FirstOrDefault(x => x.id_movimiento_detalle_presupuesto == id_movimiento_detalle_presupuesto);
                if (detalle_presupuesto != null)
                {
                    detalle_presupuesto.estado = 0;
                    db.SaveChanges();
                    return Json(new { data = 1 }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { data = -1 }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(new { data = -1 }, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult GetVerificarStatusMovimiento(int? id_movimiento_detalle_presupuesto)
        {
            var status = db.vobo.Where(x => x.id_movimiento_detalle_presupuesto == id_movimiento_detalle_presupuesto && x.id_etapa_vobo != 4);
            if (status.Count() > 0)
            {
                var conteo = 0;
                foreach (var item in status)
                {
                    var finalizado = db.vobo.FirstOrDefault(x => x.id_vobo == item.id_vobo && x.id_etapa_vobo == 1);
                    if (finalizado != null)
                    {
                        conteo++;
                    }
                }
                if (conteo == status.Count())
                {
                    return Json(new { data = 1 }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { data = -1 }, JsonRequestBehavior.AllowGet);
                }
                
            }
            else
            {
                return Json(new { data = -1 }, JsonRequestBehavior.AllowGet);
            }
                
        }


        public bool GetVerificarStatusMovimiento_backoffice(int? id_movimiento_detalle_presupuesto)
        {
            var status = db.vobo.Where(x => x.id_movimiento_detalle_presupuesto == id_movimiento_detalle_presupuesto && x.id_etapa_vobo != 4);
            if (status.Count() > 0)
            {
                var conteo = 0;
                foreach (var item in status)
                {
                    var finalizado = db.vobo.FirstOrDefault(x => x.id_vobo == item.id_vobo && x.id_etapa_vobo == 1);
                    if (finalizado != null)
                    {
                        conteo++;
                    }
                }
                if (conteo == status.Count())
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
            else
            {
                return false;
            }

        }



        public ActionResult IniciarProceso(int? id_detalle_presupuesto)
        {
            var presupuesto = db.detalle_presupuesto.FirstOrDefault(x => x.id_detalle_presupuesto == id_detalle_presupuesto);
            if (presupuesto != null)
            {
                //VALIDAMOS QUE NO HAYA INICIADO EL PROCESO ANTERIORMENTE O NO ESTE OBSERVADO
                var validacion_proceso = db.vobo.FirstOrDefault(x => (x.id_etapa_vobo == 1 || x.id_etapa_vobo == 2) && x.id_detalle_presupuesto == id_detalle_presupuesto);
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

        public ActionResult IniciarProcesoMovimiento(int? id_movimiento_detalle_presupuesto)
        {
            var presupuesto = db.movimiento_detalle_presupuesto.FirstOrDefault(x => x.id_movimiento_detalle_presupuesto == id_movimiento_detalle_presupuesto);
            if (presupuesto != null)
            {
                //VALIDAMOS QUE NO HAYA INICIADO EL PROCESO ANTERIORMENTE O NO ESTE OBSERVADO
                var validacion_proceso = db.vobo.FirstOrDefault(x => (x.id_etapa_vobo == 1 || x.id_etapa_vobo == 2) && x.id_movimiento_detalle_presupuesto== id_movimiento_detalle_presupuesto);
                if (validacion_proceso != null)
                {
                    return Json(new { data = -1 }, JsonRequestBehavior.AllowGet);
                }

                //ANTES VAMOS A VALIDAR QUE HAYA INGRESADO EL MOTIVO DE LA MODIFICACION
                var motivo_modificacion = db.movimiento_detalle_presupuesto.FirstOrDefault(x => x.id_movimiento_detalle_presupuesto == id_movimiento_detalle_presupuesto);
                if (motivo_modificacion.motivo_movimiento == null || motivo_modificacion.motivo_movimiento == "")
                {
                    return Json(new { data = -2 }, JsonRequestBehavior.AllowGet);
                }

                //OBTENEMOS LOS VOBOS
                var persona_vobo = db.personal_vobo.Where(x => x.estado == 1).ToList();
                //VAMOS A INICIAR EL PROCESO DE VISTOS BUENOS
                foreach (var item in persona_vobo)
                {
                    //VAMOS A VALIDAR QUE SI HAY UNO EN PROCESO LOS DEMÁS SEAN PENDIENTES
                    var validacion = db.vobo.FirstOrDefault(x => x.id_movimiento_detalle_presupuesto == id_movimiento_detalle_presupuesto && x.estado == 1 && x.id_etapa_vobo == 3);
                    vobo clase = new vobo();
                    clase.id_personal_vobo = item.id_personal_vobo;
                    clase.id_movimiento_detalle_presupuesto = id_movimiento_detalle_presupuesto;
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
            return GridDetallePresupuesto();
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
            return GridDetallePresupuesto();
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


        public ActionResult ViewerReporteComprobante(int? id, int? tipo_vobo)
        {
            if (id == null)
            {
                return PartialView("~/Views/Presupuesto/repComprobante.cshtml");
            }
            
            object[] data = new object[2];
            XtraReport rp = new Gestion_presupuesto.Reportes.Comprobante();
            rp.Parameters["id"].Value = id;
            rp.Parameters["tipo_vobo"].Value = tipo_vobo;
            rp.CreateDocument();
            data[0] = rp;
            return PartialView("~/Views/Presupuesto/repComprobante.cshtml", data);
            
        }


        [ValidateInput(false)]
        public ActionResult GridMovimientoPresupuesto(int? id_detalle_presupuesto)
        {
            if (id_detalle_presupuesto != 0 && id_detalle_presupuesto != null)
            {
                Session["idp"] = id_detalle_presupuesto;
            }
            var id = Convert.ToInt32(Session["idp"]);
            id_detalle_presupuesto = id_detalle_presupuesto == 0 || id_detalle_presupuesto == null ? id : id_detalle_presupuesto;

            //var model = db.movimiento_detalle_presupuesto.Where(x=>x.id_detalle_presupuesto == id_detalle_presupuesto);

            var model = db.sp_consulta_procesos_movimiento_compra(id_detalle_presupuesto).ToList();
            List<BandejaProcesoMovimientoCompra> clase = new List<BandejaProcesoMovimientoCompra>();
            model.ForEach(x =>
            {
                BandejaProcesoMovimientoCompra bpc = new BandejaProcesoMovimientoCompra();
                bpc.id_movimiento_detalle_presupuesto = x.id_movimiento_detalle_presupuesto;
                bpc.id_detalle_presupuesto = x.id_detalle_presupuesto;
                bpc.codigo = x.codigo;
                bpc.nombre_proceso = x.nombre_proceso;
                bpc.id_metodo_contratacion = x.id_metodo_contratacion;
                bpc.fecha_inicio = x.fecha_inicio;
                bpc.fecha_fin = x.fecha_fin;
                bpc.monto = x.monto;
                bpc.id_fuente_financiamiento = x.id_fuente_financiamiento;
                bpc.id_unidad_organizativa = x.id_unidad_organizativa;
                bpc.estado = x.estado;
                bpc.motivo_movimiento = x.motivo_movimiento;
                bpc.metodo_contratacion = x.metodo_contratacion;
                bpc.fuente_financiamiento = x.fuente_financiamiento;
                bpc.estatus_general = x.estatus_general;
                clase.Add(bpc);
            });

            return PartialView("~/Views/Presupuesto/_GridMovimientoPresupuesto.cshtml", model.ToList().OrderByDescending(x=>x.id_movimiento_detalle_presupuesto));
        }


        [HttpPost, ValidateInput(false)]
        public ActionResult GridMovimientoPresupuestoUpdate([ModelBinder(typeof(DevExpressEditorsBinder))] Gestion_presupuesto.Models.movimiento_detalle_presupuesto item, int? id_detalle_presupuesto)
        {
            var model = db.movimiento_detalle_presupuesto;
            if (ModelState.IsValid)
            {
                try
                {
                    var modelItem = model.FirstOrDefault(it => it.id_movimiento_detalle_presupuesto == item.id_movimiento_detalle_presupuesto);
                    if (modelItem != null)
                    {
                        modelItem.nombre_proceso = item.nombre_proceso;
                        modelItem.id_metodo_contratacion = item.id_metodo_contratacion;
                        modelItem.fecha_inicio = item.fecha_inicio;
                        modelItem.fecha_fin = item.fecha_fin;
                        modelItem.monto = item.monto;
                        modelItem.id_fuente_financiamiento = item.id_fuente_financiamiento;
                        modelItem.id_unidad_organizativa = item.id_unidad_organizativa;
                        modelItem.motivo_movimiento = item.motivo_movimiento;
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
            return GridMovimientoPresupuesto(id_detalle_presupuesto);
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult GridMovimientoPresupuestoDelete(System.Int32 id_movimiento_detalle_presupuesto)
        {
            var model = db.movimiento_detalle_presupuesto;
            if (id_movimiento_detalle_presupuesto >= 0)
            {
                try
                {
                    var item = model.FirstOrDefault(it => it.id_movimiento_detalle_presupuesto == id_movimiento_detalle_presupuesto);
                    if (item != null)
                        model.Remove(item);
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }
            return PartialView("~/Views/Presupuesto/_GridMovimientoPresupuesto.cshtml", model.ToList());
        }
    }
}