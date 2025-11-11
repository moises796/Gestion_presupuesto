using DevExpress.XtraReports.UI;
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;

namespace Gestion_presupuesto.Reportes
{
    public partial class Consulta : DevExpress.XtraReports.UI.XtraReport
    {
        public Consulta()
        {
            InitializeComponent();
        }

        private void Consulta_DataSourceDemanded(object sender, EventArgs e)
        {
            dsConsultaTableAdapters.sp_general_proceso_compraTableAdapter c = new dsConsultaTableAdapters.sp_general_proceso_compraTableAdapter();
            c.Fill(dsConsulta1.sp_general_proceso_compra, Convert.ToInt32(anio.Value.ToString()));
        }
    }
}
