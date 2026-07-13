namespace Application.DTOs;

public class ParcelaResponseDto
{
    public int ParcelaId { get; set; }
    public int DuplicataId { get; set; }
    public int NumeroParcela { get; set; }
    public double Valor { get; set; }
    public double Multa { get; set; }
    public double Juros { get; set; }
    public double ValorTotal { get; set; }
    public DateTime Vencimento { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool CongeladaPorCliente { get; set; }
    public DateTime? DataPagamento { get; set; }
    public int? EmpresaId { get; set; }
    public string? CentroCustoDescricao { get; set; }
    public int? PlanoContasId { get; set; }
    public string? PlanoContasDescricao { get; set; }
}
