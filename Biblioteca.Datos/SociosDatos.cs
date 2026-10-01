using Biblioteca.Entidades;
using Microsoft.Data.SqlClient;

namespace Biblioteca.Datos;

public class SocioDatos
{
    private const string SelectBase =
        "SELECT SocioId, DNI, Nombre, Email, Activo FROM Socios";

    private static Socio Mapear(SqlDataReader dr) => new()
    {
        SocioId = (int)dr["SocioId"],
        DNI = ((string)dr["DNI"]).Trim(),
        Nombre = (string)dr["Nombre"],
        Email = dr["Email"] as string,
        Activo = (bool)dr["Activo"]
    };

    public Task<List<Socio>> ListarAsync() =>
        ConexionBD.ConsultarAsync(SelectBase + " WHERE Activo = 1 ORDER BY Nombre", Mapear);

    public Task<List<Socio>> BuscarAsync(string texto) =>
        ConexionBD.ConsultarAsync(
            SelectBase + " WHERE Activo = 1 AND (Nombre LIKE @texto OR DNI LIKE @texto) ORDER BY Nombre",
            Mapear,
            new SqlParameter("@texto", $"%{texto}%"));

    public async Task<Socio?> ObtenerPorIdAsync(int socioId)
    {
        var lista = await ConexionBD.ConsultarAsync(
            SelectBase + " WHERE SocioId = @id", Mapear,
            new SqlParameter("@id", socioId));
        return lista.FirstOrDefault();
    }

    public async Task<bool> ExisteDniAsync(string dni, int excluirSocioId)
    {
        var total = await ConexionBD.ContarAsync(
            "SELECT COUNT(1) FROM Socios WHERE DNI = @dni AND SocioId <> @id",
            new SqlParameter("@dni", dni),
            new SqlParameter("@id", excluirSocioId));
        return total > 0;
    }

    public Task<int> ContarLibrosPendientesAsync(int socioId) =>
        ConexionBD.ContarAsync(@"
            SELECT COUNT(1)
            FROM Prestamos p
            INNER JOIN DetallePrestamo d ON d.PrestamoId = p.PrestamoId
            WHERE p.SocioId = @id AND d.FechaDevolucion IS NULL",
            new SqlParameter("@id", socioId));

    public async Task<int> InsertarAsync(Socio socio)
    {
        await using var cn = ConexionBD.Crear();
        await using var cmd = new SqlCommand(@"
            INSERT INTO Socios (DNI, Nombre, Email)
            OUTPUT INSERTED.SocioId
            VALUES (@dni, @nombre, @email)", cn);
        cmd.Parameters.AddWithValue("@dni", socio.DNI);
        cmd.Parameters.AddWithValue("@nombre", socio.Nombre);
        cmd.Parameters.AddWithValue("@email", (object?)socio.Email ?? DBNull.Value);
        await cn.OpenAsync();
        return Convert.ToInt32(await cmd.ExecuteScalarAsync());
    }

    public async Task<bool> ActualizarAsync(Socio socio)
    {
        var filas = await ConexionBD.EjecutarAsync(@"
            UPDATE Socios
            SET DNI = @dni, Nombre = @nombre, Email = @email
            WHERE SocioId = @id",
            new SqlParameter("@dni", socio.DNI),
            new SqlParameter("@nombre", socio.Nombre),
            new SqlParameter("@email", (object?)socio.Email ?? DBNull.Value),
            new SqlParameter("@id", socio.SocioId));
        return filas > 0;
    }

    // Eliminación lógica: nunca DELETE físico
    public async Task<bool> DesactivarAsync(int socioId)
    {
        var filas = await ConexionBD.EjecutarAsync(
            "UPDATE Socios SET Activo = 0 WHERE SocioId = @id",
            new SqlParameter("@id", socioId));
        return filas > 0;
    }
}