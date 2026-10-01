using Biblioteca.Datos;
using Biblioteca.Entidades;
using Biblioteca.Datos.Interfaces;

namespace Biblioteca.Negocio;

public class LibroNegocio : NegocioBase
{
    private readonly ILibroRepositorio _libros;
    private readonly AutorDatos _autores = new();

    public LibroNegocio() : this(new LibroDatos()) { }

    public LibroNegocio(ILibroRepositorio libros)
    {
        _libros = libros;
    }

    public Task<List<Libro>> ListarAsync() =>
        ProtegerAsync(() => _libros.ListarAsync());

    public Task<List<Libro>> BuscarAsync(string? texto) =>
        ProtegerAsync(() => string.IsNullOrWhiteSpace(texto)
            ? _libros.ListarAsync()
            : _libros.BuscarAsync(texto.Trim()));

    public Task<List<Autor>> ListarAutoresAsync() =>
        ProtegerAsync(() => _autores.ListarAsync());

    public Task<int> InsertarAsync(Libro libro) => ProtegerAsync(async () =>
    {
        Validar(libro);

        if (await _libros.ExisteIsbnAsync(libro.ISBN, 0))
            throw new ReglaNegocioException($"Ya existe un libro registrado con el ISBN {libro.ISBN}.");

        return await _libros.InsertarAsync(libro);
    });

    public async Task ActualizarAsync(Libro libro)
    {
        await ProtegerAsync(async () =>
        {
            Validar(libro);

            var actual = await _libros.ObtenerPorIdAsync(libro.LibroId);
            if (actual is null || !actual.Activo)
                throw new ReglaNegocioException("El libro que intenta actualizar no existe o está dado de baja.");

            if (await _libros.ExisteIsbnAsync(libro.ISBN, libro.LibroId))
                throw new ReglaNegocioException($"Ya existe otro libro registrado con el ISBN {libro.ISBN}.");

            await _libros.ActualizarAsync(libro);
            return true;
        });
    }

    /// <summary>Eliminación lógica (Activo = 0).</summary>
    public async Task EliminarAsync(int libroId)
    {
        await ProtegerAsync(async () =>
        {
            var libro = await _libros.ObtenerPorIdAsync(libroId)
                ?? throw new ReglaNegocioException("El libro seleccionado no existe.");

            if (!libro.Activo)
                throw new ReglaNegocioException($"El libro '{libro.Titulo}' ya está dado de baja.");

            if (await _libros.TienePrestamosPendientesAsync(libroId))
                throw new ReglaNegocioException(
                    $"No se puede dar de baja '{libro.Titulo}' porque tiene préstamos pendientes.");

            await _libros.DesactivarAsync(libroId);
            return true;
        });
    }

    private static void Validar(Libro libro)
    {
        libro.Titulo = libro.Titulo?.Trim() ?? string.Empty;
        libro.ISBN = libro.ISBN?.Trim() ?? string.Empty;

        if (libro.Titulo.Length == 0)
            throw new ReglaNegocioException("El título del libro es obligatorio.");
        if (libro.ISBN.Length == 0)
            throw new ReglaNegocioException("El ISBN es obligatorio.");
        if (libro.ISBN.Length > 20)
            throw new ReglaNegocioException("El ISBN no puede tener más de 20 caracteres.");
        if (libro.AutorId <= 0)
            throw new ReglaNegocioException("Seleccione un autor.");
        if (libro.Ejemplares < 0)
            throw new ReglaNegocioException("La cantidad de ejemplares no puede ser negativa.");
    }
}