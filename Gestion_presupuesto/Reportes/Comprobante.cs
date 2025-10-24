using DevExpress.XtraReports.UI;
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;

namespace Gestion_presupuesto.Reportes
{
    public partial class Comprobante : DevExpress.XtraReports.UI.XtraReport
    {
        public Comprobante()
        {
            InitializeComponent();
        }

        private void Comprobante_DataSourceDemanded(object sender, EventArgs e)
        {
            dsComprobante1.EnforceConstraints = true;
            dsComprobanteTableAdapters.sp_comprobanteTableAdapter c = new dsComprobanteTableAdapters.sp_comprobanteTableAdapter();
            c.Fill(dsComprobante1.sp_comprobante, Convert.ToInt32(id_detalle_presupuesto.Value.ToString()));
            dsComprobanteTableAdapters.sp_voboTableAdapter v = new dsComprobanteTableAdapters.sp_voboTableAdapter();
            v.Fill(dsComprobante1.sp_vobo, Convert.ToInt32(id_detalle_presupuesto.Value.ToString()));
        }
    }
}
