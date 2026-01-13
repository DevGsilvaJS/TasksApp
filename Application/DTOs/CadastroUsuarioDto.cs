using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class CadastroUsuarioDto
{
    [Required(ErrorMessage = "Nome é obrigatório")]
    [MaxLength(255)]
    public string Nome { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Sobrenome { get; set; }

    [MaxLength(255)]
    public string? DocFederal { get; set; }

    [MaxLength(255)]
    public string? DocEstadual { get; set; }

    [Required(ErrorMessage = "Login é obrigatório")]
    [MaxLength(255)]
    public string Login { get; set; } = string.Empty;

    [Required(ErrorMessage = "Senha é obrigatória")]
    [MinLength(6, ErrorMessage = "Senha deve ter no mínimo 6 caracteres")]
    [MaxLength(255)]
    public string Senha { get; set; } = string.Empty;
}
