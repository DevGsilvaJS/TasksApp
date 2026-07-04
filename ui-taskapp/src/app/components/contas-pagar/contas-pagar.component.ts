import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DuplicataService, DuplicataResponseDto, CadastroDuplicataDto, ParcelaResponseDto, CadastroParcelaDto } from '../../services/duplicata.service';
import { EmpresaService, EmpresaResponseDto } from '../../services/empresa.service';
import {
  PlanoContasService,
  PlanoContasResponseDto,
  filtrarPlanosParaContasPagar,
  planoContasIdPermitidoEmContasPagar
} from '../../services/plano-contas.service';
import { NotificacaoService } from '../../services/notificacao.service';
import {
  criarOpcoesAgrupamento,
  carregarPreferenciaAgruparPor,
  salvarPreferenciaAgruparPor,
  deveExibirCabecalhoGrupo,
  obterRotuloAgrupamento,
  obterValorCabecalhoGrupo,
  ordenarItensParaAgrupamento
} from '../../shared/utils/grid-agrupamento.util';
import { SeletorAgrupamentoGridComponent } from '../../shared/components/seletor-agrupamento-grid/seletor-agrupamento-grid.component';

@Component({
  selector: 'app-contas-pagar',
  standalone: true,
  imports: [CommonModule, FormsModule, SeletorAgrupamentoGridComponent],
  templateUrl: './contas-pagar.component.html',
  styleUrl: './contas-pagar.component.css'
})
export class ContasPagarComponent implements OnInit {
  duplicatas: DuplicataResponseDto[] = [];
  duplicatasFiltradas: DuplicataResponseDto[] = [];
  exibirTitulosBaixados = false;
  exibirTitulosInativos = false;
  showForm = false;
  showParcelas = false;
  showModalBaixaParcela = false;
  parcelaBaixa: ParcelaResponseDto | null = null;
  dataPagamentoBaixa = '';
  loading = false;
  error: string | null = null;
  editando = false;
  edicaoParcial = false;
  duplicataEditando: DuplicataResponseDto | null = null;
  duplicataSelecionada: DuplicataResponseDto | null = null;
  termoBusca = '';
  gerarParcelasManual = false;
  parcelasManuais: CadastroParcelaDto[] = [];
  empresas: EmpresaResponseDto[] = [];
  planosContas: PlanoContasResponseDto[] = [];

  agruparPor = '';
  agruparPorOpcoes = criarOpcoesAgrupamento([
    { value: 'centroCustoDescricao', label: 'Centro de Custo' },
    { value: 'descricaoDespesa', label: 'Descrição da Despesa' },
    { value: 'dataEmissao', label: 'Data Emissão' }
  ]);
  private readonly STORAGE_KEY_AGRUPAR_POR = 'contas_pagar_agrupar_por';

  // Modal de confirmação
  showConfirmModal = false;
  confirmTitle = '';
  confirmMessage = '';
  confirmCallback: (() => void) | null = null;
  
  // Modal de sucesso
  showSuccessModal = false;
  successMessage = '';

  novaDuplicata: CadastroDuplicataDto = {
    numero: 0,
    dataEmissao: new Date().toISOString().split('T')[0],
    numeroParcelas: 1,
    valorTotal: 0,
    multa: 0,
    juros: 0,
    descricaoDespesa: undefined,
    tipo: 'CP',
    inativa: false,
    dataPrimeiroVencimento: new Date().toISOString().split('T')[0]
  };

  constructor(
    private duplicataService: DuplicataService,
    private empresaService: EmpresaService,
    private planoContasService: PlanoContasService,
    private notificacao: NotificacaoService
  ) { }

  ngOnInit() {
    this.agruparPor = carregarPreferenciaAgruparPor(this.STORAGE_KEY_AGRUPAR_POR, this.agruparPorOpcoes);
    this.carregarDuplicatas();
    this.empresaService.listarTodasEmpresas().subscribe({
      next: (data) => this.empresas = data,
      error: () => {}
    });
    this.planoContasService.listarTodosPlanosContas().subscribe({
      next: (data) => this.planosContas = filtrarPlanosParaContasPagar(data),
      error: () => {}
    });
  }

