/*
 * ==================================================================
 * Área : Rrhh
 * Autor : Cristian David Sipac Ispache
 * Carné : 9959-23-1567
 * Fecha : 06/11/2026
 * ==================================================================
 * Propósito :
 *  Presenta el reporte de detalles de nómina mediante
 *  ReportViewer. Consulta los detalles disponibles desde
 *  el controlador, los vincula al origen DsNominaDetalle
 *  y carga el diseño RDLC correspondiente.
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
    public partial class FrmReporteNominaDetalle : Form
    {
        private ClsModeloNominaDetalle NominaDetalle = new ClsModeloNominaDetalle();
        public FrmReporteNominaDetalle()
        {
            InitializeComponent();
        }

        private void FrmReporteNominaDetalle_Load(object sender, EventArgs e)
        {

            ReportDataSource reportDataSource = new ReportDataSource("DsNominaDetalle", NominaDetalle.RrhhMetObtenerTodos());
            reportViewer1.LocalReport.ReportEmbeddedResource = "CapaVista_Nomina.Reportes.RpReporteNominaDetalle.rdlc";
            reportViewer1.LocalReport.DataSources.Clear();
            reportViewer1.LocalReport.DataSources.Add(reportDataSource);
            this.reportViewer1.RefreshReport();
        }
    }
}
