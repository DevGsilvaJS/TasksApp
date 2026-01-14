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

        return new DashboardEstatisticasDto
        {
            TotalAtendimentosPorUsuario = atendimentosPorUsuarioDto.Sum(a => a.Quantidade),
            TotalContasAPagar = contasAPagar.Count(),
            TotalAtendimentosPorCliente = atendimentosPorClienteDto.Sum(a => a.Quantidade),
            AtendimentosPorUsuario = atendimentosPorUsuarioDto,
            ContasAPagar = contasAPagar,
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
            decimal valorPorMes = valorContrato;
            
            if (cliente.CliDataFinalContrato.HasValue && cliente.CliDataCadastro.HasValue)
            {
                // Se tem data final, calcular meses entre cadastro e data final
                var dataInicio = cliente.CliDataCadastro.Value;
                var dataFim = cliente.CliDataFinalContrato.Value;
                
                // Calcular número total de meses do contrato
                var mesesContrato = ((dataFim.Year - dataInicio.Year) * 12) + (dataFim.Month - dataInicio.Month) + 1;
                if (mesesContrato > 0)
                {
                    // Distribuir o valor total pelos meses do contrato
                    valorPorMes = valorContrato / mesesContrato;
                }
                
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

            // Adicionar o valor por mês para cada mês ativo
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

                resultado[chave].ValorTotal += valorPorMes;
                resultado[chave].QuantidadeContratos++;
                resultado[chave].Contratos.Add(new ContratoDetalheDto
                {
                    ClienteId = cliente.CliId,
                    ClienteCodigo = cliente.CliCodigo,
                    ClienteNome = pessoaCliente?.PesFantasia ?? "Desconhecido",
                    ValorContrato = valorPorMes
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
