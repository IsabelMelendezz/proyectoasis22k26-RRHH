using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaModelo_CierreNominas.Contratos
{
    /*
 * ==================================================================
 * Área: Recursos Humanos
 * Autores: Lourdes Isabel Melendez Pineda
 * Fecha o ultima edicion: 31/10/2026
 * ==================================================================
 * Propósito : Interface genérica que define las funciones básicas
 * que debe tener cualquier repositorio del sistema:
 * agregar, editar, eliminar y obtener todos los registros.
 * ===================================================================
 */

    public interface IRepositorioGenerico<Entidad> where Entidad : class
    {
        int RRHHMetAgregar(Entidad Entidad);
        int RRHHMetEditar(Entidad Entidad);
        int RRHHMetRemover(Entidad Entidad);
        IEnumerable<Entidad> RRHHMetObtenerTodos();
    }
}