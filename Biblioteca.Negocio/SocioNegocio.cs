using System.Net.Mail;
using System.Text.RegularExpressions;
using Biblioteca.Datos;
using Biblioteca.Entidades;

namespace Biblioteca.Negocio;

public class SocioNegocio : NegocioBase
{
    private readonly SocioDatos _socios = new();

    public Task<List<Socio>> ListarAsync() =>
        ProtegerAsync(() => _socios.ListarAsync());

    public Task<List<Socio>> BuscarAsync(string? texto) =>
        ProtegerAsync(() => string.IsNullOrWhiteSpace(texto)
            ? _socios.ListarAsync()
            : _socios.BuscarAsync(texto.Trim()));

    public Task<int> InsertarAsync(Socio socio) => ProtegerAsync(async () =>
    {
        Validar(socio);

        if (await _socios.ExisteDniAsync(socio.DNI, 0))
            throw new ReglaNegocioException($"Ya existe un socio registrado con el DNI {socio.DNI}.");

        return await _socios.InsertarAsync(socio);
    });

    public async Task ActualizarAsync(Socio socio)
    {
        await ProtegerAsync(async () =>
        {
            Validar(socio);

            var actual = await _socios.ObtenerPorIdAsync(socio.SocioId);
            if (actual is null || !actual.Activo)
                throw new ReglaNegocioException("El socio que intenta actualizar no existe o está dado de baja.");

            if (await _socios.ExisteDniAsync(socio.DNI, socio.SocioId))
                throw new ReglaNegocioException($"Ya existe otro socio registrado con el DNI {socio.DNI}.");

            await _socios.ActualizarAsync(socio);
            return true;
        });
    }

    /// <summary>Eliminación lógica (Activo = 0).</summary>
    public async Task EliminarAsync(int socioId)
    {
        await ProtegerAsync(async () =>
        {
            var socio = await _socios.ObtenerPorIdAsync(socioId)
                ?? throw new ReglaNegocioException("El socio seleccionado no existe.");

            if (!socio.Activo)
                throw new ReglaNegocioException($"El socio {socio.Nombre} ya está dado de baja.");

            var pendientes = await _socios.ContarLibrosPendientesAsync(socioId);
            if (pendientes > 0)
                throw new ReglaNegocioException(
                    $"No se puede dar de baja a {socio.Nombre} porque tiene {pendientes} libro(s) pendiente(s) de devolución.");

            await _socios.DesactivarAsync(socioId);
            return true;
        });
    }

    private static void Validar(Socio socio)
    {
        socio.DNI = socio.DNI?.Trim() ?? string.Empty;
        socio.Nombre = socio.Nombre?.Trim() ?? string.Empty;
        socio.Email = string.IsNullOrWhiteSpace(socio.Email) ? null : socio.Email.Trim();

        if (!Regex.IsMatch(socio.DNI, @"^\d{8}$"))
            throw new ReglaNegocioException("El DNI debe tener exactamente 8 dígitos numéricos.");
        if (socio.Nombre.Length == 0)
            throw new ReglaNegocioException("El nombre del socio es obligatorio.");
        if (socio.Email is not null && !EmailValido(socio.Email))
            throw new ReglaNegocioException("El correo electrónico no tiene un formato válido.");
    }

    private static bool EmailValido(string email)
    {
        try
        {
            return new MailAddress(email).Address == email;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}