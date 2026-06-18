namespace Application.Services;

public static class EmailConteudoNormalizador
{
    public static string NormalizarAssunto(string assunto) =>
        NormalizarEspacos(assunto).Trim();

    public static string NormalizarCorpoHtml(string corpoHtml) =>
        NormalizarEspacos(corpoHtml ?? string.Empty);

    private static string NormalizarEspacos(string texto) =>
        texto
            .Replace("&nbsp;", " ", StringComparison.OrdinalIgnoreCase)
            .Replace('\u00A0', ' ');
}
