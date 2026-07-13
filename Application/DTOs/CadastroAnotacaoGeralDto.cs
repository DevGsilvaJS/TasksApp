using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class CadastroAnotacaoGeralDto
{
    [Required(ErrorMessage = "Descrição é obrigatória")]
    [MaxLength(1000)]
    public string Descricao { get; set; } = string.Empty;

    [MaxLength(3000)]
    public string? Observacoes { get; set; }

    [MaxLength(500)]
    public string? Link { get; set; }

    /// <summary>ANOTACAO (padrão) ou REGRA_EMPRESA.</summary>
    [MaxLength(30)]
    public string Tipo { get; set; } = "ANOTACAO";
}
