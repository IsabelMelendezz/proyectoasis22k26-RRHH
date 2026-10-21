using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CapaVista_Liquidacion_Rrhh.Formularios
{
    public partial class FrmMotivoBajaNavegador : Form
    {
        public FrmMotivoBajaNavegador()
        {
            InitializeComponent();
            RrhhNavMotivoBaja.NavegadorMetConfigurar("tblMotivoBaja", 9000, 9702);
        }
    }
}
