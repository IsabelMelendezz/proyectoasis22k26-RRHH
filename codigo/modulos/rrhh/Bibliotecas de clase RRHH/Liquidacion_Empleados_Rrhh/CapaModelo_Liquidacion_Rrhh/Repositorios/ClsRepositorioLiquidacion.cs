using CapaModelo_Liquidacion_Rrhh.Contratos;
using CapaModelo_Liquidacion_Rrhh.Entidades;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;

namespace CapaModelo_Liquidacion_Rrhh.Repositorios
{
    public class ClsRepositorioLiquidacion : ClsSentencias, IRepositorioLiquidacion
    {
        private string _SelectAll;
        private string _Insert;
        private string _Update;
        private string _Delete;

        public ClsRepositorioLiquidacion()
        {
            _SelectAll = "SELECT idLiquidacion, idEmpresa, idEmpleado, idBajaEmpleado, fechaIngresoLiquidacion, fechaBajaLiquidacion, diasLaboradosLiquidacion, salarioMensualLiquidacion, salarioPromedioLiquidacion, baseIndemnizacionLiquidacion, salarioDiaLiquidacion, totalPercepcionLiquidacion, totalDeduccionLiquidacion, totalNetoLiquidacion, parametrosUsadosLiquidacion, numeroFiniquitoLiquidacion, montoLetrasFiniquitoLiquidacion, estadoLiquidacion, idUsuarioCrea, idUsuarioAprueba FROM tblLiquidacion";

            _Insert = "INSERT INTO tblLiquidacion (idEmpresa, idEmpleado, idBajaEmpleado, fechaIngresoLiquidacion, fechaBajaLiquidacion, diasLaboradosLiquidacion, salarioMensualLiquidacion, salarioPromedioLiquidacion, baseIndemnizacionLiquidacion, salarioDiaLiquidacion, totalPercepcionLiquidacion, totalDeduccionLiquidacion, totalNetoLiquidacion, parametrosUsadosLiquidacion, numeroFiniquitoLiquidacion, montoLetrasFiniquitoLiquidacion, estadoLiquidacion, idUsuarioCrea, idUsuarioAprueba) VALUES (?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)";

            _Update = "UPDATE tblLiquidacion SET idEmpresa=?, idEmpleado=?, idBajaEmpleado=?, fechaIngresoLiquidacion=?, fechaBajaLiquidacion=?, diasLaboradosLiquidacion=?, salarioMensualLiquidacion=?, salarioPromedioLiquidacion=?, baseIndemnizacionLiquidacion=?, salarioDiaLiquidacion=?, totalPercepcionLiquidacion=?, totalDeduccionLiquidacion=?, totalNetoLiquidacion=?, parametrosUsadosLiquidacion=?, numeroFiniquitoLiquidacion=?, montoLetrasFiniquitoLiquidacion=?, estadoLiquidacion=?, idUsuarioCrea=?, idUsuarioAprueba=? WHERE idLiquidacion=?";

            _Delete = "DELETE FROM tblLiquidacion WHERE idLiquidacion=?";
        }

        public int LiquidacionMetAgregar(ClsLiquidacion Entidad)
        {
            var Parametros = CrearListaParametros(Entidad, false);
            return RrhhMetEjecucionNonQuery(_Insert, Parametros, CommandType.Text);
        }

        public int LiquidacionMetEditar(ClsLiquidacion Entidad)
        {
            var Parametros = CrearListaParametros(Entidad, true);
            return RrhhMetEjecucionNonQuery(_Update, Parametros, CommandType.Text);
        }

