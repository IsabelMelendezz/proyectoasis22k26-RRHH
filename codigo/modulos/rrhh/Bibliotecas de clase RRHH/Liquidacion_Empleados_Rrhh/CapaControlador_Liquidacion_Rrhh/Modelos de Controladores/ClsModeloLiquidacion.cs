
using CapaControlador_Liquidacion_Rrhh.Objetos_de_valor;
using CapaModelo_Liquidacion_Rrhh.Entidades;
using CapaModelo_Liquidacion_Rrhh.Repositorios;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Odbc;

namespace CapaControlador_Liquidacion_Rrhh
{
    public class ClsModeloLiquidacion
    {
        // Campos privados completos
        private int _IdLiquidacion;
        private int _IdEmpresa;
        private int _IdEmpleado;
        private int _IdBajaEmpleado;
        private DateTime _FechaIngresoLiquidacion;
        private DateTime _FechaBajaLiquidacion;
        private int _DiasLaboradosLiquidacion;
        private decimal _SalarioMensualLiquidacion;
        private decimal _SalarioPromedioLiquidacion;
        private decimal _BaseIndemnizacionLiquidacion;
        private decimal _SalarioDiaLiquidacion;
        private decimal _TotalPercepcionLiquidacion;
        private decimal _TotalDeduccionLiquidacion;
        private decimal _TotalNetoLiquidacion;
        private string _ParametrosUsadosLiquidacion;
        private string _NumeroFiniquitoLiquidacion;
        private string _MontoLetrasFiniquitoLiquidacion;
        private string _EstadoLiquidacion;
        private int _IdUsuarioCrea;
        private int _IdUsuarioAprueba;
        private List<ClsModeloDetalleLiquidacion> _DetallesLiquidacion =
            new List<ClsModeloDetalleLiquidacion>();

        // Extra para vistas (opcional, según joins de tu DB)
        private string _NombresEmpleado;
        private string _NombreEmpresa;
        private string _NombreEmpleado;
        private string _NombreMotivoBaja;

        private ClsRepositorioLiquidacion _RepositorioLiquidacion;
        private List<ClsModeloLiquidacion> _ListaLiquidacion;

        // Propiedad de estado para el CRUD
        public EstadoEntidadLiquidacion Estado { private get; set; }

        // Propiedades públicas
        public int IdLiquidacion { get => _IdLiquidacion; set => _IdLiquidacion = value; }

        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una empresa válida")]
        public int IdEmpresa { get => _IdEmpresa; set => _IdEmpresa = value; }

