using Domain.Enums;

namespace Application.DTOs;

public class ClienteResponseDto
{
    public int ClienteId { get; set; }
    public int PessoaId { get; set; }
    public string Fantasia { get; set; } = string.Empty;
    public string? DocFederal { get; set; }
    public string? DocEstadual { get; set; }
    public int Codigo { get; set; }
    public int UsuarioId { get; set; }
    public string UsuarioNome { get; set; } = string.Empty;
    public DateTime? DataCadastro { get; set; }
    public decimal? ValorContrato { get; set; }
    public DateTime? DataFinalContrato { get; set; }
    public int? DiaPagamento { get; set; }
    public StatusCliente Status { get; set; }
    public string StatusDescricao { get; set; } = string.Empty;
}
