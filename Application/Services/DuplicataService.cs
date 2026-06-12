using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;

namespace Application.Services;

public class DuplicataService : IDuplicataService
{
    private readonly IRepository<Duplicata> _duplicataRepository;
    private readonly IRepository<Parcela> _parcelaRepository;
    private readonly IRepository<Cliente> _clienteRepository;
    private readonly IRepository<Pessoa> _pessoaRepository;
    private readonly IRepository<CentroCusto> _centroCustoRepository;
    private readonly IRepository<Empresa> _empresaRepository;
    private readonly IRepository<PlanoContas> _planoContasRepository;

    public DuplicataService(
        IRepository<Duplicata> duplicataRepository,
        IRepository<Parcela> parcelaRepository,
        IRepository<Cliente> clienteRepository,
        IRepository<Pessoa> pessoaRepository,
        IRepository<CentroCusto> centroCustoRepository,
        IRepository<Empresa> empresaRepository,
        IRepository<PlanoContas> planoContasRepository)
    {
        _duplicataRepository = duplicataRepository;
        _parcelaRepository = parcelaRepository;
        _clienteRepository = clienteRepository;
        _pessoaRepository = pessoaRepository;
        _centroCustoRepository = centroCustoRepository;
        _empresaRepository = empresaRepository;
        _planoContasRepository = planoContasRepository;
    }

