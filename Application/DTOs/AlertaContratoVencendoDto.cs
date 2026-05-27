namespace Application.DTOs;

public class AlertaContratoVencendoDto
{
    public int ClienteId { get; set; }
    public string ClienteCodigo { get; set; } = string.Empty;
    public string ClienteNome { get; set; } = string.Empty;
    public DateTime DataFimVigencia { get; set; }
    public int DiasParaVencer { get; set; }
    public decimal ValorMensalVigente { get; set; }
}

