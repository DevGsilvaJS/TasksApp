namespace Application.DTOs;

public class DashboardEstatisticasDto
{
    public int TotalAtendimentosPorUsuario { get; set; }
    public int TotalContasAPagar { get; set; }
    public int TotalAtendimentosPorCliente { get; set; }
    public int TotalContasPagas { get; set; }
    public decimal ValorTotalContasPagas { get; set; }
    public List<AtendimentoPorUsuarioDto> AtendimentosPorUsuario { get; set; } = new();
    public List<ContaAPagarDto> ContasAPagar { get; set; } = new();
    public List<ContaAPagarDto> ContasPagas { get; set; } = new();
    public List<AtendimentoPorClienteDto> AtendimentosPorCliente { get; set; } = new();
}

public class AtendimentoPorUsuarioDto
{
    public int UsuarioId { get; set; }
    public string UsuarioNome { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public List<DetalheAtendimentoDto> Detalhes { get; set; } = new();
}

public class DetalheAtendimentoDto
{
    public int TarefaId { get; set; }
    public int? Numero { get; set; }
    public int ClienteId { get; set; }
    public int ClienteCodigo { get; set; }
    public string ClienteNome { get; set; } = string.Empty;
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

public class ValorPorMesPorUsuarioDto
{
    public int UsuarioId { get; set; }
    public string UsuarioNome { get; set; } = string.Empty;
    public int Ano { get; set; }
    public int Mes { get; set; }
    public string MesNome { get; set; } = string.Empty;
    public decimal ValorTotal { get; set; }
    public int QuantidadeContratos { get; set; }
    public List<ContratoDetalheDto> Contratos { get; set; } = new();
}

public class ContratoDetalheDto
{
    public int ClienteId { get; set; }
    public int ClienteCodigo { get; set; }
    public string ClienteNome { get; set; } = string.Empty;
    public decimal ValorContrato { get; set; }
}
