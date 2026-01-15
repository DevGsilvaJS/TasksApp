using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class CadastroDuplicataDto
{
    [Required(ErrorMessage = "Número é obrigatório")]
    public int Numero { get; set; }

    [Required(ErrorMessage = "Data de emissão é obrigatória")]
    public DateTime DataEmissao { get; set; }

    [Required(ErrorMessage = "Número de parcelas é obrigatório")]
    [Range(1, int.MaxValue, ErrorMessage = "Número de parcelas deve ser maior que zero")]
    public int NumeroParcelas { get; set; }

    [Required(ErrorMessage = "Valor total é obrigatório")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Valor total deve ser maior que zero")]
    public double ValorTotal { get; set; }

    public double? Multa { get; set; }

    public double? Juros { get; set; }

    [MaxLength(500)]
    public string? DescricaoDespesa { get; set; }

    // Se não fornecido, será usado para gerar automaticamente
    public DateTime? DataPrimeiroVencimento { get; set; }

    // Lista opcional de parcelas com datas personalizadas
    public List<CadastroParcelaDto>? Parcelas { get; set; }
}
