using Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces;

public interface IEmailEnvioCampanhaService
{
    Task<EnfileirarCampanhaEmailResponseDto> EnfileirarCampanhaAsync(
        string assunto,
        string corpoHtml,
        IEnumerable<string> emailsDestinatarios,
        IEnumerable<IFormFile>? anexos);

    Task<CampanhaEmailStatusResponseDto?> ObterStatusCampanhaAsync(int campanhaId);
    Task<CampanhaEmailStatusResponseDto?> ObterCampanhaAtivaAsync();
    Task<RelatorioCampanhaEmailResponseDto?> ObterRelatorioAsync(int campanhaId);
    Task<IReadOnlyList<RelatorioCampanhaEmailResponseDto>> ListarRelatoriosAsync();
}
