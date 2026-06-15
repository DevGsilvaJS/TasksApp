using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;

namespace Application.Services;

public class DuplicataService : IDuplicataService
{
    private const string PlanoReceitaConsultoria = "RECEITA DE CONSULTORIA";

    private readonly IRepository<Duplicata> _duplicataRepository;
    private readonly IRepository<Parcela> _parcelaRepository;
    private readonly IRepository<Cliente> _clienteRepository;
    private readonly IRepository<Pessoa> _pessoaRepository;
    private readonly IRepository<Empresa> _empresaRepository;
    private readonly IRepository<PlanoContas> _planoContasRepository;
    private readonly IRepository<CentroCusto> _centroCustoRepository;

    public DuplicataService(
        IRepository<Duplicata> duplicataRepository,
        IRepository<Parcela> parcelaRepository,
        IRepository<Cliente> clienteRepository,
        IRepository<Pessoa> pessoaRepository,
        IRepository<Empresa> empresaRepository,
        IRepository<PlanoContas> planoContasRepository,
        IRepository<CentroCusto> centroCustoRepository)
    {
        _duplicataRepository = duplicataRepository;
        _parcelaRepository = parcelaRepository;
        _clienteRepository = clienteRepository;
        _pessoaRepository = pessoaRepository;
        _empresaRepository = empresaRepository;
        _planoContasRepository = planoContasRepository;
        _centroCustoRepository = centroCustoRepository;
    }

    public async Task<DuplicataResponseDto> CadastrarDuplicataAsync(CadastroDuplicataDto dto)
    {
        await AplicarPadroesContasReceberAsync(dto);
        await ValidarClienteContasReceberAsync(dto);
        await ValidarCadastroDuplicataAsync(dto);
        int numeroDuplicata = dto.Numero;
        if (numeroDuplicata == 0)
        {
            numeroDuplicata = await ObterProximoNumeroAsync(dto.Tipo ?? "CP");
        }

        var duplicata = new Duplicata
        {
            DupNumero = numeroDuplicata,
            DupDataEmissao = dto.DataEmissao.ToUniversalTime(),
            DupNumeroParcelas = dto.NumeroParcelas,
            DupDescricaoDespesa = dto.DescricaoDespesa,
            DupTipo = dto.Tipo ?? "CP",
            CliId = dto.ClienteId,
            EmpId = dto.EmpresaId,
            PlcId = ResolverPlanoContasIdCadastro(dto)
        };

        await _duplicataRepository.InserirAsync(duplicata);
        await _duplicataRepository.SalvarAlteracoesAsync();

        if (dto.Parcelas != null && dto.Parcelas.Any())
        {
            foreach (var parcelaDto in dto.Parcelas.OrderBy(p => p.NumeroParcela))
            {
                var parcela = new Parcela
                {
                    DupId = duplicata.DupId,
                    ParNumeroParcela = parcelaDto.NumeroParcela,
                    ParValor = parcelaDto.Valor,
                    ParMulta = parcelaDto.Multa ?? dto.Multa ?? 0,
                    ParJuros = parcelaDto.Juros ?? dto.Juros ?? 0,
                    ParVencimento = parcelaDto.Vencimento.ToUniversalTime(),
                    ParStatus = "Pendente",
                    ParDataPagamento = null,
                    PlcId = ResolverPlanoContasIdCadastro(dto)
                };
                await _parcelaRepository.InserirAsync(parcela);
            }
        }
        else
        {
            if (!dto.DataPrimeiroVencimento.HasValue)
            {
                throw new InvalidOperationException("Data de primeiro vencimento é obrigatória quando não há parcelas personalizadas.");
            }

            var valorPorParcela = dto.ValorTotal;
            var planoIdCadastro = ResolverPlanoContasIdCadastro(dto);

            for (int i = 1; i <= dto.NumeroParcelas; i++)
            {
                var parcela = new Parcela
                {
                    DupId = duplicata.DupId,
                    ParNumeroParcela = i,
                    ParValor = valorPorParcela,
                    ParMulta = dto.Multa ?? 0,
                    ParJuros = dto.Juros ?? 0,
                    ParVencimento = dto.DataPrimeiroVencimento.Value.AddMonths(i - 1).ToUniversalTime(),
                    ParStatus = "Pendente",
                    ParDataPagamento = null,
                    PlcId = planoIdCadastro
                };
                await _parcelaRepository.InserirAsync(parcela);
            }
        }

        await _parcelaRepository.SalvarAlteracoesAsync();

        return await MontarDuplicataResponseDto(duplicata);
    }

