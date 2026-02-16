namespace Application.DTOs;

public class AnotacaoClienteResponseDto
{
    public int AnotacaoClienteId { get; set; }
    public int ClienteId { get; set; }
    public string? Descricao { get; set; }
    public DateTime? DataCadastro { get; set; }
    public string? ClienteFantasia { get; set; }
}
