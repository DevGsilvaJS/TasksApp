namespace Application.DTOs;

/// <summary>
/// DTO de resposta para possível cliente (importado da planilha).
/// </summary>
public class PossivelClienteResponseDto
{
    public int PocId { get; set; }
    public string PocCodigo { get; set; } = string.Empty;
    public string? PocLoja { get; set; }
    public string? PocStatus { get; set; }
    public string? PocFantasia { get; set; }
    public string? PocDdd { get; set; }
    public string? PocCnpj { get; set; }
    public string? PocRazaoSocial { get; set; }
    public string? PocEmailComercial { get; set; }
    public string? PocCelDdd { get; set; }
    public string? PocCelular { get; set; }
    public DateTime? PocDataImportacao { get; set; }
    public int? PocStatusAtendimento { get; set; }
    public string? PocMotivoPerda { get; set; }
    public DateTime? PocDataStatusAtendimento { get; set; }
}