    public async Task<DuplicataResponseDto?> ObterDuplicataPorIdAsync(int id)
    {
        var duplicata = await _duplicataRepository.GetByIdAsync(id);
        if (duplicata == null)
            return null;

        return await MontarDuplicataResponseDto(duplicata);
    }

    public async Task<IEnumerable<DuplicataResponseDto>> ListarTodasDuplicatasAsync()
    {
        var duplicatas = await _duplicataRepository.ListarTodosAsync();
        var duplicatasDto = new List<DuplicataResponseDto>();

        foreach (var duplicata in duplicatas)
        {
            duplicatasDto.Add(await MontarDuplicataResponseDto(duplicata));
        }

        return duplicatasDto.OrderByDescending(d => d.DataEmissao);
    }

    public async Task<IEnumerable<DuplicataResponseDto>> ListarDuplicatasPorTipoAsync(string tipo)
    {
        var duplicatas = await _duplicataRepository.BuscarTodosAsync(d => d.DupTipo == tipo);
        var duplicatasDto = new List<DuplicataResponseDto>();

        foreach (var duplicata in duplicatas)
        {
            duplicatasDto.Add(await MontarDuplicataResponseDto(duplicata));
        }

        return duplicatasDto.OrderByDescending(d => d.DataEmissao);
    }

    public async Task<DuplicataResponseDto> AtualizarDuplicataAsync(int id, CadastroDuplicataDto dto)
    {
        await AplicarPadroesContasReceberAsync(dto);
        await ValidarClienteContasReceberAsync(dto);
        await ValidarCadastroDuplicataAsync(dto);

        var duplicata = await _duplicataRepository.GetByIdAsync(id);
        if (duplicata == null)
            throw new InvalidOperationException("Duplicata não encontrada.");

        var parcelas = await _parcelaRepository.BuscarTodosAsync(p => p.DupId == duplicata.DupId);
        var temParcelaPaga = parcelas.Any(ParcelaEstaPaga);

        if (temParcelaPaga)
        {
            return await AtualizarDuplicataComParcelasPagasAsync(duplicata, dto, parcelas);
        }

        duplicata.DupNumero = dto.Numero;
        duplicata.DupDataEmissao = dto.DataEmissao.ToUniversalTime();
        duplicata.DupNumeroParcelas = dto.NumeroParcelas;
        duplicata.DupDescricaoDespesa = dto.DescricaoDespesa;
        duplicata.DupTipo = dto.Tipo ?? "CP";
        duplicata.CliId = dto.ClienteId;
        duplicata.EmpId = dto.EmpresaId;
        duplicata.PlcId = ResolverPlanoContasIdCadastro(dto);

        await _duplicataRepository.AtualizarAsync(duplicata);

        var parcelasAntigas = await _parcelaRepository.BuscarTodosAsync(p => p.DupId == duplicata.DupId);
        foreach (var parcela in parcelasAntigas)
        {
            await _parcelaRepository.ExcluirAsync(parcela);
        }
        await _parcelaRepository.SalvarAlteracoesAsync();

        var planoIdCadastro = ResolverPlanoContasIdCadastro(dto);

        if (dto.Parcelas != null && dto.Parcelas.Any())
        {
            foreach (var parcelaDto in dto.Parcelas.OrderBy(p => p.NumeroParcela))
            {
                var parcela = new Parcela
                {
                    DupId = duplicata.DupId,
                    ParNumeroParcela = parcelaDto.NumeroParcela,
                    ParValor = parcelaDto.Valor,
                    ParMulta = parcelaDto.Multa ?? dto.Multa ?? 0,
                    ParJuros = parcelaDto.Juros ?? dto.Juros ?? 0,
                    ParVencimento = parcelaDto.Vencimento.ToUniversalTime(),
                    ParStatus = "Pendente",
                    ParDataPagamento = null,
                    PlcId = planoIdCadastro
                };
                await _parcelaRepository.InserirAsync(parcela);
            }
        }
        else
        {
            if (!dto.DataPrimeiroVencimento.HasValue)
            {
                throw new InvalidOperationException("Data de primeiro vencimento é obrigatória quando não há parcelas personalizadas.");
            }

            var valorPorParcela = dto.ValorTotal;
            for (int i = 1; i <= dto.NumeroParcelas; i++)
            {
                var parcela = new Parcela
                {
                    DupId = duplicata.DupId,
                    ParNumeroParcela = i,
                    ParValor = valorPorParcela,
                    ParMulta = dto.Multa ?? 0,
                    ParJuros = dto.Juros ?? 0,
                    ParVencimento = dto.DataPrimeiroVencimento.Value.AddMonths(i - 1).ToUniversalTime(),
                    ParStatus = "Pendente",
                    ParDataPagamento = null,
                    PlcId = planoIdCadastro
                };
                await _parcelaRepository.InserirAsync(parcela);
            }
        }
        await _parcelaRepository.SalvarAlteracoesAsync();

        return await MontarDuplicataResponseDto(duplicata);
    }

