namespace Application.DTOs;

/// <summary>
/// Opcional na baixa. Centro de custo fica na duplicata; aqui só o plano de contas da parcela pode ser informado.
/// </summary>
public class BaixarParcelaDto
{
    public int? PlanoContasId { get; set; }
    /// <summary>Data em que o pagamento/recebimento ocorreu (pode ser retroativa).</summary>
    public DateTime? DataPagamento { get; set; }
}
