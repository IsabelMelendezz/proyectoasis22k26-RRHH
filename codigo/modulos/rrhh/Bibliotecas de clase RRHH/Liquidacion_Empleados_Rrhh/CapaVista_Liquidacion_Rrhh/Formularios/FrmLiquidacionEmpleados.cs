using CapaControlador_Liquidacion_Rrhh;
using CapaControlador_Liquidacion_Rrhh.Objetos_de_valor;
using CapaControlador_Seguridad;
using CapaControlador_Seguridad.Objetos_de_valor;
using CapaVista_Liquidacion_Rrhh.frmReportes;
using CapaVista_Seguridad.Ayudas;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CapaVista_Liquidacion_Rrhh.Formularios
{
    public partial class FrmLiquidacionEmpleados : Form
    {
        private ClsModeloLiquidacion _Liquidacion = new ClsModeloLiquidacion();
        private ClsPermisoAplicacion _MisPermisos;
        private BindingSource _BindingSource = new BindingSource();
        private List<ClsModeloLiquidacion> _ListaLiquidaciones = new List<ClsModeloLiquidacion>();
        private bool _EsLiquidacionExistente;
        private decimal _TotalPercepcionBase;
        private decimal _TotalDeduccionBase;
        private bool _RecalculandoConceptos;
        private DataTable _CatalogoPercepciones;
        private readonly Dictionary<int, ClsModeloDetalleLiquidacion>
            _DetallesOriginales =
                new Dictionary<int, ClsModeloDetalleLiquidacion>();

        private const int ID_MODULO = 9000;
        private const int ID_APLICACION = 9701;

        public FrmLiquidacionEmpleados()
        {
            InitializeComponent();
        }

       

        private void RrhhBtnGuardarLiqui_Click(object sender, EventArgs e)
        {
            if (!_MisPermisos.PuedeInsertar)
            {
                MessageBox.Show("No tienes permiso para agregar liquidaciones.", "Acceso denegado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                if (_EsLiquidacionExistente)
                {
                    MessageBox.Show(
                        "Ya existe una liquidación para el empleado y la baja seleccionados. Use Modificar para actualizarla.",
                        "Liquidación existente", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (string.IsNullOrWhiteSpace(RrhhTxtIdEmpleado.Text) || string.IsNullOrWhiteSpace(RrhhTxtIdBajaEmpleado.Text))
                {
                    MessageBox.Show("Seleccione un empleado y una baja del listado.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!RrhhMetValidarEmpresaSeleccionada())
                    return;

                if (!RrhhMetValidarNumeroFiniquito(0))
                    return;

                if (!RrhhMetPrepararDetallesParaGuardar(
                    out List<ClsModeloDetalleLiquidacion> detalles))
                    return;

                LlenarModeloDesdeUI();
                _Liquidacion.DetallesLiquidacion = detalles;
                _Liquidacion.Estado = EstadoEntidadLiquidacion.Added;

                bool Valido = new ClsValidacionDatos(_Liquidacion).SeguridadMetValidar();
                if (Valido)
                {
                    string Resultado = _Liquidacion.LiquidacionMetGrabarCambios();
                    MessageBox.Show(Resultado);
                    RrhhdMetListarLiquidacion();
                }
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.ToString());
            }
        }

        private void RrhhBtnModificarLiqui_Click(object sender, EventArgs e)
        {
            if (!_MisPermisos.PuedeEditar)
            {
                MessageBox.Show("No tienes permiso para modificar liquidaciones.", "Acceso denegado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                if (string.IsNullOrWhiteSpace(RrhhTxtIdLiquidacion.Text))
                {
                    MessageBox.Show("Seleccione o haga clic sobre una fila del listado para modificar.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!RrhhMetValidarEmpresaSeleccionada())
                    return;

                int idLiquidacion = Convert.ToInt32(RrhhTxtIdLiquidacion.Text);
                if (!RrhhMetValidarNumeroFiniquito(idLiquidacion))
                    return;

                if (!RrhhMetPrepararDetallesParaGuardar(
                    out List<ClsModeloDetalleLiquidacion> detalles))
                    return;

                LlenarModeloDesdeUI();
                _Liquidacion.DetallesLiquidacion = detalles;
                _Liquidacion.IdLiquidacion = idLiquidacion;
                _Liquidacion.Estado = EstadoEntidadLiquidacion.Modified;

                bool Valido = new ClsValidacionDatos(_Liquidacion).SeguridadMetValidar();
                if (Valido)
                {
                    string Resultado = _Liquidacion.LiquidacionMetGrabarCambios();
                    MessageBox.Show(Resultado);
                    RrhhdMetListarLiquidacion();
                }
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.ToString());
            }
        }

        private void RrhhBtnEliminarLiqui_Click(object sender, EventArgs e)
        {
            if (!_MisPermisos.PuedeEliminar)
            {
                MessageBox.Show("No tienes permiso para eliminar liquidaciones.", "Acceso denegado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                if (string.IsNullOrWhiteSpace(RrhhTxtIdLiquidacion.Text))
                {
                    MessageBox.Show("Seleccione una fila del listado para eliminar.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DialogResult Confirmacion = MessageBox.Show(
                    "¿Está seguro de que desea eliminar la Liquidación seleccionada?",
                    "Confirmar eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (Confirmacion == DialogResult.Yes)
                {
                    _Liquidacion.IdLiquidacion = Convert.ToInt32(RrhhTxtIdLiquidacion.Text);
                    _Liquidacion.Estado = EstadoEntidadLiquidacion.Deleted;

                    string Resultado = _Liquidacion.LiquidacionMetGrabarCambios();
                    MessageBox.Show(Resultado);
                    RrhhdMetListarLiquidacion();
                }
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.ToString());
            }
        }

        private void RrhhBtnImprimirLiqui_Click(object sender, EventArgs e)
        {
            FrmReporteLiquidacionEmpleado ReporteLiquidacion = new FrmReporteLiquidacionEmpleado();
            ReporteLiquidacion.ShowDialog();
        }

      

      
    


private void RrhhBtnCatalogoMotivoBaja_Click(object sender, EventArgs e)
        {
            FrmMotivoBajaNavegador MotivoBaja = new FrmMotivoBajaNavegador();
            MotivoBaja.ShowDialog();
        }

        private void RrhhBtnCatalogoParametroLey_Click(object sender, EventArgs e)
        {
            FrmParametrosLeyNavegador ParametroLey = new FrmParametrosLeyNavegador();
            ParametroLey.ShowDialog();
        }

        private void RrhhBtnCatalogoBajaEmpleado_Click(object sender, EventArgs e)
        {
            FrmBajaEmpleado BajaEmpleado = new FrmBajaEmpleado();
            BajaEmpleado.ShowDialog();
        }

        private void RrhhDgvLiquidaciones_RowHeaderMouseClick_1(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow fila = RrhhDgvLiquidaciones.Rows[e.RowIndex];
            if (fila.IsNewRow) return;

            try
            {
                RrhhTxtIdLiquidacion.Text = fila.Cells["colIdLiquidacion"].Value?.ToString();
                RrhhTxtIdEmpresa.Text = fila.Cells["colIdEmpresa"].Value?.ToString();
                if (int.TryParse(RrhhTxtIdEmpresa.Text, out int idEmpresa))
                    RrhhMetSeleccionarEmpresa(idEmpresa);
                RrhhTxtIdEmpleado.Text = fila.Cells["colIdEmpleado"].Value?.ToString();
                RrhhTxtIdBajaEmpleado.Text = fila.Cells["colIdBajaEmpleado"].Value?.ToString();
                RrhhTxtDiasLaborados.Text = fila.Cells["colDiasLaborados"].Value?.ToString();
                RrhhTxtSalarioMensual.Text = fila.Cells["colSalarioMensual"].Value?.ToString();
                RrhhTxtSalarioPromedio.Text = fila.Cells["colSalarioPromedio"].Value?.ToString();
                RrhhTxtBaseIndenmizacion.Text = fila.Cells["colBaseIndemnizacion"].Value?.ToString();
                RrhhTxtSalarioDia.Text = fila.Cells["colSalarioDia"].Value?.ToString();
                RrhhTxtTotalPercepcion.Text = fila.Cells["colTotalPercepcion"].Value?.ToString();
                RrhhTxtTotalDeduccion.Text = fila.Cells["colTotalDeduccion"].Value?.ToString();
                RrhhTxtTotalNeto.Text = fila.Cells["colTotalNeto"].Value?.ToString();
                RrhhTxtParametrosUsados.Text = fila.Cells["colParametrosUsados"].Value?.ToString();
                RrhhTxtNumeroFiniquito.Text = fila.Cells["colNumeroFiniquito"].Value?.ToString();
                RrhhTxtMontoLetras.Text = fila.Cells["colMontoLetras"].Value?.ToString();
                RrhhTxtIdUsuCrea.Text = fila.Cells["colIdUsuarioCrea"].Value?.ToString();
                RrhhTxtUsuAprueba.Text = fila.Cells["colIdUsuarioAprueba"].Value?.ToString();
                _EsLiquidacionExistente = true;

                string estadoString = fila.Cells["colEstado"].Value?.ToString();
                RrhhMetSeleccionarEstadoLiquidacion(estadoString);

                if (int.TryParse(RrhhTxtIdEmpleado.Text, out int idEmp))
                    RrhhCboEmpleado.SelectedValue = idEmp;

                if (int.TryParse(RrhhTxtIdBajaEmpleado.Text, out int idBaja))
                    RrhhCboBajaEmpleado.SelectedValue = idBaja;

                if (DateTime.TryParse(fila.Cells["colFechaIngreso"].Value?.ToString(), out DateTime fechaIng))
                    RrhhDtpFechaIngresoEmp.Value = fechaIng;

                if (DateTime.TryParse(fila.Cells["colFechaBaja"].Value?.ToString(), out DateTime fechaBaja))
                    RrhhDtpFechaBajaEmpleado.Value = fechaBaja;

                int idLiquidacionSeleccionada = Convert.ToInt32(
                    fila.Cells["colIdLiquidacion"].Value);
                RrhhTxtIdLiquidacion.Text =
                    idLiquidacionSeleccionada.ToString();
                RrhhMetCargarDetallesLiquidacion(idLiquidacionSeleccionada);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar los datos: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FrmLiquidacionEmpleados_Load(object sender, EventArgs e)
        {
             
            try
            {
                var MapaBotones = new Dictionary<Control, TipoPermiso>
                {
                    { RrhhBtnGuardarLiqui,   TipoPermiso.Insertar },
                    { RrhhBtnModificarLiqui, TipoPermiso.Editar },
                    { RrhhBtnImprimirLiqui,  TipoPermiso.Imprimir },
                    { RrhhBtnEliminarLiqui,  TipoPermiso.Eliminar }
                };

                _MisPermisos = ClsSeguridadFormHelper.SeguridadMetInicializarSeguridad(
                    this, ID_MODULO, ID_APLICACION, MapaBotones);

                if (!_MisPermisos.TieneAcceso)
                    return;

                RrhhMetConfigurarEstadosLiquidacion();
                RrhhMetCargarCombos();
                _BindingSource.CurrentChanged += RrhhBsCurrentChanged;
                RrhhMetConfigurarColumnasLiquidacion();
                RrhhMetConfigurarGridConceptos();
                RrhhMetConfigurarGridLiquidacionDetalle();
                _CatalogoPercepciones =
                    _Liquidacion.LiquidacionMetObtenerPercepcionesActivas();
                RrhhDgvLiquidaciones.DataSource = _BindingSource;
                RrhhdMetListarLiquidacion();

                RrhhBtnInicioLiqui.Click += RrhhBtnInicioLiqui_Click;
                RrhhBtnAnteriorLiqui.Click += RrhhBtnAnteriorLiqui_Click;
                RrhhBtnSiguienteLiqui.Click += RrhhBtnSiguienteLiqui_Click;
                RrhhBtnFinLiqui.Click += RrhhBtnFinLiqui_Click;
            }
            catch (Exception Ex)
            {
                MessageBox.Show("Error al cargar el formulario: " + Ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RrhhBsCurrentChanged(object sender, EventArgs e)
        {
            if (_BindingSource.Current is ClsModeloLiquidacion liquidacion)
            {
                _EsLiquidacionExistente = true;
                RrhhTxtIdLiquidacion.Text = liquidacion.IdLiquidacion.ToString();
                RrhhTxtIdEmpresa.Text = liquidacion.IdEmpresa.ToString();
                RrhhMetSeleccionarEmpresa(liquidacion.IdEmpresa);
                RrhhTxtIdEmpleado.Text = liquidacion.IdEmpleado.ToString();
                RrhhTxtIdBajaEmpleado.Text = liquidacion.IdBajaEmpleado.ToString();

                RrhhDtpFechaIngresoEmp.Value = liquidacion.FechaIngresoLiquidacion;
                RrhhDtpFechaBajaEmpleado.Value = liquidacion.FechaBajaLiquidacion;

                RrhhTxtDiasLaborados.Text = liquidacion.DiasLaboradosLiquidacion.ToString();
                RrhhTxtSalarioMensual.Text = liquidacion.SalarioMensualLiquidacion.ToString();
                RrhhTxtSalarioPromedio.Text = liquidacion.SalarioPromedioLiquidacion.ToString();
                RrhhTxtBaseIndenmizacion.Text = liquidacion.BaseIndemnizacionLiquidacion.ToString();
                RrhhTxtSalarioDia.Text = liquidacion.SalarioDiaLiquidacion.ToString();
                RrhhTxtTotalPercepcion.Text = liquidacion.TotalPercepcionLiquidacion.ToString();
                RrhhTxtTotalDeduccion.Text = liquidacion.TotalDeduccionLiquidacion.ToString();
                RrhhTxtTotalNeto.Text = liquidacion.TotalNetoLiquidacion.ToString();

                RrhhTxtParametrosUsados.Text = liquidacion.ParametrosUsadosLiquidacion;
                RrhhTxtNumeroFiniquito.Text = liquidacion.NumeroFiniquitoLiquidacion;
                RrhhTxtMontoLetras.Text = liquidacion.MontoLetrasFiniquitoLiquidacion;
                RrhhTxtUsuAprueba.Text = liquidacion.IdUsuarioAprueba.ToString();

                string estadoModelo = liquidacion.EstadoLiquidacion;
                RrhhMetSeleccionarEstadoLiquidacion(estadoModelo);

                if (RrhhCboEmpleado.DataSource != null)
                    RrhhCboEmpleado.SelectedValue = liquidacion.IdEmpleado;

                if (RrhhCboBajaEmpleado.DataSource != null)
                    RrhhCboBajaEmpleado.SelectedValue = liquidacion.IdBajaEmpleado;

                RrhhTxtIdLiquidacion.Text =
                    liquidacion.IdLiquidacion.ToString();
                RrhhMetCargarDetallesLiquidacion(liquidacion.IdLiquidacion);
            }
        }

        private void RrhhMetCargarCombos()
        {
            try
            {
                // 1. Cargar empresas activas sin seleccionar una por defecto.
                RrhhCboEmpresa.SelectedIndexChanged -= RrhhCboEmpresa_SelectedIndexChanged;
                RrhhCboEmpresa.DataSource = null;
                RrhhCboEmpresa.DisplayMember = "nombreEmpresa";
                RrhhCboEmpresa.ValueMember = "idEmpresa";
                RrhhCboEmpresa.DropDownStyle = ComboBoxStyle.DropDownList;
                RrhhCboEmpresa.DataSource = _Liquidacion.LiquidacionMetObtenerEmpresas();
                RrhhCboEmpresa.SelectedIndex = -1;
                RrhhTxtIdEmpresa.Clear();
                RrhhCboEmpresa.SelectedIndexChanged += RrhhCboEmpresa_SelectedIndexChanged;

                // 2. Cargar el ComboBox de empleados.
                RrhhCboEmpleado.SelectedIndexChanged -= RrhhCboEmpleado_SelectedIndexChanged;

                RrhhCboEmpleado.DisplayMember = "nombreCompleto";
                RrhhCboEmpleado.ValueMember = "idEmpleado";


                RrhhCboEmpleado.DataSource = _Liquidacion.LiquidacionMetObtenerEmpleados();

                RrhhCboEmpleado.SelectedIndex = -1;
                RrhhCboEmpleado.SelectedIndexChanged += RrhhCboEmpleado_SelectedIndexChanged;

                // 3. Limpiar el ComboBox de Bajas (se llenará hasta que elijas un empleado)
                RrhhCboBajaEmpleado.DataSource = null;
                RrhhCboBajaEmpleado.Items.Clear();

                // 4. Cargar Usuario Creador (Mapeado a Sesión)
                RrhhTxtIdUsuCrea.Text = ClsSesionSeguridad.IdUsuario.ToString();
                RrhhTxtUsuAprueba.Text = ClsSesionSeguridad.IdUsuario.ToString();

                RrhhMetSeleccionarEstadoLiquidacion("BORRADOR");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar combos: {ex.Message}");
            }
        }

        private void RrhhCboEmpresa_SelectedIndexChanged(object sender, EventArgs e)
        {
            RrhhTxtIdEmpresa.Text = RrhhCboEmpresa.SelectedValue != null &&
                                    int.TryParse(RrhhCboEmpresa.SelectedValue.ToString(),
                                        out int idEmpresa)
                ? idEmpresa.ToString()
                : string.Empty;
        }

        private void RrhhMetSeleccionarEmpresa(int idEmpresa)
        {
            if (RrhhCboEmpresa.DataSource == null || idEmpresa <= 0)
            {
                RrhhCboEmpresa.SelectedIndex = -1;
                RrhhTxtIdEmpresa.Clear();
                return;
            }

            RrhhCboEmpresa.SelectedValue = idEmpresa;
            RrhhTxtIdEmpresa.Text = RrhhCboEmpresa.SelectedIndex >= 0
                ? idEmpresa.ToString()
                : string.Empty;
        }

        private bool RrhhMetValidarEmpresaSeleccionada()
        {
            if (RrhhCboEmpresa.SelectedValue != null &&
                int.TryParse(RrhhCboEmpresa.SelectedValue.ToString(), out int idEmpresa) &&
                idEmpresa > 0)
            {
                RrhhTxtIdEmpresa.Text = idEmpresa.ToString();
                return true;
            }

            RrhhTxtIdEmpresa.Clear();
            MessageBox.Show(
                "Debe seleccionar una empresa para guardar la liquidación.",
                "Empresa requerida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            RrhhCboEmpresa.Focus();
            return false;
        }

        private void RrhhMetConfigurarColumnasLiquidacion()
        {
            RrhhDgvLiquidaciones.AutoGenerateColumns = false;
            RrhhDgvLiquidaciones.Columns.Clear();
            RrhhDgvLiquidaciones.ReadOnly = true;
            RrhhDgvLiquidaciones.AllowUserToAddRows = false;

            // Se configuran las 19 columnas correspondientes
            RrhhDgvLiquidaciones.Columns.Add(new DataGridViewTextBoxColumn { Name = "colIdLiquidacion", DataPropertyName = "IdLiquidacion", HeaderText = "ID Liq", Width = 50 });
            RrhhDgvLiquidaciones.Columns.Add(new DataGridViewTextBoxColumn { Name = "colIdEmpresa", DataPropertyName = "IdEmpresa", HeaderText = "ID Emp", Width = 50, Visible = false });
            RrhhDgvLiquidaciones.Columns.Add(new DataGridViewTextBoxColumn { Name = "colNombreEmpresa", DataPropertyName = "NombreEmpresa", HeaderText = "Empresa", Width = 140 });
            RrhhDgvLiquidaciones.Columns.Add(new DataGridViewTextBoxColumn { Name = "colIdEmpleado", DataPropertyName = "IdEmpleado", HeaderText = "ID Empleado", Width = 60, Visible = false });
            RrhhDgvLiquidaciones.Columns.Add(new DataGridViewTextBoxColumn { Name = "colNombreEmpleado", DataPropertyName = "NombreEmpleado", HeaderText = "Empleado", Width = 140 });
            RrhhDgvLiquidaciones.Columns.Add(new DataGridViewTextBoxColumn { Name = "colIdBajaEmpleado", DataPropertyName = "IdBajaEmpleado", HeaderText = "ID Baja", Width = 50, Visible = false });
            RrhhDgvLiquidaciones.Columns.Add(new DataGridViewTextBoxColumn { Name = "colNombreMotivoBaja", DataPropertyName = "NombreMotivoBaja", HeaderText = "Motivo Baja", Width = 110 });
            RrhhDgvLiquidaciones.Columns.Add(new DataGridViewTextBoxColumn { Name = "colFechaIngreso", DataPropertyName = "FechaIngresoLiquidacion", HeaderText = "Ingreso", Width = 80, DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy" } });
            RrhhDgvLiquidaciones.Columns.Add(new DataGridViewTextBoxColumn { Name = "colFechaBaja", DataPropertyName = "FechaBajaLiquidacion", HeaderText = "Baja", Width = 80, DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy" } });
            RrhhDgvLiquidaciones.Columns.Add(new DataGridViewTextBoxColumn { Name = "colDiasLaborados", DataPropertyName = "DiasLaboradosLiquidacion", HeaderText = "Días Lab", Width = 70 });
            RrhhDgvLiquidaciones.Columns.Add(new DataGridViewTextBoxColumn { Name = "colSalarioMensual", DataPropertyName = "SalarioMensualLiquidacion", HeaderText = "Sal. Mensual", Width = 80, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });
            RrhhDgvLiquidaciones.Columns.Add(new DataGridViewTextBoxColumn { Name = "colSalarioPromedio", DataPropertyName = "SalarioPromedioLiquidacion", HeaderText = "Sal. Prom", Width = 80, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });
            RrhhDgvLiquidaciones.Columns.Add(new DataGridViewTextBoxColumn { Name = "colBaseIndemnizacion", DataPropertyName = "BaseIndemnizacionLiquidacion", HeaderText = "Base Indem", Width = 80, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });
            RrhhDgvLiquidaciones.Columns.Add(new DataGridViewTextBoxColumn { Name = "colSalarioDia", DataPropertyName = "SalarioDiaLiquidacion", HeaderText = "Sal. Día", Width = 70, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });
            RrhhDgvLiquidaciones.Columns.Add(new DataGridViewTextBoxColumn { Name = "colTotalPercepcion", DataPropertyName = "TotalPercepcionLiquidacion", HeaderText = "Tot. Percep", Width = 80, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });
            RrhhDgvLiquidaciones.Columns.Add(new DataGridViewTextBoxColumn { Name = "colTotalDeduccion", DataPropertyName = "TotalDeduccionLiquidacion", HeaderText = "Tot. Deduc", Width = 80, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });
            RrhhDgvLiquidaciones.Columns.Add(new DataGridViewTextBoxColumn { Name = "colTotalNeto", DataPropertyName = "TotalNetoLiquidacion", HeaderText = "Total Neto", Width = 90, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });
            RrhhDgvLiquidaciones.Columns.Add(new DataGridViewTextBoxColumn { Name = "colParametrosUsados", DataPropertyName = "ParametrosUsadosLiquidacion", HeaderText = "Parámetros", Width = 100 });
            RrhhDgvLiquidaciones.Columns.Add(new DataGridViewTextBoxColumn { Name = "colNumeroFiniquito", DataPropertyName = "NumeroFiniquitoLiquidacion", HeaderText = "No. Finiquito", Width = 90 });
            RrhhDgvLiquidaciones.Columns.Add(new DataGridViewTextBoxColumn { Name = "colMontoLetras", DataPropertyName = "MontoLetrasFiniquitoLiquidacion", HeaderText = "Monto en Letras", Width = 120 });
            RrhhDgvLiquidaciones.Columns.Add(new DataGridViewTextBoxColumn { Name = "colEstado", DataPropertyName = "EstadoLiquidacion", HeaderText = "Estado", Width = 60 });
            RrhhDgvLiquidaciones.Columns.Add(new DataGridViewTextBoxColumn { Name = "colIdUsuarioCrea", DataPropertyName = "IdUsuarioCrea", HeaderText = "Usu Crea", Width = 60 });
            RrhhDgvLiquidaciones.Columns.Add(new DataGridViewTextBoxColumn { Name = "colIdUsuarioAprueba", DataPropertyName = "IdUsuarioAprueba", HeaderText = "Usu Aprueba", Width = 75 });
        }

        private void RrhhdMetListarLiquidacion()
        {
            try
            {
                _ListaLiquidaciones = _Liquidacion.LiquidacionMetObtenerTodos();
                _BindingSource.DataSource = _ListaLiquidaciones;
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.ToString());
            }
        }

        private void RrhhCboEmpleado_SelectedIndexChanged(object sender, EventArgs e)
        {
            // El empleado cambió: ningún detalle del registro anterior debe
            // permanecer visible mientras se selecciona y valida su baja.
            _EsLiquidacionExistente = false;
            RrhhTxtIdLiquidacion.Clear();
            RrhhMetLimpiarConceptosYDetalle();

            // Validar que haya un empleado seleccionado
            if (RrhhCboEmpleado.SelectedValue != null &&
                int.TryParse(RrhhCboEmpleado.SelectedValue.ToString(), out int idEmpSeleccionado))
            {
                // 1. ACTUALIZAR EL TEXTBOX DE EMPLEADO (¡Faltaba esto!)
                RrhhTxtIdEmpleado.Text = idEmpSeleccionado.ToString();

                // Desactivar evento del combo de bajas temporalmente
                RrhhCboBajaEmpleado.SelectedIndexChanged -= RrhhCboBajaEmpleado_SelectedIndexChanged;

                // Aquí llamamos al método nuevo pasándole el ID del empleado seleccionado
                RrhhCboBajaEmpleado.DataSource = _Liquidacion.LiquidacionMetObtenerBajasPorEmpleado(idEmpSeleccionado);

                RrhhCboBajaEmpleado.DisplayMember = "DescripcionBaja"; // Texto: "Despido justificado (2025-01-10)"
                RrhhCboBajaEmpleado.ValueMember = "idBajaEmpleado";    // Valor: 10
                RrhhCboBajaEmpleado.SelectedIndex = -1;

                // Limpiar el textbox de baja porque acabamos de cargar nuevas opciones
                RrhhTxtIdBajaEmpleado.Text = string.Empty;

                RrhhCboBajaEmpleado.SelectedIndexChanged += RrhhCboBajaEmpleado_SelectedIndexChanged;
            }
            else
            {
                // Si no hay empleado seleccionado, limpiamos las bajas y los textboxes
                RrhhCboBajaEmpleado.DataSource = null;
                RrhhTxtIdEmpleado.Text = string.Empty;
                RrhhTxtIdBajaEmpleado.Text = string.Empty;
            }
        }

        private void RrhhCboBajaEmpleado_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (RrhhCboBajaEmpleado.SelectedValue != null &&
                int.TryParse(RrhhCboBajaEmpleado.SelectedValue.ToString(), out int idBajaEmpleado) &&
                int.TryParse(RrhhTxtIdEmpleado.Text, out int idEmpleado))
            {
                // La nueva baja todavía no ha sido consultada; se elimina
                // cualquier detalle perteneciente a la selección anterior.
                RrhhMetLimpiarConceptosYDetalle();
                RrhhTxtIdBajaEmpleado.Text = idBajaEmpleado.ToString();

                if (RrhhCboBajaEmpleado.SelectedItem is DataRowView bajaSeleccionada &&
                    bajaSeleccionada.Row["fechaBajaEmpleado"] != DBNull.Value)
                    RrhhDtpFechaBajaEmpleado.Value = Convert.ToDateTime(
                        bajaSeleccionada.Row["fechaBajaEmpleado"]);

                RrhhMetAutocompletarLiquidacion(idEmpleado, idBajaEmpleado);
            }
        }

        private void RrhhMetAutocompletarLiquidacion(int idEmpleado, int idBajaEmpleado)
        {
            try
            {
                DataTable datos = _Liquidacion.LiquidacionMetObtenerDatosAutocompletado(
                    idEmpleado, idBajaEmpleado);

                if (datos.Rows.Count == 0)
                {
                    _EsLiquidacionExistente = false;
                    MessageBox.Show(
                        "No se encontraron datos consistentes para el empleado y la baja seleccionados.",
                        "Sin datos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DataRow fila = datos.Rows[0];
                _EsLiquidacionExistente = Convert.ToInt32(fila["ExisteLiquidacion"]) == 1;

                RrhhTxtIdLiquidacion.Text = fila["IdLiquidacion"] == DBNull.Value
                    ? string.Empty
                    : fila["IdLiquidacion"].ToString();
                if (_EsLiquidacionExistente && fila["IdEmpresa"] != DBNull.Value)
                    RrhhMetSeleccionarEmpresa(Convert.ToInt32(fila["IdEmpresa"]));
                RrhhTxtIdEmpleado.Text = fila["IdEmpleado"].ToString();
                RrhhTxtIdBajaEmpleado.Text = fila["IdBajaEmpleado"].ToString();
                RrhhDtpFechaIngresoEmp.Value = Convert.ToDateTime(fila["FechaIngresoLiquidacion"]);
                RrhhDtpFechaBajaEmpleado.Value = Convert.ToDateTime(fila["FechaBajaLiquidacion"]);
                RrhhTxtDiasLaborados.Text = fila["DiasLaboradosLiquidacion"].ToString();

                RrhhTxtSalarioMensual.Text = RrhhMetFormatearDecimal(fila["SalarioMensualLiquidacion"]);
                RrhhTxtSalarioPromedio.Text = RrhhMetFormatearDecimal(fila["SalarioPromedioLiquidacion"]);
                RrhhTxtBaseIndenmizacion.Text = RrhhMetFormatearDecimal(fila["BaseIndemnizacionLiquidacion"]);
                RrhhTxtSalarioDia.Text = RrhhMetFormatearDecimal(fila["SalarioDiaLiquidacion"]);
                RrhhTxtTotalPercepcion.Text = RrhhMetFormatearDecimal(fila["TotalPercepcionLiquidacion"]);
                RrhhTxtTotalDeduccion.Text = RrhhMetFormatearDecimal(fila["TotalDeduccionLiquidacion"]);
                RrhhTxtTotalNeto.Text = RrhhMetFormatearDecimal(fila["TotalNetoLiquidacion"]);

                RrhhTxtParametrosUsados.Text = fila["ParametrosUsadosLiquidacion"] == DBNull.Value
                    ? string.Empty
                    : fila["ParametrosUsadosLiquidacion"].ToString();
                RrhhTxtNumeroFiniquito.Text = fila["NumeroFiniquitoLiquidacion"] == DBNull.Value
                    ? string.Empty
                    : fila["NumeroFiniquitoLiquidacion"].ToString();
                RrhhTxtMontoLetras.Text = fila["MontoLetrasFiniquitoLiquidacion"] == DBNull.Value
                    ? string.Empty
                    : fila["MontoLetrasFiniquitoLiquidacion"].ToString();

                string estado = fila["EstadoLiquidacion"].ToString();
                RrhhMetSeleccionarEstadoLiquidacion(estado);

                if (_EsLiquidacionExistente && fila["IdUsuarioCrea"] != DBNull.Value)
                    RrhhTxtIdUsuCrea.Text = fila["IdUsuarioCrea"].ToString();
                else
                    RrhhTxtIdUsuCrea.Text = ClsSesionSeguridad.IdUsuario.ToString();

                if (_EsLiquidacionExistente && fila["IdUsuarioAprueba"] != DBNull.Value)
                    RrhhTxtUsuAprueba.Text = fila["IdUsuarioAprueba"].ToString();
                else
                    RrhhTxtUsuAprueba.Text = ClsSesionSeguridad.IdUsuario.ToString();

                if (!_EsLiquidacionExistente)
                    RrhhMetAsignarSiguienteNumeroFiniquito();

                if (_EsLiquidacionExistente &&
                    int.TryParse(RrhhTxtIdLiquidacion.Text, out int idLiquidacion))
                    RrhhMetCargarDetallesLiquidacion(idLiquidacion);
                else
                    RrhhMetRestablecerConceptosConTotalesBase();
            }
            catch (Exception ex)
            {
                _EsLiquidacionExistente = false;
                MessageBox.Show(
                    "No fue posible autocompletar la liquidación: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string RrhhMetFormatearDecimal(object valor)
        {
            return valor == DBNull.Value
                ? "0.0000"
                : Convert.ToDecimal(valor).ToString("0.0000", CultureInfo.CurrentCulture);
        }

        private void RrhhMetConfigurarEstadosLiquidacion()
        {
            RrhhCboEstadoLiqui.DataSource = null;
            RrhhCboEstadoLiqui.Items.Clear();
            RrhhCboEstadoLiqui.DropDownStyle = ComboBoxStyle.DropDownList;
            RrhhCboEstadoLiqui.Items.AddRange(new object[]
            {
                "BORRADOR",
                "APROBADA",
                "PAGADA"
            });
            RrhhCboEstadoLiqui.SelectedItem = "BORRADOR";
        }

        private void RrhhMetSeleccionarEstadoLiquidacion(string estado)
        {
            string estadoNormalizado = estado?.Trim().ToUpperInvariant();
            RrhhCboEstadoLiqui.SelectedIndex =
                RrhhCboEstadoLiqui.Items.IndexOf(estadoNormalizado);
        }

        private void RrhhMetAsignarSiguienteNumeroFiniquito()
        {
            int anio = RrhhDtpFechaEmision.Value.Year;
            RrhhTxtNumeroFiniquito.Text =
                _Liquidacion.LiquidacionMetObtenerSiguienteNumeroFiniquito(anio);
        }

        private bool RrhhMetValidarNumeroFiniquito(int idLiquidacionExcluir)
        {
            if (string.IsNullOrWhiteSpace(RrhhTxtNumeroFiniquito.Text))
                RrhhMetAsignarSiguienteNumeroFiniquito();

            string numero = RrhhTxtNumeroFiniquito.Text.Trim().ToUpperInvariant();
            Match coincidencia = Regex.Match(numero, @"^FIN-(\d{4})-(\d{4})$");

            if (!coincidencia.Success)
            {
                MessageBox.Show(
                    "El número de finiquito debe usar el formato FIN-AÑO-0000, por ejemplo FIN-2026-0001.",
                    "Finiquito inválido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (_Liquidacion.LiquidacionMetExisteNumeroFiniquito(
                numero, idLiquidacionExcluir))
            {
                MessageBox.Show(
                    $"El número de finiquito {numero} ya está registrado.",
                    "Finiquito duplicado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            RrhhTxtNumeroFiniquito.Text = numero;
            return true;
        }

        private void RrhhMetConfigurarGridConceptos()
        {
            RrhhDtvConceptos.AutoGenerateColumns = false;
            RrhhDtvConceptos.Columns.Clear();
            RrhhDtvConceptos.AllowUserToAddRows = true;
            RrhhDtvConceptos.AllowUserToDeleteRows = true;
            RrhhDtvConceptos.AllowUserToOrderColumns = false;
            RrhhDtvConceptos.EditMode = DataGridViewEditMode.EditOnEnter;
            RrhhDtvConceptos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            RrhhDtvConceptos.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colDescripcionConcepto",
                HeaderText = "Descripción",
                FillWeight = 50,
                SortMode = DataGridViewColumnSortMode.NotSortable
            });

            RrhhDtvConceptos.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colMontoConcepto",
                HeaderText = "Monto",
                ValueType = typeof(decimal),
                FillWeight = 25,
                SortMode = DataGridViewColumnSortMode.NotSortable,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "N4",
                    Alignment = DataGridViewContentAlignment.MiddleRight
                }
            });

            var columnaTipo = new DataGridViewComboBoxColumn
            {
                Name = "colTipoConcepto",
                HeaderText = "Tipo",
                FillWeight = 25,
                FlatStyle = FlatStyle.Flat,
                SortMode = DataGridViewColumnSortMode.NotSortable
            };
            columnaTipo.Items.AddRange("Percepción", "Deducción");
            RrhhDtvConceptos.Columns.Add(columnaTipo);

            RrhhDtvConceptos.DefaultValuesNeeded +=
                RrhhDtvConceptos_DefaultValuesNeeded;
            RrhhDtvConceptos.CellValueChanged += RrhhDtvConceptos_CellValueChanged;
            RrhhDtvConceptos.RowsRemoved += RrhhDtvConceptos_RowsRemoved;
            RrhhDtvConceptos.CurrentCellDirtyStateChanged +=
                RrhhDtvConceptos_CurrentCellDirtyStateChanged;
            RrhhDtvConceptos.CellValidating += RrhhDtvConceptos_CellValidating;
            RrhhDtvConceptos.DataError += RrhhDtvConceptos_DataError;
        }

        private void RrhhMetConfigurarGridLiquidacionDetalle()
        {
            RrhhDtvLiquidacionDetalle.AutoGenerateColumns = false;
            RrhhDtvLiquidacionDetalle.Columns.Clear();
            RrhhDtvLiquidacionDetalle.ReadOnly = true;
            RrhhDtvLiquidacionDetalle.AllowUserToAddRows = false;
            RrhhDtvLiquidacionDetalle.AllowUserToDeleteRows = false;
            RrhhDtvLiquidacionDetalle.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;

            RrhhDtvLiquidacionDetalle.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "colDetalleConcepto",
                    HeaderText = "Concepto",
                    FillWeight = 45
                });
            RrhhDtvLiquidacionDetalle.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "colDetalleDias",
                    HeaderText = "Días",
                    FillWeight = 15,
                    DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Format = "N2",
                        Alignment = DataGridViewContentAlignment.MiddleRight
                    }
                });
            RrhhDtvLiquidacionDetalle.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "colDetalleBase",
                    HeaderText = "Base",
                    FillWeight = 20,
                    DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Format = "N4",
                        Alignment = DataGridViewContentAlignment.MiddleRight
                    }
                });
            RrhhDtvLiquidacionDetalle.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "colDetalleMonto",
                    HeaderText = "Monto",
                    FillWeight = 20,
                    DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Format = "N4",
                        Alignment = DataGridViewContentAlignment.MiddleRight
                    }
                });
        }

        private void RrhhDtvConceptos_DefaultValuesNeeded(
            object sender, DataGridViewRowEventArgs e)
        {
            e.Row.Cells["colTipoConcepto"].Value = "Percepción";
        }

        private void RrhhDtvConceptos_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (RrhhDtvConceptos.IsCurrentCellDirty)
                RrhhDtvConceptos.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        private void RrhhDtvConceptos_CellValueChanged(
            object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
                RrhhMetRecalcularTotalesConceptos();
        }

        private void RrhhDtvConceptos_RowsRemoved(
            object sender, DataGridViewRowsRemovedEventArgs e)
        {
            RrhhMetRecalcularTotalesConceptos();
        }

        private void RrhhDtvConceptos_CellValidating(
            object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (RrhhDtvConceptos.Columns[e.ColumnIndex].Name != "colMontoConcepto")
                return;

            string texto = Convert.ToString(e.FormattedValue)?.Trim();
            if (string.IsNullOrEmpty(texto))
                return;

            if (!RrhhMetTryParseDecimal(texto, out decimal monto) || monto < 0)
            {
                e.Cancel = true;
                MessageBox.Show(
                    "El monto del concepto debe ser un número mayor o igual a cero.",
                    "Monto inválido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void RrhhDtvConceptos_DataError(
            object sender, DataGridViewDataErrorEventArgs e)
        {
            e.ThrowException = false;
        }

        private void RrhhMetRestablecerConceptosConTotalesBase()
        {
            RrhhMetLimpiarConceptosYDetalle();

            RrhhMetTryParseDecimal(RrhhTxtTotalPercepcion.Text,
                out _TotalPercepcionBase);
            RrhhMetTryParseDecimal(RrhhTxtTotalDeduccion.Text,
                out _TotalDeduccionBase);

            RrhhMetRecalcularTotalesConceptos();
        }

        private void RrhhMetLimpiarConceptosYDetalle()
        {
            _RecalculandoConceptos = true;
            try
            {
                RrhhDtvConceptos.Rows.Clear();
                RrhhDtvLiquidacionDetalle.Rows.Clear();
                _DetallesOriginales.Clear();
            }
            finally
            {
                _RecalculandoConceptos = false;
            }
        }

        private void RrhhMetRecalcularTotalesConceptos()
        {
            if (_RecalculandoConceptos)
                return;

            decimal percepcionesAdicionales = 0m;
            decimal deduccionesAdicionales = 0m;

            foreach (DataGridViewRow fila in RrhhDtvConceptos.Rows)
            {
                if (fila.IsNewRow ||
                    !RrhhMetTryParseDecimal(
                        fila.Cells["colMontoConcepto"].Value, out decimal monto))
                    continue;

                string tipoConcepto = Convert.ToString(
                    fila.Cells["colTipoConcepto"].Value)?.Trim();
                if (string.Equals(tipoConcepto, "Deducción",
                    StringComparison.OrdinalIgnoreCase))
                    deduccionesAdicionales += monto;
                else
                    percepcionesAdicionales += monto;
            }

            decimal totalPercepciones = _TotalPercepcionBase + percepcionesAdicionales;
            decimal totalDeducciones = _TotalDeduccionBase + deduccionesAdicionales;
            decimal totalNeto = totalPercepciones - totalDeducciones;

            RrhhTxtTotalPercepcion.Text = totalPercepciones.ToString(
                "0.0000", CultureInfo.CurrentCulture);
            RrhhTxtTotalDeduccion.Text = totalDeducciones.ToString(
                "0.0000", CultureInfo.CurrentCulture);
            RrhhTxtTotalNeto.Text = totalNeto.ToString(
                "0.0000", CultureInfo.CurrentCulture);

            RrhhMetActualizarVistaDetalle(
                RrhhMetConstruirDetalles(false, out _));
        }

        private void RrhhMetCargarDetallesLiquidacion(int idLiquidacion)
        {
            RrhhMetTryParseDecimal(RrhhTxtTotalPercepcion.Text,
                out decimal totalPercepcionGuardado);
            RrhhMetTryParseDecimal(RrhhTxtTotalDeduccion.Text,
                out decimal totalDeduccionGuardado);

            DataTable tablaDetalles =
                _Liquidacion.LiquidacionMetObtenerDetalles(idLiquidacion);
            decimal totalDetalles = 0m;

            _RecalculandoConceptos = true;
            try
            {
                RrhhDtvConceptos.Rows.Clear();
                RrhhDtvLiquidacionDetalle.Rows.Clear();
                _DetallesOriginales.Clear();

                foreach (DataRow fila in tablaDetalles.Rows)
                {
                    var detalle = new ClsModeloDetalleLiquidacion
                    {
                        IdDetalleLiquidacion = Convert.ToInt32(
                            fila["idDetalleLiquidacion"]),
                        IdLiquidacion = Convert.ToInt32(fila["idLiquidacion"]),
                        IdPercepcionDeduccion = Convert.ToInt32(
                            fila["idPercepcionDeduccion"]),
                        NombrePercepcionDeduccion = Convert.ToString(
                            fila["nombrePercepcionDeduccion"]),
                        DiasDetalleLiquidacion = Convert.ToDecimal(
                            fila["diasDetalleLiquidacion"]),
                        BaseDetalleLiquidacion = Convert.ToDecimal(
                            fila["baseDetalleLiquidacion"]),
                        MontoDetalleLiquidacion = Convert.ToDecimal(
                            fila["montoDetalleLiquidacion"])
                    };

                    _DetallesOriginales[detalle.IdPercepcionDeduccion] = detalle;
                    totalDetalles += detalle.MontoDetalleLiquidacion;
                    RrhhDtvConceptos.Rows.Add(
                        detalle.NombrePercepcionDeduccion,
                        detalle.MontoDetalleLiquidacion,
                        "Percepción");
                }

                _TotalPercepcionBase = totalPercepcionGuardado - totalDetalles;
                _TotalDeduccionBase = totalDeduccionGuardado;
            }
            finally
            {
                _RecalculandoConceptos = false;
            }

            RrhhMetRecalcularTotalesConceptos();
        }

        private bool RrhhMetPrepararDetallesParaGuardar(
            out List<ClsModeloDetalleLiquidacion> detalles)
        {
            RrhhDtvConceptos.EndEdit();
            RrhhMetRecalcularTotalesConceptos();
            detalles = RrhhMetConstruirDetalles(true, out string error);
            if (string.IsNullOrEmpty(error))
                return true;

            MessageBox.Show(error, "Detalle de liquidación",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        private List<ClsModeloDetalleLiquidacion> RrhhMetConstruirDetalles(
            bool validar, out string error)
        {
            error = null;
            var agrupados =
                new Dictionary<int, ClsModeloDetalleLiquidacion>();

            foreach (DataGridViewRow fila in RrhhDtvConceptos.Rows)
            {
                if (fila.IsNewRow)
                    continue;

                string descripcion = Convert.ToString(
                    fila.Cells["colDescripcionConcepto"].Value)?.Trim();
                object valorMonto = fila.Cells["colMontoConcepto"].Value;
                bool filaVacia = string.IsNullOrWhiteSpace(descripcion) &&
                                 (valorMonto == null ||
                                  string.IsNullOrWhiteSpace(
                                      Convert.ToString(valorMonto)));
                if (filaVacia)
                    continue;

                if (!RrhhMetTryParseDecimal(valorMonto, out decimal monto) ||
                    monto <= 0)
                {
                    if (validar)
                        error = "Todos los conceptos deben tener un monto mayor que cero.";
                    continue;
                }

                bool esDeduccion = string.Equals(
                    Convert.ToString(fila.Cells["colTipoConcepto"].Value)?.Trim(),
                    "Deducción", StringComparison.OrdinalIgnoreCase);
                int idPercepcion = esDeduccion ? 16 : 15;
                string nombre = string.IsNullOrWhiteSpace(descripcion)
                    ? (esDeduccion ? "OtrosDeduccion" : "OtrosPercepcion")
                    : descripcion;

                if (agrupados.TryGetValue(idPercepcion,
                    out ClsModeloDetalleLiquidacion existente))
                {
                    existente.MontoDetalleLiquidacion += monto;
                    continue;
                }

                agrupados[idPercepcion] = new ClsModeloDetalleLiquidacion
                {
                    IdPercepcionDeduccion = idPercepcion,
                    NombrePercepcionDeduccion = nombre,
                    MontoDetalleLiquidacion = monto
                };
            }

            return agrupados.Values
                .OrderBy(d => d.NombrePercepcionDeduccion)
                .ToList();
        }

        private DataRow RrhhMetBuscarPercepcion(string descripcion)
        {
            if (_CatalogoPercepciones == null ||
                string.IsNullOrWhiteSpace(descripcion))
                return null;

            string buscada = descripcion.Trim();
            foreach (DataRow fila in _CatalogoPercepciones.Rows)
            {
                if (string.Equals(
                    Convert.ToString(fila["nombrePercepcionDeduccion"])?.Trim(),
                    buscada,
                    StringComparison.OrdinalIgnoreCase))
                    return fila;
            }

            return null;
        }

        private void RrhhMetActualizarVistaDetalle(
            IEnumerable<ClsModeloDetalleLiquidacion> detalles)
        {
            RrhhDtvLiquidacionDetalle.Rows.Clear();
            foreach (ClsModeloDetalleLiquidacion detalle in detalles)
            {
                RrhhDtvLiquidacionDetalle.Rows.Add(
                    detalle.NombrePercepcionDeduccion,
                    detalle.DiasDetalleLiquidacion,
                    detalle.BaseDetalleLiquidacion,
                    detalle.MontoDetalleLiquidacion);
            }
        }

        private static bool RrhhMetTryParseDecimal(object valor, out decimal resultado)
        {
            string texto = Convert.ToString(valor)?.Trim();
            return decimal.TryParse(texto, NumberStyles.Number,
                       CultureInfo.CurrentCulture, out resultado) ||
                   decimal.TryParse(texto, NumberStyles.Number,
                       CultureInfo.InvariantCulture, out resultado);
        }

        private void RrhhBtnInicioLiqui_Click(object sender, EventArgs e) { if (_BindingSource.Count > 0) _BindingSource.MoveFirst(); }
        private void RrhhBtnAnteriorLiqui_Click(object sender, EventArgs e) { if (_BindingSource.Position > 0) _BindingSource.MovePrevious(); }
        private void RrhhBtnSiguienteLiqui_Click(object sender, EventArgs e) { if (_BindingSource.Position < _BindingSource.Count - 1) _BindingSource.MoveNext(); }
        private void RrhhBtnFinLiqui_Click(object sender, EventArgs e) { if (_BindingSource.Count > 0) _BindingSource.MoveLast(); }

        private void LlenarModeloDesdeUI()
        {
            // Mapeo completo de las 19 columnas de la interfaz gráfica al modelo
            _Liquidacion.IdEmpresa = string.IsNullOrWhiteSpace(RrhhTxtIdEmpresa.Text)
                ? 0
                : Convert.ToInt32(RrhhTxtIdEmpresa.Text);
            _Liquidacion.IdEmpleado = Convert.ToInt32(RrhhTxtIdEmpleado.Text);
            _Liquidacion.IdBajaEmpleado = string.IsNullOrWhiteSpace(RrhhTxtIdBajaEmpleado.Text) ? 0 : Convert.ToInt32(RrhhTxtIdBajaEmpleado.Text);
            _Liquidacion.FechaIngresoLiquidacion = RrhhDtpFechaIngresoEmp.Value.Date;
            _Liquidacion.FechaBajaLiquidacion = RrhhDtpFechaBajaEmpleado.Value.Date;

            _Liquidacion.DiasLaboradosLiquidacion = string.IsNullOrWhiteSpace(RrhhTxtDiasLaborados.Text) ? 0 : Convert.ToInt32(RrhhTxtDiasLaborados.Text);
            _Liquidacion.SalarioMensualLiquidacion = string.IsNullOrWhiteSpace(RrhhTxtSalarioMensual.Text) ? 0 : Convert.ToDecimal(RrhhTxtSalarioMensual.Text);
            _Liquidacion.SalarioPromedioLiquidacion = string.IsNullOrWhiteSpace(RrhhTxtSalarioPromedio.Text) ? 0 : Convert.ToDecimal(RrhhTxtSalarioPromedio.Text);
            _Liquidacion.BaseIndemnizacionLiquidacion = string.IsNullOrWhiteSpace(RrhhTxtBaseIndenmizacion.Text) ? 0 : Convert.ToDecimal(RrhhTxtBaseIndenmizacion.Text);
            _Liquidacion.SalarioDiaLiquidacion = string.IsNullOrWhiteSpace(RrhhTxtSalarioDia.Text) ? 0 : Convert.ToDecimal(RrhhTxtSalarioDia.Text);
            _Liquidacion.TotalPercepcionLiquidacion = string.IsNullOrWhiteSpace(RrhhTxtTotalPercepcion.Text) ? 0 : Convert.ToDecimal(RrhhTxtTotalPercepcion.Text);
            _Liquidacion.TotalDeduccionLiquidacion = string.IsNullOrWhiteSpace(RrhhTxtTotalDeduccion.Text) ? 0 : Convert.ToDecimal(RrhhTxtTotalDeduccion.Text);
            _Liquidacion.TotalNetoLiquidacion = string.IsNullOrWhiteSpace(RrhhTxtTotalNeto.Text) ? 0 : Convert.ToDecimal(RrhhTxtTotalNeto.Text);

            _Liquidacion.ParametrosUsadosLiquidacion = RrhhTxtParametrosUsados.Text;
            _Liquidacion.NumeroFiniquitoLiquidacion = RrhhTxtNumeroFiniquito.Text;
            _Liquidacion.MontoLetrasFiniquitoLiquidacion = RrhhTxtMontoLetras.Text;

            _Liquidacion.EstadoLiquidacion = string.IsNullOrWhiteSpace(RrhhCboEstadoLiqui.Text)
                ? "BORRADOR"
                : RrhhCboEstadoLiqui.Text.Trim();
            _Liquidacion.IdUsuarioCrea = string.IsNullOrWhiteSpace(RrhhTxtIdUsuCrea.Text) ? 0 : Convert.ToInt32(RrhhTxtIdUsuCrea.Text);
            _Liquidacion.IdUsuarioAprueba = string.IsNullOrWhiteSpace(RrhhTxtUsuAprueba.Text)
                ? _Liquidacion.IdUsuarioCrea
                : Convert.ToInt32(RrhhTxtUsuAprueba.Text);
        }

        private void RrhhBtnAyudaLiqui_Click(object sender, EventArgs e)
        {
            RrhhMetAbrirAyuda(this, "LiquidacionEmpleado__RecursosHumanos.html");
        }
        internal static void RrhhMetAbrirAyuda(Control Ventana, string Pagina)
        {
            const string Ayuda = @"ayuda\modulos\rrhh\RrhhAyudas\RecursosHumanosAyudas.chm";
            for (var Carpeta = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory); Carpeta != null; Carpeta = Carpeta.Parent)
            {
                string Ruta = Path.Combine(Carpeta.FullName, Ayuda);
                if (File.Exists(Ruta))
                {
                    Help.ShowHelp(Ventana, Ruta, Pagina);
                    return;
                }
            }
            MessageBox.Show("No se encontró el archivo de ayuda de RRHH.", "Ayuda",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

    }
    }
