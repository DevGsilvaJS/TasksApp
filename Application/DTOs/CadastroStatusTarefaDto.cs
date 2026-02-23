namespace Application.DTOs;

public class CadastroStatusTarefaResponseDto
{
    public int Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public bool Ativo { get; set; }
}

public class CadastroStatusTarefaRequestDto
{
    public string Descricao { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;
}
