using Biblioteca.Entidades;

namespace Biblioteca.Datos.Interfaces;

public interface ILibroRepositorio
{
    Task<List<Libro>> ListarAsync();
    Task<List<Libro>> BuscarAsync(string texto);
    Task<Libro?> ObtenerPorIdAsync(int libroId);
    Task<bool> ExisteIsbnAsync(string isbn, int excluirLibroId);
    Task<bool> TienePrestamosPendientesAsync(int libroId);
    Task<int> InsertarAsync(Libro libro);
    Task<bool> ActualizarAsync(Libro libro);
    Task<bool> DesactivarAsync(int libroId);
}