    public async Task ExcluirDuplicataAsync(int id)
    {
        var duplicata = await _duplicataRepository.GetByIdAsync(id);
        if (duplicata == null)
            throw new InvalidOperationException("Duplicata não encontrada.");

        var parcelas = await _parcelaRepository.BuscarTodosAsync(p => p.DupId == duplicata.DupId);
        var temParcelaPaga = parcelas.Any(ParcelaEstaPaga);

        if (temParcelaPaga)
        {
            throw new InvalidOperationException("Não é possível excluir uma duplicata que possui parcelas pagas.");
        }

        foreach (var parcela in parcelas)
        {
            await _parcelaRepository.ExcluirAsync(parcela);
        }
        await _parcelaRepository.SalvarAlteracoesAsync();

        await _duplicataRepository.ExcluirAsync(duplicata);
        await _duplicataRepository.SalvarAlteracoesAsync();
    }

    public async Task<ParcelaResponseDto> BaixarParcelaAsync(int parcelaId, BaixarParcelaDto? dto = null)
    {
        var parcela = await _parcelaRepository.GetByIdAsync(parcelaId);
        if (parcela == null)
            throw new InvalidOperationException("Parcela não encontrada.");

        if (ParcelaEstaPaga(parcela))
        {
            throw new InvalidOperationException("Parcela já está paga.");
        }

        var duplicata = await _duplicataRepository.GetByIdAsync(parcela.DupId);
        if (duplicata == null)
            throw new InvalidOperationException("Duplicata não encontrada.");

        var isContasReceber = string.Equals(duplicata.DupTipo, "CR", StringComparison.OrdinalIgnoreCase);
        var centrosPorId = (await _centroCustoRepository.ListarTodosAsync()).ToDictionary(c => c.CcuId);
        var empresaId = ResolverEmpresaIdDuplicata(duplicata, centrosPorId);

        if (dto?.PlanoContasId is > 0)
            parcela.PlcId = dto.PlanoContasId;
        if (!parcela.PlcId.HasValue)
            parcela.PlcId = duplicata.PlcId;

        if (isContasReceber && (!parcela.PlcId.HasValue || parcela.PlcId.Value <= 0))
            parcela.PlcId = await ObterPlanoReceitaConsultoriaIdAsync();

        var planoId = parcela.PlcId ?? duplicata.PlcId;

        if (!empresaId.HasValue || empresaId.Value <= 0)
            throw new InvalidOperationException("Informe o centro de custo na duplicata antes de baixar a parcela.");

        if (isContasReceber)
        {
            if (!planoId.HasValue || planoId.Value <= 0)
                planoId = await ObterPlanoReceitaConsultoriaIdAsync();
        }
        else if (planoId is > 0)
        {
            await ValidarPlanoProibidoEmContasPagarAsync(planoId.Value);
        }

        parcela.PlcId = planoId is > 0 ? planoId : null;
        parcela.ParStatus = "Paga";
        parcela.ParDataPagamento = DateTime.UtcNow;

        await _parcelaRepository.AtualizarAsync(parcela);
        await _parcelaRepository.SalvarAlteracoesAsync();

        return await MontarParcelaResponseDtoAsync(parcela, duplicata);
    }

