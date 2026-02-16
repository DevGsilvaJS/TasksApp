namespace Application.DTOs;

/// <summary>
/// Item do card de notas de serviço do mês (pendente ou já enviado).
/// </summary>
public class NotaServicoItemDto
{
    public int ClienteId { get; set; }
    public int ClienteCodigo { get; set; }
    public string Fantasia { get; set; } = string.Empty;
    /// <summary>Dia do mês (1-31) da NF de serviço.</summary>
    public int DiaNfServico { get; set; }
    public int Ano { get; set; }
    public int Mes { get; set; }
    public bool Enviado { get; set; }
    public DateTime? DataEnvio { get; set; }
    /// <summary>Id do registro de envio (para atualizar ao marcar enviado).</summary>
    public int? EnvioNotaServicoId { get; set; }
}
