/*
 * ==================================================================
 * Área : Rrhh
 * Autor : Cristian David Sipac Ispache
 * Carné : 9959-23-1567
 * Fecha : 06/11/2026
 * ==================================================================
 * Propósito :
 *  Presenta el reporte de rangos salariales de percepciones
 *  y deducciones mediante ReportViewer. Obtiene los rangos
 *  registrados, los vincula al origen DsRango y carga
 *  el diseño RDLC correspondiente.
 * ==================================================================
 */

using CapaControlador_Nomina.Modelos_de_controladores;
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

namespace CapaVista_Nomina.Reportes
{
    public partial class FrmReporteRangos : Form
    {
        private ClsModeloPercepcionDeduccionRango Rango = new ClsModeloPercepcionDeduccionRango();
        public FrmReporteRangos()
        {
            InitializeComponent();
        }

        private void FrmReporteRangos_Load(object sender, EventArgs e)
        {

            ReportDataSource reportDataSource = new ReportDataSource("DsRango", Rango.RrhhMetObtenerTodos());
            reportViewer1.LocalReport.ReportEmbeddedResource = "CapaVista_Nomina.Reportes.RpReporteRangos.rdlc";
            reportViewer1.LocalReport.DataSources.Clear();
            reportViewer1.LocalReport.DataSources.Add(reportDataSource);
            this.reportViewer1.RefreshReport();
        }
    }
}
