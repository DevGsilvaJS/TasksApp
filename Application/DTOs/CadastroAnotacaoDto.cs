using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class CadastroAnotacaoDto
{
    [Required(ErrorMessage = "Tarefa é obrigatória")]
    public int TarefaId { get; set; }

    [Required(ErrorMessage = "Usuário é obrigatório")]
    public int UsuarioId { get; set; }

    [Required(ErrorMessage = "Descrição é obrigatória")]
    [MaxLength(255)]
    public string Descricao { get; set; } = string.Empty;
}
