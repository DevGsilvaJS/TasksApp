using Application.Interfaces;
using Application.Models;
using Domain.Enums;

namespace Application.Services;

public class EmailCampanhaMemoriaStore : IEmailCampanhaMemoriaStore
{
    private const int MaxHistorico = 30;
    private readonly object _lock = new();
    private int _proximoId;
    private CampanhaEmailMemoria? _ativa;
    private readonly List<CampanhaEmailMemoria> _historico = [];

    public CampanhaEmailMemoria Criar(CampanhaEmailMemoria campanha)
    {
        lock (_lock)
        {
            campanha.Id = Interlocked.Increment(ref _proximoId);
            campanha.Status = StatusCampanhaEmailComercial.Fila;
            campanha.DataCriacao = DateTime.UtcNow;
            _ativa = campanha;
            return campanha;
        }
    }

    public CampanhaEmailMemoria? ObterPorId(int id)
    {
        lock (_lock)
        {
            if (_ativa?.Id == id)
                return _ativa;

            return _historico.FirstOrDefault(c => c.Id == id);
        }
    }

    public CampanhaEmailMemoria? ObterCampanhaAtiva()
    {
        lock (_lock)
        {
            if (_ativa == null)
                return null;

            if (_ativa.Status is StatusCampanhaEmailComercial.Fila or StatusCampanhaEmailComercial.Processando)
                return _ativa;

            return null;
        }
    }

    public IReadOnlyList<CampanhaEmailMemoria> ListarHistorico()
    {
        lock (_lock)
        {
            return _historico.OrderByDescending(c => c.DataConclusao ?? c.DataCriacao).ToList();
        }
    }

    public void Atualizar(CampanhaEmailMemoria campanha)
    {
        lock (_lock)
        {
            if (_ativa?.Id != campanha.Id)
                return;

            _ativa = campanha;
        }
    }

    public void MarcarConcluida(CampanhaEmailMemoria campanha)
    {
        lock (_lock)
        {
            if (_ativa?.Id != campanha.Id)
                return;

            campanha.Status = StatusCampanhaEmailComercial.Concluida;
            campanha.DataConclusao = DateTime.UtcNow;
            campanha.PausaAte = null;
            _historico.Insert(0, campanha);

            if (_historico.Count > MaxHistorico)
                _historico.RemoveRange(MaxHistorico, _historico.Count - MaxHistorico);

            _ativa = null;
        }
    }
}
