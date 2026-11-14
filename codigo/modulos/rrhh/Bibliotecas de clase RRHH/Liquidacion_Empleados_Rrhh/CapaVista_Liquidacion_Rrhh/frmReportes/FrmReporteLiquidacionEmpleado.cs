using CapaControlador_Liquidacion_Rrhh;
using Microsoft.Reporting.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CapaVista_Liquidacion_Rrhh.frmReportes
{
    public partial class FrmReporteLiquidacionEmpleado : Form
    {
        private ClsModeloLiquidacion Liquidacion = new ClsModeloLiquidacion();
        public FrmReporteLiquidacionEmpleado()
        {
            InitializeComponent();
        }

        private void FrmReporteLiquidacionEmpleado_Load(object sender, EventArgs e)
        {
            try
            {
                ReportDataSource Fuente = new ReportDataSource("DsLiquidacion", Liquidacion.LiquidacionMetObtenerTodos());
                RrhhRdvLiquidacion.LocalReport.ReportEmbeddedResource = "CapaVista_Liquidacion_Rrhh.Reportes.RpLiquidacion.rdlc";
                RrhhRdvLiquidacion.LocalReport.DataSources.Clear();
                RrhhRdvLiquidacion.LocalReport.DataSources.Add(Fuente);
                RrhhRdvLiquidacion.RefreshReport();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            this.RrhhRdvLiquidacion.RefreshReport();

        }
    }
}