  /** Recarrega a lista de duplicatas. Opcionalmente chama onConcluido após atualizar (ex.: atualizar parcelas na tela). */
  carregarDuplicatas(onConcluido?: () => void) {
    this.loading = true;
    this.error = null;
    this.duplicataService.listarDuplicatasPorTipo('CP').subscribe({
      next: (data) => {
        this.duplicatas = data;
        this.aplicarFiltros();
        this.loading = false;
        onConcluido?.();
      },
      error: (err) => {
        this.error = 'Erro ao carregar contas a pagar. Verifique se a API está rodando.';
        this.loading = false;
        console.error(err);
      }
    });
  }

  abrirFormularioNovo() {
    this.editando = false;
    this.edicaoParcial = false;
    this.duplicataEditando = null;
    this.gerarParcelasManual = false;
    this.parcelasManuais = [];
    
    // Buscar próximo número automaticamente
    this.duplicataService.obterProximoNumero('CP').subscribe({
      next: (proximoNumero) => {
        this.novaDuplicata = {
          numero: proximoNumero,
          dataEmissao: new Date().toISOString().split('T')[0],
          numeroParcelas: 1,
          valorTotal: 0,
          multa: 0,
          juros: 0,
          descricaoDespesa: undefined,
          tipo: 'CP',
          inativa: false,
          empresaId: undefined,
          planoContasId: undefined,
          dataPrimeiroVencimento: new Date().toISOString().split('T')[0]
        };
        this.showForm = true;
        this.error = null;
        window.scrollTo(0, 0);
      },
      error: (err) => {
        this.error = 'Erro ao obter próximo número. Tente novamente.';
        console.error(err);
      }
    });
  }

  abrirFormularioEdicao(duplicata: DuplicataResponseDto) {
    this.editando = true;
    this.edicaoParcial = this.duplicataPossuiParcelaPaga(duplicata);
    this.duplicataEditando = duplicata;
    this.novaDuplicata = {
      numero: duplicata.numero,
      dataEmissao: duplicata.dataEmissao.split('T')[0],
      numeroParcelas: duplicata.numeroParcelas,
      valorTotal: duplicata.valorTotal,
      multa: duplicata.parcelas[0]?.multa || 0,
      juros: duplicata.parcelas[0]?.juros || 0,
      descricaoDespesa: duplicata.descricaoDespesa,
      tipo: duplicata.tipo || 'CP',
      inativa: duplicata.inativa ?? false,
      empresaId: duplicata.empresaId,
      planoContasId: planoContasIdPermitidoEmContasPagar(duplicata.planoContasId, this.planosContas),
      dataPrimeiroVencimento: duplicata.parcelas[0]?.vencimento.split('T')[0] || new Date().toISOString().split('T')[0]
    };
    this.showForm = true;
    this.error = null;
    window.scrollTo(0, 0);
  }

  obterLabelEmpresa(empresa: EmpresaResponseDto): string {
    const cnpj = empresa.cnpj?.replace(/\D/g, '') ?? '';
    const cnpjFmt = cnpj.length === 14
      ? cnpj.replace(/^(\d{2})(\d{3})(\d{3})(\d{4})(\d{2})$/, '$1.$2.$3/$4-$5')
      : empresa.cnpj ?? '';
    return `${empresa.fantasia}${cnpjFmt ? ' — ' + cnpjFmt : ''}`;
  }

  fecharFormulario() {
    this.showForm = false;
    this.editando = false;
    this.edicaoParcial = false;
    this.duplicataEditando = null;
    this.gerarParcelasManual = false;
    this.parcelasManuais = [];
    this.error = null;
  }

  toggleGerarParcelasManual() {
    // O valor já foi atualizado pelo ngModel
    if (this.gerarParcelasManual) {
      // Ativando geração manual - gerar parcelas
      this.gerarParcelas();
    } else {
      // Desativando geração manual - limpar parcelas
      this.parcelasManuais = [];
      this.novaDuplicata.parcelas = undefined;
    }
  }

  gerarParcelas() {
    const numParcelas = this.novaDuplicata.numeroParcelas || 1;
    // O valor total informado é o valor de cada parcela, não o total dividido
    const valorPorParcela = this.novaDuplicata.valorTotal;
    this.parcelasManuais = [];

    for (let i = 1; i <= numParcelas; i++) {
      const dataVencimento = this.novaDuplicata.dataPrimeiroVencimento 
        ? new Date(this.novaDuplicata.dataPrimeiroVencimento)
        : new Date();
      
      // Se não for a primeira parcela, adiciona meses
      if (i > 1 && this.novaDuplicata.dataPrimeiroVencimento) {
        dataVencimento.setMonth(dataVencimento.getMonth() + (i - 1));
      }

      this.parcelasManuais.push({
        numeroParcela: i,
        valor: valorPorParcela,
        vencimento: dataVencimento.toISOString().split('T')[0],
        multa: this.novaDuplicata.multa || 0,
        juros: this.novaDuplicata.juros || 0
      });
    }
  }

