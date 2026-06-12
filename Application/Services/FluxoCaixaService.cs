using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;

namespace Application.Services;

public class FluxoCaixaService : IFluxoCaixaService
{
    private static readonly string[] NomesMeses =
    {
        "Jan", "Fev", "Mar", "Abr", "Mai", "Jun",
        "Jul", "Ago", "Set", "Out", "Nov", "Dez"
    };

    private readonly IRepository<Parcela> _parcelaRepository;
    private readonly IRepository<Duplicata> _duplicataRepository;
    private readonly IRepository<CentroCusto> _centroCustoRepository;
    private readonly IRepository<Empresa> _empresaRepository;
    private readonly IRepository<PlanoContas> _planoContasRepository;

    public FluxoCaixaService(
        IRepository<Parcela> parcelaRepository,
        IRepository<Duplicata> duplicataRepository,
        IRepository<CentroCusto> centroCustoRepository,
        IRepository<Empresa> empresaRepository,
        IRepository<PlanoContas> planoContasRepository)
    {
        _parcelaRepository = parcelaRepository;
        _duplicataRepository = duplicataRepository;
        _centroCustoRepository = centroCustoRepository;
        _empresaRepository = empresaRepository;
        _planoContasRepository = planoContasRepository;
    }

    public async Task<FluxoCaixaResponseDto> ObterFluxoCaixaPorAnoAsync(int ano)
    {
        var centros = (await _centroCustoRepository.ListarTodosAsync()).OrderBy(c => c.CcuId).ToList();
        var empresas = (await _empresaRepository.ListarTodosAsync()).ToDictionary(e => e.EmpId);
        var planos = (await _planoContasRepository.ListarTodosAsync()).ToDictionary(p => p.PlcId);
        var duplicatas = (await _duplicataRepository.ListarTodosAsync()).ToDictionary(d => d.DupId);

        var parcelasPagas = (await _parcelaRepository.BuscarTodosAsync(p =>
            p.ParStatus != null &&
            p.ParStatus.ToUpper() == "PAGA" &&
            p.ParDataPagamento.HasValue &&
            p.ParDataPagamento.Value.Year == ano)).ToList();

        var acumuladores = new Dictionary<int, CentroAcumulador>();
        var centrosPorId = centros.ToDictionary(c => c.CcuId);

        foreach (var parcela in parcelasPagas)
        {
            if (!duplicatas.TryGetValue(parcela.DupId, out var duplicata))
                continue;

            var centroId = parcela.CcuId ?? duplicata.CcuId;
            var planoId = parcela.PlcId ?? duplicata.PlcId;

            if (!centroId.HasValue || centroId.Value <= 0 || !planoId.HasValue || planoId.Value <= 0)
                continue;

            if (!centrosPorId.TryGetValue(centroId.Value, out var centro) || !planos.TryGetValue(planoId.Value, out var plano))
                continue;

            if (!acumuladores.ContainsKey(centroId.Value))
            {
                empresas.TryGetValue(centro.EmpId, out var empresa);
                acumuladores[centroId.Value] = new CentroAcumulador(
                    CriarLinhaCentro(centroId.Value, empresa?.EmpFantasia ?? "-", empresa?.EmpCnpj));
            }

            var acc = acumuladores[centroId.Value];
            var linhaPlano = acc.ObterOuCriarPlano(planoId.Value, plano.PlcDescricao);

            var mes = parcela.ParDataPagamento!.Value.Month;
            var valor = parcela.ParValor + parcela.ParMulta + parcela.ParJuros;
            var isReceita = string.Equals(duplicata.DupTipo, "CR", StringComparison.OrdinalIgnoreCase);

            AplicarValor(linhaPlano, mes, valor, isReceita);
            AplicarValor(acc.Centro, mes, valor, isReceita);
        }

        var centrosDto = acumuladores.Values
            .Select(FinalizarCentro)
            .Where(c => c.TotalReceitas > 0 || c.TotalDespesas > 0)
            .OrderBy(c => c.EmpresaFantasia)
            .ToList();

        var totaisMensais = Enumerable.Range(1, 12).Select(m =>
        {
            var receitas = centrosDto.Sum(c => c.Meses[m - 1].Receitas);
            var despesas = centrosDto.Sum(c => c.Meses[m - 1].Despesas);
            return new FluxoCaixaMesDto
            {
                Mes = m,
                NomeMes = NomesMeses[m - 1],
                Receitas = receitas,
                Despesas = despesas,
                Saldo = receitas - despesas
            };
        }).ToList();

        var totalReceitas = centrosDto.Sum(c => c.TotalReceitas);
        var totalDespesas = centrosDto.Sum(c => c.TotalDespesas);

        return new FluxoCaixaResponseDto
        {
            Ano = ano,
            Centros = centrosDto,
            TotaisMensais = totaisMensais,
            TotalReceitasAno = totalReceitas,
            TotalDespesasAno = totalDespesas,
            SaldoAno = totalReceitas - totalDespesas
        };
    }

