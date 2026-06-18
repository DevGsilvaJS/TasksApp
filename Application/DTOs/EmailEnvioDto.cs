namespace Application.DTOs;

public class DestinatarioEmailResponseDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public bool NaoEnviar { get; set; }
}

public class DestinatariosEmailPaginadoResponseDto
{
    public IEnumerable<DestinatarioEmailResponseDto> Itens { get; set; } = [];
    public int Total { get; set; }
    public int Pagina { get; set; }
    public int TamanhoPagina { get; set; }
    public int TotalPaginas { get; set; }
}

public class AlterarNaoEnviarEmailDto
{
    public bool NaoEnviar { get; set; }
}

public class EnfileirarCampanhaEmailResponseDto
{
    public int CampanhaId { get; set; }
    public int TotalDestinatarios { get; set; }
    public string Mensagem { get; set; } = string.Empty;
}

public class CampanhaEmailStatusResponseDto
{
    public int Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Assunto { get; set; } = string.Empty;
    public int TotalItens { get; set; }
    public int Enviados { get; set; }
    public int Erros { get; set; }
    public int Pendentes { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? PausaAte { get; set; }
}

public class RelatorioCampanhaEmailResponseDto
{
    public int Id { get; set; }
    public string Assunto { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime DataCriacao { get; set; }
    public DateTime? DataConclusao { get; set; }
    public int TotalItens { get; set; }
    public int Enviados { get; set; }
    public int Erros { get; set; }
    public IEnumerable<RelatorioItemEmailDto> ItensEnviados { get; set; } = [];
    public IEnumerable<RelatorioItemEmailDto> ItensComErro { get; set; } = [];
}

public class RelatorioItemEmailDto
{
    public string Email { get; set; } = string.Empty;
    public string? RemetenteEmail { get; set; }
    public DateTime? DataEnvio { get; set; }
    public string? MensagemErro { get; set; }
}
