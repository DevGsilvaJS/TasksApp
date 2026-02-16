using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class CadastroAnotacaoClienteDto
{
    [Required(ErrorMessage = "Cliente é obrigatório")]
    public int ClienteId { get; set; }

    [MaxLength(3000)]
    public string? Descricao { get; set; }
}
