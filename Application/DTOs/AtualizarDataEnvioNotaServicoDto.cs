namespace Application.DTOs;

public class AtualizarDataEnvioNotaServicoDto
{
    /// <summary>
    /// Data em que a nota de serviços foi enviada ao cliente. Null para limpar.
    /// </summary>
    public DateTime? Data { get; set; }
}