        public int LiquidacionMetGuardarConDetalles(
            ClsLiquidacion entidad,
            IEnumerable<ClsDetalleLiquidacion> detalles,
            bool esNuevo)
        {
            using (OdbcConnection conexion = RrhhMetObtenerConexion())
            {
                conexion.Open();
                using (OdbcTransaction transaccion = conexion.BeginTransaction())
                {
                    try
                    {
                        var parametrosCabecera = CrearListaParametros(
                            entidad, !esNuevo);
                        RrhhMetEjecucionNonQuery(
                            esNuevo ? _Insert : _Update,
                            parametrosCabecera,
                            CommandType.Text,
                            conexion,
                            transaccion);

                        int idLiquidacion = esNuevo
                            ? ObtenerUltimoIdInsertado(conexion, transaccion)
                            : entidad.IdLiquidacion;

                        const string eliminarDetalles = @"
                            DELETE FROM tbldetalleliquidacion
                            WHERE idLiquidacion = ?";
                        RrhhMetEjecucionNonQuery(
                            eliminarDetalles,
                            new List<OdbcParameter>
                            {
                                CrearParametro("p_idLiquidacion", OdbcType.Int,
                                    idLiquidacion)
                            },
                            CommandType.Text,
                            conexion,
                            transaccion);

                        const string insertarDetalle = @"
                            INSERT INTO tbldetalleliquidacion
                            (
                                idLiquidacion,
                                idPercepcionDeduccion,
                                diasDetalleLiquidacion,
                                baseDetalleLiquidacion,
                                montoDetalleLiquidacion
                            )
                            VALUES (?,?,?,?,?)";

                        foreach (ClsDetalleLiquidacion detalle in
                            detalles ?? new List<ClsDetalleLiquidacion>())
                        {
                            var parametrosDetalle = new List<OdbcParameter>
                            {
                                CrearParametro("p_idLiquidacion", OdbcType.Int,
                                    idLiquidacion),
                                CrearParametro("p_idPercepcionDeduccion", OdbcType.Int,
                                    detalle.IdPercepcionDeduccion),
                                CrearParametroDecimal("p_diasDetalle",
                                    detalle.DiasDetalleLiquidacion, 8, 2),
                                CrearParametroDecimal("p_baseDetalle",
                                    detalle.BaseDetalleLiquidacion),
                                CrearParametroDecimal("p_montoDetalle",
                                    detalle.MontoDetalleLiquidacion)
                            };

                            RrhhMetEjecucionNonQuery(
                                insertarDetalle,
                                parametrosDetalle,
                                CommandType.Text,
                                conexion,
                                transaccion);
                        }

                        transaccion.Commit();
                        return idLiquidacion;
                    }
                    catch
                    {
                        transaccion.Rollback();
                        throw;
                    }
                }
            }
        }

        public int LiquidacionMetRemover(ClsLiquidacion Entidad)
        {
            var Parametros = new List<OdbcParameter> { new OdbcParameter("p_id", Entidad.IdLiquidacion) };
            return RrhhMetEjecucionNonQuery(_Delete, Parametros, CommandType.Text);
        }

        public IEnumerable<ClsLiquidacion> LiquidacionMetObtenerTodos()
        {
            var Lista = new List<ClsLiquidacion>();
            var TablaDatos = RrhhMetEjecucionConsulta(_SelectAll, CommandType.Text);

            foreach (DataRow Fila in TablaDatos.Rows)
            {
                Lista.Add(new ClsLiquidacion
                {
                    IdLiquidacion = Convert.ToInt32(Fila[0]),
                    IdEmpresa = Convert.ToInt32(Fila[1]),
                    IdEmpleado = Convert.ToInt32(Fila[2]),
                    IdBajaEmpleado = Convert.ToInt32(Fila[3]),
                    FechaIngresoLiquidacion = Convert.ToDateTime(Fila[4]),
                    FechaBajaLiquidacion = Convert.ToDateTime(Fila[5]),
                    DiasLaboradosLiquidacion = Convert.ToInt32(Fila[6]),
                    SalarioMensualLiquidacion = Convert.ToDecimal(Fila[7]),
                    SalarioPromedioLiquidacion = Convert.ToDecimal(Fila[8]),
                    BaseIndemnizacionLiquidacion = Convert.ToDecimal(Fila[9]),
                    SalarioDiaLiquidacion = Convert.ToDecimal(Fila[10]),
                    TotalPercepcionLiquidacion = Convert.ToDecimal(Fila[11]),
                    TotalDeduccionLiquidacion = Convert.ToDecimal(Fila[12]),
                    TotalNetoLiquidacion = Convert.ToDecimal(Fila[13]),
                    ParametrosUsadosLiquidacion = Fila[14].ToString(),
                    NumeroFiniquitoLiquidacion = Fila[15].ToString(),
                    MontoLetrasFiniquitoLiquidacion = Fila[16].ToString(),
                    EstadoLiquidacion = Fila[17].ToString(),
                    IdUsuarioCrea = Convert.ToInt32(Fila[18]),
                    IdUsuarioAprueba = Convert.ToInt32(Fila[19])
                });
            }
            return Lista;
        }
        public DataTable LiquidacionMetObtenerEmpleados()
        {
            string sql = @"
        SELECT DISTINCT 
            e.idEmpleado, 
            CONCAT(e.nombresEmpleado, ' ', e.apellidosEmpleado) AS nombreCompleto
        FROM tblempleado e
        INNER JOIN tblbajaempleado b ON e.idEmpleado = b.idEmpleado";

            return RrhhMetEjecucionConsulta(sql, CommandType.Text);
        }

