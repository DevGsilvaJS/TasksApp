using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class CadastroRegimentoDto
{
    [Required(ErrorMessage = "Título é obrigatório")]
    [MaxLength(300)]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Descrição é obrigatória")]
    public string Descricao { get; set; } = string.Empty;

    public bool Ativo { get; set; } = true;
}

public class RegimentoResponseDto
{
    public int RegimentoId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public string SituacaoAprovacao { get; set; } = string.Empty;
    public int QuantidadeAceites { get; set; }
    public bool PossuiAceites { get; set; }
}

public class RegimentoDetalheResponseDto
{
    public int RegimentoId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public string SituacaoAprovacao { get; set; } = string.Empty;
    public int QuantidadeAceites { get; set; }
    public bool PossuiAceites { get; set; }
    public RegimentoAceiteResponseDto? MeuAceiteAtual { get; set; }
    public IEnumerable<RegimentoAceiteResponseDto> Aceites { get; set; } = [];
}

public class RegimentoAceiteResponseDto
{
    public int AceiteId { get; set; }
    public int UsuarioId { get; set; }
    public string UsuarioNome { get; set; } = string.Empty;
    public bool Aceito { get; set; }
    public string Situacao { get; set; } = string.Empty;
    public string? Observacao { get; set; }
    public DateTime? DataAceite { get; set; }
}

public class CadastroRegimentoAceiteDto
{
    [Required]
    public bool Aceito { get; set; }

    [MaxLength(2000)]
    public string? Observacao { get; set; }
}

public class RegimentoAceiteLogResponseDto
{
    public int LogId { get; set; }
    public int UsuarioId { get; set; }
    public string UsuarioNome { get; set; } = string.Empty;
    public string Acao { get; set; } = string.Empty;
    public string? Observacao { get; set; }
    public DateTime Data { get; set; }
}
