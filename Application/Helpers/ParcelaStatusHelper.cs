namespace Application.Helpers;

public static class ParcelaStatusHelper
{
    public static bool IsPendente(string? status) =>
        string.Equals(status, "Pendente", StringComparison.OrdinalIgnoreCase);

    public static bool IsPaga(string? status) =>
        string.Equals(status, "Paga", StringComparison.OrdinalIgnoreCase);

    public static bool IsCancelada(string? status) =>
        string.Equals(status, "Cancelada", StringComparison.OrdinalIgnoreCase);
}
