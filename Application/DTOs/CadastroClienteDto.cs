using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Application.DTOs;

public class CadastroClienteDto
{
    [Required(ErrorMessage = "Fantasia é obrigatório")]
    [MaxLength(255)]
    public string Fantasia { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? DocFederal { get; set; }

    [MaxLength(255)]
    public string? DocEstadual { get; set; }

    [Required(ErrorMessage = "Código do cliente é obrigatório")]
    public int Codigo { get; set; }

    [Required(ErrorMessage = "Usuário é obrigatório")]
    public int UsuarioId { get; set; }

    public decimal? ValorContrato { get; set; }

    public DateTime? DataFinalContrato { get; set; }

    public int? DiaPagamento { get; set; }

    /// <summary>
    /// Dia do mês (1-31) em que vence a NF de serviço. Mensal.
    /// </summary>
    public int? DiaNfServico { get; set; }

    /// <summary>
    /// E-mails do cliente (armazenados na tabela TB_EMAIL via Pessoa).
    /// </summary>
    public List<string>? Emails { get; set; }

    public StatusCliente Status { get; set; } = StatusCliente.Ativo;
}
