namespace Application.DTOs;

public class AnotacaoGeralResponseDto
{
    public int AnotacaoId { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public string? Observacoes { get; set; }
    public string? Link { get; set; }
    public string Tipo { get; set; } = "ANOTACAO";
    public DateTime? DataCadastro { get; set; }
}
