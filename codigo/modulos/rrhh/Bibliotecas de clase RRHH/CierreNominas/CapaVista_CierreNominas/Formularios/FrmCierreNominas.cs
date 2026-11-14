using CapaControlador_CierreNominas;
using CapaControlador_CierreNominas.Objetos_de_valor;
using CapaControlador_Seguridad;
using CapaControlador_Seguridad.Objetos_de_valor;
using CapaVista_Nomina.Reportes;
using CapaVista_Seguridad.Ayudas;
using CapaVista_Seguridad.frmReportes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

/*
 * ==================================================================
 * Área: Recursos Humanos
 * Autores: Lourdes Isabel Melendez Pineda
 * Fecha o ultima edicion: 13/11/2026
 * ==================================================================
 * Propósito : Ventana que cierra la nómina de un mes: muestra los
 * meses abiertos, el resumen del período con la verificación
 * de cuadre y el detalle por empleado, y ejecuta el cierre
 * (póliza a Contabilidad) con seguridad por botón.
 * ===================================================================
 */

namespace CapaVista_CierreNominas.Formularios
{
    public partial class FrmCierreNominas : Form
    {
        private ClsModeloCierreNomina _CierreNomina = new ClsModeloCierreNomina();

        private ClsPermisoAplicacion _MisPermisos;
        private ClsResumenCierreNominaValor _ResumenActual;
        private bool _CargandoPeriodos;
        private readonly ToolTip _Ayuda = new ToolTip();
        private const int ID_MODULO = 9000;
        private const int ID_APLICACION = 9601;

        // ---- Barra de cuadre (Detalle y Cuadre) ----
        private Panel RrhhPnlCuadre, RrhhPnlIconoCuadre, RrhhPnlSeparadorCuadre;
        private Panel RrhhPnlDiferencia, RrhhPnlIconoDiferencia;
        private Label RrhhLblTituloCuadre, RrhhLblDescripcionCuadre;
        private Label RrhhLblTxtPercepciones, RrhhLblMontoPercepciones;
        private Label RrhhLblTxtDeducciones, RrhhLblMontoDeducciones;
        private Label RrhhLblTxtDiferencia, RrhhLblMontoDiferencia;
        private int _EstadoCuadre;                    // 0 = sin datos, 1 = correcto, 2 = incorrecto
        private Color _ColorBordeCuadre = Color.Gray;

        public FrmCierreNominas()
        {
            InitializeComponent();
            Load += FrmCierreNominas_Load;
            RrhhCboPeriodo.SelectedIndexChanged += RrhhCboPeriodo_SelectedIndexChanged;
            RrhhBtnCerrarNomina.Click += RrhhBtnCerrarNomina_Click;
        }

        private void FrmCierreNominas_Load(object sender, EventArgs e)
        {
            var MapaBotones = new Dictionary<Control, TipoPermiso>
            {
                { RrhhBtnCerrarNomina, TipoPermiso.Insertar }
            };

            _MisPermisos = ClsSeguridadFormHelper.SeguridadMetInicializarSeguridad(
                this, ID_MODULO, ID_APLICACION, MapaBotones);

            if (!_MisPermisos.TieneAcceso)
                return;

            RrhhCboPeriodo.DropDownStyle = ComboBoxStyle.DropDownList;
            RrhhMetConfigurarColumnas();
            RrhhMetCrearPanelCuadre();
            RrhhMetLimpiarResumen();
            RrhhMetCargarPeriodos();
        }

        private void RrhhMetConfigurarColumnas()
        {
            var Tabla = RrhhDgvVistaCierreNomina;
            Tabla.AutoGenerateColumns = false;
            Tabla.AllowUserToAddRows = false;
            Tabla.AllowUserToDeleteRows = false;
            Tabla.ReadOnly = true;
            Tabla.RowHeadersVisible = false;
            Tabla.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            Tabla.Columns.Clear();

            var Derecha = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight };
            var Centro = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter };

            Tabla.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Codigo", HeaderText = "Código", Name = "Codigo", DefaultCellStyle = Centro });
            Tabla.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Empleado", HeaderText = "Empleado", Name = "Empleado" });
            Tabla.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Percepciones", HeaderText = "Percepciones", Name = "Percepciones", DefaultCellStyle = Derecha });
            Tabla.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Deducciones", HeaderText = "Deducciones", Name = "Deducciones", DefaultCellStyle = Derecha });

