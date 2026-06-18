using Application.Models;
using Domain.Enums;

namespace Application.Interfaces;

public interface IEmailCampanhaMemoriaStore
{
    CampanhaEmailMemoria Criar(CampanhaEmailMemoria campanha);
    CampanhaEmailMemoria? ObterPorId(int id);
    CampanhaEmailMemoria? ObterCampanhaAtiva();
    IReadOnlyList<CampanhaEmailMemoria> ListarHistorico();
    void Atualizar(CampanhaEmailMemoria campanha);
    void MarcarConcluida(CampanhaEmailMemoria campanha);
}
