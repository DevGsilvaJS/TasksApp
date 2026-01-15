using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;

namespace Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IRepository<Tarefa> _tarefaRepository;
    private readonly IRepository<Parcela> _parcelaRepository;
    private readonly IRepository<Duplicata> _duplicataRepository;
    private readonly IRepository<Usuario> _usuarioRepository;
    private readonly IRepository<Cliente> _clienteRepository;
    private readonly IRepository<Pessoa> _pessoaRepository;

    public DashboardService(
        IRepository<Tarefa> tarefaRepository,
        IRepository<Parcela> parcelaRepository,
        IRepository<Duplicata> duplicataRepository,
        IRepository<Usuario> usuarioRepository,
        IRepository<Cliente> clienteRepository,
        IRepository<Pessoa> pessoaRepository)
    {
        _tarefaRepository = tarefaRepository;
        _parcelaRepository = parcelaRepository;
        _duplicataRepository = duplicataRepository;
        _usuarioRepository = usuarioRepository;
        _clienteRepository = clienteRepository;
        _pessoaRepository = pessoaRepository;
    }

    public async Task<DashboardEstatisticasDto> ObterEstatisticasAsync(DateTime dataInicio, DateTime dataFim)
    {
        // Normalizar datas para UTC
        var inicioUtc = dataInicio.ToUniversalTime();
        var fimUtc = dataFim.ToUniversalTime().AddDays(1).AddTicks(-1); // Incluir o dia inteiro

        // 1. Atendimentos por usuário
        var todasTarefas = await _tarefaRepository.BuscarTodosAsync(t => 
            t.TarDtCadastro.HasValue && 
            t.TarDtCadastro.Value >= inicioUtc && 
            t.TarDtCadastro.Value <= fimUtc);

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
                    if (cliente != null)
                    {
                        var pessoaCliente = await _pessoaRepository.GetByIdAsync(cliente.PesId);
                        detalhes.Add(new DetalheAtendimentoDto
                        {
                            TarefaId = tarefa.TarId,
                            Numero = tarefa.TarNumero,
                            ClienteId = cliente.CliId,
                            ClienteCodigo = cliente.CliCodigo,
                            ClienteNome = pessoaCliente?.PesFantasia ?? "Desconhecido"
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

        // 2. Contas a pagar (parcelas não pagas no período)
        var todasParcelas = await _parcelaRepository.BuscarTodosAsync(p => 
            p.ParVencimento >= inicioUtc && 
            p.ParVencimento <= fimUtc && 
            p.ParStatus == "Pendente");

        var todasDuplicatas = await _duplicataRepository.ListarTodosAsync();
        var duplicatasDict = todasDuplicatas.ToDictionary(d => d.DupId);

        var contasAPagar = todasParcelas
            .Where(p => duplicatasDict.ContainsKey(p.DupId))
            .Select(parcela =>
            {
                var duplicata = duplicatasDict[parcela.DupId];
                return new ContaAPagarDto
                {
                    ParcelaId = parcela.ParId,
                    DuplicataId = duplicata.DupId,
                    NumeroDuplicata = duplicata.DupNumero.ToString(),
                    DataVencimento = parcela.ParVencimento,
                    Valor = (decimal)parcela.ParValor,
                    Paga = parcela.ParStatus == "Paga"
                };
            })
            .ToList();

        // 3. Atendimentos por cliente
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
            if (cliente != null)
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

        // 4. Contas pagas no mês atual
        var mesAtual = DateTime.UtcNow;
        // Criar data de início do mês em UTC (primeiro dia do mês, 00:00:00)
        var inicioMesAtual = new DateTime(mesAtual.Year, mesAtual.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        // Criar data de fim do mês em UTC (último dia do mês, 23:59:59.999)
        var fimMesAtual = new DateTime(mesAtual.Year, mesAtual.Month, DateTime.DaysInMonth(mesAtual.Year, mesAtual.Month), 23, 59, 59, 999, DateTimeKind.Utc);

        // Buscar todas as parcelas pagas
        var todasParcelasPagas = await _parcelaRepository.BuscarTodosAsync(p => p.ParStatus == "Paga" && p.ParDataPagamento.HasValue);
        
        // Filtrar apenas as do mês atual, garantindo comparação correta em UTC
        var contasPagasMes = todasParcelasPagas.Where(p => 
        {
            if (!p.ParDataPagamento.HasValue) return false;
            
            var dataPagamento = p.ParDataPagamento.Value;
            // Garantir que estamos comparando em UTC
            var dataPagamentoUtc = dataPagamento.Kind == DateTimeKind.Utc 
                ? dataPagamento 
                : dataPagamento.ToUniversalTime();
            
            // Comparar apenas ano e mês para evitar problemas de timezone
            return dataPagamentoUtc.Year == mesAtual.Year && 
                   dataPagamentoUtc.Month == mesAtual.Month;
        }).ToList();

        var contasPagasDto = new List<ContaAPagarDto>();
        foreach (var parcela in contasPagasMes)
        {
            if (duplicatasDict.ContainsKey(parcela.DupId))
            {
                var duplicata = duplicatasDict[parcela.DupId];
                contasPagasDto.Add(new ContaAPagarDto
                {
                    ParcelaId = parcela.ParId,
                    DuplicataId = parcela.DupId,
                    NumeroDuplicata = duplicata.DupNumero.ToString(),
                    DataVencimento = parcela.ParVencimento,
                    Valor = (decimal)(parcela.ParValor + parcela.ParMulta + parcela.ParJuros),
                    Paga = true
                });
            }
        }

        var valorTotalContasPagas = contasPagasDto.Sum(c => c.Valor);

        return new DashboardEstatisticasDto
        {
            TotalAtendimentosPorUsuario = atendimentosPorUsuarioDto.Sum(a => a.Quantidade),
            TotalContasAPagar = contasAPagar.Count(),
            TotalAtendimentosPorCliente = atendimentosPorClienteDto.Sum(a => a.Quantidade),
            TotalContasPagas = contasPagasDto.Count,
            ValorTotalContasPagas = valorTotalContasPagas,
            AtendimentosPorUsuario = atendimentosPorUsuarioDto,
            ContasAPagar = contasAPagar,
            ContasPagas = contasPagasDto,
            AtendimentosPorCliente = atendimentosPorClienteDto
        };
    }

    public async Task<List<ValorPorMesPorUsuarioDto>> ObterValoresPorMesPorUsuarioAsync(int? ano = null)
    {
        var anoFiltro = ano ?? DateTime.UtcNow.Year;
        
        // Buscar todos os clientes com valor de contrato
        var todosClientes = await _clienteRepository.BuscarTodosAsync(c => 
            c.CliValorContrato.HasValue && 
            c.CliValorContrato.Value > 0);

        var resultado = new Dictionary<(int UsuarioId, int Mes), ValorPorMesPorUsuarioDto>();
        var meses = new[] { "Janeiro", "Fevereiro", "Março", "Abril", "Maio", "Junho", 
                           "Julho", "Agosto", "Setembro", "Outubro", "Novembro", "Dezembro" };

        foreach (var cliente in todosClientes)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(cliente.UsuId);
            if (usuario == null) continue;

            var pessoaUsuario = await _pessoaRepository.GetByIdAsync(usuario.PesId);
            var nomeUsuario = pessoaUsuario?.PesFantasia ?? "Desconhecido";
            var pessoaCliente = await _pessoaRepository.GetByIdAsync(cliente.PesId);
            var valorContrato = cliente.CliValorContrato ?? 0;

            // Determinar os meses em que o cliente gera receita
            var mesesAtivos = new List<int>();
            // O valor do contrato é mensal, não precisa dividir
            decimal valorMensal = valorContrato;
            
            if (cliente.CliDataFinalContrato.HasValue && cliente.CliDataCadastro.HasValue)
            {
                // Se tem data final, verificar quais meses do ano filtro estão dentro do período
                var dataInicio = cliente.CliDataCadastro.Value;
                var dataFim = cliente.CliDataFinalContrato.Value;
                
                // Para cada mês no ano filtro que está dentro do período do contrato
                for (int mes = 1; mes <= 12; mes++)
                {
                    var dataMes = new DateTime(anoFiltro, mes, 1);
                    var ultimoDiaMes = dataMes.AddMonths(1).AddDays(-1);
                    
                    // Verificar se o mês está dentro do período do contrato
                    if (dataMes <= dataFim.Date && ultimoDiaMes >= dataInicio.Date)
                    {
                        mesesAtivos.Add(mes);
                    }
                }
            }
            else if (cliente.CliDataCadastro.HasValue)
            {
                // Se não tem data final, considerar apenas o mês de cadastro no ano filtro
                var dataCadastro = cliente.CliDataCadastro.Value;
                if (dataCadastro.Year == anoFiltro)
                {
                    mesesAtivos.Add(dataCadastro.Month);
                }
            }

            // Adicionar o valor mensal para cada mês ativo
            foreach (var mes in mesesAtivos)
            {
                var chave = (cliente.UsuId, mes);
                
                if (!resultado.ContainsKey(chave))
                {
                    resultado[chave] = new ValorPorMesPorUsuarioDto
                    {
                        UsuarioId = cliente.UsuId,
                        UsuarioNome = nomeUsuario,
                        Ano = anoFiltro,
                        Mes = mes,
                        MesNome = meses[mes - 1],
                        ValorTotal = 0,
                        QuantidadeContratos = 0,
                        Contratos = new List<ContratoDetalheDto>()
                    };
                }

                resultado[chave].ValorTotal += valorMensal;
                resultado[chave].QuantidadeContratos++;
                resultado[chave].Contratos.Add(new ContratoDetalheDto
                {
                    ClienteId = cliente.CliId,
                    ClienteCodigo = cliente.CliCodigo,
                    ClienteNome = pessoaCliente?.PesFantasia ?? "Desconhecido",
                    ValorContrato = valorMensal
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
}
