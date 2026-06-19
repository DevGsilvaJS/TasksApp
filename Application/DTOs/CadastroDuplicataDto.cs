using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Application.DTOs;
public class CadastroDuplicataDto
{
    // Número é opcional - se não informado, será gerado automaticamente
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

    [Required(ErrorMessage = "Tipo é obrigatório")]
    [MaxLength(2)]
    public string Tipo { get; set; } = "CP"; // CP = Contas a Pagar, CR = Contas a Receber

    /// <summary>Cliente (obrigatório em Contas a Receber).</summary>
    public int? ClienteId { get; set; }

    /// <summary>Centro de custo (MEI/empresa) para fluxo de caixa.</summary>
    [Required(ErrorMessage = "Centro de custo é obrigatório")]
    public int? EmpresaId { get; set; }

    /// <summary>Plano de contas (obrigatório em CR; opcional em CP — sem plano não entra no fluxo de caixa).</summary>
    public int? PlanoContasId { get; set; }

    // Se não fornecido, será usado para gerar automaticamente
    public DateTime? DataPrimeiroVencimento { get; set; }

    // Lista opcional de parcelas com datas personalizadas
    public List<CadastroParcelaDto>? Parcelas { get; set; }

    /// <summary>Inativa contas a pagar (CP). Título inativo não aparece na listagem nem no dashboard.</summary>
    [JsonPropertyName("inativa")]
    public bool Inativa { get; set; }}
