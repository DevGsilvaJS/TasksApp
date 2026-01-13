using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class CadastroParcelaDto
{
    [Required(ErrorMessage = "Número da parcela é obrigatório")]
    public int NumeroParcela { get; set; }

    [Required(ErrorMessage = "Valor é obrigatório")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Valor deve ser maior que zero")]
    public double Valor { get; set; }

    [Required(ErrorMessage = "Data de vencimento é obrigatória")]
    public DateTime Vencimento { get; set; }

    public double? Multa { get; set; }

    public double? Juros { get; set; }
}
