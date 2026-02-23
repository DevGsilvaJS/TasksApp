namespace Application.DTOs;

public class CadastroStatusAtendimentoComercialResponseDto
{
    public int Id { get; set; }
    public int Numero { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public bool Ativo { get; set; }
}

public class CadastroStatusAtendimentoComercialRequestDto
{
    public string Descricao { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;
}