            // Columna calculada (no enlazada a datos)
            Tabla.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Salario Líquido", Name = "SalarioLiquido", DefaultCellStyle = Derecha });

            // Estado: se dibuja con ícono
            Tabla.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Estado", Name = "Estado", DefaultCellStyle = Centro });

            Tabla.CellFormatting -= RrhhDgvVistaCierreNomina_CellFormatting;
            Tabla.CellFormatting += RrhhDgvVistaCierreNomina_CellFormatting;
            Tabla.CellPainting -= RrhhDgvVistaCierreNomina_CellPainting;
            Tabla.CellPainting += RrhhDgvVistaCierreNomina_CellPainting;
        }

        // Calcula Salario Líquido = Percepciones - Deducciones
        private void RrhhDgvVistaCierreNomina_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (RrhhDgvVistaCierreNomina.Columns[e.ColumnIndex].Name != "SalarioLiquido") return;

            var Item = RrhhDgvVistaCierreNomina.Rows[e.RowIndex].DataBoundItem as ClsFilaCierreNominaValor;
            if (Item == null) return;

            e.Value = (Convert.ToDecimal(Item.Percepciones) - Convert.ToDecimal(Item.Deducciones)).ToString("N2");
            e.FormattingApplied = true;
        }

        // Dibuja el círculo con check + texto "Correcto" / "Incorrecto"
        private void RrhhDgvVistaCierreNomina_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || RrhhDgvVistaCierreNomina.Columns[e.ColumnIndex].Name != "Estado")
                return;

            var Item = RrhhDgvVistaCierreNomina.Rows[e.RowIndex].DataBoundItem as ClsFilaCierreNominaValor;
            if (Item == null) return;

            e.PaintBackground(e.CellBounds, false);

            bool Ok = Item.Verificado;
            Color ColorIcono = Ok ? Color.FromArgb(34, 160, 80) : Color.Firebrick;
            string Texto = Ok ? "Correcto" : "Incorrecto";

            var G = e.Graphics;
            G.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            Size TamTexto = TextRenderer.MeasureText(Texto, e.CellStyle.Font);
            int Diametro = 16, Espacio = 6;
            int AnchoTotal = Diametro + Espacio + TamTexto.Width;
            int X = e.CellBounds.X + (e.CellBounds.Width - AnchoTotal) / 2;
            int Y = e.CellBounds.Y + (e.CellBounds.Height - Diametro) / 2;

            using (var Pincel = new SolidBrush(ColorIcono))
                G.FillEllipse(Pincel, X, Y, Diametro, Diametro);

            using (var Lapiz = new Pen(Color.White, 2f))
            {
                G.DrawLines(Lapiz, new[]
                {
            new Point(X + 4, Y + 8),
            new Point(X + 7, Y + 11),
            new Point(X + 12, Y + 5)
        });
            }

            TextRenderer.DrawText(G, Texto, e.CellStyle.Font,
                new Point(X + Diametro + Espacio, e.CellBounds.Y + (e.CellBounds.Height - TamTexto.Height) / 2),
                e.CellStyle.ForeColor);

            e.Handled = true;
        }

        // Llena el combo con los meses abiertos y lo deja sin selección
        private List<ClsPeriodoNominaValor> RrhhMetCargarPeriodos()
        {
            List<ClsPeriodoNominaValor> Periodos = new List<ClsPeriodoNominaValor>();
            try
            {
                _CargandoPeriodos = true;
                Periodos = _CierreNomina.RrhhMetObtenerPeriodosAbiertos();
                RrhhCboPeriodo.DataSource = null;
                RrhhCboPeriodo.DisplayMember = "Texto";
                RrhhCboPeriodo.ValueMember = "PeriodoNomina";
                RrhhCboPeriodo.DataSource = Periodos;
                RrhhCboPeriodo.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                _CargandoPeriodos = false;
            }
            return Periodos;
        }

        private void RrhhCboPeriodo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_CargandoPeriodos)
                return;

            var Periodo = RrhhCboPeriodo.SelectedItem as ClsPeriodoNominaValor;
            if (Periodo == null)
            {
                RrhhMetLimpiarResumen();
                return;
            }

            try
            {
                Cursor = Cursors.WaitCursor;
                _ResumenActual = _CierreNomina.RrhhMetObtenerPrevio(Periodo.FechaInicioNomina, Periodo.FechaFinalNomina);
                RrhhMetMostrarResumen(_ResumenActual);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                RrhhMetLimpiarResumen();
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void RrhhMetMostrarResumen(ClsResumenCierreNominaValor Resumen)
        {
            RrhhLblValorResumen.Text = Resumen.EmpleadosEnNomina.ToString();
            RrhhLblValorResumen2.Text = "Q " + Resumen.TotalPercepciones.ToString("N2");
            RrhhLblValorResumen3.Text = "Q " + Resumen.TotalDeducciones.ToString("N2");
            RrhhLblValorResumen4.Text = "Q " + Resumen.SalarioLiquido.ToString("N2");

            RrhhLblVerificacionCuadre.Text = "Verificación de cuadre: " + Resumen.TextoCuadre;
            RrhhLblVerificacionCuadre.ForeColor = Resumen.CuadreCorrecto
                ? Color.FromArgb(14, 76, 85)
                : Color.Firebrick;

            // El motivo del bloqueo se ve al pasar el mouse por la verificación
            string Motivo = Resumen.PuedeCerrar ? string.Empty : Resumen.MensajeBloqueo;
            _Ayuda.SetToolTip(RrhhLblVerificacionCuadre, Motivo);
            _Ayuda.SetToolTip(RrhhPnlDecorativo5, Motivo);

            RrhhDgvVistaCierreNomina.DataSource = null;
            RrhhDgvVistaCierreNomina.DataSource = Resumen.Filas;
            foreach (DataGridViewRow Fila in RrhhDgvVistaCierreNomina.Rows)
            {
                var Item = Fila.DataBoundItem as ClsFilaCierreNominaValor;
                if (Item != null && !Item.Verificado)
                    Fila.DefaultCellStyle.ForeColor = Color.Firebrick;
            }

            RrhhMetActualizarCuadre(Resumen);
            RrhhMetActualizarBoton();
        }

        private void RrhhMetLimpiarResumen()
        {
            _ResumenActual = null;
            RrhhLblValorResumen.Text = "0";
            RrhhLblValorResumen2.Text = "0";
            RrhhLblValorResumen3.Text = "0";
            RrhhLblValorResumen4.Text = "0";
            RrhhLblVerificacionCuadre.Text = "Verificación de cuadre";
            RrhhLblVerificacionCuadre.ForeColor = Color.FromArgb(14, 76, 85);
            _Ayuda.SetToolTip(RrhhLblVerificacionCuadre, string.Empty);
            _Ayuda.SetToolTip(RrhhPnlDecorativo5, string.Empty);
            RrhhDgvVistaCierreNomina.DataSource = null;
            RrhhMetActualizarCuadre(null);
            RrhhMetActualizarBoton();
        }

        // Crea la barra de cuadre debajo de la grilla
        private void RrhhMetCrearPanelCuadre()
        {
            // La grilla se achica para dejar espacio a la barra
            RrhhDgvVistaCierreNomina.Height = 130;

            RrhhPnlCuadre = new Panel
            {
                Location = new Point(RrhhDgvVistaCierreNomina.Left, RrhhDgvVistaCierreNomina.Bottom + 10),
                Size = new Size(RrhhDgvVistaCierreNomina.Width, 80),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            RrhhPnlCuadre.Paint += RrhhPnlBorde_Paint;

            // Ícono grande (círculo con check / X)
            RrhhPnlIconoCuadre = new Panel { Location = new Point(22, 16), Size = new Size(48, 48), BackColor = Color.Transparent };
            RrhhPnlIconoCuadre.Paint += RrhhPnlIcono_Paint;

            RrhhLblTituloCuadre = RrhhMetCrearEtiqueta(RrhhPnlCuadre, "Cuadre Correcto", 84, 14, 14F, FontStyle.Bold);
            RrhhLblDescripcionCuadre = RrhhMetCrearEtiqueta(RrhhPnlCuadre, "", 84, 44, 10F, FontStyle.Regular);

            // Línea separadora vertical
            RrhhPnlSeparadorCuadre = new Panel { Location = new Point(700, 12), Size = new Size(2, 56), BackColor = Color.FromArgb(14, 76, 85) };

            // Totales
            RrhhLblTxtPercepciones = RrhhMetCrearEtiqueta(RrhhPnlCuadre, "Total Percepciones:", 740, 16, 10.8F, FontStyle.Regular);
            RrhhLblMontoPercepciones = RrhhMetCrearEtiqueta(RrhhPnlCuadre, "Q 0.00", 920, 15, 11F, FontStyle.Bold);
            RrhhLblTxtDeducciones = RrhhMetCrearEtiqueta(RrhhPnlCuadre, "Total Deducciones:", 740, 44, 10.8F, FontStyle.Regular);
            RrhhLblMontoDeducciones = RrhhMetCrearEtiqueta(RrhhPnlCuadre, "Q 0.00", 920, 43, 11F, FontStyle.Bold);

            // Recuadro de diferencia
            RrhhPnlDiferencia = new Panel { Location = new Point(1060, 10), Size = new Size(216, 60) };
            RrhhPnlDiferencia.Paint += RrhhPnlBorde_Paint;
            RrhhPnlIconoDiferencia = new Panel { Location = new Point(14, 14), Size = new Size(32, 32), BackColor = Color.Transparent };
            RrhhPnlIconoDiferencia.Paint += RrhhPnlIcono_Paint;
            RrhhLblTxtDiferencia = RrhhMetCrearEtiqueta(RrhhPnlDiferencia, "Diferencia:", 58, 6, 9.5F, FontStyle.Bold);
            RrhhLblMontoDiferencia = RrhhMetCrearEtiqueta(RrhhPnlDiferencia, "Q 0.00", 58, 24, 15F, FontStyle.Bold);
            RrhhPnlDiferencia.Controls.Add(RrhhPnlIconoDiferencia);

            RrhhPnlCuadre.Controls.Add(RrhhPnlIconoCuadre);
            RrhhPnlCuadre.Controls.Add(RrhhPnlSeparadorCuadre);
            RrhhPnlCuadre.Controls.Add(RrhhPnlDiferencia);

            Controls.Add(RrhhPnlCuadre);
            RrhhPnlCuadre.BringToFront();
        }

        private Label RrhhMetCrearEtiqueta(Control Padre, string Texto, int X, int Y, float Tamano, FontStyle Estilo)
        {
            var Etiqueta = new Label
            {
                AutoSize = true,
                BackColor = Color.Transparent,
                Font = new Font("Tahoma", Tamano, Estilo, GraphicsUnit.Point),
                Location = new Point(X, Y),
                Text = Texto
            };
            Padre.Controls.Add(Etiqueta);
            return Etiqueta;
        }

        // Cambia textos y colores según el resumen (null = sin período seleccionado)
        private void RrhhMetActualizarCuadre(ClsResumenCierreNominaValor Resumen)
        {
            if (RrhhPnlCuadre == null) return;

            Color Fondo, Borde, Texto;

            if (Resumen == null)
            {
                _EstadoCuadre = 0;
                Fondo = Color.FromArgb(240, 244, 243);
                Borde = Color.FromArgb(160, 170, 170);
                Texto = Color.FromArgb(14, 76, 85);
                RrhhLblTituloCuadre.Text = "Verificación de cuadre";
                RrhhLblDescripcionCuadre.Text = "Seleccione un mes para ver el cuadre.";
                RrhhLblMontoPercepciones.Text = "Q 0.00";
                RrhhLblMontoDeducciones.Text = "Q 0.00";
                RrhhLblMontoDiferencia.Text = "Q 0.00";
            }
            else
            {
                bool Ok = Resumen.CuadreCorrecto;
                _EstadoCuadre = Ok ? 1 : 2;
                Fondo = Ok ? Color.FromArgb(232, 245, 233) : Color.FromArgb(253, 236, 234);
                Borde = Ok ? Color.FromArgb(34, 160, 80) : Color.Firebrick;
                Texto = Ok ? Color.FromArgb(27, 94, 32) : Color.Firebrick;

                RrhhLblTituloCuadre.Text = Ok ? "Cuadre Correcto" : "Cuadre Incorrecto";
                RrhhLblDescripcionCuadre.Text = Ok
                    ? "Los totales de percepciones y deducciones coinciden."
                    : (string.IsNullOrEmpty(Resumen.MensajeBloqueo) ? Resumen.TextoCuadre : Resumen.MensajeBloqueo);

                RrhhLblMontoPercepciones.Text = "Q " + Resumen.TotalPercepciones.ToString("N2");
                RrhhLblMontoDeducciones.Text = "Q " + Resumen.TotalDeducciones.ToString("N2");
                RrhhLblMontoDiferencia.Text = "Q " + (Resumen.TotalDebe - Resumen.TotalHaber).ToString("N2");
            }

            _ColorBordeCuadre = Borde;
            RrhhPnlCuadre.BackColor = Fondo;
            RrhhPnlDiferencia.BackColor = Fondo;
            RrhhLblTituloCuadre.ForeColor = Texto;
            RrhhLblMontoDiferencia.ForeColor = Texto;
            RrhhLblTxtDiferencia.ForeColor = Texto;
            RrhhLblDescripcionCuadre.ForeColor = Color.FromArgb(60, 70, 70);

            Color Oscuro = Color.FromArgb(14, 76, 85);
            RrhhLblTxtPercepciones.ForeColor = Oscuro;
            RrhhLblTxtDeducciones.ForeColor = Oscuro;
            RrhhLblMontoPercepciones.ForeColor = Oscuro;
            RrhhLblMontoDeducciones.ForeColor = Oscuro;

            RrhhPnlCuadre.Invalidate();
            RrhhPnlDiferencia.Invalidate();
            RrhhPnlIconoCuadre.Invalidate();
            RrhhPnlIconoDiferencia.Invalidate();
        }

        // Dibuja el borde del panel con el color del estado
        private void RrhhPnlBorde_Paint(object sender, PaintEventArgs e)
        {
            var Panel = (Control)sender;
            using (var Lapiz = new Pen(_ColorBordeCuadre, 2f))
                e.Graphics.DrawRectangle(Lapiz, 1, 1, Panel.Width - 3, Panel.Height - 3);
        }

        // Dibuja el círculo con check (correcto), X (incorrecto) o vacío (sin datos)
        private void RrhhPnlIcono_Paint(object sender, PaintEventArgs e)
        {
            var Icono = (Control)sender;
            float T = Icono.Width;
            var G = e.Graphics;
            G.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            Color ColorCirculo = _EstadoCuadre == 1 ? Color.FromArgb(34, 160, 80)
                               : _EstadoCuadre == 2 ? Color.Firebrick
                               : Color.Gray;

            using (var Pincel = new SolidBrush(ColorCirculo))
                G.FillEllipse(Pincel, 0, 0, T - 1, T - 1);

            using (var Lapiz = new Pen(Color.White, T / 9f))
            {
                Lapiz.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                Lapiz.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                Lapiz.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;

                if (_EstadoCuadre == 1)
                {
                    G.DrawLines(Lapiz, new[]
                    {
                        new PointF(T * 0.27f, T * 0.52f),
                        new PointF(T * 0.43f, T * 0.68f),
                        new PointF(T * 0.74f, T * 0.32f)
                    });
                }
                else if (_EstadoCuadre == 2)
                {
                    G.DrawLine(Lapiz, T * 0.32f, T * 0.32f, T * 0.68f, T * 0.68f);
                    G.DrawLine(Lapiz, T * 0.68f, T * 0.32f, T * 0.32f, T * 0.68f);
                }
            }
        }

        // El botón solo se habilita si el rol puede insertar y el período está listo para cerrar
        private void RrhhMetActualizarBoton()
        {
            RrhhBtnCerrarNomina.Enabled = _MisPermisos != null
                && _MisPermisos.PuedeInsertar
                && _ResumenActual != null
                && _ResumenActual.PuedeCerrar;
        }

        private void RrhhBtnCerrarNomina_Click(object sender, EventArgs e)
        {
            var Periodo = RrhhCboPeriodo.SelectedItem as ClsPeriodoNominaValor;
            if (Periodo == null || _ResumenActual == null || !_ResumenActual.PuedeCerrar)
            {
                MessageBox.Show("Seleccione un mes con nómina lista para cerrar.");
                return;
            }

            string Pregunta = "Va a cerrar la nómina de " + Periodo.Texto + "." + Environment.NewLine + Environment.NewLine +
                "Empleados: " + _ResumenActual.EmpleadosEnNomina + Environment.NewLine +
                "Salario líquido: Q " + _ResumenActual.SalarioLiquido.ToString("N2") + Environment.NewLine + Environment.NewLine +
                "Se enviará la póliza a Contabilidad. Este proceso es irreversible. ¿Desea continuar?";

            if (MessageBox.Show(Pregunta, "Cerrar nómina", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                return;

            try
            {
                Cursor = Cursors.WaitCursor;
                RrhhBtnCerrarNomina.Enabled = false;

                _CierreNomina.PeriodoCierreNomina = Periodo.PeriodoNomina;
                _CierreNomina.FechaInicioCierreNomina = Periodo.FechaInicioNomina;
                _CierreNomina.FechaFinalCierreNomina = Periodo.FechaFinalNomina;

                bool Valido = new ClsValidacionDatos(_CierreNomina).SeguridadMetValidar();
                if (Valido)
                {
                    string Resultado = _CierreNomina.RrhhMetCerrarNomina();

                    // Si el mes ya no está entre los abiertos, el cierre se realizó
                    List<ClsPeriodoNominaValor> Abiertos = RrhhMetCargarPeriodos();
                    int Posicion = Abiertos.FindIndex(p => p.PeriodoNomina == Periodo.PeriodoNomina);
                    bool Cerrada = Posicion < 0;

                    if (Cerrada)
                        RrhhMetLimpiarResumen();
                    else
                        RrhhCboPeriodo.SelectedIndex = Posicion;

                    MessageBox.Show(Resultado, "Cierre de nómina", MessageBoxButtons.OK,
                        Cerrada ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                Cursor = Cursors.Default;
                RrhhMetActualizarBoton();
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            RrhhMetAjustarAPantalla();
        }

        private void RrhhMetAjustarAPantalla()
        {
            Rectangle Area = Screen.FromControl(this).WorkingArea;
            double Factor = Math.Min(1d, Math.Min((double)Area.Width / Width, (double)Area.Height / Height));
            if (Factor >= 1d)
                return;

            List<KeyValuePair<Control, Font>> Fuentes = new List<KeyValuePair<Control, Font>>();
            RrhhMetGuardarFuentes(this, Fuentes);

            AutoScaleMode = AutoScaleMode.None;
            Scale(new SizeF((float)Factor, (float)Factor));
            foreach (KeyValuePair<Control, Font> Par in Fuentes)
                Par.Key.Font = RrhhMetEscalarFuente(Par.Value, Factor);

            foreach (DataGridView Tabla in Fuentes.Select(Par => Par.Key).OfType<DataGridView>())
            {
                Tabla.ColumnHeadersDefaultCellStyle.Font = RrhhMetEscalarFuente(Tabla.ColumnHeadersDefaultCellStyle.Font, Factor);
                Tabla.DefaultCellStyle.Font = RrhhMetEscalarFuente(Tabla.DefaultCellStyle.Font, Factor);
            }

            if (Owner != null)
                CenterToParent();
            else
                CenterToScreen();
        }

        private void RrhhMetGuardarFuentes(Control Padre, List<KeyValuePair<Control, Font>> Fuentes)
        {
            Fuentes.Add(new KeyValuePair<Control, Font>(Padre, Padre.Font));
            foreach (Control Hijo in Padre.Controls)
                RrhhMetGuardarFuentes(Hijo, Fuentes);
        }

        private Font RrhhMetEscalarFuente(Font Original, double Factor)
        {
            if (Original == null)
                return null;
            return new Font(Original.FontFamily, (float)(Original.Size * Factor), Original.Style, Original.Unit);
        }

        private void RrhhBtnReporteCierreNomina_Click(object sender, EventArgs e)
        {
            FrmReporteCierreNominas reporte = new FrmReporteCierreNominas();
            reporte.Show();
        }

        private void RrhhBtnAyudaCierreNomina_Click(object sender, EventArgs e)
        {
            RrhhMetAbrirAyuda(this, "CierreNominas__RecursosHumanos.html");

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