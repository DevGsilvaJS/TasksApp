using Application.DTOs;
using Application.Helpers;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IRepository<Tarefa> _tarefaRepository;
    private readonly IRepository<Parcela> _parcelaRepository;
    private readonly IRepository<Duplicata> _duplicataRepository;
    private readonly IRepository<Usuario> _usuarioRepository;
    private readonly IRepository<Cliente> _clienteRepository;
    private readonly IRepository<Pessoa> _pessoaRepository;
    private readonly IRepository<PossivelClienteAnotacao> _anotacaoTelemarketingRepository;
    private readonly IRepository<ClienteContratoValor> _clienteContratoValorRepository;
    private readonly IRepository<Empresa> _empresaRepository;
    private readonly IRepository<PlanoContas> _planoContasRepository;

    public DashboardService(
        IRepository<Tarefa> tarefaRepository,
        IRepository<Parcela> parcelaRepository,
        IRepository<Duplicata> duplicataRepository,
        IRepository<Usuario> usuarioRepository,
        IRepository<Cliente> clienteRepository,
        IRepository<Pessoa> pessoaRepository,
        IRepository<PossivelClienteAnotacao> anotacaoTelemarketingRepository,
        IRepository<ClienteContratoValor> clienteContratoValorRepository,
        IRepository<Empresa> empresaRepository,
        IRepository<PlanoContas> planoContasRepository)
    {
        _tarefaRepository = tarefaRepository;
        _parcelaRepository = parcelaRepository;
        _duplicataRepository = duplicataRepository;
        _usuarioRepository = usuarioRepository;
        _clienteRepository = clienteRepository;
        _pessoaRepository = pessoaRepository;
        _anotacaoTelemarketingRepository = anotacaoTelemarketingRepository;
        _clienteContratoValorRepository = clienteContratoValorRepository;
        _empresaRepository = empresaRepository;
        _planoContasRepository = planoContasRepository;
    }

    public async Task<DashboardEstatisticasDto> ObterEstatisticasAsync(DateTime dataInicio, DateTime dataFim)
    {
        // Normalizar datas para UTC
        var inicioUtc = dataInicio.ToUniversalTime();
        var fimUtc = dataFim.ToUniversalTime().AddDays(1).AddTicks(-1); // Incluir o dia inteiro

        // 1. Atendimentos por usuário
        // Buscar todas as tarefas com data de cadastro e filtrar em memória para garantir comparação correta
        var todasTarefasComData = await _tarefaRepository.BuscarTodosAsync(t => t.TarDtCadastro.HasValue);
        
        var todasTarefas = todasTarefasComData.Where(t => 
        {
            if (!t.TarDtCadastro.HasValue) return false;
            
            var dataCadastro = t.TarDtCadastro.Value;
            // Garantir que estamos comparando em UTC
            var dataCadastroUtc = dataCadastro.Kind == DateTimeKind.Utc 
                ? dataCadastro 
                : dataCadastro.ToUniversalTime();
            
            // Comparar com as datas de início e fim do período
            return dataCadastroUtc >= inicioUtc && dataCadastroUtc <= fimUtc;
        }).ToList();

        var atendimentosPorUsuario = todasTarefas
            .GroupBy(t => t.UsuId)
            .Select(g => new
            {
                UsuarioId = g.Key,
                Tarefas = g.ToList()
            })
            .ToList();

        var atendimentosPorUsuarioDto = new List<AtendimentoPorUsuarioDto>();
        foreach (var item in atendimentosPorUsuario)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(item.UsuarioId);
            if (usuario != null)
            {
                var pessoa = await _pessoaRepository.GetByIdAsync(usuario.PesId);
                
                var detalhes = new List<DetalheAtendimentoDto>();
                foreach (var tarefa in item.Tarefas)
                {
                    var cliente = await _clienteRepository.GetByIdAsync(tarefa.CliId);
                    if (cliente != null && cliente.CliStatus == StatusCliente.Ativo)
                    {
                        var pessoaCliente = await _pessoaRepository.GetByIdAsync(cliente.PesId);
                        detalhes.Add(new DetalheAtendimentoDto
                        {
                            TarefaId = tarefa.TarId,
                            Numero = tarefa.TarNumero,
                            Titulo = tarefa.TarTitulo,
                            DataCadastro = tarefa.TarDtCadastro,
                            ClienteId = cliente.CliId,
                            ClienteCodigo = cliente.CliCodigo,
                            ClienteNome = pessoaCliente?.PesFantasia ?? "Desconhecido",
                            Prioridade = tarefa.TarPrioridade,
                            PrioridadeDescricao = ObterDescricaoPrioridade(tarefa.TarPrioridade)
                        });
                    }
                }
                
                atendimentosPorUsuarioDto.Add(new AtendimentoPorUsuarioDto
                {
                    UsuarioId = item.UsuarioId,
                    UsuarioNome = pessoa?.PesFantasia ?? "Desconhecido",
                    Quantidade = item.Tarefas.Count,
                    Detalhes = detalhes
                });
            }
        }

        // 2. Contas a pagar (parcelas pendentes do tipo CP que vencem no mês atual)
        // O card mostra "Mês Atual", então sempre buscar do mês atual, não do período selecionado
        var mesAtual = DateTime.UtcNow;
        var inicioMesAtual = new DateTime(mesAtual.Year, mesAtual.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var fimMesAtual = new DateTime(mesAtual.Year, mesAtual.Month, DateTime.DaysInMonth(mesAtual.Year, mesAtual.Month), 23, 59, 59, 999, DateTimeKind.Utc);

        var todasParcelas = await _parcelaRepository.BuscarTodosAsync(p =>
            p.ParVencimento >= inicioMesAtual &&
            p.ParVencimento <= fimMesAtual &&
            p.ParStatus != null && p.ParStatus.ToUpper() == "PENDENTE");

        var todasDuplicatas = await _duplicataRepository.ListarTodosAsync();
        var duplicatasDict = todasDuplicatas.ToDictionary(d => d.DupId);
        var empresasDict = (await _empresaRepository.ListarTodosAsync()).ToDictionary(e => e.EmpId);
        var planosDict = (await _planoContasRepository.ListarTodosAsync()).ToDictionary(p => p.PlcId);
        var clientesCrDict = await ObterNomesClientesPorDuplicatasAsync(
            duplicatasDict.Values.Where(d => d.DupTipo == "CR"));

        var contasAPagar = todasParcelas
            .Where(p => duplicatasDict.ContainsKey(p.DupId) && duplicatasDict[p.DupId].DupTipo == "CP")
            .Where(p => !duplicatasDict[p.DupId].DupInativa)
            .Select(parcela => MapearContaParcela(
                parcela,
                duplicatasDict[parcela.DupId],
                (decimal)parcela.ParValor,
                ParcelaStatusHelper.IsPaga(parcela.ParStatus),
                parcela.ParDataPagamento,
                empresasDict,
                planosDict,
                incluirClassificacao: true))
            .ToList();

        // 2.1. Contas a receber (parcelas não recebidas do mês atual do tipo CR)
        var parcelasAReceber = await _parcelaRepository.BuscarTodosAsync(p =>
            p.ParVencimento >= inicioMesAtual && 
            p.ParVencimento <= fimMesAtual && 
            p.ParStatus != null && p.ParStatus.ToUpper() == "PENDENTE");
        
        var clientesAtivosIds = (await _clienteRepository.BuscarTodosAsync(c => c.CliStatus == StatusCliente.Ativo))
            .Select(c => c.CliId)
            .ToHashSet();

        var contasAReceber = parcelasAReceber
            .Where(p => duplicatasDict.ContainsKey(p.DupId) && duplicatasDict[p.DupId].DupTipo == "CR")
            .Where(p =>
            {
                var duplicata = duplicatasDict[p.DupId];
                return !duplicata.CliId.HasValue || clientesAtivosIds.Contains(duplicata.CliId.Value);
            })
            .Select(parcela => MapearContaParcela(
                parcela,
                duplicatasDict[parcela.DupId],
                (decimal)parcela.ParValor,
                ParcelaStatusHelper.IsPaga(parcela.ParStatus),
                parcela.ParDataPagamento,
                empresasDict,
                planosDict,
                incluirClassificacao: false,
                clientesNomes: clientesCrDict))
            .ToList();

        // 3. Atendimentos por cliente (respeita o período selecionado)
        var atendimentosPorCliente = todasTarefas
            .GroupBy(t => t.CliId)
            .Select(g => new
            {
                ClienteId = g.Key,
                Quantidade = g.Count()
            })
            .ToList();

        var atendimentosPorClienteDto = new List<AtendimentoPorClienteDto>();
        foreach (var item in atendimentosPorCliente)
        {
            var cliente = await _clienteRepository.GetByIdAsync(item.ClienteId);
            if (cliente != null && cliente.CliStatus == StatusCliente.Ativo)
            {
                var pessoa = await _pessoaRepository.GetByIdAsync(cliente.PesId);
                atendimentosPorClienteDto.Add(new AtendimentoPorClienteDto
                {
                    ClienteId = item.ClienteId,
                    ClienteNome = pessoa?.PesFantasia ?? "Desconhecido",
                    Quantidade = item.Quantidade
                });
            }
        }

        // 4. Contas pagas (CP) e contas recebidas (CR) no mês atual
        // CP = por vencimento (títulos que vencem no mês e já foram pagos). CR = por data de pagamento (como estava).

        // Contas a Pagar - Pagas: filtrar por vencimento no mês atual
        var parcelasPagas = await _parcelaRepository.BuscarTodosAsync(p =>
            p.ParStatus != null && p.ParStatus.ToUpper() == "PAGA");
        var contasPagasMes = parcelasPagas.Where(p =>
        {
            var vencimentoUtc = p.ParVencimento.Kind == DateTimeKind.Utc ? p.ParVencimento : p.ParVencimento.ToUniversalTime();
            return vencimentoUtc >= inicioMesAtual && vencimentoUtc <= fimMesAtual;
        }).ToList();

        // Contas a Receber - Recebidas: filtrar por data de pagamento no mês atual (sem alteração)
        var parcelasRecebidas = await _parcelaRepository.BuscarTodosAsync(p =>
            p.ParStatus != null && p.ParStatus.ToUpper() == "PAGA" && p.ParDataPagamento.HasValue);
        var contasRecebidasMes = parcelasRecebidas.Where(p =>
        {
            var dataPagamentoUtc = p.ParDataPagamento!.Value.Kind == DateTimeKind.Utc ? p.ParDataPagamento.Value : p.ParDataPagamento.Value.ToUniversalTime();
            return dataPagamentoUtc.Year == mesAtual.Year && dataPagamentoUtc.Month == mesAtual.Month;
        }).ToList();

        var contasPagasDto = new List<ContaAPagarDto>();
        var contasRecebidasDto = new List<ContaAPagarDto>();

        foreach (var parcela in contasPagasMes)
        {
            if (duplicatasDict.ContainsKey(parcela.DupId)
                && duplicatasDict[parcela.DupId].DupTipo == "CP"
                && !duplicatasDict[parcela.DupId].DupInativa)
            {
                contasPagasDto.Add(MapearContaParcela(
                    parcela,
                    duplicatasDict[parcela.DupId],
                    (decimal)(parcela.ParValor + parcela.ParMulta + parcela.ParJuros),
                    true,
                    parcela.ParDataPagamento ?? parcela.ParVencimento,
                    empresasDict,
                    planosDict,
                    incluirClassificacao: true));
            }
        }

        foreach (var parcela in contasRecebidasMes)
        {
            if (duplicatasDict.ContainsKey(parcela.DupId) && duplicatasDict[parcela.DupId].DupTipo == "CR")
            {
                contasRecebidasDto.Add(MapearContaParcela(
                    parcela,
                    duplicatasDict[parcela.DupId],
                    (decimal)(parcela.ParValor + parcela.ParMulta + parcela.ParJuros),
                    true,
                    parcela.ParDataPagamento ?? parcela.ParVencimento,
                    empresasDict,
                    planosDict,
                    incluirClassificacao: false,
                    clientesNomes: clientesCrDict));
            }
        }

        // Ordenar por data de pagamento (mais recente primeiro)
        contasPagasDto = contasPagasDto.OrderByDescending(c => c.DataPagamento).ToList();
        contasRecebidasDto = contasRecebidasDto.OrderByDescending(c => c.DataPagamento).ToList();

        var valorTotalContasAPagar = contasAPagar.Sum(c => c.Valor);
        var valorTotalContasAReceber = contasAReceber.Sum(c => c.Valor);
        var valorTotalContasPagas = contasPagasDto.Sum(c => c.Valor);
        var valorTotalContasRecebidas = contasRecebidasDto.Sum(c => c.Valor);

        // 5. Atendimentos por cliente no período (com percentual) — mesma base da seção 3
        var totalAtendimentosPeriodo = atendimentosPorClienteDto.Sum(a => a.Quantidade);
        var atendimentosPorClienteMesDto = atendimentosPorClienteDto
            .Select(a => new AtendimentoPorClienteMesDto
            {
                ClienteId = a.ClienteId,
                ClienteNome = a.ClienteNome,
                Quantidade = a.Quantidade,
                Percentual = totalAtendimentosPeriodo > 0 ? (decimal)a.Quantidade / totalAtendimentosPeriodo * 100 : 0
            })
            .OrderByDescending(a => a.Quantidade)
            .ToList();

        // 5.1 Atendimentos por prioridade no período
        var totalPorPrioridade = todasTarefas.Count;
        var atendimentosPorPrioridadeDto = Enum.GetValues<PrioridadeTarefa>()
            .Select(p =>
            {
                var quantidade = todasTarefas.Count(t => t.TarPrioridade == p);
                return new AtendimentoPorPrioridadeDto
                {
                    Prioridade = p,
                    PrioridadeDescricao = ObterDescricaoPrioridade(p),
                    Quantidade = quantidade,
                    Percentual = totalPorPrioridade > 0
                        ? Math.Round((decimal)quantidade / totalPorPrioridade * 100, 1)
                        : 0
                };
            })
            .OrderByDescending(a => a.Prioridade)
            .ToList();

        // Calcular lucro (Contas Recebidas - Contas Pagas)
        // Garantir que os valores foram calculados corretamente
        // O lucro é calculado como: Contas Recebidas (CR) - Contas Pagas (CP)
        var lucro = valorTotalContasRecebidas - valorTotalContasPagas;

        // Média diária no período selecionado por dias úteis (seg–sex)
        // Equipe: total ÷ dias úteis | Operador: média da equipe ÷ qtd. operadores do período
        var inicioPeriodoLocal = dataInicio.Date;
        var fimPeriodoLocal = dataFim.Date;

        // Em períodos amplos (ex.: Geral), contar dias úteis a partir do 1º atendimento
        if (todasTarefas.Count > 0)
        {
            var primeiraData = todasTarefas
                .Where(t => t.TarDtCadastro.HasValue)
                .Select(t => t.TarDtCadastro!.Value.Date)
                .DefaultIfEmpty(inicioPeriodoLocal)
                .Min();
            if (primeiraData > inicioPeriodoLocal)
                inicioPeriodoLocal = primeiraData;
        }

        var diasUteis = ContarDiasUteis(inicioPeriodoLocal, fimPeriodoLocal);
        var atendimentosPeriodo = todasTarefas.Count;
        var operadoresPeriodo = todasTarefas.Select(t => t.UsuId).Distinct().Count();

        var mediaDiariaAtendimentos = diasUteis > 0
            ? Math.Round((decimal)atendimentosPeriodo / diasUteis, 1)
            : 0;
        var mediaDiariaPorOperador = diasUteis > 0 && operadoresPeriodo > 0
            ? Math.Round((decimal)atendimentosPeriodo / diasUteis / operadoresPeriodo, 1)
            : 0;

        var mediasPorOperador = new List<MediaPorOperadorDto>();
        foreach (var grupo in todasTarefas.GroupBy(t => t.UsuId))
        {
            var usuario = await _usuarioRepository.GetByIdAsync(grupo.Key);
            var pessoa = usuario != null ? await _pessoaRepository.GetByIdAsync(usuario.PesId) : null;
            var quantidade = grupo.Count();
            mediasPorOperador.Add(new MediaPorOperadorDto
            {
                UsuarioId = grupo.Key,
                UsuarioNome = pessoa?.PesFantasia ?? "Desconhecido",
                Quantidade = quantidade,
                MediaDiaria = diasUteis > 0
                    ? Math.Round((decimal)quantidade / diasUteis, 1)
                    : 0
            });
        }
        mediasPorOperador = mediasPorOperador
            .OrderByDescending(m => m.Quantidade)
            .ThenBy(m => m.UsuarioNome)
            .ToList();

        return new DashboardEstatisticasDto
        {
            TotalAtendimentosPorUsuario = atendimentosPorUsuarioDto.Sum(a => a.Quantidade),
            MediaDiariaAtendimentos = mediaDiariaAtendimentos,
            MediaDiariaPorOperador = mediaDiariaPorOperador,
            DiasUteisMesAtual = diasUteis,
            MediasPorOperador = mediasPorOperador,
            TotalContasAPagar = contasAPagar.Count(),
            ValorTotalContasAPagar = valorTotalContasAPagar,
            TotalAtendimentosPorCliente = atendimentosPorClienteDto.Sum(a => a.Quantidade),
            TotalContasPagas = contasPagasDto.Count,
            ValorTotalContasPagas = valorTotalContasPagas,
            TotalContasAReceber = contasAReceber.Count(),
            ValorTotalContasAReceber = valorTotalContasAReceber,
            TotalContasRecebidas = contasRecebidasDto.Count,
            ValorTotalContasRecebidas = valorTotalContasRecebidas,
            Lucro = lucro, // Calculado como: valorTotalContasRecebidas - valorTotalContasPagas
            AtendimentosPorUsuario = atendimentosPorUsuarioDto,
            ContasAPagar = contasAPagar,
            ContasPagas = contasPagasDto,
            ContasAReceber = contasAReceber,
            ContasRecebidas = contasRecebidasDto,
            AtendimentosPorCliente = atendimentosPorClienteDto,
            AtendimentosPorClienteMes = atendimentosPorClienteMesDto,
            AtendimentosPorPrioridade = atendimentosPorPrioridadeDto
        };
    }

    private static string ObterDescricaoPrioridade(PrioridadeTarefa prioridade)
    {
        return prioridade switch
        {
            PrioridadeTarefa.Baixa => "Baixa",
            PrioridadeTarefa.Media => "Média",
            PrioridadeTarefa.Alta => "Alta",
            _ => prioridade.ToString()
        };
    }

    /// <summary>Conta dias úteis (segunda a sexta) entre as datas, inclusive.</summary>
    private static int ContarDiasUteis(DateTime inicio, DateTime fim)
    {
        if (fim < inicio) return 0;

        var dias = 0;
        for (var d = inicio.Date; d <= fim.Date; d = d.AddDays(1))
        {
            if (d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday)
                dias++;
        }
        return dias;
    }

    public async Task<List<ValorPorMesPorUsuarioDto>> ObterValoresPorMesPorUsuarioAsync(int? ano = null)
    {
        var anoFiltro = ano ?? DateTime.UtcNow.Year;
        var mesAtual = DateTime.UtcNow.Month;
        var todosClientes = await _clienteRepository.BuscarTodosAsync(c => c.CliStatus == StatusCliente.Ativo);
        List<ClienteContratoValor> todosContratos;
        try
        {
            todosContratos = (await _clienteContratoValorRepository.ListarTodosAsync()).ToList();
        }
        catch
        {
            // Se o banco ainda não tem a tabela, não há como obter vigência atual.
            todosContratos = new List<ClienteContratoValor>();
        }

        var resultado = new Dictionary<(int UsuarioId, int Mes), ValorPorMesPorUsuarioDto>();
        var meses = new[] { "Janeiro", "Fevereiro", "Março", "Abril", "Maio", "Junho", 
                           "Julho", "Agosto", "Setembro", "Outubro", "Novembro", "Dezembro" };

        foreach (var cliente in todosClientes)
        {
            var contratosCliente = todosContratos.Where(c => c.CliId == cliente.CliId).ToList();

            var usuario = await _usuarioRepository.GetByIdAsync(cliente.UsuId);
            if (usuario == null) continue;

            var pessoaUsuario = await _pessoaRepository.GetByIdAsync(usuario.PesId);
            var nomeUsuario = pessoaUsuario?.PesFantasia ?? "Desconhecido";
            var pessoaCliente = await _pessoaRepository.GetByIdAsync(cliente.PesId);

            if (contratosCliente.Count == 0)
            {
                // Sem contratos cadastrados: não exibe no dashboard (não há vigência atual).
                continue;
            }
            else
            {
                // Regra nova: valores do mês com base no contrato vigente ATUAL (hoje)
                var hoje = DateTime.UtcNow.Date;
                var vigente = contratosCliente
                    .Where(c => c.CvcDataInicio.Date <= hoje && (!c.CvcDataFim.HasValue || c.CvcDataFim.Value.Date >= hoje))
                    .OrderByDescending(c => c.CvcDataInicio)
                    .FirstOrDefault();

                if (vigente == null || vigente.CvcValorMensal <= 0) continue;

                var chave = (cliente.UsuId, mesAtual);
                if (!resultado.ContainsKey(chave))
                {
                    resultado[chave] = new ValorPorMesPorUsuarioDto
                    {
                        UsuarioId = cliente.UsuId,
                        UsuarioNome = nomeUsuario,
                        Ano = anoFiltro,
                        Mes = mesAtual,
                        MesNome = meses[mesAtual - 1],
                        ValorTotal = 0,
                        QuantidadeContratos = 0,
                        Contratos = new List<ContratoDetalheDto>()
                    };
                }

                resultado[chave].ValorTotal += vigente.CvcValorMensal;
                resultado[chave].QuantidadeContratos++;
                resultado[chave].Contratos.Add(new ContratoDetalheDto
                {
                    ClienteId = cliente.CliId,
                    ClienteCodigo = cliente.CliCodigo,
                    ClienteNome = pessoaCliente?.PesFantasia ?? "Desconhecido",
                    ValorContrato = vigente.CvcValorMensal
                });
            }
        }

        // Ordenar por usuário, ano e mês
        return resultado.Values
            .OrderBy(r => r.UsuarioNome)
            .ThenBy(r => r.Ano)
            .ThenBy(r => r.Mes)
            .ToList();
    }

    public async Task<TelemarketingContatosDto> ObterContatosTelemarketingAsync()
    {
        // Usar horário local do servidor para "dia/semana/mês/ano atual" (ex.: Brasil)
        var nowLocal = DateTime.Now;
        var hojeInicioLocal = new DateTime(nowLocal.Year, nowLocal.Month, nowLocal.Day, 0, 0, 0, DateTimeKind.Local);
        var hojeFimLocal = hojeInicioLocal.AddDays(1).AddTicks(-1);
        var hojeInicioUtc = hojeInicioLocal.ToUniversalTime();
        var hojeFimUtc = hojeFimLocal.ToUniversalTime();

        // Semana atual: domingo a hoje (fim do dia)
        var diaSemana = (int)nowLocal.DayOfWeek;
        var inicioSemanaLocal = hojeInicioLocal.AddDays(-diaSemana);
        var inicioSemanaUtc = inicioSemanaLocal.ToUniversalTime();

        // Mês atual
        var inicioMesLocal = new DateTime(nowLocal.Year, nowLocal.Month, 1, 0, 0, 0, DateTimeKind.Local);
        var inicioMesUtc = inicioMesLocal.ToUniversalTime();

        // Ano atual
        var inicioAnoLocal = new DateTime(nowLocal.Year, 1, 1, 0, 0, 0, DateTimeKind.Local);
        var inicioAnoUtc = inicioAnoLocal.ToUniversalTime();

        var todas = await _anotacaoTelemarketingRepository.BuscarTodosAsync(_ => true);
        var lista = todas.ToList();

        DateTime ToUtc(DateTime d)
        {
            return d.Kind == DateTimeKind.Utc ? d : d.Kind == DateTimeKind.Local ? d.ToUniversalTime() : DateTime.SpecifyKind(d, DateTimeKind.Utc);
        }

        int ContarNoPeriodo(DateTime inicioUtc, DateTime fimUtc)
        {
            return lista.Count(a =>
            {
                var dt = ToUtc(a.PcaDtCadastro);
                return dt >= inicioUtc && dt <= fimUtc;
            });
        }

        return new TelemarketingContatosDto
        {
            ContatosNoDia = ContarNoPeriodo(hojeInicioUtc, hojeFimUtc),
            ContatosSemanaAtual = ContarNoPeriodo(inicioSemanaUtc, hojeFimUtc),
            ContatosMesAtual = ContarNoPeriodo(inicioMesUtc, hojeFimUtc),
            ContatosAnoAtual = ContarNoPeriodo(inicioAnoUtc, hojeFimUtc)
        };
    }

    public async Task<List<AlertaContratoVencendoDto>> ObterAlertasContratosVencendoAsync(int diasAntecedencia = 30)
    {
        var hoje = DateTime.UtcNow.Date;
        var dataLimite = hoje.AddDays(diasAntecedencia);

        var clientesAtivos = (await _clienteRepository.BuscarTodosAsync(c => c.CliStatus == StatusCliente.Ativo)).ToList();

        List<ClienteContratoValor> todosContratos;
        try
        {
            todosContratos = (await _clienteContratoValorRepository.ListarTodosAsync()).ToList();
        }
        catch
        {
            // Sem tabela/contratos: não há alertas.
            return new List<AlertaContratoVencendoDto>();
        }

        var alertas = new List<AlertaContratoVencendoDto>();

        foreach (var cliente in clientesAtivos)
        {
            var contratosCliente = todosContratos.Where(c => c.CliId == cliente.CliId).ToList();
            if (contratosCliente.Count == 0) continue;

            var vigenteHoje = contratosCliente
                .Where(c => c.CvcDataInicio.Date <= hoje && (!c.CvcDataFim.HasValue || c.CvcDataFim.Value.Date >= hoje))
                .OrderByDescending(c => c.CvcDataInicio)
                .FirstOrDefault();

            if (vigenteHoje == null) continue;
            if (!vigenteHoje.CvcDataFim.HasValue) continue; // contrato sem fim definido: não vence

            var fim = vigenteHoje.CvcDataFim.Value.Date;
            if (fim < hoje) continue;
            if (fim > dataLimite) continue;

            var pessoaCliente = await _pessoaRepository.GetByIdAsync(cliente.PesId);
            var diasParaVencer = (int)Math.Ceiling((fim - hoje).TotalDays);

            alertas.Add(new AlertaContratoVencendoDto
            {
                ClienteId = cliente.CliId,
                ClienteCodigo = cliente.CliCodigo,
                ClienteNome = pessoaCliente?.PesFantasia ?? "Desconhecido",
                DataFimVigencia = fim,
                DiasParaVencer = diasParaVencer,
                ValorMensalVigente = vigenteHoje.CvcValorMensal
            });
        }

        return alertas
            .OrderBy(a => a.DiasParaVencer)
            .ThenBy(a => a.ClienteNome)
            .ToList();
    }

    private async Task<Dictionary<int, string>> ObterNomesClientesPorDuplicatasAsync(IEnumerable<Duplicata> duplicatas)
    {
        var clienteIds = duplicatas
            .Where(d => d.CliId.HasValue)
            .Select(d => d.CliId!.Value)
            .Distinct()
            .ToList();

        var nomes = new Dictionary<int, string>();
        foreach (var clienteId in clienteIds)
        {
            var cliente = await _clienteRepository.GetByIdAsync(clienteId);
            if (cliente == null) continue;

            var pessoa = await _pessoaRepository.GetByIdAsync(cliente.PesId);
            nomes[clienteId] = $"{cliente.CliCodigo} - {pessoa?.PesFantasia ?? "—"}";
        }

        return nomes;
    }

    private static ContaAPagarDto MapearContaParcela(
        Parcela parcela,
        Duplicata duplicata,
        decimal valor,
        bool paga,
        DateTime? dataPagamento,
        IReadOnlyDictionary<int, Empresa> empresas,
        IReadOnlyDictionary<int, PlanoContas> planos,
        bool incluirClassificacao,
        IReadOnlyDictionary<int, string>? clientesNomes = null)
    {
        string? centroCusto = null;
        string? planoContas = null;
        string? clienteNome = null;

        if (duplicata.CliId.HasValue && clientesNomes != null &&
            clientesNomes.TryGetValue(duplicata.CliId.Value, out var nomeCliente))
        {
            clienteNome = nomeCliente;
        }

        if (incluirClassificacao)
        {
            if (duplicata.EmpId.HasValue && empresas.TryGetValue(duplicata.EmpId.Value, out var empresa))
                centroCusto = empresa.EmpFantasia;

            var planoId = parcela.PlcId ?? duplicata.PlcId;
            if (planoId.HasValue && planos.TryGetValue(planoId.Value, out var plano))
                planoContas = plano.PlcDescricao;
        }

        return new ContaAPagarDto
        {
            ParcelaId = parcela.ParId,
            DuplicataId = duplicata.DupId,
            NumeroDuplicata = duplicata.DupNumero.ToString(),
            DescricaoDespesa = duplicata.DupDescricaoDespesa,
            DataVencimento = parcela.ParVencimento,
            DataPagamento = dataPagamento,
            Valor = valor,
            Paga = paga,
            ClienteNome = clienteNome,
            CentroCustoDescricao = centroCusto,
            PlanoContasDescricao = planoContas
        };
    }
}
