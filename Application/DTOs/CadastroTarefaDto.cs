using System.ComponentModel.DataAnnotations;
using Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace Application.DTOs;

public class CadastroTarefaDto
{
    [Required(ErrorMessage = "Cliente é obrigatório")]
    public int ClienteId { get; set; }

    [Required(ErrorMessage = "Usuário é obrigatório")]
    public int UsuarioId { get; set; }

    /// <summary>
    /// Id do status cadastrado em "Parâmetros de atendimentos" (TB_CAD_STATUS_TAREFA).
    /// Mantém compatibilidade com os valores antigos (1..5).
    /// </summary>
    public int Status { get; set; } = (int)StatusTarefa.EmAberto;

    public DateTime? DataConclusao { get; set; }

    public string? Descricao { get; set; }

    [MaxLength(255)]
    public string? Titulo { get; set; }

    [MaxLength(6)]
    public string? Protocolo { get; set; }

    [MaxLength(255)]
    public string? Solicitante { get; set; }

    [MaxLength(20)]
    public string? CelularSolicitante { get; set; }

    /// <summary>
    /// Id do andamento cadastrado em "Parâmetros de atendimentos" (TB_CAD_ANDAMENTO).
    /// </summary>
    public int Andamento { get; set; } = (int)AndamentoTarefa.AFazer;

    /// <summary>
    /// Id do tipo de atendimento cadastrado (TB_CAD_TIPO_ATENDIMENTO).
    /// </summary>
    public int? TipoAtendimento { get; set; }

    public PrioridadeTarefa Prioridade { get; set; } = PrioridadeTarefa.Media;

    /// <summary>
    /// Id do tipo de contato cadastrado (TB_CAD_TIPO_CONTATO).
    /// </summary>
    public int? TipoContato { get; set; }

    public List<IFormFile>? Imagens { get; set; }
}