        [Required(ErrorMessage = "Debe seleccionar un empleado")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un empleado válido")]
        public int IdEmpleado { get => _IdEmpleado; set => _IdEmpleado = value; }

        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una baja válida")]
        public int IdBajaEmpleado { get => _IdBajaEmpleado; set => _IdBajaEmpleado = value; }
        public DateTime FechaIngresoLiquidacion { get => _FechaIngresoLiquidacion; set => _FechaIngresoLiquidacion = value; }
        public DateTime FechaBajaLiquidacion { get => _FechaBajaLiquidacion; set => _FechaBajaLiquidacion = value; }
        [Range(0, int.MaxValue, ErrorMessage = "Los días laborados no pueden ser negativos")]
        public int DiasLaboradosLiquidacion { get => _DiasLaboradosLiquidacion; set => _DiasLaboradosLiquidacion = value; }
        public decimal SalarioMensualLiquidacion { get => _SalarioMensualLiquidacion; set => _SalarioMensualLiquidacion = value; }
        public decimal SalarioPromedioLiquidacion { get => _SalarioPromedioLiquidacion; set => _SalarioPromedioLiquidacion = value; }
        public decimal BaseIndemnizacionLiquidacion { get => _BaseIndemnizacionLiquidacion; set => _BaseIndemnizacionLiquidacion = value; }
        public decimal SalarioDiaLiquidacion { get => _SalarioDiaLiquidacion; set => _SalarioDiaLiquidacion = value; }
        public decimal TotalPercepcionLiquidacion { get => _TotalPercepcionLiquidacion; set => _TotalPercepcionLiquidacion = value; }
        public decimal TotalDeduccionLiquidacion { get => _TotalDeduccionLiquidacion; set => _TotalDeduccionLiquidacion = value; }
        public decimal TotalNetoLiquidacion { get => _TotalNetoLiquidacion; set => _TotalNetoLiquidacion = value; }
        public string ParametrosUsadosLiquidacion { get => _ParametrosUsadosLiquidacion; set => _ParametrosUsadosLiquidacion = value; }
        [StringLength(30, ErrorMessage = "El número de finiquito no puede exceder 30 caracteres")]
        [RegularExpression(@"^FIN-\d{4}-\d{4}$",
            ErrorMessage = "El número de finiquito debe usar el formato FIN-AÑO-0000")]
        public string NumeroFiniquitoLiquidacion { get => _NumeroFiniquitoLiquidacion; set => _NumeroFiniquitoLiquidacion = value; }

        [StringLength(255, ErrorMessage = "El monto en letras no puede exceder 255 caracteres")]
        public string MontoLetrasFiniquitoLiquidacion { get => _MontoLetrasFiniquitoLiquidacion; set => _MontoLetrasFiniquitoLiquidacion = value; }

        [Required(ErrorMessage = "Debe seleccionar un estado de liquidación")]
        public string EstadoLiquidacion { get => _EstadoLiquidacion; set => _EstadoLiquidacion = value; }
        public int IdUsuarioCrea { get => _IdUsuarioCrea; set => _IdUsuarioCrea = value; }

        [Range(1, int.MaxValue, ErrorMessage = "Debe asignar un usuario aprobador válido")]
        public int IdUsuarioAprueba { get => _IdUsuarioAprueba; set => _IdUsuarioAprueba = value; }

        public string NombresEmpleado { get => _NombresEmpleado; set => _NombresEmpleado = value; }
        public string NombreEmpresa { get => _NombreEmpresa; set => _NombreEmpresa = value; }
        public string NombreEmpleado { get => _NombreEmpleado; set => _NombreEmpleado = value; }
        public string NombreMotivoBaja { get => _NombreMotivoBaja; set => _NombreMotivoBaja = value; }
        public List<ClsModeloDetalleLiquidacion> DetallesLiquidacion
        {
            get => _DetallesLiquidacion;
            set => _DetallesLiquidacion = value ??
                new List<ClsModeloDetalleLiquidacion>();
        }

        // Constructor
        public ClsModeloLiquidacion()
        {
            _RepositorioLiquidacion = new ClsRepositorioLiquidacion();
        }

        // Método principal para Guardar, Editar o Eliminar
        public string LiquidacionMetGrabarCambios()
        {
            string Mensaje = null;
            try
            {
                var Entidad = new ClsLiquidacion
                {
                    IdLiquidacion = _IdLiquidacion,
                    IdEmpresa = _IdEmpresa,
                    IdEmpleado = _IdEmpleado,
                    IdBajaEmpleado = _IdBajaEmpleado,
                    FechaIngresoLiquidacion = _FechaIngresoLiquidacion,
                    FechaBajaLiquidacion = _FechaBajaLiquidacion,
                    DiasLaboradosLiquidacion = _DiasLaboradosLiquidacion,
                    SalarioMensualLiquidacion = _SalarioMensualLiquidacion,
                    SalarioPromedioLiquidacion = _SalarioPromedioLiquidacion,
                    BaseIndemnizacionLiquidacion = _BaseIndemnizacionLiquidacion,
                    SalarioDiaLiquidacion = _SalarioDiaLiquidacion,
                    TotalPercepcionLiquidacion = _TotalPercepcionLiquidacion,
                    TotalDeduccionLiquidacion = _TotalDeduccionLiquidacion,
                    TotalNetoLiquidacion = _TotalNetoLiquidacion,
                    ParametrosUsadosLiquidacion = _ParametrosUsadosLiquidacion,
                    NumeroFiniquitoLiquidacion = _NumeroFiniquitoLiquidacion,
                    MontoLetrasFiniquitoLiquidacion = _MontoLetrasFiniquitoLiquidacion,
                    EstadoLiquidacion = _EstadoLiquidacion,
                    IdUsuarioCrea = _IdUsuarioCrea,
                    IdUsuarioAprueba = _IdUsuarioAprueba
                };

                switch (Estado)
                {
                    case EstadoEntidadLiquidacion.Added:
                        _IdLiquidacion = _RepositorioLiquidacion
                            .LiquidacionMetGuardarConDetalles(
                                Entidad, LiquidacionMetConvertirDetalles(), true);
                        Mensaje = "Grabacion exitosa";
                        break;

                    case EstadoEntidadLiquidacion.Modified:
                        _RepositorioLiquidacion.LiquidacionMetGuardarConDetalles(
                            Entidad, LiquidacionMetConvertirDetalles(), false);
                        Mensaje = "Actualizacion exitosa";
                        break;

                    case EstadoEntidadLiquidacion.Deleted:
                        _RepositorioLiquidacion.LiquidacionMetRemover(Entidad);
                        Mensaje = "Eliminacion exitosa";
                        break;
                }
            }
            catch (OdbcException exOdbc)
            {
                if (exOdbc.Errors[0].NativeError == 1062)
                    Mensaje = "Este registro de liquidación ya existe o infringe una llave duplicada.";
                else
                    Mensaje = exOdbc.Message;
            }
            catch (Exception ex)
            {
                Mensaje = ex.ToString();
            }

            return Mensaje;
        }

        // Obtener la lista completa mapeando todos los campos
        public List<ClsModeloLiquidacion> LiquidacionMetObtenerTodos()
        {
            var Obtenidos = _RepositorioLiquidacion.LiquidacionMetObtenerTodos();
            _ListaLiquidacion = new List<ClsModeloLiquidacion>();

            foreach (var Item in Obtenidos)
            {
                _ListaLiquidacion.Add(new ClsModeloLiquidacion
                {
                    _IdLiquidacion = Item.IdLiquidacion,
                    _IdEmpresa = Item.IdEmpresa,
                    _IdEmpleado = Item.IdEmpleado,
                    _IdBajaEmpleado = Item.IdBajaEmpleado,
                    _FechaIngresoLiquidacion = Item.FechaIngresoLiquidacion,
                    _FechaBajaLiquidacion = Item.FechaBajaLiquidacion,
                    _DiasLaboradosLiquidacion = Item.DiasLaboradosLiquidacion,
                    _SalarioMensualLiquidacion = Item.SalarioMensualLiquidacion,
                    _SalarioPromedioLiquidacion = Item.SalarioPromedioLiquidacion,
                    _BaseIndemnizacionLiquidacion = Item.BaseIndemnizacionLiquidacion,
                    _SalarioDiaLiquidacion = Item.SalarioDiaLiquidacion,
                    _TotalPercepcionLiquidacion = Item.TotalPercepcionLiquidacion,
                    _TotalDeduccionLiquidacion = Item.TotalDeduccionLiquidacion,
                    _TotalNetoLiquidacion = Item.TotalNetoLiquidacion,
                    _ParametrosUsadosLiquidacion = Item.ParametrosUsadosLiquidacion,
                    _NumeroFiniquitoLiquidacion = Item.NumeroFiniquitoLiquidacion,
                    _MontoLetrasFiniquitoLiquidacion = Item.MontoLetrasFiniquitoLiquidacion,
                    _EstadoLiquidacion = Item.EstadoLiquidacion,
                    _IdUsuarioCrea = Item.IdUsuarioCrea,
                    _IdUsuarioAprueba = Item.IdUsuarioAprueba,
                    _NombreEmpresa = Item.NombreEmpresa,
                    _NombreEmpleado = Item.NombreEmpleado,
                    _NombreMotivoBaja = Item.NombreMotivoBaja
                });
            }

            return _ListaLiquidacion;
        }

        // Métodos para Combobox (Llaman directo al Repositorio)
        public DataTable LiquidacionMetObtenerBajasPorEmpleado(int idEmpleado)
        {
            return _RepositorioLiquidacion.LiquidacionMetObtenerBajasPorEmpleado(idEmpleado);
        }
        public DataTable LiquidacionMetObtenerEmpleados()
        {
            return _RepositorioLiquidacion.LiquidacionMetObtenerEmpleados();
        }

        public DataTable LiquidacionMetObtenerEmpresas()
        {
            return _RepositorioLiquidacion.LiquidacionMetObtenerEmpresas();
        }

        public DataTable LiquidacionMetObtenerPercepcionesActivas()
        {
            return _RepositorioLiquidacion.LiquidacionMetObtenerPercepcionesActivas();
        }

        public DataTable LiquidacionMetObtenerDetalles(int idLiquidacion)
        {
            if (idLiquidacion <= 0)
                throw new ArgumentOutOfRangeException(nameof(idLiquidacion),
                    "La liquidación seleccionada no es válida.");

            return _RepositorioLiquidacion.LiquidacionMetObtenerDetalles(
                idLiquidacion);
        }

        public DataTable LiquidacionMetObtenerDatosAutocompletado(int idEmpleado, int idBajaEmpleado)
        {
            if (idEmpleado <= 0)
                throw new ArgumentOutOfRangeException(nameof(idEmpleado),
                    "El empleado seleccionado no es válido.");

            if (idBajaEmpleado <= 0)
                throw new ArgumentOutOfRangeException(nameof(idBajaEmpleado),
                    "La baja seleccionada no es válida.");

            return _RepositorioLiquidacion.LiquidacionMetObtenerDatosAutocompletado(
                idEmpleado, idBajaEmpleado);
        }

        public bool LiquidacionMetExisteNumeroFiniquito(
            string numeroFiniquito, int idLiquidacionExcluir = 0)
        {
            if (string.IsNullOrWhiteSpace(numeroFiniquito))
                throw new ArgumentException(
                    "El número de finiquito es obligatorio.", nameof(numeroFiniquito));

            return _RepositorioLiquidacion.LiquidacionMetExisteNumeroFiniquito(
                numeroFiniquito.Trim().ToUpperInvariant(), idLiquidacionExcluir);
        }

        public string LiquidacionMetObtenerSiguienteNumeroFiniquito(int anio)
        {
            if (anio < 1900 || anio > 9999)
                throw new ArgumentOutOfRangeException(nameof(anio),
                    "El año indicado para el finiquito no es válido.");

            int correlativo = _RepositorioLiquidacion
                .LiquidacionMetObtenerSiguienteCorrelativoFiniquito(anio);

            if (correlativo > 9999)
                throw new InvalidOperationException(
                    $"Se agotó el correlativo de cuatro dígitos para el año {anio}.");

            return $"FIN-{anio}-{correlativo:0000}";
        }

        private List<ClsDetalleLiquidacion> LiquidacionMetConvertirDetalles()
        {
            var detalles = new List<ClsDetalleLiquidacion>();
            foreach (ClsModeloDetalleLiquidacion item in _DetallesLiquidacion)
            {
                detalles.Add(new ClsDetalleLiquidacion
                {
                    IdDetalleLiquidacion = item.IdDetalleLiquidacion,
                    IdLiquidacion = item.IdLiquidacion,
                    IdPercepcionDeduccion = item.IdPercepcionDeduccion,
                    NombrePercepcionDeduccion = item.NombrePercepcionDeduccion,
                    DiasDetalleLiquidacion = item.DiasDetalleLiquidacion,
                    BaseDetalleLiquidacion = item.BaseDetalleLiquidacion,
                    MontoDetalleLiquidacion = item.MontoDetalleLiquidacion
                });
            }

            return detalles;
        }
    }
}
