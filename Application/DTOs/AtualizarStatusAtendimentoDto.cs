namespace Application.DTOs;

/// <summary>
/// DTO para atualizar status de atendimento comercial do possível cliente (1 a 9).
/// </summary>
public class AtualizarStatusAtendimentoDto
{
    public int StatusAtendimento { get; set; }
    public string? MotivoPerda { get; set; }
}
