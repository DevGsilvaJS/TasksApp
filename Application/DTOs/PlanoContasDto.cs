using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class CadastroPlanoContasDto
{
    [Required(ErrorMessage = "Descrição é obrigatória")]
    [MaxLength(500)]
    public string Descricao { get; set; } = string.Empty;
}

public class PlanoContasResponseDto
{
    public int PlanoContasId { get; set; }
    public string Descricao { get; set; } = string.Empty;
}
