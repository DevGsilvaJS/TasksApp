namespace Application.DTOs;

public class DuplicataResponseDto
{
    public int DuplicataId { get; set; }
    public int Numero { get; set; }
    public DateTime DataEmissao { get; set; }
    public int NumeroParcelas { get; set; }
    public string? DescricaoDespesa { get; set; }
    public string Tipo { get; set; } = "CP"; // CP = Contas a Pagar, CR = Contas a Receber
    public int? ClienteId { get; set; }
    public string? ClienteNome { get; set; }
    public int? CentroCustoId { get; set; }
    public string? CentroCustoEmpresaFantasia { get; set; }
    public int? PlanoContasId { get; set; }
    public string? PlanoContasDescricao { get; set; }
    public List<ParcelaResponseDto> Parcelas { get; set; } = new();
    public double ValorTotal { get; set; }
    public double ValorPago { get; set; }
    public double ValorPendente { get; set; }
}
