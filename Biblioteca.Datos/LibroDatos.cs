using Biblioteca.Entidades;
using Microsoft.Data.SqlClient;
using Biblioteca.Datos.Interfaces;

namespace Biblioteca.Datos;

public class LibroDatos : ILibroRepositorio
{
    private const string SelectBase = @"
        SELECT l.LibroId, l.Titulo, l.ISBN, l.AutorId, a.Nombre AS AutorNombre, l.Ejemplares, l.Activo
        FROM Libros l
        INNER JOIN Autores a ON a.AutorId = l.AutorId";

    private static Libro Mapear(SqlDataReader dr) => new()
    {
        LibroId = (int)dr["LibroId"],
        Titulo = (string)dr["Titulo"],
        ISBN = (string)dr["ISBN"],
        AutorId = (int)dr["AutorId"],
        AutorNombre = (string)dr["AutorNombre"],
        Ejemplares = (int)dr["Ejemplares"],
        Activo = (bool)dr["Activo"]
    };

    public Task<List<Libro>> ListarAsync() =>
        ConexionBD.ConsultarAsync(SelectBase + " WHERE l.Activo = 1 ORDER BY l.Titulo", Mapear);

    public Task<List<Libro>> BuscarAsync(string texto) =>
        ConexionBD.ConsultarAsync(
            SelectBase + @" WHERE l.Activo = 1
                            AND (l.Titulo LIKE @texto OR a.Nombre LIKE @texto)
                            ORDER BY l.Titulo",
            Mapear,
            new SqlParameter("@texto", $"%{texto}%"));

    public async Task<Libro?> ObtenerPorIdAsync(int libroId)
    {
        var lista = await ConexionBD.ConsultarAsync(
            SelectBase + " WHERE l.LibroId = @id", Mapear,
            new SqlParameter("@id", libroId));
        return lista.FirstOrDefault();
    }

    public async Task<bool> ExisteIsbnAsync(string isbn, int excluirLibroId)
    {
        var total = await ConexionBD.ContarAsync(
            "SELECT COUNT(1) FROM Libros WHERE ISBN = @isbn AND LibroId <> @id",
            new SqlParameter("@isbn", isbn),
            new SqlParameter("@id", excluirLibroId));
        return total > 0;
    }

    public async Task<bool> TienePrestamosPendientesAsync(int libroId)
    {
        var total = await ConexionBD.ContarAsync(
            "SELECT COUNT(1) FROM DetallePrestamo WHERE LibroId = @id AND FechaDevolucion IS NULL",
            new SqlParameter("@id", libroId));
        return total > 0;
    }

    public async Task<int> InsertarAsync(Libro libro)
    {
        await using var cn = ConexionBD.Crear();
        await using var cmd = new SqlCommand(@"
            INSERT INTO Libros (Titulo, ISBN, AutorId, Ejemplares)
            OUTPUT INSERTED.LibroId
            VALUES (@titulo, @isbn, @autor, @ejemplares)", cn);
        cmd.Parameters.AddWithValue("@titulo", libro.Titulo);
        cmd.Parameters.AddWithValue("@isbn", libro.ISBN);
        cmd.Parameters.AddWithValue("@autor", libro.AutorId);
        cmd.Parameters.AddWithValue("@ejemplares", libro.Ejemplares);
        await cn.OpenAsync();
        return Convert.ToInt32(await cmd.ExecuteScalarAsync());
    }

    public async Task<bool> ActualizarAsync(Libro libro)
    {
        var filas = await ConexionBD.EjecutarAsync(@"
            UPDATE Libros
            SET Titulo = @titulo, ISBN = @isbn, AutorId = @autor, Ejemplares = @ejemplares
            WHERE LibroId = @id",
            new SqlParameter("@titulo", libro.Titulo),
            new SqlParameter("@isbn", libro.ISBN),
            new SqlParameter("@autor", libro.AutorId),
            new SqlParameter("@ejemplares", libro.Ejemplares),
            new SqlParameter("@id", libro.LibroId));
        return filas > 0;
    }

    // Eliminación lógica: nunca DELETE físico
    public async Task<bool> DesactivarAsync(int libroId)
    {
        var filas = await ConexionBD.EjecutarAsync(
            "UPDATE Libros SET Activo = 0 WHERE LibroId = @id",
            new SqlParameter("@id", libroId));
        return filas > 0;
    }
}