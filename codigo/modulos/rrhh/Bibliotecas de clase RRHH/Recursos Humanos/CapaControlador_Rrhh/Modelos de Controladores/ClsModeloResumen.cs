using CapaModelo_Rrhh.Repositorios;
using System;
using System.Data;

/*
 * ==================================================================
 * Área : Recursos Humanos
 * Autor : Byron Alexander Chiquito Paredes
 * Carné : 0901-23-3747
 * Fecha : 14/11/2026
 * ==================================================================
 * Propósito :
 *  Le pasa al MDI (9000) los seis indicadores del Resumen de Recursos
 *  Humanos. La vista solo lee las propiedades; la consulta queda en
 *  el repositorio.
 * ===================================================================
*/

namespace CapaControlador_Rrhh.Modelos_de_Controladores
{
    public class ClsModeloResumen
    {
        private readonly ClsRepositorioResumen _Repositorio = new ClsRepositorioResumen();

        public int TotalColaboradores { get; private set; }
        public int PermisosPendientes { get; private set; }
        public int CapacitacionesPendientes { get; private set; }
        public int EvaluacionesPendientes { get; private set; }
        public int NominasEnCurso { get; private set; }
        public int EmpleadosLiquidacion { get; private set; }

        public void RrhhMetCargar()
        {
            DataTable Tabla = _Repositorio.RrhhMetObtenerResumen();
            if (Tabla.Rows.Count == 0) return;

            DataRow Fila = Tabla.Rows[0];
            TotalColaboradores = RrhhMetEntero(Fila["totalColaboradores"]);
            PermisosPendientes = RrhhMetEntero(Fila["permisosPendientes"]);
            CapacitacionesPendientes = RrhhMetEntero(Fila["capacitacionesPendientes"]);
            EvaluacionesPendientes = RrhhMetEntero(Fila["evaluacionesPendientes"]);
            NominasEnCurso = RrhhMetEntero(Fila["nominasEnCurso"]);
            EmpleadosLiquidacion = RrhhMetEntero(Fila["empleadosLiquidacion"]);
        }

        // MySQL devuelve COUNT como BIGINT, por eso se convierte aquí
        private static int RrhhMetEntero(object Valor)
        {
            return Valor == DBNull.Value ? 0 : Convert.ToInt32(Valor);
        }
    }
}
