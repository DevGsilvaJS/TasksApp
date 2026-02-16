using Domain.Enums;

namespace Application.DTOs;

public class DasResponseDto
{
    public int DasId { get; set; }
    public string? Referencia { get; set; }
    public DateTime? DataVencimento { get; set; }
    public StatusDas Status { get; set; }
    public string StatusDescricao { get; set; } = string.Empty;
    public DateTime? DataCadastro { get; set; }
}
