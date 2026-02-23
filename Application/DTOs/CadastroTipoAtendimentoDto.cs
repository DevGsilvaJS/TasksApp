namespace Application.DTOs;

public class CadastroTipoAtendimentoResponseDto
{
    public int Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public bool Ativo { get; set; }
}

public class CadastroTipoAtendimentoRequestDto
{
    public string Descricao { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;
}
