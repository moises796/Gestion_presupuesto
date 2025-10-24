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
            public int? id_detalle_presupuesto { get; set; }
            public string codigo { get; set; }
            public string nombre_proceso { get; set; }
            public int? id_metodo_contratacion { get; set; }
            public DateTime? fecha_inicio { get; set; }
            public DateTime? fecha_fin { get; set; }
            public double? monto { get; set; }
            public int? id_fuente_financiamiento { get; set; }
            public int? id_unidad_organizativa { get; set; }
            public int? estado { get; set; }
        }
        public partial class EmpleadoVobo
        {
            public int? id_personal_vobo { get; set; }
            public string nombre_empleado { get; set; }
        }
    }
}