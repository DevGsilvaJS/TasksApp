namespace Application.DTOs;

public class ClienteResponseDto
{
    public int ClienteId { get; set; }
    public int PessoaId { get; set; }
    public string Fantasia { get; set; } = string.Empty;
    public string? DocFederal { get; set; }
    public string? DocEstadual { get; set; }
    public int Codigo { get; set; }
    public DateTime? DataCadastro { get; set; }
    public decimal? ValorContrato { get; set; }
}
