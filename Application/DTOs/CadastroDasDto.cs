using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Application.DTOs;

public class CadastroDasDto
{
    [MaxLength(50)]
    public string? Referencia { get; set; }

    public DateTime? DataVencimento { get; set; }

    public StatusDas Status { get; set; } = StatusDas.Pendente;
}
