namespace Application.DTOs;

public class AnotacaoResponseDto
{
    public int AnotacaoId { get; set; }
    public int TarefaId { get; set; }
    public int UsuarioId { get; set; }
    public string UsuarioNome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public DateTime? DataCadastro { get; set; }
    public string DescricaoFormatada { get; set; } = string.Empty;
}
