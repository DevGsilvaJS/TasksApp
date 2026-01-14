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
                Quantidade = g.Count()
            })
            .ToList();

        var atendimentosPorUsuarioDto = new List<AtendimentoPorUsuarioDto>();
        foreach (var item in atendimentosPorUsuario)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(item.UsuarioId);
            if (usuario != null)
            {
                var pessoa = await _pessoaRepository.GetByIdAsync(usuario.PesId);
                atendimentosPorUsuarioDto.Add(new AtendimentoPorUsuarioDto
                {
                    UsuarioId = item.UsuarioId,
                    UsuarioNome = pessoa?.PesFantasia ?? "Desconhecido",
                    Quantidade = item.Quantidade
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
}
