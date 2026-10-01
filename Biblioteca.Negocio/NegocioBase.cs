using Microsoft.Data.SqlClient;

namespace Biblioteca.Negocio;

/// <summary>
/// Base de las clases de Negocio. Traduce los errores técnicos de la capa de Datos
/// a ReglaNegocioException, para que la capa WPF nunca tenga que capturar SqlException.
/// </summary>
public abstract class NegocioBase
{
    protected static async Task<T> ProtegerAsync<T>(Func<Task<T>> accion)
    {
        try
        {
            return await accion();
        }
        catch (ReglaNegocioException)
        {
            throw;
        }
        catch (SqlException ex) when (ex.Number is 2627 or 2601)
        {
            throw new ReglaNegocioException("Ya existe un registro con el mismo ISBN o DNI.", ex);
        }
        catch (SqlException ex)
        {
            throw new ReglaNegocioException(
                "No se pudo completar la operación con la base de datos. Revise la conexión e inténtelo nuevamente.", ex);
        }
        catch (InvalidOperationException ex)
        {
            throw new ReglaNegocioException(ex.Message, ex);
        }
    }
}