namespace Application.DTOs;

public class RelatorioGerencialResponseDto
{
    public string TipoRelatorio { get; set; } = string.Empty;
    public string TituloRelatorio { get; set; } = string.Empty;
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public List<RelatorioGerencialLinhaDto> Itens { get; set; } = new();
    public decimal TotalValor { get; set; }
    public int TotalRegistros { get; set; }
}

public class RelatorioGerencialLinhaDto
{
    public int ParcelaId { get; set; }
    public int DuplicataId { get; set; }
    public int NumeroDuplicata { get; set; }
    public int NumeroParcela { get; set; }
    public string? DescricaoDespesa { get; set; }
    public string? ClienteNome { get; set; }
    public DateTime DataEmissao { get; set; }
    public DateTime DataVencimento { get; set; }
    public DateTime? DataPagamento { get; set; }
    public decimal Valor { get; set; }
    public decimal Multa { get; set; }
    public decimal Juros { get; set; }
    public decimal ValorTotal { get; set; }
    public string Status { get; set; } = string.Empty;
}
