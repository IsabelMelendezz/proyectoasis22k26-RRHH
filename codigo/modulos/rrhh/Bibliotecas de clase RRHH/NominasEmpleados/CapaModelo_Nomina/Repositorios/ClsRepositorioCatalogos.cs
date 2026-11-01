using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaModelo_Nomina.Repositorios
{
    public class ClsRepositorioCatalogos : ClsSentencias
    {
        public List<KeyValuePair<int, string>> RrhhMetObtenerEmpleados()
        {
            var Lista = new List<KeyValuePair<int, string>>();
            var TablaDatos = RrhhMetEjecucionConsulta(
                "SELECT idEmpleado, nombresEmpleado, apellidosEmpleado FROM tblEmpleado " +
                "ORDER BY nombresEmpleado, apellidosEmpleado", CommandType.Text);
            foreach (DataRow Fila in TablaDatos.Rows)
            {
                string Nombre = (Fila["nombresEmpleado"].ToString() + " " + Fila["apellidosEmpleado"].ToString()).Trim();
                Lista.Add(new KeyValuePair<int, string>(Convert.ToInt32(Fila["idEmpleado"]), Nombre));
            }
            TablaDatos.Clear();
            TablaDatos = null;
            return Lista;
        }

        public List<KeyValuePair<int, string>> RrhhMetObtenerDepartamentos()
        {
            var Lista = new List<KeyValuePair<int, string>>();
            var TablaDatos = RrhhMetEjecucionConsulta(
                "SELECT idDepartamento, nombreDepartamento FROM tblDepartamento " +
                "ORDER BY nombreDepartamento", CommandType.Text);
            foreach (DataRow Fila in TablaDatos.Rows)
            {
                Lista.Add(new KeyValuePair<int, string>(
                    Convert.ToInt32(Fila["idDepartamento"]), Fila["nombreDepartamento"].ToString()));
            }
            TablaDatos.Clear();
            TablaDatos = null;
            return Lista;
        }
    }
}
