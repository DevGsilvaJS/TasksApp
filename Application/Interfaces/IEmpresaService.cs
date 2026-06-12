using Application.DTOs;

namespace Application.Interfaces;

public interface IEmpresaService
{
    Task<EmpresaResponseDto> CadastrarEmpresaAsync(CadastroEmpresaDto dto);
    Task<EmpresaResponseDto?> ObterEmpresaPorIdAsync(int id);
    Task<IEnumerable<EmpresaResponseDto>> ListarTodasEmpresasAsync();
    Task<EmpresaResponseDto> AtualizarEmpresaAsync(int id, CadastroEmpresaDto dto);
    Task ExcluirEmpresaAsync(int id);
}
