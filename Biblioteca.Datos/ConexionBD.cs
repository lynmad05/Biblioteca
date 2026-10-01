using System.Configuration;
using Microsoft.Data.SqlClient;

namespace Biblioteca.Datos;

internal static class ConexionBD
{
    public static SqlConnection Crear()
    {
        var cadena = ConfigurationManager.ConnectionStrings["BibliotecaDB"]?.ConnectionString;
        if (string.IsNullOrWhiteSpace(cadena))
            throw new InvalidOperationException(
                "No se encontró la cadena de conexión 'BibliotecaDB' en el App.config.");
        return new SqlConnection(cadena);
    }

    public static async Task<List<T>> ConsultarAsync<T>(
        string sql, Func<SqlDataReader, T> mapear, params SqlParameter[] parametros)
    {
        var lista = new List<T>();
        await using var cn = Crear();
        await using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddRange(parametros);
        await cn.OpenAsync();
        await using var dr = await cmd.ExecuteReaderAsync();
        while (await dr.ReadAsync())
            lista.Add(mapear(dr));
        return lista;
    }

    public static async Task<int> ContarAsync(string sql, params SqlParameter[] parametros)
    {
        await using var cn = Crear();
        await using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddRange(parametros);
        await cn.OpenAsync();
        return Convert.ToInt32(await cmd.ExecuteScalarAsync());
    }

    public static async Task<int> EjecutarAsync(string sql, params SqlParameter[] parametros)
    {
        await using var cn = Crear();
        await using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddRange(parametros);
        await cn.OpenAsync();
        return await cmd.ExecuteNonQueryAsync();
    }
}