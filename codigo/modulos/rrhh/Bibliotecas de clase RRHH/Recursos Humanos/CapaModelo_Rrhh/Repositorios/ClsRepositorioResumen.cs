using System.Data;

/*
 * ==================================================================
 * Área : Recursos Humanos
 * Autor : Byron Alexander Chiquito Paredes
 * Carné : 0901-23-3747
 * Fecha : 14/11/2026
 * ==================================================================
 * Propósito :
 *  Consulta los números del Resumen de Recursos Humanos que se ven
 *  en el MDI (9000). Todo sale en una sola consulta para no abrir
 *  seis conexiones cada vez que se refresca el menú.
 * ===================================================================
*/

namespace CapaModelo_Rrhh.Repositorios
{
    public class ClsRepositorioResumen : ClsSentencias
    {
        // Colaboradores: activos y sin una baja que ya haya llegado a su fecha.
        // Nómina en curso: meses con nóminas Aprobadas o Pagadas que todavía no
        // están en un cierre (la misma regla que usa Cierre de Nóminas).
        // Evaluaciones: colaboradores que todavía no tienen evaluación en el trimestre
        // actual (las evaluaciones se hacen cada trimestre).
        // Liquidación: empleados con una liquidación que todavía no se paga.
        private const string SelectResumen = @"
        SELECT
            (SELECT COUNT(*) FROM tblEmpleado e
              WHERE e.is_active = 1
                AND NOT EXISTS (SELECT 1 FROM tblBajaEmpleado b
                                 WHERE b.idEmpleado = e.idEmpleado AND b.is_active = 1
                                   AND b.fechaBajaEmpleado <= CURDATE())) AS totalColaboradores,
            (SELECT COUNT(*) FROM tblPermisoEmpleado
              WHERE estadoPermisoEmpleado = 'PENDIENTE' AND is_active = 1) AS permisosPendientes,
            (SELECT COUNT(*) FROM tblCapacitacion
              WHERE estadoCapacitacion = 1 AND fechaFinCapacitacion >= CURDATE()) AS capacitacionesPendientes,
            (SELECT COUNT(*) FROM tblEmpleado e
              WHERE e.is_active = 1
                AND NOT EXISTS (SELECT 1 FROM tblBajaEmpleado b
                                 WHERE b.idEmpleado = e.idEmpleado AND b.is_active = 1
                                   AND b.fechaBajaEmpleado <= CURDATE())
                AND NOT EXISTS (SELECT 1 FROM tblEvaluacionDesempeno ed
                                 WHERE ed.idEmpleado = e.idEmpleado AND ed.is_active = 1
                                   AND YEAR(ed.fechaEvaluacionDesempeno) = YEAR(CURDATE())
                                   AND QUARTER(ed.fechaEvaluacionDesempeno) = QUARTER(CURDATE()))) AS evaluacionesPendientes,
            (SELECT COUNT(DISTINCT YEAR(ne.fechaFinalNominaEncabezado), MONTH(ne.fechaFinalNominaEncabezado))
               FROM tblNominaEncabezado ne
              WHERE ne.estadoNminaEncabezado IN ('Aprobada', 'Pagada')
                AND NOT EXISTS (SELECT 1 FROM tblCierreNominaEmpleado ce
                                 WHERE ce.idNominaEncabezado = ne.idNominaEncabezado)) AS nominasEnCurso,
            (SELECT COUNT(DISTINCT idEmpleado) FROM tblLiquidacion
              WHERE estadoLiquidacion IN ('BORRADOR', 'APROBADA') AND is_active = 1) AS empleadosLiquidacion";

        public DataTable RrhhMetObtenerResumen()
        {
            return RrhhMetEjecucionConsulta(SelectResumen, CommandType.Text);
        }
    }
}
