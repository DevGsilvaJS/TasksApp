namespace Application.DTOs;

public class PossivelClienteAnotacaoResponseDto
{
    public int PcaId { get; set; }
    public int PocId { get; set; }
    public int UsuId { get; set; }
    public string? UsuarioNome { get; set; }
    public string? Descricao { get; set; }
    public DateTime DataCadastro { get; set; }
}
