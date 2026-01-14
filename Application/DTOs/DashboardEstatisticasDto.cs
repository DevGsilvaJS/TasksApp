namespace Application.DTOs;

public class DashboardEstatisticasDto
{
    public int TotalAtendimentosPorUsuario { get; set; }
    public int TotalContasAPagar { get; set; }
    public int TotalAtendimentosPorCliente { get; set; }
    public List<AtendimentoPorUsuarioDto> AtendimentosPorUsuario { get; set; } = new();
    public List<ContaAPagarDto> ContasAPagar { get; set; } = new();
    public List<AtendimentoPorClienteDto> AtendimentosPorCliente { get; set; } = new();
}

public class AtendimentoPorUsuarioDto
{
    public int UsuarioId { get; set; }
    public string UsuarioNome { get; set; } = string.Empty;
    public int Quantidade { get; set; }
}

public class ContaAPagarDto
{
    public int ParcelaId { get; set; }
    public int DuplicataId { get; set; }
    public string NumeroDuplicata { get; set; } = string.Empty;
    public DateTime DataVencimento { get; set; }
    public decimal Valor { get; set; }
    public bool Paga { get; set; }
}

public class AtendimentoPorClienteDto
{
    public int ClienteId { get; set; }
    public string ClienteNome { get; set; } = string.Empty;
    public int Quantidade { get; set; }
}
