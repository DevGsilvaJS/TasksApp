using Application.DTOs;

namespace Application.Interfaces;

public interface IEmailEnvioService
{
    Task<DestinatariosEmailPaginadoResponseDto> PesquisarDestinatariosAsync(string? termo, int pagina, int tamanhoPagina);
    Task<DestinatarioEmailResponseDto?> AtualizarNaoEnviarAsync(int id, bool naoEnviar);
}
