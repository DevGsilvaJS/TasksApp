using Application.DTOs;
using Application.Helpers;
using Application.Interfaces;
using Domain.Entities;

namespace Application.Services;

public class RelatorioGerencialService : IRelatorioGerencialService
{
    private readonly IRepository<Parcela> _parcelaRepository;
    private readonly IRepository<Duplicata> _duplicataRepository;
    private readonly IRepository<Cliente> _clienteRepository;
    private readonly IRepository<Pessoa> _pessoaRepository;

    private static readonly Dictionary<string, (string Titulo, string TipoDuplicata, bool ApenasPaga, bool FiltrarPorDataPagamento)> Configuracoes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["contas-a-receber"] = ("Relatório de contas a receber", "CR", false, false),
        ["contas-recebidas"] = ("Relatório de contas recebidas", "CR", true, true),
        ["contas-pagas"] = ("Relatório de contas pagas", "CP", true, false),
        ["contas-a-pagar"] = ("Relatório de contas a pagar", "CP", false, false)
    };

    public RelatorioGerencialService(
        IRepository<Parcela> parcelaRepository,
        IRepository<Duplicata> duplicataRepository,
        IRepository<Cliente> clienteRepository,
        IRepository<Pessoa> pessoaRepository)
    {
        _parcelaRepository = parcelaRepository;
        _duplicataRepository = duplicataRepository;
        _clienteRepository = clienteRepository;
        _pessoaRepository = pessoaRepository;
    }

    public async Task<RelatorioGerencialResponseDto> ObterRelatorioAsync(DateTime dataInicio, DateTime dataFim, string tipoRelatorio)
    {
        if (!Configuracoes.TryGetValue(tipoRelatorio, out var config))
            throw new InvalidOperationException("Tipo de relatório inválido.");

        var inicio = DataBrasilHelper.ParseDataConsulta(dataInicio);
        var fim = DataBrasilHelper.ParseDataConsulta(dataFim);

        if (fim < inicio)
            throw new InvalidOperationException("A data final deve ser maior ou igual à data inicial.");

        var duplicatas = await _duplicataRepository.BuscarTodosAsync(d => d.DupTipo == config.TipoDuplicata);
        var duplicatasDict = duplicatas.ToDictionary(d => d.DupId);

        if (duplicatasDict.Count == 0)
        {
            return CriarRespostaVazia(tipoRelatorio, config.Titulo, inicio, fim);
        }

        var dupIds = duplicatasDict.Keys.ToList();
        var parcelas = await _parcelaRepository.BuscarTodosAsync(p => dupIds.Contains(p.DupId));

        var parcelasFiltradas = parcelas.Where(p => ParcelaPassaFiltro(p, config, inicio, fim)).ToList();

        var clientesCache = new Dictionary<int, string?>();
        var itens = new List<RelatorioGerencialLinhaDto>();

        foreach (var parcela in parcelasFiltradas.OrderBy(p => ObterDataReferencia(p, config)).ThenBy(p => p.ParNumeroParcela))
        {
            var duplicata = duplicatasDict[parcela.DupId];
            string? clienteNome = null;

            if (duplicata.CliId.HasValue)
            {
                if (!clientesCache.TryGetValue(duplicata.CliId.Value, out clienteNome))
                {
                    clienteNome = await ObterNomeClienteAsync(duplicata.CliId.Value);
                    clientesCache[duplicata.CliId.Value] = clienteNome;
                }
            }

            var valorTotal = (decimal)(parcela.ParValor + parcela.ParMulta + parcela.ParJuros);

            itens.Add(new RelatorioGerencialLinhaDto
            {
                ParcelaId = parcela.ParId,
                DuplicataId = duplicata.DupId,
                NumeroDuplicata = duplicata.DupNumero,
                NumeroParcela = parcela.ParNumeroParcela,
                DescricaoDespesa = duplicata.DupDescricaoDespesa,
                ClienteNome = clienteNome,
                DataEmissao = duplicata.DupDataEmissao,
                DataVencimento = parcela.ParVencimento,
                DataPagamento = parcela.ParDataPagamento,
                Valor = (decimal)parcela.ParValor,
                Multa = (decimal)parcela.ParMulta,
                Juros = (decimal)parcela.ParJuros,
                ValorTotal = valorTotal,
                Status = ParcelaStatusHelper.IsPaga(parcela.ParStatus) ? "Paga" : "Pendente"
            });
        }

        return new RelatorioGerencialResponseDto
        {
            TipoRelatorio = tipoRelatorio,
            TituloRelatorio = config.Titulo,
            DataInicio = inicio.ToDateTime(TimeOnly.MinValue),
            DataFim = fim.ToDateTime(TimeOnly.MinValue),
            Itens = itens,
            TotalValor = itens.Sum(i => i.ValorTotal),
            TotalRegistros = itens.Count
        };
    }

    private static bool ParcelaPassaFiltro(
        Parcela parcela,
        (string Titulo, string TipoDuplicata, bool ApenasPaga, bool FiltrarPorDataPagamento) config,
        DateOnly inicio,
        DateOnly fim)
    {
        var isPaga = ParcelaStatusHelper.IsPaga(parcela.ParStatus);

        if (config.ApenasPaga && !isPaga) return false;
        if (!config.ApenasPaga && !ParcelaStatusHelper.IsPendente(parcela.ParStatus)) return false;

        var dataReferencia = ObterDataReferencia(parcela, config);
        return DataBrasilHelper.NoPeriodoInclusive(dataReferencia, inicio, fim);
    }

    /// <summary>
    /// Contas recebidas/pagas: data de pagamento (fallback vencimento se ausente).
    /// Contas a receber/pagar: data de vencimento.
    /// </summary>
    private static DateTime ObterDataReferencia(
        Parcela parcela,
        (string Titulo, string TipoDuplicata, bool ApenasPaga, bool FiltrarPorDataPagamento) config)
    {
        if (config.FiltrarPorDataPagamento)
            return parcela.ParDataPagamento ?? parcela.ParVencimento;

        return parcela.ParVencimento;
    }

    private static RelatorioGerencialResponseDto CriarRespostaVazia(
        string tipoRelatorio,
        string titulo,
        DateOnly inicio,
        DateOnly fim) =>
        new()
        {
            TipoRelatorio = tipoRelatorio,
            TituloRelatorio = titulo,
            DataInicio = inicio.ToDateTime(TimeOnly.MinValue),
            DataFim = fim.ToDateTime(TimeOnly.MinValue),
            Itens = new List<RelatorioGerencialLinhaDto>(),
            TotalValor = 0,
            TotalRegistros = 0
        };

    private async Task<string?> ObterNomeClienteAsync(int clienteId)
    {
        var cliente = await _clienteRepository.GetByIdAsync(clienteId);
        if (cliente == null) return null;

        var pessoa = await _pessoaRepository.GetByIdAsync(cliente.PesId);
        return pessoa?.PesFantasia;
    }
}
