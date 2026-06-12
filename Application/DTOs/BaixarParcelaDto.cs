namespace Application.DTOs;

/// <summary>
/// Opcional na baixa. Centro de custo fica na duplicata; aqui só o plano de contas da parcela pode ser informado.
/// </summary>
public class BaixarParcelaDto
{
    public int? PlanoContasId { get; set; }
}
