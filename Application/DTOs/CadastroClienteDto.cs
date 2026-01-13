using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class CadastroClienteDto
{
    [Required(ErrorMessage = "Fantasia é obrigatório")]
    [MaxLength(255)]
    public string Fantasia { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? DocFederal { get; set; }

    [MaxLength(255)]
    public string? DocEstadual { get; set; }

    [Required(ErrorMessage = "Código do cliente é obrigatório")]
    public int Codigo { get; set; }
}
