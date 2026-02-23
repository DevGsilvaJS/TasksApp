namespace Application.DTOs;

/// <summary>
/// Contagens de contatos (anotações/registros) da tela de Telemarketing por período.
/// Baseado na data de cadastro do registro (TB_POSSIVEL_CLIENTE_ANOTACAO.PCADTCADASTRO).
/// </summary>
public class TelemarketingContatosDto
{
    public int ContatosNoDia { get; set; }
    public int ContatosSemanaAtual { get; set; }
    public int ContatosMesAtual { get; set; }
    public int ContatosAnoAtual { get; set; }
}
