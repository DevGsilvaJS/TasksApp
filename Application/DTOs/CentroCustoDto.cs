using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class CadastroCentroCustoDto
{
    [Required(ErrorMessage = "Empresa é obrigatória")]
    public int EmpresaId { get; set; }
}

public class CentroCustoResponseDto
{
    public int CentroCustoId { get; set; }
    public int EmpresaId { get; set; }
    public string? EmpresaFantasia { get; set; }
    public string? EmpresaCnpj { get; set; }
}
