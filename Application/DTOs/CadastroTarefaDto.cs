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

    public StatusTarefa Status { get; set; } = StatusTarefa.EmAberto;

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

    public TipoAtendimento? TipoAtendimento { get; set; }

    public PrioridadeTarefa Prioridade { get; set; } = PrioridadeTarefa.Media;

    public List<IFormFile>? Imagens { get; set; }
}