  atualizarNumeroParcelas() {
    if (this.gerarParcelasManual) {
      this.gerarParcelas();
    }
  }

  atualizarValorTotal() {
    if (this.gerarParcelasManual && this.parcelasManuais.length > 0) {
      // O valor total informado é o valor de cada parcela, não o total dividido
      const valorPorParcela = this.novaDuplicata.valorTotal;
      this.parcelasManuais.forEach(p => p.valor = valorPorParcela);
    }
  }

  atualizarMultaJuros() {
    if (this.gerarParcelasManual && this.parcelasManuais.length > 0) {
      this.parcelasManuais.forEach(p => {
        if (this.novaDuplicata.multa !== undefined) {
          p.multa = this.novaDuplicata.multa;
        }
        if (this.novaDuplicata.juros !== undefined) {
          p.juros = this.novaDuplicata.juros;
        }
      });
    }
  }

  salvarDuplicata() {
    if (this.loading) return;

    if (!this.novaDuplicata.empresaId) {
      this.error = 'Selecione o centro de custo.';
      this.notificacao.aviso(this.error);
      return;
    }
    if (this.edicaoParcial) {
      this.novaDuplicata.parcelas = undefined;
    } else if (this.gerarParcelasManual) {
      if (this.parcelasManuais.length !== this.novaDuplicata.numeroParcelas) {
        this.error = 'Número de parcelas não corresponde ao número informado.';
        this.notificacao.aviso(this.error);
        return;
      }
      
      const todasTemData = this.parcelasManuais.every(p => p.vencimento);
      if (!todasTemData) {
        this.error = 'Todas as parcelas devem ter data de vencimento preenchida.';
        this.notificacao.aviso(this.error);
        return;
      }

      this.novaDuplicata.parcelas = this.parcelasManuais;
      this.novaDuplicata.dataPrimeiroVencimento = undefined;
    } else {
      this.novaDuplicata.parcelas = undefined;
    }

    // Para novo cadastro, enviar número como 0 para gerar automaticamente
    if (!this.editando) {
      this.novaDuplicata.numero = 0;
    }

    this.novaDuplicata.tipo = 'CP';
    this.novaDuplicata.inativa = this.novaDuplicata.inativa === true;

    this.loading = true;
    this.error = null;

    const operacao = this.editando && this.duplicataEditando
      ? this.duplicataService.atualizarDuplicata(this.duplicataEditando.duplicataId, this.novaDuplicata)
      : this.duplicataService.cadastrarDuplicata(this.novaDuplicata);

    operacao.subscribe({
      next: () => {
        const inativou = this.novaDuplicata.inativa === true;
        this.carregarDuplicatas();
        this.fecharFormulario();
        this.loading = false;
        if (inativou) {
          this.notificacao.sucesso('Conta a pagar inativada com sucesso.');
        } else {
          this.notificacao.sucesso(this.editando ? 'Conta a pagar atualizada com sucesso.' : 'Conta a pagar cadastrada com sucesso.');
        }
      },
      error: (err) => {
        this.error = err.error?.message || 'Erro ao salvar conta a pagar.';
        this.loading = false;
        console.error(err);
      }
    });
  }

  baixarParcela(parcela: ParcelaResponseDto) {
    this.parcelaBaixa = parcela;
    this.dataPagamentoBaixa = this.obterDataHojeInput();
    this.showModalBaixaParcela = true;
  }

  fecharModalBaixaParcela() {
    this.showModalBaixaParcela = false;
    this.parcelaBaixa = null;
    this.dataPagamentoBaixa = '';
  }

  get dataHojeInput(): string {
    return this.obterDataHojeInput();
  }