    public async Task<DuplicataResponseDto> CadastrarDuplicataAsync(CadastroDuplicataDto dto)
    {
        await ValidarCentroEPlanoContasAsync(dto);
        // Se número não foi informado, buscar o próximo disponível para o tipo
        int numeroDuplicata = dto.Numero;
        if (numeroDuplicata == 0)
        {
            numeroDuplicata = await ObterProximoNumeroAsync(dto.Tipo ?? "CP");
        }

        // Criar Duplicata
        var duplicata = new Duplicata
        {
            DupNumero = numeroDuplicata,
            DupDataEmissao = dto.DataEmissao.ToUniversalTime(),
            DupNumeroParcelas = dto.NumeroParcelas,
            DupDescricaoDespesa = dto.DescricaoDespesa,
            DupTipo = dto.Tipo ?? "CP",
            CliId = dto.ClienteId,
            CcuId = dto.CentroCustoId,
            PlcId = dto.PlanoContasId
        };

        await _duplicataRepository.InserirAsync(duplicata);
        await _duplicataRepository.SalvarAlteracoesAsync();

        // Verificar se há parcelas personalizadas
        if (dto.Parcelas != null && dto.Parcelas.Any())
        {
            // Usar parcelas personalizadas
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
                    CcuId = dto.CentroCustoId,
                    PlcId = dto.PlanoContasId
                };
                await _parcelaRepository.InserirAsync(parcela);
            }
        }
        else
        {
            // Gerar parcelas automaticamente
            if (!dto.DataPrimeiroVencimento.HasValue)
            {
                throw new InvalidOperationException("Data de primeiro vencimento é obrigatória quando não há parcelas personalizadas.");
            }

            // O valor total informado é o valor de cada parcela, não o total dividido
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
                    CcuId = dto.CentroCustoId,
                    PlcId = dto.PlanoContasId
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
        await ValidarCentroEPlanoContasAsync(dto);

        var duplicata = await _duplicataRepository.GetByIdAsync(id);
        if (duplicata == null)
            throw new InvalidOperationException("Duplicata não encontrada.");

        var parcelas = await _parcelaRepository.BuscarTodosAsync(p => p.DupId == duplicata.DupId);
        var temParcelaPaga = parcelas.Any(ParcelaEstaPaga);

        if (temParcelaPaga)
        {
            return await AtualizarDuplicataComParcelasPagasAsync(duplicata, dto, parcelas);
        }

        // Atualizar Duplicata
        duplicata.DupNumero = dto.Numero;
        duplicata.DupDataEmissao = dto.DataEmissao.ToUniversalTime();
        duplicata.DupNumeroParcelas = dto.NumeroParcelas;
        duplicata.DupDescricaoDespesa = dto.DescricaoDespesa;
        duplicata.DupTipo = dto.Tipo ?? "CP";
        duplicata.CliId = dto.ClienteId;
        duplicata.CcuId = dto.CentroCustoId;
        duplicata.PlcId = dto.PlanoContasId;

        await _duplicataRepository.AtualizarAsync(duplicata);

        // Remover parcelas antigas
        var parcelasAntigas = await _parcelaRepository.BuscarTodosAsync(p => p.DupId == duplicata.DupId);
        foreach (var parcela in parcelasAntigas)
        {
            await _parcelaRepository.ExcluirAsync(parcela);
        }
        await _parcelaRepository.SalvarAlteracoesAsync();

        // Verificar se há parcelas personalizadas
        if (dto.Parcelas != null && dto.Parcelas.Any())
        {
            // Usar parcelas personalizadas
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
                    CcuId = dto.CentroCustoId,
                    PlcId = dto.PlanoContasId
                };
                await _parcelaRepository.InserirAsync(parcela);
            }
        }
        else
        {
            // Gerar parcelas automaticamente
            if (!dto.DataPrimeiroVencimento.HasValue)
            {
                throw new InvalidOperationException("Data de primeiro vencimento é obrigatória quando não há parcelas personalizadas.");
            }

            // O valor total informado é o valor de cada parcela, não o total dividido
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
                    CcuId = dto.CentroCustoId,
                    PlcId = dto.PlanoContasId
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

        // Verificar se há parcelas pagas
        var parcelas = await _parcelaRepository.BuscarTodosAsync(p => p.DupId == duplicata.DupId);
        var temParcelaPaga = parcelas.Any(ParcelaEstaPaga);

        if (temParcelaPaga)
        {
            throw new InvalidOperationException("Não é possível excluir uma duplicata que possui parcelas pagas.");
        }

        // Excluir parcelas
        foreach (var parcela in parcelas)
        {
            await _parcelaRepository.ExcluirAsync(parcela);
        }
        await _parcelaRepository.SalvarAlteracoesAsync();

        // Excluir duplicata
        await _duplicataRepository.ExcluirAsync(duplicata);
        await _duplicataRepository.SalvarAlteracoesAsync();
    }

    public async Task<ParcelaResponseDto> BaixarParcelaAsync(int parcelaId)
    {
        var parcela = await _parcelaRepository.GetByIdAsync(parcelaId);
        if (parcela == null)
            throw new InvalidOperationException("Parcela não encontrada.");

        if (ParcelaEstaPaga(parcela))
        {
            throw new InvalidOperationException("Parcela já está paga.");
        }

        var duplicata = await _duplicataRepository.GetByIdAsync(parcela.DupId);

        parcela.ParStatus = "Paga";
        parcela.ParDataPagamento = DateTime.UtcNow;
        if (!parcela.CcuId.HasValue)
            parcela.CcuId = duplicata?.CcuId;
        if (!parcela.PlcId.HasValue)
            parcela.PlcId = duplicata?.PlcId;

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

        if (dto.CentroCustoId <= 0)
            throw new InvalidOperationException("Centro de custo é obrigatório.");

        if (dto.PlanoContasId <= 0)
            throw new InvalidOperationException("Plano de contas é obrigatório.");

        var centro = await _centroCustoRepository.GetByIdAsync(dto.CentroCustoId);
        if (centro == null)
            throw new InvalidOperationException("Centro de custo informado não existe.");

        var plano = await _planoContasRepository.GetByIdAsync(dto.PlanoContasId);
        if (plano == null)
            throw new InvalidOperationException("Plano de contas informado não existe.");

        parcela.CcuId = dto.CentroCustoId;
        parcela.PlcId = dto.PlanoContasId;

        await _parcelaRepository.AtualizarAsync(parcela);
        await _parcelaRepository.SalvarAlteracoesAsync();

        var duplicata = await _duplicataRepository.GetByIdAsync(parcela.DupId);
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

        string? centroEmpresaFantasia = null;
        if (duplicata.CcuId.HasValue)
        {
            var centro = await _centroCustoRepository.GetByIdAsync(duplicata.CcuId.Value);
            if (centro != null)
            {
                var empresa = await _empresaRepository.GetByIdAsync(centro.EmpId);
                centroEmpresaFantasia = empresa?.EmpFantasia;
            }
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
            CentroCustoId = duplicata.CcuId,
            CentroCustoEmpresaFantasia = centroEmpresaFantasia,
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
        duplicata.CcuId = dto.CentroCustoId;
        duplicata.PlcId = dto.PlanoContasId;

        await _duplicataRepository.AtualizarAsync(duplicata);

        foreach (var parcela in parcelas)
        {
            parcela.CcuId = dto.CentroCustoId;
            parcela.PlcId = dto.PlanoContasId;
            await _parcelaRepository.AtualizarAsync(parcela);
        }

        await _parcelaRepository.SalvarAlteracoesAsync();
        return await MontarDuplicataResponseDto(duplicata);
    }

    private static bool ParcelaEstaPaga(Parcela parcela) =>
        ParcelaDtoEstaPaga(parcela.ParStatus);

    private static bool ParcelaDtoEstaPaga(string? status) =>
        string.Equals(status, "Paga", StringComparison.OrdinalIgnoreCase);

    private async Task ValidarCentroEPlanoContasAsync(CadastroDuplicataDto dto)
    {
        if (!dto.CentroCustoId.HasValue || dto.CentroCustoId.Value <= 0)
            throw new InvalidOperationException("Centro de custo é obrigatório.");

        if (!dto.PlanoContasId.HasValue || dto.PlanoContasId.Value <= 0)
            throw new InvalidOperationException("Plano de contas é obrigatório.");

        var centro = await _centroCustoRepository.GetByIdAsync(dto.CentroCustoId.Value);
        if (centro == null)
            throw new InvalidOperationException("Centro de custo informado não existe.");

        var plano = await _planoContasRepository.GetByIdAsync(dto.PlanoContasId.Value);
        if (plano == null)
            throw new InvalidOperationException("Plano de contas informado não existe.");
    }

    private async Task<ParcelaResponseDto> MontarParcelaResponseDtoAsync(Parcela parcela, Duplicata? duplicata)
    {
        var valorTotal = parcela.ParValor + parcela.ParMulta + parcela.ParJuros;
        var centroCustoId = parcela.CcuId ?? duplicata?.CcuId;
        var planoContasId = parcela.PlcId ?? duplicata?.PlcId;

        string? centroEmpresaFantasia = null;
        if (centroCustoId.HasValue)
        {
            var centro = await _centroCustoRepository.GetByIdAsync(centroCustoId.Value);
            if (centro != null)
            {
                var empresa = await _empresaRepository.GetByIdAsync(centro.EmpId);
                centroEmpresaFantasia = empresa?.EmpFantasia;
            }
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
            CentroCustoId = centroCustoId,
            CentroCustoEmpresaFantasia = centroEmpresaFantasia,
            PlanoContasId = planoContasId,
            PlanoContasDescricao = planoContasDescricao
        };
    }
}
