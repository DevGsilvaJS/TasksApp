namespace Application.DTOs;

public class AnotacaoGeralResponseDto
{
    public int AnotacaoId { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public string? Link { get; set; }
    public DateTime? DataCadastro { get; set; }
}
