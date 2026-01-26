using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gestion_presupuesto.Helpers
{
    public class Clases
    {
        public partial class BandejaVobo
        {
            public int? id_vobo {  get; set; }
            public int? id { get; set; }
            public string codigo { get; set; }
            public string nombre_proceso { get; set; }
            public int? id_metodo_contratacion { get; set; }
            public DateTime? fecha_inicio { get; set; }
            public DateTime? fecha_fin { get; set; }
            public double? monto { get; set; }
            public double? monto_goes { get; set; }
            public double? monto_propio { get; set; }
            public double? monto_proyectos { get; set; }
            public double? monto_compensacion { get; set; }
            public int? id_fuente_financiamiento { get; set; }
            public int? id_unidad_organizativa { get; set; }
            public int? estado { get; set; }
            public int? tipo_vobo { get; set; }
            public string motivo_movimiento { get; set; }
            public string proceso_column { get; set; }
            public DateTime? fecha_movimiento { get; set; }

        }

        public partial class BandejaProcesoCompra
        {
            public int? id_detalle_presupuesto { get; set; }
            public string codigo { get; set; }
            public string nombre_proceso { get; set; }
            public int? id_metodo_contratacion { get; set; }
            public DateTime? fecha_inicio { get; set; }
            public DateTime? fecha_fin { get; set; }
            public double? monto { get; set; }
            public double? monto_goes { get; set; }
            public double? monto_propio { get; set; }
            public double? monto_proyectos { get; set; }
            public double? monto_compensacion { get; set; }
            public int? id_fuente_financiamiento { get; set; }
            public int? id_tipo_fuente_financiamiento { get; set; }
            public int? identificador_fuente_financiamiento { get; set; }
            public int? id_unidad_organizativa { get; set; }
            public int? estado { get; set; }
            public string metodo_contratacion { get; set; }
            public string fuente_financiamiento { get; set; }
            public string estatus_proceso { get; set; }
            public string estatus_general { get; set; }
        }
        public partial class BandejaProcesoMovimientoCompra
        {
            public int? id_movimiento_detalle_presupuesto { get; set; }
            public int? id_detalle_presupuesto { get; set; }
            public string codigo { get; set; }
            public string nombre_proceso { get; set; }
            public int? id_metodo_contratacion { get; set; }
            public DateTime? fecha_inicio { get; set; }
            public DateTime? fecha_fin { get; set; }
            public double? monto { get; set; }
            public double? monto_goes { get; set; }
            public double? monto_propio { get; set; }
            public double? monto_proyectos { get; set; }
            public double? monto_compensacion { get; set; }
            public int? id_fuente_financiamiento { get; set; }
            public int? id_tipo_fuente_financiamiento { get; set; }
            public int? identificador_fuente_financiamiento { get; set; }
            public int? id_unidad_organizativa { get; set; }
            public int? estado { get; set; }
            public string motivo_movimiento { get; set; }
            public string metodo_contratacion { get; set; }
            public string fuente_financiamiento { get; set; }
            public string estatus_general { get; set; }
        }
        public partial class EmpleadoVobo
        {
            public int? id_personal_vobo { get; set; }
            public string nombre_empleado { get; set; }
        }
    }
}