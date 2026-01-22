using Application.DTOs;

namespace Application.Interfaces;

public interface IAnotacaoGeralService
{
    Task<AnotacaoGeralResponseDto> CadastrarAnotacaoAsync(CadastroAnotacaoGeralDto dto);
    Task<IEnumerable<AnotacaoGeralResponseDto>> ListarTodasAnotacoesAsync();
    Task<AnotacaoGeralResponseDto?> ObterAnotacaoPorIdAsync(int id);
    Task<AnotacaoGeralResponseDto> AtualizarAnotacaoAsync(int id, CadastroAnotacaoGeralDto dto);
    Task ExcluirAnotacaoAsync(int id);
}
