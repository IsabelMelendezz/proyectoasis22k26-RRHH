/*
 * ==================================================================
 * Área : Recursos Humanos
 * Autor : daniella jurado
 * Carné : 0901-23-3679
 * Fecha : 23/10/2026
 * Modificado por : Daniella Jurado 0901-23-3679 (03/11/2026)
 * ==================================================================
 * Propósito :
 *  Ventana 9202 - Permisos. Mantenimiento de tblPermisoEmpleado con el Navegador.
 * Reglas especificas:
 *  Se ingresa el empleado, la fecha inicio y fin, el motivo y si es con
 *  goce de sueldo. Los días (totalPermisoEmpleado) se calculan solos con
 *  las fechas y el campo queda de solo lectura; así se guardan sin
 *  depender de un trigger. La fecha fin no puede ser anterior a la
 *  fecha inicio.
 * ===================================================================
*/

using System;
using System.Windows.Forms;
using CapaControlador_PermisosVacaciones.Clases;
using CapaVista_PermisosVacaciones.Utilidades;

namespace CapaVista_PermisosVacaciones.Formularios
{
    public partial class FrmPermisos : Form
    {
        private readonly ClsControladorPermisosVacaciones _Controlador = new ClsControladorPermisosVacaciones();

        public FrmPermisos()
        {
            InitializeComponent();
            RrhhNavPermisos.NavegadorMetConfigurar(
                _Controlador.RrhhFuncTablaPermisos(),
                _Controlador.RrhhFuncIdModulo(),
                _Controlador.RrhhFuncIdAplicacionPermisos());
            ClsValidacionFechas.RrhhMetConfigurar(this, RrhhNavPermisos, _Controlador.RrhhFuncFechasPermisos());

            // El panel de Ingresar/Modificar lo arma el Navegador después de este evento, por eso se espera con BeginInvoke.
            RrhhNavPermisos.NavegadorAccionSolicitada += Accion =>
            {
                if (Accion == "INGRESAR" || Accion == "MODIFICAR")
                    BeginInvoke((Action)RrhhMetConectarDias);
            };
        }

        // Los días no se escriben: se calculan con las fechas cada vez que cambian.
        private void RrhhMetConectarDias()
        {
            string[] Fechas = _Controlador.RrhhFuncFechasPermisos();
            var Inicio = RrhhFuncBuscar("NavegadorDtp" + Fechas[0]) as DateTimePicker;
            var Fin = RrhhFuncBuscar("NavegadorDtp" + Fechas[1]) as DateTimePicker;
            var Dias = RrhhFuncBuscar("NavegadorTxttotalPermisoEmpleado") as TextBox;
            if (Inicio == null || Fin == null || Dias == null) return;

            Dias.ReadOnly = true;
            EventHandler Calcular = (Remitente, Evento) =>
                Dias.Text = _Controlador.RrhhFuncDiasPermiso(Inicio.Value, Fin.Value).ToString();
            Inicio.ValueChanged += Calcular;
            Fin.ValueChanged += Calcular;
            Calcular(null, EventArgs.Empty);
        }

        private Control RrhhFuncBuscar(string Nombre)
        {
            Control[] Encontrados = RrhhNavPermisos.Controls.Find(Nombre, true);
            return Encontrados.Length > 0 ? Encontrados[0] : null;
        }
    }
}
