using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class LoginDto
{
    [Required(ErrorMessage = "Login é obrigatório")]
    public string Login { get; set; } = string.Empty;

    [Required(ErrorMessage = "Senha é obrigatória")]
    public string Senha { get; set; } = string.Empty;
}
