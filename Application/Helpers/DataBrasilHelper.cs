namespace Application.Helpers;

/// <summary>
/// Comparações de data alinhadas ao calendário do Brasil (UTC-3).
/// </summary>
public static class DataBrasilHelper
{
    private static readonly TimeZoneInfo FusoBrasil = ObterFusoBrasil();

    public static DateOnly ParaDataCalendarioBrasil(DateTime valor)
    {
        var utc = NormalizarParaUtc(valor);
        var local = TimeZoneInfo.ConvertTimeFromUtc(utc, FusoBrasil);
        return DateOnly.FromDateTime(local);
    }

    public static bool NoPeriodoInclusive(DateTime data, DateOnly inicio, DateOnly fim)
    {
        var dia = ParaDataCalendarioBrasil(data);
        return dia >= inicio && dia <= fim;
    }

    public static DateOnly ParseDataConsulta(DateTime valor) => DateOnly.FromDateTime(valor.Date);

    private static DateTime NormalizarParaUtc(DateTime valor)
    {
        return valor.Kind switch
        {
            DateTimeKind.Utc => valor,
            DateTimeKind.Local => valor.ToUniversalTime(),
            _ => DateTime.SpecifyKind(valor, DateTimeKind.Utc)
        };
    }

    private static TimeZoneInfo ObterFusoBrasil()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(
                OperatingSystem.IsWindows() ? "E. South America Standard Time" : "America/Sao_Paulo");
        }
        catch
        {
            return TimeZoneInfo.CreateCustomTimeZone("Brazil", TimeSpan.FromHours(-3), "Brazil", "Brazil");
        }
    }
}
