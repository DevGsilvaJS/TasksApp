using Domain.Enums;

namespace Application.Models;

public class CampanhaEmailMemoria
{
    public int Id { get; set; }
    public string Assunto { get; set; } = string.Empty;
    public string CorpoHtml { get; set; } = string.Empty;
    public StatusCampanhaEmailComercial Status { get; set; } = StatusCampanhaEmailComercial.Fila;
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    public DateTime? DataConclusao { get; set; }
    public DateTime? PausaAte { get; set; }
    public List<ItemCampanhaEmailMemoria> Itens { get; set; } = [];
}

public class ItemCampanhaEmailMemoria
{
    public string Email { get; set; } = string.Empty;
    public int Ordem { get; set; }
    public StatusItemCampanhaEmail Status { get; set; } = StatusItemCampanhaEmail.Pendente;
    public DateTime? DataEnvio { get; set; }
    public string? RemetenteEmail { get; set; }
    public string? MensagemErro { get; set; }
}
