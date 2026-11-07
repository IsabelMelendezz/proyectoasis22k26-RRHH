/*
 * ==================================================================
 * Área : Rrhh
 * Autor : Cristian David Sipac Ispache
 * Carné : 9959-23-1567
 * Fecha : 06/11/2026
 * ==================================================================
 * Propósito :
 *  Valida las propiedades de un modelo mediante sus atributos
 *  de DataAnnotations. Reúne los errores encontrados y los
 *  muestra en un mensaje de advertencia, devolviendo si los
 *  datos cumplen las validaciones definidas.
 * ==================================================================
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ComponentModel.DataAnnotations;


namespace CapaVista_Nomina.Ayudas
{
    public class ClsValidacionDatos
    {
        private readonly object _Modelo;

        public ClsValidacionDatos(object Modelo)
        {
            _Modelo = Modelo;
        }

        public bool RrhhMetValidar()
        {
            var Resultados = new List<ValidationResult>();
            var Contexto = new ValidationContext(_Modelo);
            bool Valido = Validator.TryValidateObject(_Modelo, Contexto, Resultados, true);

            if (!Valido)
            {
                var Mensaje = new StringBuilder();
                foreach (var Resultado in Resultados)
                    Mensaje.AppendLine("• " + Resultado.ErrorMessage);
                MessageBox.Show(Mensaje.ToString(), "Datos inválidos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return Valido;
        }
    }
}
