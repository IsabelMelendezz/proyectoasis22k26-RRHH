using System.Data;
using System.Data.Odbc;

/*
 * ==================================================================
 * Área : Recursos Humanos
 * Autor : Daniella Jurado
 * Carné : 0901-23-3679
 * Fecha : 02/11/2026
 * ==================================================================
 * Propósito :
 *  Ejecución de sentencias SQL con parámetros "?" de ODBC. Las
 *  sobrecargas que reciben conexión y transacción permiten que el
 *  repositorio agrupe varias sentencias en una sola transacción.
 * ===================================================================
*/

namespace CapaModelo_EvaluacionDesempeño.Repositorios
{
    public class ClsSentencias : ClsConexion
    {
        // Puntaje (sobre 5.00) calculado con las 5 notas de la evaluación (alias "e" en las consultas).
        // Se calcula al consultar porque la BD no tiene trigger que lo guarde.
        protected const string _PuntajeCalculado =
            @"ROUND(e.calidadEvaluacionDesempeno * 0.25 + e.productividadEvaluacionDesempeno * 0.25
                  + e.trabajoEquipoEvaluacionDesempeno * 0.20 + e.puntualidadEvaluacionDesempeno * 0.15
                  + e.iniciativaEvaluacionDesempeno * 0.15, 2)";

        public DataTable RrhhMetEjecucionConsulta(string Consulta, params object[] Valores)
        {
            var Tabla = new DataTable();
            using (OdbcConnection Conexion = RrhhMetObtenerConexion())
            using (OdbcCommand Comando = RrhhMetCrearComando(Consulta, Conexion, null, Valores))
            {
                Conexion.Open();
                using (OdbcDataAdapter Adaptador = new OdbcDataAdapter(Comando))
                {
                    Adaptador.Fill(Tabla);
                }
            }
            return Tabla;
        }

        public int RrhhMetEjecucionNonQuery(string Consulta, params object[] Valores)
        {
            using (OdbcConnection Conexion = RrhhMetObtenerConexion())
            using (OdbcCommand Comando = RrhhMetCrearComando(Consulta, Conexion, null, Valores))
            {
                Conexion.Open();
                return Comando.ExecuteNonQuery();
            }
        }

        protected int RrhhMetEjecucionNonQuery(string Consulta, OdbcConnection Conexion, OdbcTransaction Transaccion, params object[] Valores)
        {
            using (OdbcCommand Comando = RrhhMetCrearComando(Consulta, Conexion, Transaccion, Valores))
            {
                return Comando.ExecuteNonQuery();
            }
        }

        protected object RrhhMetEjecucionEscalar(string Consulta, OdbcConnection Conexion, OdbcTransaction Transaccion, params object[] Valores)
        {
            using (OdbcCommand Comando = RrhhMetCrearComando(Consulta, Conexion, Transaccion, Valores))
            {
                return Comando.ExecuteScalar();
            }
        }

        // Los parámetros de ODBC son posicionales: se agregan en el mismo orden de los "?".
        private OdbcCommand RrhhMetCrearComando(string Consulta, OdbcConnection Conexion, OdbcTransaction Transaccion, object[] Valores)
        {
            var Comando = new OdbcCommand(Consulta, Conexion, Transaccion);
            Comando.CommandType = CommandType.Text;
            for (int Indice = 0; Indice < Valores.Length; Indice++)
            {
                Comando.Parameters.AddWithValue("p" + Indice, Valores[Indice] ?? System.DBNull.Value);
            }
            return Comando;
        }
    }
}
