namespace Application.DTOs;

public class CadastroTipoContatoResponseDto
{
    public int Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public bool Ativo { get; set; }
}

public class CadastroTipoContatoRequestDto
{
    public string Descricao { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;
}