        public DataTable LiquidacionMetObtenerEmpresas()
        {
            const string sql = @"
                SELECT
                    idEmpresa,
                    CONCAT(
                        razonSocialEmpresa,
                        CASE
                            WHEN nombreComercialEmpresa IS NULL
                                 OR TRIM(nombreComercialEmpresa) = '' THEN ''
                            ELSE CONCAT(' - ', nombreComercialEmpresa)
                        END
                    ) AS nombreEmpresa
                FROM tblempresa
                WHERE is_active = 1
                ORDER BY razonSocialEmpresa";

            return RrhhMetEjecucionConsulta(sql, CommandType.Text);
        }

        public DataTable LiquidacionMetObtenerPercepcionesActivas()
        {
            const string sql = @"
                SELECT
                    idPercepcionDeduccion,
                    nombrePercepcionDeduccion
                FROM tblpercepciondeduccion
                WHERE is_active = 1
                  AND efectoPercepcionDeduccion = 1
                ORDER BY
                    CASE WHEN UPPER(nombrePercepcionDeduccion) = 'OTROS'
                         THEN 1 ELSE 0 END,
                    nombrePercepcionDeduccion";

            return RrhhMetEjecucionConsulta(sql, CommandType.Text);
        }

        public DataTable LiquidacionMetObtenerDetalles(int idLiquidacion)
        {
            const string sql = @"
                SELECT
                    d.idDetalleLiquidacion,
                    d.idLiquidacion,
                    d.idPercepcionDeduccion,
                    p.nombrePercepcionDeduccion,
                    d.diasDetalleLiquidacion,
                    d.baseDetalleLiquidacion,
                    d.montoDetalleLiquidacion
                FROM tbldetalleliquidacion d
                INNER JOIN tblpercepciondeduccion p
                    ON p.idPercepcionDeduccion = d.idPercepcionDeduccion
                WHERE d.idLiquidacion = ?
                  AND p.efectoPercepcionDeduccion = 1
                ORDER BY d.idDetalleLiquidacion";

            return RrhhMetEjecucionConsulta(
                sql,
                CommandType.Text,
                new List<OdbcParameter>
                {
                    CrearParametro("p_idLiquidacion", OdbcType.Int,
                        idLiquidacion)
                });
        }

        public DataTable LiquidacionMetObtenerBajasPorEmpleado(int idEmpleado)
        {
            // Asegúrate de cambiar "nombreMotivo" por el nombre real de tu columna en tblmotivobaja
            string sql = @"
        SELECT 
            b.idBajaEmpleado, 
            CONCAT(m.nombreMotivoBaja, ' (', DATE_FORMAT(b.fechaBajaEmpleado, '%d/%m/%Y'), ')') AS DescripcionBaja,
            b.fechaBajaEmpleado
        FROM tblbajaempleado b
        INNER JOIN tblmotivobaja m ON b.idMotivoBaja = m.idMotivoBaja
        WHERE b.idEmpleado = ?";

            // ODBC uses '?' as parameter placeholders instead of '@paramName'

            List<OdbcParameter> parametros = new List<OdbcParameter>
    {
        new OdbcParameter("idEmpleado", idEmpleado)
    };

            // Call the method inherited from ClsSentencias
            return RrhhMetEjecucionConsulta(sql, CommandType.Text, parametros);
        }

