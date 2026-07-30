namespace Application.DTOs;

public class DashboardEstatisticasDto
{
    public int TotalAtendimentosPorUsuario { get; set; }
    /// <summary>Média da equipe por dia útil no mês atual.</summary>
    public decimal MediaDiariaAtendimentos { get; set; }
    /// <summary>Média por operador por dia útil no mês atual.</summary>
    public decimal MediaDiariaPorOperador { get; set; }
    public int DiasUteisMesAtual { get; set; }
    public List<MediaPorOperadorDto> MediasPorOperador { get; set; } = new();
    public int TotalContasAPagar { get; set; }
    public decimal ValorTotalContasAPagar { get; set; }
    public int TotalAtendimentosPorCliente { get; set; }
    public int TotalContasPagas { get; set; }
    public decimal ValorTotalContasPagas { get; set; }
    public int TotalContasAReceber { get; set; }
    public decimal ValorTotalContasAReceber { get; set; }
    public int TotalContasRecebidas { get; set; }
    public decimal ValorTotalContasRecebidas { get; set; }
    public decimal Lucro { get; set; }
    public List<AtendimentoPorUsuarioDto> AtendimentosPorUsuario { get; set; } = new();
    public List<ContaAPagarDto> ContasAPagar { get; set; } = new();
    public List<ContaAPagarDto> ContasPagas { get; set; } = new();
    public List<ContaAPagarDto> ContasAReceber { get; set; } = new();
    public List<ContaAPagarDto> ContasRecebidas { get; set; } = new();
    public List<AtendimentoPorClienteDto> AtendimentosPorCliente { get; set; } = new();
    public List<AtendimentoPorClienteMesDto> AtendimentosPorClienteMes { get; set; } = new();
}

public class AtendimentoPorUsuarioDto
{
    public int UsuarioId { get; set; }
    public string UsuarioNome { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public List<DetalheAtendimentoDto> Detalhes { get; set; } = new();
}

public class MediaPorOperadorDto
{
    public int UsuarioId { get; set; }
    public string UsuarioNome { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal MediaDiaria { get; set; }
}

public class DetalheAtendimentoDto
{
    public int TarefaId { get; set; }
    public int? Numero { get; set; }
    public string? Titulo { get; set; }
    public DateTime? DataCadastro { get; set; }
    public int ClienteId { get; set; }
    public string ClienteCodigo { get; set; } = string.Empty;
    public string ClienteNome { get; set; } = string.Empty;
}

public class ContaAPagarDto
{
    public int ParcelaId { get; set; }
    public int DuplicataId { get; set; }
    public string NumeroDuplicata { get; set; } = string.Empty;
    public string? DescricaoDespesa { get; set; }
    public DateTime DataVencimento { get; set; }
    public DateTime? DataPagamento { get; set; }
    public decimal Valor { get; set; }
    public bool Paga { get; set; }
    public string? ClienteNome { get; set; }
    public string? CentroCustoDescricao { get; set; }
    public string? PlanoContasDescricao { get; set; }
}

public class AtendimentoPorClienteDto
{
    public int ClienteId { get; set; }
    public string ClienteNome { get; set; } = string.Empty;
    public int Quantidade { get; set; }
}

public class AtendimentoPorClienteMesDto
{
    public int ClienteId { get; set; }
    public string ClienteNome { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal Percentual { get; set; }
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
    public string ClienteCodigo { get; set; } = string.Empty;
    public string ClienteNome { get; set; } = string.Empty;
    public decimal ValorContrato { get; set; }
}
