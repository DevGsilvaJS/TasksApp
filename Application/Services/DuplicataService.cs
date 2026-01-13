using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;

namespace Application.Services;

public class DuplicataService : IDuplicataService
{
    private readonly IRepository<Duplicata> _duplicataRepository;
    private readonly IRepository<Parcela> _parcelaRepository;

    public DuplicataService(
        IRepository<Duplicata> duplicataRepository,
        IRepository<Parcela> parcelaRepository)
    {
        _duplicataRepository = duplicataRepository;
        _parcelaRepository = parcelaRepository;
    }

    public async Task<DuplicataResponseDto> CadastrarDuplicataAsync(CadastroDuplicataDto dto)
    {
        // Criar Duplicata
        var duplicata = new Duplicata
        {
            DupNumero = dto.Numero,
            DupDataEmissao = dto.DataEmissao.ToUniversalTime(),
            DupNumeroParcelas = dto.NumeroParcelas
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
                    ParDataPagamento = null
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

            var valorPorParcela = dto.ValorTotal / dto.NumeroParcelas;

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
                    ParDataPagamento = null
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

    public async Task<DuplicataResponseDto> AtualizarDuplicataAsync(int id, CadastroDuplicataDto dto)
    {
        var duplicata = await _duplicataRepository.GetByIdAsync(id);
        if (duplicata == null)
            throw new InvalidOperationException("Duplicata não encontrada.");

        // Verificar se há parcelas pagas
        var parcelas = await _parcelaRepository.BuscarTodosAsync(p => p.DupId == duplicata.DupId);
        var temParcelaPaga = parcelas.Any(p => p.ParStatus == "Paga");
        
        if (temParcelaPaga)
        {
            throw new InvalidOperationException("Não é possível atualizar uma duplicata que possui parcelas pagas.");
        }

        // Atualizar Duplicata
        duplicata.DupNumero = dto.Numero;
        duplicata.DupDataEmissao = dto.DataEmissao.ToUniversalTime();
        duplicata.DupNumeroParcelas = dto.NumeroParcelas;

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
                    ParDataPagamento = null
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

            var valorPorParcela = dto.ValorTotal / dto.NumeroParcelas;
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
                    ParDataPagamento = null
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
        var temParcelaPaga = parcelas.Any(p => p.ParStatus == "Paga");
        
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

        if (parcela.ParStatus == "Paga")
        {
            throw new InvalidOperationException("Parcela já está paga.");
        }

        parcela.ParStatus = "Paga";
        parcela.ParDataPagamento = DateTime.UtcNow;

        await _parcelaRepository.AtualizarAsync(parcela);
        await _parcelaRepository.SalvarAlteracoesAsync();

        return MontarParcelaResponseDto(parcela);
    }

    private async Task<DuplicataResponseDto> MontarDuplicataResponseDto(Duplicata duplicata)
    {
        var parcelas = await _parcelaRepository.BuscarTodosAsync(p => p.DupId == duplicata.DupId);
        var parcelasDto = parcelas.Select(MontarParcelaResponseDto).OrderBy(p => p.NumeroParcela).ToList();

        var valorTotal = parcelasDto.Sum(p => p.Valor);
        var valorPago = parcelasDto.Where(p => p.Status == "Paga").Sum(p => p.ValorTotal);
        var valorPendente = valorTotal - valorPago;

        return new DuplicataResponseDto
        {
            DuplicataId = duplicata.DupId,
            Numero = duplicata.DupNumero,
            DataEmissao = duplicata.DupDataEmissao,
            NumeroParcelas = duplicata.DupNumeroParcelas,
            Parcelas = parcelasDto,
            ValorTotal = valorTotal,
            ValorPago = valorPago,
            ValorPendente = valorPendente
        };
    }

    private ParcelaResponseDto MontarParcelaResponseDto(Parcela parcela)
    {
        var valorTotal = parcela.ParValor + parcela.ParMulta + parcela.ParJuros;

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
            DataPagamento = parcela.ParDataPagamento
        };
    }
}
