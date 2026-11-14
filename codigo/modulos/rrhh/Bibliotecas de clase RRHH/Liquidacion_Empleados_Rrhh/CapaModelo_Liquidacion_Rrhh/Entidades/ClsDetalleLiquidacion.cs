namespace CapaModelo_Liquidacion_Rrhh.Entidades
{
    public class ClsDetalleLiquidacion
    {
        public int IdDetalleLiquidacion { get; set; }
        public int IdLiquidacion { get; set; }
        public int IdPercepcionDeduccion { get; set; }
        public string NombrePercepcionDeduccion { get; set; }
        public decimal DiasDetalleLiquidacion { get; set; }
        public decimal BaseDetalleLiquidacion { get; set; }
        public decimal MontoDetalleLiquidacion { get; set; }
    }
}