        public DataTable LiquidacionMetObtenerDatosAutocompletado(int idEmpleado, int idBajaEmpleado)
        {
            const string sqlLiquidacionExistente = @"
                SELECT
                    1 AS ExisteLiquidacion,
                    l.idLiquidacion AS IdLiquidacion,
                    l.idEmpresa AS IdEmpresa,
                    l.idEmpleado AS IdEmpleado,
                    l.idBajaEmpleado AS IdBajaEmpleado,
                    l.fechaIngresoLiquidacion AS FechaIngresoLiquidacion,
                    l.fechaBajaLiquidacion AS FechaBajaLiquidacion,
                    l.diasLaboradosLiquidacion AS DiasLaboradosLiquidacion,
                    l.salarioMensualLiquidacion AS SalarioMensualLiquidacion,
                    l.salarioPromedioLiquidacion AS SalarioPromedioLiquidacion,
                    l.baseIndemnizacionLiquidacion AS BaseIndemnizacionLiquidacion,
                    l.salarioDiaLiquidacion AS SalarioDiaLiquidacion,
                    l.totalPercepcionLiquidacion AS TotalPercepcionLiquidacion,
                    l.totalDeduccionLiquidacion AS TotalDeduccionLiquidacion,
                    l.totalNetoLiquidacion AS TotalNetoLiquidacion,
                    l.parametrosUsadosLiquidacion AS ParametrosUsadosLiquidacion,
                    l.numeroFiniquitoLiquidacion AS NumeroFiniquitoLiquidacion,
                    l.montoLetrasFiniquitoLiquidacion AS MontoLetrasFiniquitoLiquidacion,
                    l.estadoLiquidacion AS EstadoLiquidacion,
                    l.idUsuarioCrea AS IdUsuarioCrea,
                    l.idUsuarioAprueba AS IdUsuarioAprueba,
                    0 AS CantidadPeriodos
                FROM tblLiquidacion l
                WHERE l.idEmpleado = ?
                  AND l.idBajaEmpleado = ?
                ORDER BY l.idLiquidacion DESC
                LIMIT 1";

            var parametrosExistente = new List<OdbcParameter>
            {
                CrearParametro("p_idEmpleado", OdbcType.Int, idEmpleado),
                CrearParametro("p_idBajaEmpleado", OdbcType.Int, idBajaEmpleado)
            };

            DataTable liquidacionExistente = RrhhMetEjecucionConsulta(
                sqlLiquidacionExistente, CommandType.Text, parametrosExistente);

            if (liquidacionExistente.Rows.Count > 0)
                return liquidacionExistente;

            const string sqlNuevaLiquidacion = @"
                WITH Contexto AS
                (
                    SELECT
                        e.idEmpleado,
                        e.fechaContratacionEmpleado,
                        e.salarioEmpleado,
                        b.idBajaEmpleado,
                        b.fechaBajaEmpleado
                    FROM tblempleado e
                    INNER JOIN tblbajaempleado b
                        ON b.idEmpleado = e.idEmpleado
                    WHERE e.idEmpleado = ?
                      AND b.idBajaEmpleado = ?
                ),
                HistorialMensual AS
                (
                    SELECT
                        cn.idCierreNomina,
                        cn.periodoCierreNomina,
                        cn.fechaFinalCierreNomina,
                        MAX(cne.salarioBaseCierreNominaEmpleado) AS salarioBaseCierreNominaEmpleado,
                        SUM(COALESCE(ne.totalHorasExtraNominaEncabezado, 0)) AS TotalHorasExtra,
                        SUM(cne.totalPercepcionCierreNominaEmpleado) AS totalPercepcionCierreNominaEmpleado,
                        SUM(cne.totalDeduccionCierreNominaEmpleado) AS totalDeduccionCierreNominaEmpleado,
                        SUM(cne.totalNetoCierreNominaEmpleado) AS totalNetoCierreNominaEmpleado
                    FROM tblcierrenominaempleado cne
                    INNER JOIN tblcierrenomina cn
                        ON cn.idCierreNomina = cne.idCierreNomina
                    INNER JOIN tblnominaencabezado ne
                        ON ne.idNominaEncabezado = cne.idNominaEncabezado
                    INNER JOIN Contexto ctx
                        ON ctx.idEmpleado = cne.idEmpleado
                    WHERE cn.fechaFinalCierreNomina <= ctx.fechaBajaEmpleado
                      AND cn.estadoPolizaCierreNomina IN ('VALIDADA', 'APLICADA')
                      AND ne.estadoNminaEncabezado IN ('Aprobada', 'Pagada')
                    GROUP BY cn.idCierreNomina,
                             cn.periodoCierreNomina,
                             cn.fechaFinalCierreNomina
                ),
                Historial AS
                (
                    SELECT *
                    FROM HistorialMensual
                    ORDER BY fechaFinalCierreNomina DESC,
                             idCierreNomina DESC
                    LIMIT 6
                ),
                Resumen AS
                (
                    SELECT
                        COUNT(*) AS CantidadPeriodos,
                        AVG(salarioBaseCierreNominaEmpleado + TotalHorasExtra) AS SalarioPromedio
                    FROM Historial
                ),
                UltimoCierre AS
                (
                    SELECT *
                    FROM Historial
                    ORDER BY fechaFinalCierreNomina DESC
                    LIMIT 1
                )
                SELECT
                    0 AS ExisteLiquidacion,
                    NULL AS IdLiquidacion,
                    NULL AS IdEmpresa,
                    ctx.idEmpleado AS IdEmpleado,
                    ctx.idBajaEmpleado AS IdBajaEmpleado,
                    ctx.fechaContratacionEmpleado AS FechaIngresoLiquidacion,
                    ctx.fechaBajaEmpleado AS FechaBajaLiquidacion,
                    GREATEST(DATEDIFF(ctx.fechaBajaEmpleado, ctx.fechaContratacionEmpleado) + 1, 0)
                        AS DiasLaboradosLiquidacion,
                    ROUND(COALESCE(uc.salarioBaseCierreNominaEmpleado, ctx.salarioEmpleado), 4)
                        AS SalarioMensualLiquidacion,
                    ROUND(COALESCE(r.SalarioPromedio, ctx.salarioEmpleado), 4)
                        AS SalarioPromedioLiquidacion,
                    ROUND(COALESCE(r.SalarioPromedio, ctx.salarioEmpleado) * 7 / 6, 4)
                        AS BaseIndemnizacionLiquidacion,
                    ROUND(COALESCE(uc.salarioBaseCierreNominaEmpleado, ctx.salarioEmpleado) / 30, 4)
                        AS SalarioDiaLiquidacion,
                    ROUND(COALESCE(uc.totalPercepcionCierreNominaEmpleado, 0), 4)
                        AS TotalPercepcionLiquidacion,
                    ROUND(COALESCE(uc.totalDeduccionCierreNominaEmpleado, 0), 4)
                        AS TotalDeduccionLiquidacion,
                    ROUND(COALESCE(uc.totalNetoCierreNominaEmpleado, 0), 4)
                        AS TotalNetoLiquidacion,
                    CONCAT(
                        'Origen: cierres de nomina por empleado; periodos=', r.CantidadPeriodos,
                        '; promedio=salario base + horas extra; base indemnizacion=promedio + 1/12 aguinaldo + 1/12 Bono 14',
                        CASE WHEN r.CantidadPeriodos = 0
                             THEN '; sin cierres previos: se uso tblempleado.salarioEmpleado'
                             ELSE '' END
                    ) AS ParametrosUsadosLiquidacion,
                    NULL AS NumeroFiniquitoLiquidacion,
                    NULL AS MontoLetrasFiniquitoLiquidacion,
                    'BORRADOR' AS EstadoLiquidacion,
                    NULL AS IdUsuarioCrea,
                    NULL AS IdUsuarioAprueba,
                    r.CantidadPeriodos AS CantidadPeriodos
                FROM Contexto ctx
                CROSS JOIN Resumen r
                LEFT JOIN UltimoCierre uc ON 1 = 1";

            var parametrosNueva = new List<OdbcParameter>
            {
                CrearParametro("p_idEmpleado", OdbcType.Int, idEmpleado),
                CrearParametro("p_idBajaEmpleado", OdbcType.Int, idBajaEmpleado)
            };

            return RrhhMetEjecucionConsulta(
                sqlNuevaLiquidacion, CommandType.Text, parametrosNueva);
        }

