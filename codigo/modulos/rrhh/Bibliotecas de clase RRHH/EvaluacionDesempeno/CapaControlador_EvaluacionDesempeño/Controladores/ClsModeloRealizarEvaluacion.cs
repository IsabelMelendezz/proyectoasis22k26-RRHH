/*
 * ==================================================================
 * Área : Recursos Humanos
 * Autor : Daniella Jurado
 * Carné : 0901-23-3679
 * Fecha : 03/11/2026
 * ==================================================================
 * Propósito :
 *  Ventana 9402 - Realizar Evaluación. Calcula el puntaje (sobre 5.00)
 *  con las notas de los 5 aspectos, porque la BD no tiene trigger que
 *  lo calcule. Los pesos son los mismos que usa la Capa Modelo.
 * ===================================================================
*/

namespace CapaControlador_EvaluacionDesempeño.Controladores
{
    public class ClsModeloRealizarEvaluacion
    {
        // Calidad, Productividad, Trabajo en equipo, Puntualidad, Iniciativa
        private static readonly decimal[] _Pesos = { 0.25m, 0.25m, 0.20m, 0.15m, 0.15m };

        // Notas en el mismo orden de los pesos. Devuelve null si falta una nota o alguna no está entre 1 y 5.
        public decimal? RrhhFuncCalcularPuntaje(int?[] Notas)
        {
            decimal Puntaje = 0;
            for (int Indice = 0; Indice < _Pesos.Length; Indice++)
            {
                int? Nota = Notas[Indice];
                if (Nota == null || Nota < 1 || Nota > 5) return null;
                Puntaje += Nota.Value * _Pesos[Indice];
            }
            return Puntaje;
        }
    }
}
