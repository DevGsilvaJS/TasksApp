using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class NotaServicoService : INotaServicoService
{
    private readonly IRepository<Cliente> _clienteRepository;
    private readonly IRepository<Pessoa> _pessoaRepository;
    private readonly IRepository<EnvioNotaServico> _envioRepository;

    public NotaServicoService(
        IRepository<Cliente> clienteRepository,
        IRepository<Pessoa> pessoaRepository,
        IRepository<EnvioNotaServico> envioRepository)
    {
        _clienteRepository = clienteRepository;
        _pessoaRepository = pessoaRepository;
        _envioRepository = envioRepository;
    }

    public async Task<List<NotaServicoItemDto>> ListarNotasDoMesAsync(int ano, int mes)
    {
        var clientesComDiaNf = await _clienteRepository.BuscarTodosAsync(c =>
            c.CliStatus == StatusCliente.Ativo && c.CliDiaNfServico.HasValue && c.CliDiaNfServico >= 1 && c.CliDiaNfServico <= 31);

        var resultado = new List<NotaServicoItemDto>();
        foreach (var cliente in clientesComDiaNf)
        {
            var envio = await _envioRepository.BuscarAsync(e =>
                e.CliId == cliente.CliId && e.EnsAno == ano && e.EnsMes == mes);

            if (envio == null)
            {
                envio = new EnvioNotaServico
                {
                    CliId = cliente.CliId,
                    EnsAno = ano,
                    EnsMes = mes,
                    EnsDataEnvio = null
                };
                await _envioRepository.InserirAsync(envio);
                await _envioRepository.SalvarAlteracoesAsync();
            }

            var pessoa = await _pessoaRepository.GetByIdAsync(cliente.PesId);
            resultado.Add(new NotaServicoItemDto
            {
                ClienteId = cliente.CliId,
                ClienteCodigo = cliente.CliCodigo,
                Fantasia = pessoa?.PesFantasia ?? "—",
                DiaNfServico = cliente.CliDiaNfServico!.Value,
                Ano = ano,
                Mes = mes,
                Enviado = envio.EnsDataEnvio.HasValue,
                DataEnvio = envio.EnsDataEnvio,
                EnvioNotaServicoId = envio.EnsId
            });
        }

        return resultado.OrderBy(x => x.DiaNfServico).ThenBy(x => x.Fantasia).ToList();
    }

    public async Task<List<NotaServicoItemDto>> ListarPendentesDoMesAsync(int ano, int mes)
    {
        var todas = await ListarNotasDoMesAsync(ano, mes);
        return todas.Where(x => !x.Enviado).ToList();
    }

    public async Task<NotaServicoItemDto?> MarcarComoEnviadoAsync(int clienteId, int ano, int mes, DateTime? dataEnvio = null)
    {
        var cliente = await _clienteRepository.GetByIdAsync(clienteId);
        if (cliente == null || !cliente.CliDiaNfServico.HasValue)
            return null;

        var envio = await _envioRepository.BuscarAsync(e =>
            e.CliId == clienteId && e.EnsAno == ano && e.EnsMes == mes);

        if (envio == null)
        {
            envio = new EnvioNotaServico
            {
                CliId = clienteId,
                EnsAno = ano,
                EnsMes = mes,
                EnsDataEnvio = (dataEnvio ?? DateTime.UtcNow).ToUniversalTime()
            };
            await _envioRepository.InserirAsync(envio);
        }
        else
        {
            envio.EnsDataEnvio = (dataEnvio ?? DateTime.UtcNow).ToUniversalTime();
            await _envioRepository.AtualizarAsync(envio);
        }

        await _envioRepository.SalvarAlteracoesAsync();

        var pessoa = await _pessoaRepository.GetByIdAsync(cliente.PesId);
        return new NotaServicoItemDto
        {
            ClienteId = cliente.CliId,
            ClienteCodigo = cliente.CliCodigo,
            Fantasia = pessoa?.PesFantasia ?? "—",
            DiaNfServico = cliente.CliDiaNfServico.Value,
            Ano = ano,
            Mes = mes,
            Enviado = true,
            DataEnvio = envio.EnsDataEnvio,
            EnvioNotaServicoId = envio.EnsId
        };
    }
}
