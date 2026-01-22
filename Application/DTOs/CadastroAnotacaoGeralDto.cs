using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class CadastroAnotacaoGeralDto
{
    [Required(ErrorMessage = "Descrição é obrigatória")]
    [MaxLength(1000)]
    public string Descricao { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Link { get; set; }
}