  confirmarBaixaParcela() {
    if (!this.parcelaBaixa) {
      return;
    }

    if (!this.dataPagamentoBaixa) {
      this.notificacao.aviso('Informe a data do pagamento.');
      return;
    }

    if (this.dataPagamentoBaixa > this.obterDataHojeInput()) {
      this.notificacao.aviso('A data do pagamento não pode ser futura.');
      return;
    }

    const parcela = this.parcelaBaixa;
    this.loading = true;
    this.error = null;

    this.duplicataService.baixarParcela(parcela.parcelaId, {
      dataPagamento: this.dataPagamentoBaixa
    }).subscribe({
      next: () => {
        this.fecharModalBaixaParcela();
        const duplicataIdSelecionada = this.duplicataSelecionada?.duplicataId;
        this.carregarDuplicatas(() => {
          if (duplicataIdSelecionada != null) {
            const duplicataAtualizada = this.duplicatas.find(d => d.duplicataId === duplicataIdSelecionada);
            if (duplicataAtualizada) {
              this.duplicataSelecionada = duplicataAtualizada;
            }
          }
          this.loading = false;
        });
        this.notificacao.sucesso('Parcela baixada com sucesso.');
      },
      error: (err) => {
        this.error = err.error?.message || 'Erro ao baixar parcela.';
        this.loading = false;
        console.error(err);
      }
    });
  }

  private obterDataHojeInput(): string {
    const hoje = new Date();
    const yyyy = hoje.getFullYear();
    const mm = String(hoje.getMonth() + 1).padStart(2, '0');
    const dd = String(hoje.getDate()).padStart(2, '0');
    return `${yyyy}-${mm}-${dd}`;
  }

  reativarParcela(parcela: ParcelaResponseDto) {
    this.confirmarReativacaoParcela(parcela);
  }

  private async confirmarReativacaoParcela(parcela: ParcelaResponseDto): Promise<void> {
    const ok = await this.notificacao.confirmar(
      'Confirmar reativação',
      `Deseja reativar a parcela ${parcela.numeroParcela}?`,
      'Confirmar',
      'Cancelar'
    );
    if (!ok) return;

      this.loading = true;
      this.error = null;

      this.duplicataService.reativarParcela(parcela.parcelaId).subscribe({
        next: () => {
          const duplicataIdSelecionada = this.duplicataSelecionada?.duplicataId;
          this.carregarDuplicatas(() => {
            if (duplicataIdSelecionada != null) {
              const duplicataAtualizada = this.duplicatas.find(d => d.duplicataId === duplicataIdSelecionada);
              if (duplicataAtualizada) {
                this.duplicataSelecionada = duplicataAtualizada;
              }
            }
          });
          this.notificacao.sucesso('Parcela reativada com sucesso.');
        },
        error: (err) => {
          this.error = err.error?.message || 'Erro ao reativar parcela.';
          this.loading = false;
          console.error(err);
        }
      });
  }

  confirmarAcao() {
    if (this.confirmCallback) {
      this.confirmCallback();
    }
    this.fecharConfirmModal();
  }

  fecharConfirmModal() {
    this.showConfirmModal = false;
    this.confirmTitle = '';
    this.confirmMessage = '';
    this.confirmCallback = null;
  }

  fecharSuccessModal() {
    this.showSuccessModal = false;
    this.successMessage = '';
  }

  abrirParcelas(duplicata: DuplicataResponseDto) {
    this.duplicataSelecionada = duplicata;
    this.showParcelas = true;
  }

  fecharParcelas() {
    this.showParcelas = false;
    this.duplicataSelecionada = null;
  }

  parcelaPaga(parcela: ParcelaResponseDto): boolean {
    return parcela.status?.toLowerCase() === 'paga';
  }

  parcelaPendente(parcela: ParcelaResponseDto): boolean {
    return parcela.status?.toLowerCase() === 'pendente';
  }

  filtrarDuplicatas() {
    this.aplicarFiltros();
  }

  onToggleExibirTitulosBaixados(exibir: boolean) {
    this.exibirTitulosBaixados = exibir;
    this.aplicarFiltros();
  }

  onToggleExibirTitulosInativos(exibir: boolean) {
    this.exibirTitulosInativos = exibir;
    this.aplicarFiltros();
  }

  tituloInativo(duplicata: DuplicataResponseDto): boolean {
    return duplicata.inativa === true;
  }

  possuiTitulosOcultosPorFiltro(): boolean {
    if (this.duplicatas.length === 0) {
      return false;
    }

    return this.duplicatas.some((duplicata) => {
      if (this.tituloInativo(duplicata)) {
        return !this.exibirTitulosInativos;
      }

      if (this.possuiParcelaEmAberto(duplicata)) {
        return false;
      }

      return !this.exibirTitulosBaixados;
    });
  }

