namespace Application.DTOs;

public class AtualizarClassificacaoParcelaDto
{
    public int EmpresaId { get; set; }

    /// <summary>Opcional em CP. Obrigatório em CR (ou padrão RECEITA DE CONSULTORIA).</summary>
    public int? PlanoContasId { get; set; }
}
