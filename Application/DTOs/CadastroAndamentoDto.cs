namespace Application.DTOs;

public class CadastroAndamentoResponseDto
{
    public int Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public bool Ativo { get; set; }
}

public class CadastroAndamentoRequestDto
{
    public string Descricao { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;
}