        public bool LiquidacionMetExisteNumeroFiniquito(
            string numeroFiniquito, int idLiquidacionExcluir)
        {
            const string sql = @"
                SELECT COUNT(*) AS Total
                FROM tblLiquidacion
                WHERE numeroFiniquitoLiquidacion = ?
                  AND idLiquidacion <> ?";

            var parametros = new List<OdbcParameter>
            {
                CrearParametro("p_numeroFiniquito", OdbcType.VarChar,
                    numeroFiniquito, 30),
                CrearParametro("p_idLiquidacionExcluir", OdbcType.Int,
                    idLiquidacionExcluir)
            };

            DataTable resultado = RrhhMetEjecucionConsulta(
                sql, CommandType.Text, parametros);

            return resultado.Rows.Count > 0 &&
                   Convert.ToInt32(resultado.Rows[0]["Total"]) > 0;
        }

        public int LiquidacionMetObtenerSiguienteCorrelativoFiniquito(int anio)
        {
            const string sql = @"
                SELECT COALESCE(
                    MAX(CAST(RIGHT(numeroFiniquitoLiquidacion, 4) AS UNSIGNED)),
                    0
                ) + 1 AS SiguienteCorrelativo
                FROM tblLiquidacion
                WHERE numeroFiniquitoLiquidacion REGEXP ?";

            string patron = $"^FIN-{anio}-[0-9]{{4}}$";
            var parametros = new List<OdbcParameter>
            {
                CrearParametro("p_patronFiniquito", OdbcType.VarChar,
                    patron, 40)
            };

            DataTable resultado = RrhhMetEjecucionConsulta(
                sql, CommandType.Text, parametros);

            return resultado.Rows.Count == 0
                ? 1
                : Convert.ToInt32(resultado.Rows[0]["SiguienteCorrelativo"]);
        }

