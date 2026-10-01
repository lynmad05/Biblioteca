using Biblioteca.Entidades;
using Microsoft.Data.SqlClient;

namespace Biblioteca.Datos;

public class PrestamoDatos
{
    private const string SelectReporte = @"
        SELECT p.PrestamoId, s.Nombre AS Socio, l.LibroId, l.Titulo AS Libro,
               p.FechaPrestamo, p.FechaLimite, p.Estado
        FROM Prestamos p
        INNER JOIN DetallePrestamo d ON d.PrestamoId = p.PrestamoId
        INNER JOIN Libros l          ON l.LibroId    = d.LibroId
        INNER JOIN Socios s          ON s.SocioId    = p.SocioId";

    private static PrestamoReporte MapearReporte(SqlDataReader dr) => new()
    {
        PrestamoId = (int)dr["PrestamoId"],
        Socio = (string)dr["Socio"],
        LibroId = (int)dr["LibroId"],
        Libro = (string)dr["Libro"],
        FechaPrestamo = (DateTime)dr["FechaPrestamo"],
        FechaLimite = (DateTime)dr["FechaLimite"],
        Estado = (string)dr["Estado"]
    };

    
    public Task<List<PrestamoReporte>> ReporteAsync(DateTime desde, DateTime hasta) =>
        ConexionBD.ConsultarAsync(
            SelectReporte + @" WHERE p.FechaPrestamo BETWEEN @desde AND @hasta
                               ORDER BY p.FechaPrestamo, p.PrestamoId",
            MapearReporte,
            new SqlParameter("@desde", desde.Date),
            new SqlParameter("@hasta", hasta.Date));

    
    public Task<List<PrestamoReporte>> ListarPendientesPorSocioAsync(int socioId) =>
        ConexionBD.ConsultarAsync(
            SelectReporte + @" WHERE p.SocioId = @socio AND d.FechaDevolucion IS NULL
                               ORDER BY p.FechaLimite",
            MapearReporte,
            new SqlParameter("@socio", socioId));

    public async Task<Prestamo?> ObtenerPorIdAsync(int prestamoId)
    {
        var lista = await ConexionBD.ConsultarAsync(
            "SELECT PrestamoId, SocioId, FechaPrestamo, FechaLimite, Estado FROM Prestamos WHERE PrestamoId = @id",
            dr => new Prestamo
            {
                PrestamoId = (int)dr["PrestamoId"],
                SocioId = (int)dr["SocioId"],
                FechaPrestamo = (DateTime)dr["FechaPrestamo"],
                FechaLimite = (DateTime)dr["FechaLimite"],
                Estado = (string)dr["Estado"]
            },
            new SqlParameter("@id", prestamoId));
        return lista.FirstOrDefault();
    }

    public Task<int> ContarPendientesPrestamoAsync(int prestamoId) =>
        ConexionBD.ContarAsync(
            "SELECT COUNT(1) FROM DetallePrestamo WHERE PrestamoId = @id AND FechaDevolucion IS NULL",
            new SqlParameter("@id", prestamoId));

   
    public async Task<int> RegistrarAsync(Prestamo prestamo)
    {
        await using var cn = ConexionBD.Crear();
        await cn.OpenAsync();
        await using var tx = (SqlTransaction)await cn.BeginTransactionAsync();
        try
        {
            int prestamoId;
            await using (var cmd = new SqlCommand(@"
                INSERT INTO Prestamos (SocioId, FechaPrestamo, FechaLimite, Estado)
                OUTPUT INSERTED.PrestamoId
                VALUES (@socio, @fecha, @limite, @estado)", cn, tx))
            {
                cmd.Parameters.AddWithValue("@socio", prestamo.SocioId);
                cmd.Parameters.AddWithValue("@fecha", prestamo.FechaPrestamo.Date);
                cmd.Parameters.AddWithValue("@limite", prestamo.FechaLimite.Date);
                cmd.Parameters.AddWithValue("@estado", prestamo.Estado);
                prestamoId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            }

            foreach (var detalle in prestamo.Detalles)
            {
                await EjecutarAsync(cn, tx,
                    "INSERT INTO DetallePrestamo (PrestamoId, LibroId) VALUES (@p, @l)",
                    new SqlParameter("@p", prestamoId),
                    new SqlParameter("@l", detalle.LibroId));

                var filas = await EjecutarAsync(cn, tx,
                    "UPDATE Libros SET Ejemplares = Ejemplares - 1 WHERE LibroId = @l AND Ejemplares > 0",
                    new SqlParameter("@l", detalle.LibroId));

                if (filas == 0)
                    throw new InvalidOperationException(
                        $"El libro {detalle.LibroId} ya no tiene ejemplares disponibles.");
            }

            await tx.CommitAsync();
            return prestamoId;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    // Guarda la fecha, devuelve el ejemplar al stock y, si Negocio lo indica, cierra el préstamo
    public async Task<bool> RegistrarDevolucionAsync(
        int prestamoId, int libroId, DateTime fechaDevolucion, bool cerrarPrestamo)
    {
        await using var cn = ConexionBD.Crear();
        await cn.OpenAsync();
        await using var tx = (SqlTransaction)await cn.BeginTransactionAsync();
        try
        {
            var filas = await EjecutarAsync(cn, tx, @"
                UPDATE DetallePrestamo SET FechaDevolucion = @fecha
                WHERE PrestamoId = @p AND LibroId = @l AND FechaDevolucion IS NULL",
                new SqlParameter("@fecha", fechaDevolucion.Date),
                new SqlParameter("@p", prestamoId),
                new SqlParameter("@l", libroId));

            if (filas == 0)
            {
                await tx.RollbackAsync();
                return false; 
            }

            await EjecutarAsync(cn, tx,
                "UPDATE Libros SET Ejemplares = Ejemplares + 1 WHERE LibroId = @l",
                new SqlParameter("@l", libroId));

            if (cerrarPrestamo)
                await EjecutarAsync(cn, tx,
                    "UPDATE Prestamos SET Estado = 'Devuelto' WHERE PrestamoId = @p",
                    new SqlParameter("@p", prestamoId));

            await tx.CommitAsync();
            return true;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    private static async Task<int> EjecutarAsync(
        SqlConnection cn, SqlTransaction tx, string sql, params SqlParameter[] parametros)
    {
        await using var cmd = new SqlCommand(sql, cn, tx);
        cmd.Parameters.AddRange(parametros);
        return await cmd.ExecuteNonQueryAsync();
    }
}