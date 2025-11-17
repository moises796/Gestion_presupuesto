using DevExpress.CodeParser.VB;
using DevExpress.Web.Mvc;
using Gestion_presupuesto.Helpers;
using Gestion_presupuesto.Models;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity.Validation;
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


        public ActionResult VoboPresidencia()
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
                bv.id = x.id;
                bv.codigo = x.codigo;
                bv.nombre_proceso = x.nombre_proceso;
                bv.id_metodo_contratacion = x.id_metodo_contratacion;
                bv.fecha_inicio = x.fecha_inicio;
                bv.fecha_fin = x.fecha_fin;
                bv.monto = x.monto;
                bv.monto_goes = x.monto_goes;
                bv.monto_propio = x.monto_propio;
                bv.monto_proyectos = x.monto_proyectos;
                bv.monto_compensacion = x.monto_compensacion;
                bv.id_fuente_financiamiento = x.id_fuente_financiamiento;
                bv.id_unidad_organizativa = x.id_unidad_organizativa;
                bv.estado = x.estado;
                bv.motivo_movimiento = x.motivo_movimiento == null ? "N/A" : x.motivo_movimiento;
                bv.tipo_vobo  = x.tipo_vobo;
                bv.proceso_column = x.proceso_column;
                bv.fecha_movimiento = x.fecha_movimiento;
                clase.Add(bv);
            });

            if (clase.ToList().Count>0)
            {
                return PartialView("~/Views/Vobo/_GridVobo.cshtml", clase.ToList().OrderByDescending(x => x.fecha_movimiento));
            }
            else
            {
                return PartialView("~/Views/Vobo/_GridVobo.cshtml");
            }

            
        }


        [HttpPost, ValidateInput(false)]
        public ActionResult GridVoboUpdate([ModelBinder(typeof(DevExpressEditorsBinder))] Gestion_presupuesto.Helpers.Clases.BandejaVobo item)
        {
            if (item.tipo_vobo == 1 || item.tipo_vobo == 3)
            {
                var model = db.detalle_presupuesto;
                if (ModelState.IsValid)
                {
                    try
                    {
                        var modelItem = model.FirstOrDefault(it => it.id_detalle_presupuesto == item.id);
                        if (modelItem != null)
                        {
                            modelItem.nombre_proceso = item.nombre_proceso;
                            modelItem.id_metodo_contratacion = item.id_metodo_contratacion;
                            modelItem.fecha_inicio = item.fecha_inicio;
                            modelItem.fecha_fin = item.fecha_fin;
                            if (item.id_fuente_financiamiento == 5)
                            {
                                //ES MIXTO
                                modelItem.monto_goes = item.monto_goes;
                                modelItem.monto_propio = item.monto_propio;
                                modelItem.monto_proyectos = item.monto_proyectos;
                                modelItem.monto_compensacion = item.monto_compensacion;
                                modelItem.monto = item.monto_goes + item.monto_propio + item.monto_proyectos + item.monto_compensacion;
                            }
                            else
                            {
                                modelItem.monto_goes = 0;
                                modelItem.monto_propio = 0;
                                modelItem.monto_proyectos = 0;
                                modelItem.monto_compensacion = 0;
                                modelItem.monto = item.monto;
                            }
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
            else 
            {
                var model = db.movimiento_detalle_presupuesto;
                if (ModelState.IsValid)
                {
                    try
                    {
                        var modelItem = model.FirstOrDefault(it => it.id_movimiento_detalle_presupuesto == item.id);
                        if (modelItem != null)
                        {
                            modelItem.nombre_proceso = item.nombre_proceso;
                            modelItem.id_metodo_contratacion = item.id_metodo_contratacion;
                            modelItem.fecha_inicio = item.fecha_inicio;
                            modelItem.fecha_fin = item.fecha_fin;
                            if (item.id_fuente_financiamiento == 5)
                            {
                                //ES MIXTO
                                modelItem.monto_goes = item.monto_goes;
                                modelItem.monto_propio = item.monto_propio;
                                modelItem.monto_proyectos = item.monto_proyectos;
                                modelItem.monto_compensacion = item.monto_compensacion;
                                modelItem.monto = item.monto_goes + item.monto_propio + item.monto_proyectos + item.monto_compensacion;
                            }
                            else
                            {
                                modelItem.monto_goes = null;
                                modelItem.monto_propio = null;
                                modelItem.monto_proyectos = null;
                                modelItem.monto_compensacion = null;
                                modelItem.monto = item.monto;
                            }
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
            
        }

        public ActionResult ObservarAnulacion(int? id_vobo)
        {
            var detalle_presupuesto = db.vobo.FirstOrDefault(x => x.id_vobo == id_vobo);
            if (detalle_presupuesto != null)
            {
                //ANULAMOS TODOS LOS VOBOS
                var id_detalle_presupuesto = detalle_presupuesto.id_detalle_presupuesto;
                var lista_vobo = db.vobo.Where(x => x.id_detalle_presupuesto == id_detalle_presupuesto).ToList();
                lista_vobo.ForEach(x => {
                    x.id_etapa_vobo = 4;
                    db.SaveChanges();
                });

                //VAMOS A TRAER LOS VOBOS ANTERIORES PARA PASARLOS A ESTADO 1
                var vobos_anteriores = db.detalle_presupuesto_anulacion.Where(x => x.id_detalle_presupuesto == id_detalle_presupuesto && x.estado == 1).ToList();
                vobos_anteriores.ForEach(x => {
                    var vobo = db.vobo.FirstOrDefault(y => y.id_vobo == x.id_vobo);
                    vobo.id_etapa_vobo = 1;
                    db.SaveChanges();
                });

                //AHORA PASAMOS A ESTADO CERO LOS VOBOS DE ANULACION
                vobos_anteriores.ForEach(x =>
                {
                    x.estado = 0;
                    db.SaveChanges();
                });

            }

            return Json(new { data = 1 }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Observar(int? id_vobo, string instruccion)
        {
            var vobo = db.vobo.FirstOrDefault(x => x.id_vobo == id_vobo);
            if (vobo == null)
            {
                return Json(new { data = -1 }, JsonRequestBehavior.AllowGet);
            }
            vobo.instruccion = instruccion;
            db.SaveChanges();
            //VAMOS A PASAR TODO A LA ETAPA 4 DE OBSERVADO
            if (vobo.id_detalle_presupuesto!=null)
            {
                var lista_vobo = db.vobo.Where(x => x.id_detalle_presupuesto == vobo.id_detalle_presupuesto).ToList();
                lista_vobo.ForEach(x => {
                    x.id_etapa_vobo = 4;
                    db.SaveChanges();
                });

                return Json(new { data = 1 }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var lista_vobo = db.vobo.Where(x => x.id_movimiento_detalle_presupuesto == vobo.id_movimiento_detalle_presupuesto).ToList();
                lista_vobo.ForEach(x => {
                    x.id_etapa_vobo = 4;
                    db.SaveChanges();
                });

                return Json(new { data = 1 }, JsonRequestBehavior.AllowGet);
            }

            
        }

        public ActionResult Aprobar(int? id_vobo)
        {
            //VAMOS A PASAR TODO A LA ETAPA 4 DE OBSERVADO
            var lista_vobo = db.vobo.FirstOrDefault(x=>x.id_vobo == id_vobo);
            lista_vobo.id_etapa_vobo = 1;
            db.SaveChanges();

            if (lista_vobo.id_detalle_presupuesto != null)
            {
                var siguiente_vobo = db.vobo.FirstOrDefault(x => x.id_etapa_vobo == 2 && x.id_detalle_presupuesto == lista_vobo.id_detalle_presupuesto);
                if (siguiente_vobo != null)
                {
                    siguiente_vobo.id_etapa_vobo = 3;
                    db.SaveChanges();
                }
                else
                {
                    //QUIERE DECIR QUE YA TERMINO, AHORA DEBO VALIDAR QUE SEA SOLICITUD DE ELIMINACION
                    var solicitud_eliminacion = db.detalle_presupuesto_anulacion.Where(x => x.id_detalle_presupuesto == lista_vobo.id_detalle_presupuesto && x.estado==1).ToList();
                    if (solicitud_eliminacion.Count > 0)
                    {
                        solicitud_eliminacion.ForEach(x => {
                            x.estado = 0;
                            db.SaveChanges();
                        });

                        //AHORA PASO A ANULADA LA SOLICITUD
                        var detalle = db.detalle_presupuesto.FirstOrDefault(x => x.id_detalle_presupuesto == lista_vobo.id_detalle_presupuesto);
                        detalle.estado = 0;
                        db.SaveChanges();
                    }

                    

                }
                    return Json(new { data = 1 }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var siguiente_vobo = db.vobo.FirstOrDefault(x => x.id_etapa_vobo == 2 && x.id_movimiento_detalle_presupuesto == lista_vobo.id_movimiento_detalle_presupuesto);
                if (siguiente_vobo != null)
                {
                    siguiente_vobo.id_etapa_vobo = 3;
                    db.SaveChanges();
                }
                return Json(new { data = 1 }, JsonRequestBehavior.AllowGet);
            }
            
        }

        [ValidateInput(false)]
        public ActionResult GridListadoVobo(int? id_detalle_presupuesto, int? id_movimiento_detalle_presupuesto)
        {
            if (id_detalle_presupuesto != null)
            {
                var model = db.vobo.Where(x => x.id_detalle_presupuesto == id_detalle_presupuesto && x.id_etapa_vobo != 4);

                return PartialView("~/Views/Vobo/_GridListadoVobo.cshtml", model.ToList());
            }
            else
            {
                var model = db.vobo.Where(x => x.id_movimiento_detalle_presupuesto == id_movimiento_detalle_presupuesto && x.id_etapa_vobo != 4);

                return PartialView("~/Views/Vobo/_GridListadoVobo.cshtml", model.ToList());
            }
            
        }

        //public ActionResult ObtenerEstadoVobo(int? id, int? tipo_vobo)
        //{
        //    if (id != null && tipo_vobo != null)
        //    {
        //        if (tipo_vobo == 1)
        //        {
        //            //SACAMOS TODOS LOS VOBOS
        //            var listado = db.vobo.Where(x => x.id_detalle_presupuesto == id && x.id_etapa_vobo != 5).ToList();
        //            //SACAMOS LOS APROBADOS
        //            return Json(new { data = 1 }, JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            return Json(new { data = 1 }, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //    else
        //    {
        //        return Json(new { data = -1 }, JsonRequestBehavior.AllowGet);
        //    }
            
        //}


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