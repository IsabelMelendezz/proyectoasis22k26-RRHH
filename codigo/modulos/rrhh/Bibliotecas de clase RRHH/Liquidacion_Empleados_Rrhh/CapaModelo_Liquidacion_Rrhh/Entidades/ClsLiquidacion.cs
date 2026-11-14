using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaModelo_Liquidacion_Rrhh.Entidades
{
    public class ClsLiquidacion
    {
        public int IdLiquidacion { get; set; }
        public int IdEmpresa { get; set; }
        public int IdEmpleado { get; set; }
        public int IdBajaEmpleado { get; set; }
        public DateTime FechaIngresoLiquidacion { get; set; }
        public DateTime FechaBajaLiquidacion { get; set; }
        public int DiasLaboradosLiquidacion { get; set; }
        public decimal SalarioMensualLiquidacion { get; set; }
        public decimal SalarioPromedioLiquidacion { get; set; }
        public decimal BaseIndemnizacionLiquidacion { get; set; }
        public decimal SalarioDiaLiquidacion { get; set; }
        public decimal TotalPercepcionLiquidacion { get; set; }
        public decimal TotalDeduccionLiquidacion { get; set; }
        public decimal TotalNetoLiquidacion { get; set; }
        public string ParametrosUsadosLiquidacion { get; set; }
        public string NumeroFiniquitoLiquidacion { get; set; }
        public string MontoLetrasFiniquitoLiquidacion { get; set; }
        public string EstadoLiquidacion { get; set; }
        public int IdUsuarioCrea { get; set; }
        public int IdUsuarioAprueba { get; set; }

        // Propiedades adicionales para la vista (Grilla)
        public string NombresEmpleado { get; set; }
    }
}
