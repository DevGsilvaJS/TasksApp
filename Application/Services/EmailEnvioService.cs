using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;

namespace Application.Services;

public class EmailEnvioService : IEmailEnvioService
{
    private readonly IRepository<EmailEnvioComercial> _repository;

    public EmailEnvioService(IRepository<EmailEnvioComercial> repository)
    {
        _repository = repository;
    }

    public async Task<DestinatariosEmailPaginadoResponseDto> PesquisarDestinatariosAsync(
        string? termo,
        int pagina,
        int tamanhoPagina)
    {
        pagina = pagina < 1 ? 1 : pagina;
        tamanhoPagina = tamanhoPagina < 1 ? 15 : tamanhoPagina;

        var lista = await _repository.ListarTodosAsync();
        var query = lista.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(termo))
        {
            var filtro = termo.Trim().ToUpperInvariant();
            query = query.Where(e => e.EecEmail.Contains(filtro, StringComparison.Ordinal));
        }

        var ordenada = query.OrderBy(e => e.EecEmail).ToList();
        var total = ordenada.Count;
        var totalPaginas = total == 0 ? 0 : (int)Math.Ceiling(total / (double)tamanhoPagina);
        if (totalPaginas > 0 && pagina > totalPaginas)
            pagina = totalPaginas;

        var itens = ordenada
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .Select(MapearParaDto)
            .ToList();

        return new DestinatariosEmailPaginadoResponseDto
        {
            Itens = itens,
            Total = total,
            Pagina = pagina,
            TamanhoPagina = tamanhoPagina,
            TotalPaginas = totalPaginas
        };
    }

    public async Task<DestinatarioEmailResponseDto?> AtualizarNaoEnviarAsync(int id, bool naoEnviar)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            return null;

        entity.EecNaoEnviar = naoEnviar;
        await _repository.AtualizarAsync(entity);
        await _repository.SalvarAlteracoesAsync();
        return MapearParaDto(entity);
    }

    private static DestinatarioEmailResponseDto MapearParaDto(EmailEnvioComercial entity) =>
        new()
        {
            Id = entity.EecId,
            Email = entity.EecEmail,
            NaoEnviar = entity.EecNaoEnviar
        };
}
