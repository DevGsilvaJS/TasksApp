using Application.DTOs;

namespace Application.Interfaces;

public interface ICadastroAtendimentoService
{
    Task<IEnumerable<CadastroStatusTarefaResponseDto>> ListarStatusTarefaAsync(bool? apenasAtivos = null);
    Task<CadastroStatusTarefaResponseDto?> ObterStatusTarefaPorIdAsync(int id);
    Task<CadastroStatusTarefaResponseDto> CriarStatusTarefaAsync(CadastroStatusTarefaRequestDto dto);
    Task<CadastroStatusTarefaResponseDto?> AtualizarStatusTarefaAsync(int id, CadastroStatusTarefaRequestDto dto);
    Task<bool> AlterarAtivoStatusTarefaAsync(int id, bool ativo);

    Task<IEnumerable<CadastroTipoAtendimentoResponseDto>> ListarTipoAtendimentoAsync(bool? apenasAtivos = null);
    Task<CadastroTipoAtendimentoResponseDto?> ObterTipoAtendimentoPorIdAsync(int id);
    Task<CadastroTipoAtendimentoResponseDto> CriarTipoAtendimentoAsync(CadastroTipoAtendimentoRequestDto dto);
    Task<CadastroTipoAtendimentoResponseDto?> AtualizarTipoAtendimentoAsync(int id, CadastroTipoAtendimentoRequestDto dto);
    Task<bool> AlterarAtivoTipoAtendimentoAsync(int id, bool ativo);
    Task ExcluirTipoAtendimentoAsync(int id);

    Task<IEnumerable<CadastroTipoContatoResponseDto>> ListarTipoContatoAsync(bool? apenasAtivos = null);
    Task<CadastroTipoContatoResponseDto?> ObterTipoContatoPorIdAsync(int id);
    Task<CadastroTipoContatoResponseDto> CriarTipoContatoAsync(CadastroTipoContatoRequestDto dto);
    Task<CadastroTipoContatoResponseDto?> AtualizarTipoContatoAsync(int id, CadastroTipoContatoRequestDto dto);
    Task<bool> AlterarAtivoTipoContatoAsync(int id, bool ativo);

    Task<IEnumerable<CadastroStatusAtendimentoComercialResponseDto>> ListarStatusAtendimentoComercialAsync(bool? apenasAtivos = null);
    Task<CadastroStatusAtendimentoComercialResponseDto?> ObterStatusAtendimentoComercialPorIdAsync(int id);
    Task<CadastroStatusAtendimentoComercialResponseDto> CriarStatusAtendimentoComercialAsync(CadastroStatusAtendimentoComercialRequestDto dto);
    Task<CadastroStatusAtendimentoComercialResponseDto?> AtualizarStatusAtendimentoComercialAsync(int id, CadastroStatusAtendimentoComercialRequestDto dto);
    Task<bool> AlterarAtivoStatusAtendimentoComercialAsync(int id, bool ativo);
    Task ExcluirStatusAtendimentoComercialAsync(int id);
}
