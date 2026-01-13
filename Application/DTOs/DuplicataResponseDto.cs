namespace Application.DTOs;

public class DuplicataResponseDto
{
    public int DuplicataId { get; set; }
    public int Numero { get; set; }
    public DateTime DataEmissao { get; set; }
    public int NumeroParcelas { get; set; }
    public List<ParcelaResponseDto> Parcelas { get; set; } = new();
    public double ValorTotal { get; set; }
    public double ValorPago { get; set; }
    public double ValorPendente { get; set; }
}