  mensagemListaVazia(): string {
    if (this.termoBusca.trim()) {
      return 'Nenhuma duplicata encontrada para a busca.';
    }

    if (this.possuiTitulosOcultosPorFiltro()) {
      if (!this.exibirTitulosInativos) {
        return 'Existem títulos inativos ocultos. Marque "Exibir títulos inativos" para visualizá-los.';
      }

      return 'Existem títulos baixados ocultos. Marque "Exibir títulos baixados" para visualizá-los.';
    }

    return 'Nenhuma duplicata cadastrada.';
  }

  private aplicarFiltros(): void {
    const termoBuscaNormalizado: string = this.termoBusca.trim().toLowerCase();

    let lista: DuplicataResponseDto[] = this.duplicatas;

    lista = lista.filter((duplicata: DuplicataResponseDto) => {
      if (this.tituloInativo(duplicata)) {
        return this.exibirTitulosInativos;
      }

      if (this.possuiParcelaEmAberto(duplicata)) {
        return true;
      }

      return this.exibirTitulosBaixados;
    });

    if (termoBuscaNormalizado) {
      lista = lista.filter((duplicata: DuplicataResponseDto) =>
        duplicata.numero.toString().includes(termoBuscaNormalizado) ||
        duplicata.dataEmissao.toLowerCase().includes(termoBuscaNormalizado)
      );
    }

    this.duplicatasFiltradas = lista;
  }

  private possuiParcelaEmAberto(duplicata: DuplicataResponseDto): boolean {
    return duplicata.parcelas?.some((parcela: ParcelaResponseDto) => parcela.status?.toLowerCase() === 'pendente') ?? false;
  }

  duplicataPossuiParcelaPaga(duplicata: DuplicataResponseDto): boolean {
    return duplicata.parcelas?.some((parcela: ParcelaResponseDto) => parcela.status?.toLowerCase() === 'paga') ?? false;
  }

  obterValorParcela(duplicata: DuplicataResponseDto): number {
    const primeiraParcela = duplicata.parcelas?.[0];
    if (primeiraParcela?.valor != null) {
      return primeiraParcela.valor;
    }

    if (duplicata.numeroParcelas > 0) {
      return duplicata.valorTotal / duplicata.numeroParcelas;
    }

    return 0;
  }

  formatarMoeda(valor: number): string {
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: 'BRL'
    }).format(valor);
  }

  formatarData(data: string): string {
    if (!data) {
      return '-';
    }

    // Usa só a parte da data (yyyy-MM-dd) para não recuar um dia por fuso UTC
    const parte = data.length >= 10 ? data.substring(0, 10) : data;
    if (/^\d{4}-\d{2}-\d{2}$/.test(parte)) {
      const [ano, mes, dia] = parte.split('-');
      return `${dia}/${mes}/${ano}`;
    }

    return new Date(data).toLocaleDateString('pt-BR');
  }

  getStatusClass(status: string): string {
    switch (status.toLowerCase()) {
      case 'paga':
        return 'status-paga';
      case 'pendente':
        return 'status-pendente';
      case 'cancelada':
        return 'status-cancelada';
      default:
        return '';
    }
  }

  onAgruparPorChange(valor: string) {
    this.agruparPor = valor;
    salvarPreferenciaAgruparPor(this.STORAGE_KEY_AGRUPAR_POR, valor);
  }

  get duplicatasParaTabela(): DuplicataResponseDto[] {
    return ordenarItensParaAgrupamento(this.duplicatasFiltradas, this.agruparPor);
  }

  getAgruparPorLabel(): string {
    return obterRotuloAgrupamento(this.agruparPorOpcoes, this.agruparPor);
  }

  getValorGrupoDuplicata(duplicata: DuplicataResponseDto): string {
    if (this.agruparPor === 'dataEmissao') {
      return this.formatarData(duplicata.dataEmissao);
    }

    return obterValorCabecalhoGrupo(duplicata as unknown as Record<string, unknown>, this.agruparPor);
  }

  exibirCabecalhoGrupoDuplicata(index: number): boolean {
    return deveExibirCabecalhoGrupo(
      this.duplicatasParaTabela,
      index,
      this.agruparPor,
      (duplicata) => this.getValorGrupoDuplicata(duplicata)
    );
  }
}
