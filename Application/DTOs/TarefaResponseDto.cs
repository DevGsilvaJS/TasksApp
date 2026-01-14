using Domain.Enums;

namespace Application.DTOs;

public class TarefaResponseDto
{
    public int TarefaId { get; set; }
    public int ClienteId { get; set; }
    public string ClienteNome { get; set; } = string.Empty;
    public int UsuarioId { get; set; }
    public string UsuarioNome { get; set; } = string.Empty;
    public DateTime? DataCadastro { get; set; }
    public DateTime? DataConclusao { get; set; }
    public StatusTarefa Status { get; set; }
    public string StatusDescricao { get; set; } = string.Empty;
    public string? Titulo { get; set; }
    public string? Protocolo { get; set; }
    public string? Solicitante { get; set; }
    public string? CelularSolicitante { get; set; }
    public TipoAtendimento? TipoAtendimento { get; set; }
    public string TipoAtendimentoDescricao { get; set; } = string.Empty;
    public PrioridadeTarefa Prioridade { get; set; }
    public string PrioridadeDescricao { get; set; } = string.Empty;
    public List<AnotacaoResponseDto> Anotacoes { get; set; } = new();
    public List<ImagemResponseDto> Imagens { get; set; } = new();
}
