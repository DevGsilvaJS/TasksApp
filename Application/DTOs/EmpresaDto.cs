using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class CadastroEmpresaDto
{
    [Required(ErrorMessage = "CNPJ é obrigatório")]
    [MaxLength(20)]
    public string Cnpj { get; set; } = string.Empty;

    [Required(ErrorMessage = "Razão social é obrigatória")]
    [MaxLength(500)]
    public string RazaoSocial { get; set; } = string.Empty;

    [Required(ErrorMessage = "Fantasia é obrigatória")]
    [MaxLength(300)]
    public string Fantasia { get; set; } = string.Empty;
}

public class EmpresaResponseDto
{
    public int EmpresaId { get; set; }
    public int? CentroCustoId { get; set; }
    public string Cnpj { get; set; } = string.Empty;
    public string RazaoSocial { get; set; } = string.Empty;
    public string Fantasia { get; set; } = string.Empty;
}
