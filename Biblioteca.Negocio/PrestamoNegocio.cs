using Biblioteca.Datos;
using Biblioteca.Entidades;

namespace Biblioteca.Negocio;

public class PrestamoNegocio : NegocioBase
{
    // Reglas definidas en la capa de Negocio
    public const int MaxLibrosPendientes = 3;
    public const int DiasPrestamo = 7;
    public const decimal MultaPorDia = 1.50m;   // S/ por día de retraso

    private readonly PrestamoDatos _prestamos = new();
    private readonly SocioDatos _socios = new();
    private readonly LibroDatos _libros = new();

    public static int CalcularDiasRetraso(DateTime fechaLimite, DateTime fechaDevolucion) =>
        Math.Max(0, (fechaDevolucion.Date - fechaLimite.Date).Days);

    public static decimal CalcularMulta(DateTime fechaLimite, DateTime fechaDevolucion) =>
        CalcularDiasRetraso(fechaLimite, fechaDevolucion) * MultaPorDia;

    public Task<int> ContarLibrosPendientesAsync(int socioId) =>
        ProtegerAsync(() => _socios.ContarLibrosPendientesAsync(socioId));

    public Task<List<PrestamoReporte>> ListarPendientesPorSocioAsync(int socioId) =>
        ProtegerAsync(() => _prestamos.ListarPendientesPorSocioAsync(socioId));

    /// <summary>Registra un préstamo con uno o varios libros (una sola transacción en Datos).</summary>
    public Task<int> RegistrarPrestamoAsync(int socioId, IReadOnlyList<int> libroIds) => ProtegerAsync(async () =>
    {
        if (socioId <= 0)
            throw new ReglaNegocioException("Seleccione un socio.");
        if (libroIds is null || libroIds.Count == 0)
            throw new ReglaNegocioException("Agregue al menos un libro al préstamo.");
        if (libroIds.Distinct().Count() != libroIds.Count)
            throw new ReglaNegocioException("No puede repetir el mismo libro en un préstamo.");

        var socio = await _socios.ObtenerPorIdAsync(socioId)
            ?? throw new ReglaNegocioException("El socio seleccionado no existe.");
        if (!socio.Activo)
            throw new ReglaNegocioException($"El socio {socio.Nombre} está dado de baja y no puede solicitar préstamos.");

        var pendientes = await _socios.ContarLibrosPendientesAsync(socioId);
        if (pendientes + libroIds.Count > MaxLibrosPendientes)
        {
            var disponibles = Math.Max(0, MaxLibrosPendientes - pendientes);
            throw new ReglaNegocioException(
                $"{socio.Nombre} tiene {pendientes} libro(s) pendiente(s) y el máximo permitido es {MaxLibrosPendientes}. " +
                $"Solo puede llevar {disponibles} libro(s) más.");
        }

        var prestamo = new Prestamo
        {
            SocioId = socioId,
            FechaPrestamo = DateTime.Today,
            FechaLimite = DateTime.Today.AddDays(DiasPrestamo),
            Estado = "Pendiente"
        };

        foreach (var libroId in libroIds)
        {
            var libro = await _libros.ObtenerPorIdAsync(libroId)
                ?? throw new ReglaNegocioException("Uno de los libros seleccionados no existe.");
            if (!libro.Activo)
                throw new ReglaNegocioException($"El libro '{libro.Titulo}' está dado de baja y no se puede prestar.");
            if (libro.Ejemplares <= 0)
                throw new ReglaNegocioException($"No hay ejemplares disponibles de '{libro.Titulo}'.");

            prestamo.Detalles.Add(new DetallePrestamo { LibroId = libroId });
        }

        return await _prestamos.RegistrarAsync(prestamo);
    });

    /// <summary>Registra la devolución de un libro y calcula la multa por retraso.</summary>
    public Task<ResultadoDevolucion> RegistrarDevolucionAsync(int prestamoId, int libroId) => ProtegerAsync(async () =>
    {
        var prestamo = await _prestamos.ObtenerPorIdAsync(prestamoId)
            ?? throw new ReglaNegocioException("El préstamo indicado no existe.");

        var pendientes = await _prestamos.ContarPendientesPrestamoAsync(prestamoId);
        if (pendientes == 0)
            throw new ReglaNegocioException("Este préstamo ya fue devuelto por completo.");

        var fecha = DateTime.Today;
        var cerrarPrestamo = pendientes == 1;   // era el último libro pendiente

        var ok = await _prestamos.RegistrarDevolucionAsync(prestamoId, libroId, fecha, cerrarPrestamo);
        if (!ok)
            throw new ReglaNegocioException("Ese libro ya fue devuelto anteriormente.");

        return new ResultadoDevolucion
        {
            FechaDevolucion = fecha,
            DiasRetraso = CalcularDiasRetraso(prestamo.FechaLimite, fecha),
            Multa = CalcularMulta(prestamo.FechaLimite, fecha),
            PrestamoCerrado = cerrarPrestamo
        };
    });

    public Task<List<PrestamoReporte>> ReporteAsync(DateTime desde, DateTime hasta) => ProtegerAsync(async () =>
    {
        if (desde.Date > hasta.Date)
            throw new ReglaNegocioException("La fecha inicial no puede ser mayor que la fecha final.");

        return await _prestamos.ReporteAsync(desde, hasta);
    });
}