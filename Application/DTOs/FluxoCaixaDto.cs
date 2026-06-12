namespace Application.DTOs;

public class FluxoCaixaResponseDto
{
    public int Ano { get; set; }
    public List<FluxoCaixaCentroCustoDto> Centros { get; set; } = new();
    public List<FluxoCaixaMesDto> TotaisMensais { get; set; } = new();
    public double TotalReceitasAno { get; set; }
    public double TotalDespesasAno { get; set; }
    public double SaldoAno { get; set; }
}

public class FluxoCaixaCentroCustoDto
{
    public int EmpresaId { get; set; }
    public string EmpresaFantasia { get; set; } = string.Empty;
    public string? EmpresaCnpj { get; set; }
    public List<FluxoCaixaMesDto> Meses { get; set; } = new();
    public List<FluxoCaixaPlanoContasDto> PlanosContas { get; set; } = new();
    public double TotalReceitas { get; set; }
    public double TotalDespesas { get; set; }
    public double Saldo { get; set; }
}

public class FluxoCaixaPlanoContasDto
{
    public int PlanoContasId { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public List<FluxoCaixaMesDto> Meses { get; set; } = new();
    public double TotalReceitas { get; set; }
    public double TotalDespesas { get; set; }
    public double Saldo { get; set; }
}

public class FluxoCaixaMesDto
{
    public int Mes { get; set; }
    public string NomeMes { get; set; } = string.Empty;
    public double Receitas { get; set; }
    public double Despesas { get; set; }
    public double Saldo { get; set; }
}