    private static FluxoCaixaCentroCustoDto FinalizarCentro(CentroAcumulador acc)
    {
        acc.Centro.PlanosContas = acc.Planos.Values
            .Where(p => p.TotalReceitas > 0 || p.TotalDespesas > 0)
            .OrderBy(p => p.Descricao)
            .ToList();
        return acc.Centro;
    }

    private static void AplicarValor(FluxoCaixaPlanoContasDto linha, int mes, double valor, bool isReceita)
    {
        var celula = linha.Meses[mes - 1];
        if (isReceita)
        {
            celula.Receitas += valor;
            linha.TotalReceitas += valor;
        }
        else
        {
            celula.Despesas += valor;
            linha.TotalDespesas += valor;
        }
        celula.Saldo = celula.Receitas - celula.Despesas;
        linha.Saldo = linha.TotalReceitas - linha.TotalDespesas;
    }

    private static void AplicarValor(FluxoCaixaCentroCustoDto linha, int mes, double valor, bool isReceita)
    {
        var celula = linha.Meses[mes - 1];
        if (isReceita)
        {
            celula.Receitas += valor;
            linha.TotalReceitas += valor;
        }
        else
        {
            celula.Despesas += valor;
            linha.TotalDespesas += valor;
        }
        celula.Saldo = celula.Receitas - celula.Despesas;
        linha.Saldo = linha.TotalReceitas - linha.TotalDespesas;
    }

    private static FluxoCaixaCentroCustoDto CriarLinhaCentro(int centroId, string fantasia, string? cnpj) => new()
    {
        CentroCustoId = centroId,
        EmpresaFantasia = fantasia,
        EmpresaCnpj = cnpj,
        Meses = CriarMeses()
    };

    private static FluxoCaixaPlanoContasDto CriarLinhaPlano(int planoId, string descricao) => new()
    {
        PlanoContasId = planoId,
        Descricao = descricao,
        Meses = CriarMeses()
    };

    private static List<FluxoCaixaMesDto> CriarMeses() =>
        Enumerable.Range(1, 12).Select(m => new FluxoCaixaMesDto
        {
            Mes = m,
            NomeMes = NomesMeses[m - 1]
        }).ToList();

    private sealed class CentroAcumulador
    {
        public FluxoCaixaCentroCustoDto Centro { get; }
        public Dictionary<int, FluxoCaixaPlanoContasDto> Planos { get; } = new();

        public CentroAcumulador(FluxoCaixaCentroCustoDto centro) => Centro = centro;

        public FluxoCaixaPlanoContasDto ObterOuCriarPlano(int planoId, string descricao)
        {
            if (!Planos.TryGetValue(planoId, out var linha))
            {
                linha = CriarLinhaPlano(planoId, descricao);
                Planos[planoId] = linha;
            }
            return linha;
        }
    }
}
