using Microsoft.Data.SqlClient;

namespace Biblioteca.Datos;

public class Estadisticas
{
    public int TotalLibros { get; set; }
    public int TotalSocios { get; set; }
    public int PrestamosPendientes { get; set; }
    public int DevolucionesHoy { get; set; }
}

public class EstadisticasDatos
{
    public async Task<Estadisticas> ObtenerAsync()
    {
        return new Estadisticas
        {
            TotalLibros = await ContarAsync("SELECT COUNT(*) AS Total FROM Libros WHERE Activo = 1"),
            TotalSocios = await ContarAsync("SELECT COUNT(*) AS Total FROM Socios WHERE Activo = 1"),
            PrestamosPendientes = await ContarAsync("SELECT COUNT(*) AS Total FROM Prestamos WHERE Estado = 'Pendiente'"),
            DevolucionesHoy = await ContarAsync(
                @"SELECT COUNT(*) AS Total FROM DetallePrestamo
                  WHERE FechaDevolucion IS NOT NULL
                    AND CAST(FechaDevolucion AS date) = CAST(GETDATE() AS date)")
        };
    }

    private static async Task<int> ContarAsync(string sql)
    {
        var filas = await ConexionBD.ConsultarAsync(sql, dr => (int)dr["Total"]);
        return filas.Count > 0 ? filas[0] : 0;
    }
}