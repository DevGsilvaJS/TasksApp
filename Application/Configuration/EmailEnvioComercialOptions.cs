namespace Application.Configuration;

public class EmailEnvioComercialOptions
{
    public const string Secao = "EmailEnvioComercial";

    public string SmtpHost { get; set; } = "email-ssl.com.br";
    public int SmtpPort { get; set; } = 587;
    public int PausaSegundosAposParRemetentes { get; set; } = 30;
    public string PastaAnexosCampanha { get; set; } = "EmailAssets/campanhas";
    public string PastaAssinaturas { get; set; } = "Planilha";
    public List<RemetenteEmailOptions> Remetentes { get; set; } = [];
}

public class RemetenteEmailOptions
{
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string AssinaturaArquivo { get; set; } = string.Empty;
}