    public async Task<ParcelaResponseDto> ReativarParcelaAsync(int parcelaId)
    {
        var parcela = await _parcelaRepository.GetByIdAsync(parcelaId);
        if (parcela == null)
            throw new InvalidOperationException("Parcela não encontrada.");

        if (!ParcelaEstaPaga(parcela))
        {
            throw new InvalidOperationException("Apenas parcelas pagas podem ser reativadas.");
        }

        parcela.ParStatus = "Pendente";
        parcela.ParDataPagamento = null;

        await _parcelaRepository.AtualizarAsync(parcela);
        await _parcelaRepository.SalvarAlteracoesAsync();

        var duplicata = await _duplicataRepository.GetByIdAsync(parcela.DupId);
        return await MontarParcelaResponseDtoAsync(parcela, duplicata);
    }

    public async Task<ParcelaResponseDto> AtualizarClassificacaoParcelaAsync(int parcelaId, AtualizarClassificacaoParcelaDto dto)
    {
        var parcela = await _parcelaRepository.GetByIdAsync(parcelaId);
        if (parcela == null)
            throw new InvalidOperationException("Parcela não encontrada.");

        if (!string.Equals(parcela.ParStatus, "Paga", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("A classificação só pode ser alterada em parcelas pagas.");

        if (dto.EmpresaId <= 0)
            throw new InvalidOperationException("Centro de custo é obrigatório.");

        var duplicata = await _duplicataRepository.GetByIdAsync(parcela.DupId);
        if (duplicata == null)
            throw new InvalidOperationException("Duplicata não encontrada.");

        var empresa = await _empresaRepository.GetByIdAsync(dto.EmpresaId);
        if (empresa == null)
            throw new InvalidOperationException("Centro de custo informado não existe.");

        var isContasReceber = string.Equals(duplicata.DupTipo, "CR", StringComparison.OrdinalIgnoreCase);
        int? planoId = dto.PlanoContasId;

        if (isContasReceber && (!planoId.HasValue || planoId.Value <= 0))
            planoId = await ObterPlanoReceitaConsultoriaIdAsync();

        if (planoId is > 0)
        {
            if (isContasReceber)
            {
                var plano = await _planoContasRepository.GetByIdAsync(planoId.Value);
                if (plano == null)
                    throw new InvalidOperationException("Plano de contas informado não existe.");
            }
            else
            {
                await ValidarPlanoProibidoEmContasPagarAsync(planoId.Value);
            }
        }

        duplicata.EmpId = dto.EmpresaId;
        parcela.PlcId = planoId is > 0 ? planoId : null;

        await _duplicataRepository.AtualizarAsync(duplicata);
        await _parcelaRepository.AtualizarAsync(parcela);
        await _parcelaRepository.SalvarAlteracoesAsync();

        return await MontarParcelaResponseDtoAsync(parcela, duplicata);
    }

    private async Task<DuplicataResponseDto> MontarDuplicataResponseDto(Duplicata duplicata)
    {
        var parcelas = await _parcelaRepository.BuscarTodosAsync(p => p.DupId == duplicata.DupId);
        var parcelasDto = new List<ParcelaResponseDto>();
        foreach (var parcela in parcelas.OrderBy(p => p.ParNumeroParcela))
        {
            parcelasDto.Add(await MontarParcelaResponseDtoAsync(parcela, duplicata));
        }

        var valorTotal = parcelasDto.Sum(p => p.Valor);
        var valorPago = parcelasDto.Where(p => ParcelaDtoEstaPaga(p.Status)).Sum(p => p.ValorTotal);
        var valorPendente = valorTotal - valorPago;

        string? clienteNome = null;
        if (duplicata.CliId.HasValue)
        {
            var cliente = await _clienteRepository.GetByIdAsync(duplicata.CliId.Value);
            if (cliente != null)
            {
                var pessoa = await _pessoaRepository.GetByIdAsync(cliente.PesId);
                clienteNome = pessoa?.PesFantasia ?? "—";
            }
        }

        var centrosPorId = (await _centroCustoRepository.ListarTodosAsync()).ToDictionary(c => c.CcuId);
        var empresaId = ResolverEmpresaIdDuplicata(duplicata, centrosPorId);

        string? centroCustoDescricao = null;
        if (empresaId.HasValue)
        {
            var empresa = await _empresaRepository.GetByIdAsync(empresaId.Value);
            centroCustoDescricao = empresa?.EmpFantasia;
        }

        string? planoContasDescricao = null;
        if (duplicata.PlcId.HasValue)
        {
            var plano = await _planoContasRepository.GetByIdAsync(duplicata.PlcId.Value);
            planoContasDescricao = plano?.PlcDescricao;
        }

        return new DuplicataResponseDto
        {
            DuplicataId = duplicata.DupId,
            Numero = duplicata.DupNumero,
            DataEmissao = duplicata.DupDataEmissao,
            NumeroParcelas = duplicata.DupNumeroParcelas,
            DescricaoDespesa = duplicata.DupDescricaoDespesa,
            Tipo = duplicata.DupTipo,
            ClienteId = duplicata.CliId,
            ClienteNome = clienteNome,
            EmpresaId = empresaId,
            CentroCustoDescricao = centroCustoDescricao,
            PlanoContasId = duplicata.PlcId,
            PlanoContasDescricao = planoContasDescricao,
            Parcelas = parcelasDto,
            ValorTotal = valorTotal,
            ValorPago = valorPago,
            ValorPendente = valorPendente
        };
    }

    public async Task<int> ObterProximoNumeroAsync(string tipo)
    {
        var duplicatas = await _duplicataRepository.BuscarTodosAsync(d => d.DupTipo == tipo);

        if (!duplicatas.Any())
        {
            return 1;
        }

        var maiorNumero = duplicatas.Max(d => d.DupNumero);
        return maiorNumero + 1;
    }

    private async Task<DuplicataResponseDto> AtualizarDuplicataComParcelasPagasAsync(
        Duplicata duplicata,
        CadastroDuplicataDto dto,
        IEnumerable<Parcela> parcelas)
    {
        duplicata.DupDescricaoDespesa = dto.DescricaoDespesa;
        duplicata.DupDataEmissao = dto.DataEmissao.ToUniversalTime();
        duplicata.CliId = dto.ClienteId;
        duplicata.EmpId = dto.EmpresaId;
        var planoId = ResolverPlanoContasIdCadastro(dto);
        duplicata.PlcId = planoId;

        await _duplicataRepository.AtualizarAsync(duplicata);

        foreach (var parcela in parcelas)
        {
            parcela.PlcId = planoId;
            await _parcelaRepository.AtualizarAsync(parcela);
        }

        await _parcelaRepository.SalvarAlteracoesAsync();
        return await MontarDuplicataResponseDto(duplicata);
    }

    private static bool ParcelaEstaPaga(Parcela parcela) =>
        ParcelaDtoEstaPaga(parcela.ParStatus);

    private static bool ParcelaDtoEstaPaga(string? status) =>
        string.Equals(status, "Paga", StringComparison.OrdinalIgnoreCase);

    private async Task AplicarPadroesContasReceberAsync(CadastroDuplicataDto dto)
    {
        if (!string.Equals(dto.Tipo, "CR", StringComparison.OrdinalIgnoreCase))
            return;

        if (!dto.PlanoContasId.HasValue || dto.PlanoContasId.Value <= 0)
            dto.PlanoContasId = await ObterPlanoReceitaConsultoriaIdAsync();
    }

    private async Task<int> ObterPlanoReceitaConsultoriaIdAsync()
    {
        var planos = await _planoContasRepository.ListarTodosAsync();
        var plano = planos.FirstOrDefault(p =>
            string.Equals(p.PlcDescricao.Trim(), PlanoReceitaConsultoria, StringComparison.OrdinalIgnoreCase));
        if (plano == null)
            throw new InvalidOperationException($"Plano de contas {PlanoReceitaConsultoria} não está cadastrado.");

        return plano.PlcId;
    }

    private async Task ValidarClienteContasReceberAsync(CadastroDuplicataDto dto)
    {
        if (!string.Equals(dto.Tipo, "CR", StringComparison.OrdinalIgnoreCase))
            return;

        if (!dto.ClienteId.HasValue || dto.ClienteId.Value <= 0)
            throw new InvalidOperationException("Cliente é obrigatório em contas a receber.");

        var cliente = await _clienteRepository.GetByIdAsync(dto.ClienteId.Value);
        if (cliente == null)
            throw new InvalidOperationException("Cliente informado não existe.");
    }

    private async Task ValidarCadastroDuplicataAsync(CadastroDuplicataDto dto)
    {
        if (!dto.EmpresaId.HasValue || dto.EmpresaId.Value <= 0)
            throw new InvalidOperationException("Centro de custo é obrigatório.");

        var empresa = await _empresaRepository.GetByIdAsync(dto.EmpresaId.Value);
        if (empresa == null)
            throw new InvalidOperationException("Centro de custo informado não existe.");

        var isContasReceber = string.Equals(dto.Tipo, "CR", StringComparison.OrdinalIgnoreCase);

        if (isContasReceber)
        {
            if (!dto.PlanoContasId.HasValue || dto.PlanoContasId.Value <= 0)
                throw new InvalidOperationException("Plano de contas é obrigatório em contas a receber.");

            var planoCr = await _planoContasRepository.GetByIdAsync(dto.PlanoContasId.Value);
            if (planoCr == null)
                throw new InvalidOperationException("Plano de contas informado não existe.");
        }
        else if (dto.PlanoContasId is > 0)
        {
            await ValidarPlanoProibidoEmContasPagarAsync(dto.PlanoContasId.Value);
        }
    }

    private static bool EhPlanoReceitaConsultoria(PlanoContas plano) =>
        string.Equals(plano.PlcDescricao.Trim(), PlanoReceitaConsultoria, StringComparison.OrdinalIgnoreCase);

    private async Task ValidarPlanoProibidoEmContasPagarAsync(int planoId)
    {
        var plano = await _planoContasRepository.GetByIdAsync(planoId);
        if (plano == null)
            throw new InvalidOperationException("Plano de contas informado não existe.");

        if (EhPlanoReceitaConsultoria(plano))
            throw new InvalidOperationException("O plano de contas RECEITA DE CONSULTORIA só pode ser utilizado em contas a receber.");
    }

    private static int? ResolverPlanoContasIdCadastro(CadastroDuplicataDto dto) =>
        dto.PlanoContasId is > 0 ? dto.PlanoContasId : null;

    /// <summary>Centro de custo sempre na duplicata (EMPID ou legado CCUID).</summary>
    private static int? ResolverEmpresaIdDuplicata(
        Duplicata? duplicata,
        IReadOnlyDictionary<int, CentroCusto> centrosPorId)
    {
        if (duplicata?.EmpId is > 0)
            return duplicata.EmpId;

        if (duplicata?.CcuId.HasValue == true && centrosPorId.TryGetValue(duplicata.CcuId.Value, out var centro))
            return centro.EmpId;

        return null;
    }

    private async Task<ParcelaResponseDto> MontarParcelaResponseDtoAsync(Parcela parcela, Duplicata? duplicata)
    {
        var valorTotal = parcela.ParValor + parcela.ParMulta + parcela.ParJuros;
        var centrosPorId = (await _centroCustoRepository.ListarTodosAsync()).ToDictionary(c => c.CcuId);
        var empresaId = ResolverEmpresaIdDuplicata(duplicata, centrosPorId);
        var planoContasId = parcela.PlcId ?? duplicata?.PlcId;

        string? centroCustoDescricao = null;
        if (empresaId.HasValue)
        {
            var empresa = await _empresaRepository.GetByIdAsync(empresaId.Value);
            centroCustoDescricao = empresa?.EmpFantasia;
        }

        string? planoContasDescricao = null;
        if (planoContasId.HasValue)
        {
            var plano = await _planoContasRepository.GetByIdAsync(planoContasId.Value);
            planoContasDescricao = plano?.PlcDescricao;
        }

        return new ParcelaResponseDto
        {
            ParcelaId = parcela.ParId,
            DuplicataId = parcela.DupId,
            NumeroParcela = parcela.ParNumeroParcela,
            Valor = parcela.ParValor,
            Multa = parcela.ParMulta,
            Juros = parcela.ParJuros,
            ValorTotal = valorTotal,
            Vencimento = parcela.ParVencimento,
            Status = parcela.ParStatus,
            DataPagamento = parcela.ParDataPagamento,
            EmpresaId = empresaId,
            CentroCustoDescricao = centroCustoDescricao,
            PlanoContasId = planoContasId,
            PlanoContasDescricao = planoContasDescricao
        };
    }
}