        private List<OdbcParameter> CrearListaParametros(ClsLiquidacion Entidad, bool esActualizacion)
        {
            var Parametros = new List<OdbcParameter>
            {
                CrearParametro("p_idEmpresa", OdbcType.Int, Entidad.IdEmpresa),
                CrearParametro("p_idEmpleado", OdbcType.Int, Entidad.IdEmpleado),
                CrearParametro("p_idBaja", OdbcType.Int, Entidad.IdBajaEmpleado),
                CrearParametro("p_fechaIngreso", OdbcType.Date, Entidad.FechaIngresoLiquidacion.Date),
                CrearParametro("p_fechaBaja", OdbcType.Date, Entidad.FechaBajaLiquidacion.Date),
                CrearParametro("p_diasLab", OdbcType.Int, Entidad.DiasLaboradosLiquidacion),
                CrearParametroDecimal("p_salMen", Entidad.SalarioMensualLiquidacion),
                CrearParametroDecimal("p_salPro", Entidad.SalarioPromedioLiquidacion),
                CrearParametroDecimal("p_baseInd", Entidad.BaseIndemnizacionLiquidacion),
                CrearParametroDecimal("p_salDia", Entidad.SalarioDiaLiquidacion),
                CrearParametroDecimal("p_totPer", Entidad.TotalPercepcionLiquidacion),
                CrearParametroDecimal("p_totDed", Entidad.TotalDeduccionLiquidacion),
                CrearParametroDecimal("p_totNet", Entidad.TotalNetoLiquidacion),
                CrearParametro("p_param", OdbcType.Text, (object)Entidad.ParametrosUsadosLiquidacion ?? DBNull.Value),
                CrearParametro("p_numFin", OdbcType.VarChar, (object)Entidad.NumeroFiniquitoLiquidacion ?? DBNull.Value, 30),
                CrearParametro("p_monLet", OdbcType.VarChar, (object)Entidad.MontoLetrasFiniquitoLiquidacion ?? DBNull.Value, 255),
                CrearParametro("p_estado", OdbcType.VarChar, string.IsNullOrWhiteSpace(Entidad.EstadoLiquidacion) ? "BORRADOR" : Entidad.EstadoLiquidacion, 20),
                CrearParametro("p_idUsu", OdbcType.Int, Entidad.IdUsuarioCrea),
                CrearParametro("p_idUsuAprueba", OdbcType.Int, Entidad.IdUsuarioAprueba)
            };

            if (esActualizacion)
                Parametros.Add(CrearParametro("p_idLiq", OdbcType.Int, Entidad.IdLiquidacion));

            return Parametros;
        }

        private static OdbcParameter CrearParametro(string nombre, OdbcType tipo, object valor, int tamanio = 0)
        {
            var parametro = new OdbcParameter(nombre, tipo) { Value = valor ?? DBNull.Value };
            if (tamanio > 0)
                parametro.Size = tamanio;
            return parametro;
        }

        private static OdbcParameter CrearParametroDecimal(string nombre, decimal valor)
        {
            return CrearParametroDecimal(nombre, valor, 12, 4);
        }

        private static OdbcParameter CrearParametroDecimal(
            string nombre, decimal valor, byte precision, byte escala)
        {
            return new OdbcParameter(nombre, OdbcType.Decimal)
            {
                Precision = precision,
                Scale = escala,
                Value = valor
            };
        }

        private static int ObtenerUltimoIdInsertado(
            OdbcConnection conexion, OdbcTransaction transaccion)
        {
            using (var comando = new OdbcCommand(
                "SELECT LAST_INSERT_ID()", conexion, transaccion))
            {
                return Convert.ToInt32(comando.ExecuteScalar());
            }
        }
    }
}
