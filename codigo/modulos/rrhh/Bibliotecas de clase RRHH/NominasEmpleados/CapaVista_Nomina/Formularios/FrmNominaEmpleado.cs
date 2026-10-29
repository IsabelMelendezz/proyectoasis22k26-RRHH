using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CapaVista_Nomina.Formularios;

namespace CapaVista_Nomina.Formularios
{
    public partial class FrmNominaEmpleado : Form
    {
        public FrmNominaEmpleado()
        {
            InitializeComponent();
        }

        private void RrhhBtnAbrirPercepcionDeducciones_Click(object sender, EventArgs e)
        {
            FrmPercepcionesDeducciones Concepto = new FrmPercepcionesDeducciones();
            Concepto.ShowDialog(this);
        }

        private void RrhhBtnAbrirNomina_Click(object sender, EventArgs e)
        {
            
            FrmNominaEncabezado Nomina = new FrmNominaEncabezado();
            Nomina.ShowDialog(this);
        }
    }
}
