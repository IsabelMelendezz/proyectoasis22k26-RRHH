using CapaModelo_Liquidacion_Rrhh.Contratos;
using CapaModelo_Liquidacion_Rrhh.Entidades;
using System;
using System.Collections.Generic;
using System.Data;

namespace CapaModelo_Liquidacion_Rrhh.Repositorios
{
    public class ClsRepositorioParametroLey : ClsSentencias, IRepositorioParametroLey
    {
        private string _SelectAll;

        public ClsRepositorioParametroLey()
        {
            _SelectAll = @"
                SELECT p.idParametroLey,
                       p.idEmpresa,
                       p.codigoParametroLey,
                       p.descripcionParametroLey,
                       p.valorParametroLey,
                       p.vigenteDesdeParametroLey,
                       p.vigenteHastaParametroLey,
                       p.is_active
                FROM tblParametroLey p";
        }

        public IEnumerable<ClsParametroLey> RrhhMetObtenerTodos()
        {
            var Lista = new List<ClsParametroLey>();
            var TablaDatos = RrhhMetEjecucionConsulta(_SelectAll, CommandType.Text); 

            foreach (DataRow Fila in TablaDatos.Rows)
            {
                Lista.Add(new ClsParametroLey
                {
                    IdParametroLey = Convert.ToInt32(Fila["idParametroLey"]),
                    // Validación para manejar nulos desde la DB
                    IdEmpresa = Fila["idEmpresa"] != DBNull.Value ? Convert.ToInt32(Fila["idEmpresa"]) : (int?)null,
                    CodigoParametroLey = Fila["codigoParametroLey"].ToString(),
                    DescripcionParametroLey = Fila["descripcionParametroLey"].ToString(),
                    ValorParametroLey = Convert.ToDecimal(Fila["valorParametroLey"]),
                    VigenteDesdeParametroLey = Convert.ToDateTime(Fila["vigenteDesdeParametroLey"]),
                    
                    // Validación para fechas nulas
                    VigenteHastaParametroLey = Fila["vigenteHastaParametroLey"] != DBNull.Value ? Convert.ToDateTime(Fila["vigenteHastaParametroLey"]) : (DateTime?)null,
                    IsActive = Convert.ToBoolean(Fila["is_active"])
                });
            }
            return Lista; 
        }
    }
}