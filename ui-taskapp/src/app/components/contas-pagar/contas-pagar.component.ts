import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { OverlayPanel, OverlayPanelModule } from 'primeng/overlaypanel';
import { DuplicataService, DuplicataResponseDto, CadastroDuplicataDto, ParcelaResponseDto, CadastroParcelaDto } from '../../services/duplicata.service';
import { isParcelaPendente, labelStatusParcela } from '../../utils/parcela-status.util';
import { NotificacaoService } from '../../services/notificacao.service';

@Component({
  selector: 'app-contas-pagar',
  standalone: true,
  imports: [CommonModule, FormsModule, OverlayPanelModule],
  templateUrl: './contas-pagar.component.html',
  styleUrl: './contas-pagar.component.css'
})
export class ContasPagarComponent implements OnInit {
  duplicatas: DuplicataResponseDto[] = [];
  duplicatasFiltradas: DuplicataResponseDto[] = [];
  exibirTitulosBaixados = false;
  showForm = false;
  showParcelas = false;
  loading = false;
  error: string | null = null;
  editando = false;
  duplicataEditando: DuplicataResponseDto | null = null;
  duplicataSelecionada: DuplicataResponseDto | null = null;
  termoBusca = '';
  gerarParcelasManual = false;
  parcelasManuais: CadastroParcelaDto[] = [];

  /** Filtros por coluna: valores selecionados (vazio = sem filtro). */
  filtrosColunasSelecao: Record<string, string[]> = {};
  filtroColunaAtivo: string | null = null;
  selecaoTemp: Record<string, string[]> = {};

  @ViewChild('opFiltroColuna') opFiltroColuna!: OverlayPanel;
  
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
    dataPrimeiroVencimento: new Date().toISOString().split('T')[0]
  };

  readonly isParcelaPendente = isParcelaPendente;
  readonly labelStatusParcela = labelStatusParcela;

  constructor(
    private duplicataService: DuplicataService,
    private notificacao: NotificacaoService
  ) { }

  ngOnInit() {
    this.carregarDuplicatas();
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
    this.duplicataEditando = duplicata;
    const temParcelaPaga = duplicata.valorPago > 0;
    const primeiraParcela = duplicata.parcelas[0];

    this.gerarParcelasManual = temParcelaPaga || duplicata.parcelas.length > 1;
    if (this.gerarParcelasManual) {
      this.parcelasManuais = duplicata.parcelas.map(p => ({
        numeroParcela: p.numeroParcela,
        valor: p.valor,
        vencimento: p.vencimento.split('T')[0],
        multa: p.multa,
        juros: p.juros
      }));
    } else {
      this.parcelasManuais = [];
    }

    this.novaDuplicata = {
      numero: duplicata.numero,
      dataEmissao: duplicata.dataEmissao.split('T')[0],
      numeroParcelas: duplicata.numeroParcelas,
      valorTotal: primeiraParcela?.valor ?? 0,
      multa: primeiraParcela?.multa || 0,
      juros: primeiraParcela?.juros || 0,
      descricaoDespesa: duplicata.descricaoDespesa,
      tipo: duplicata.tipo || 'CP',
      dataPrimeiroVencimento: primeiraParcela?.vencimento.split('T')[0] || new Date().toISOString().split('T')[0]
    };
    this.showForm = true;
    this.error = null;
    window.scrollTo(0, 0);
  }

  parcelaEstaPaga(numeroParcela: number): boolean {
    if (!this.duplicataEditando) return false;
    const parcela = this.duplicataEditando.parcelas.find(p => p.numeroParcela === numeroParcela);
    return parcela ? !isParcelaPendente(parcela.status) : false;
  }

  fecharFormulario() {
    this.showForm = false;
    this.editando = false;
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

    // Validar parcelas manuais se estiver gerando manualmente
    if (this.gerarParcelasManual) {
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

    this.loading = true;
    this.error = null;

    const operacao = this.editando && this.duplicataEditando
      ? this.duplicataService.atualizarDuplicata(this.duplicataEditando.duplicataId, this.novaDuplicata)
      : this.duplicataService.cadastrarDuplicata(this.novaDuplicata);

    operacao.subscribe({
      next: () => {
        this.carregarDuplicatas();
        this.fecharFormulario();
        this.loading = false;
        this.notificacao.sucesso(this.editando ? 'Conta a pagar atualizada com sucesso.' : 'Conta a pagar cadastrada com sucesso.');
      },
      error: (err) => {
        this.error = err.error?.message || 'Erro ao salvar conta a pagar.';
        this.loading = false;
        console.error(err);
      }
    });
  }

  excluirDuplicata(duplicata: DuplicataResponseDto) {
    this.confirmarExclusaoDuplicata(duplicata);
  }

  private async confirmarExclusaoDuplicata(duplicata: DuplicataResponseDto): Promise<void> {
    const ok = await this.notificacao.confirmar(
      'Confirmar exclusão',
      `Tem certeza que deseja excluir a duplicata #${duplicata.numero}?`,
      'Excluir',
      'Cancelar'
    );
    if (!ok) return;

      this.loading = true;
      this.error = null;

      this.duplicataService.excluirDuplicata(duplicata.duplicataId).subscribe({
        next: () => {
          this.carregarDuplicatas();
          this.loading = false;
          this.notificacao.sucesso('Conta a pagar excluída com sucesso.');
        },
        error: (err) => {
          this.error = err.error?.message || 'Erro ao excluir conta a pagar.';
          this.loading = false;
          console.error(err);
        }
      });
  }

  baixarParcela(parcela: ParcelaResponseDto) {
    this.confirmarBaixaParcela(parcela);
  }

  private async confirmarBaixaParcela(parcela: ParcelaResponseDto): Promise<void> {
    const ok = await this.notificacao.confirmar(
      'Confirmar baixa',
      `Deseja confirmar o pagamento (baixa) da parcela ${parcela.numeroParcela}?`,
      'Confirmar',
      'Cancelar'
    );
    if (!ok) return;

      this.loading = true;
      this.error = null;

      this.duplicataService.baixarParcela(parcela.parcelaId).subscribe({
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
          this.notificacao.sucesso('Parcela baixada com sucesso.');
        },
        error: (err) => {
          this.error = err.error?.message || 'Erro ao baixar parcela.';
          this.loading = false;
          console.error(err);
        }
      });
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

  filtrarDuplicatas() {
    this.aplicarFiltros();
  }

  onToggleExibirTitulosBaixados() {
    this.aplicarFiltros();
  }

  private aplicarFiltros(): void {
    const termoBuscaNormalizado: string = this.termoBusca.trim().toLowerCase();

    let lista: DuplicataResponseDto[] = this.duplicatas;

    if (!this.exibirTitulosBaixados) {
      lista = lista.filter((duplicata: DuplicataResponseDto) => this.possuiParcelaEmAberto(duplicata));
    }

    if (termoBuscaNormalizado) {
      lista = lista.filter((duplicata: DuplicataResponseDto) =>
        duplicata.numero.toString().includes(termoBuscaNormalizado) ||
        duplicata.dataEmissao.toLowerCase().includes(termoBuscaNormalizado) ||
        (duplicata.descricaoDespesa?.toLowerCase().includes(termoBuscaNormalizado) ?? false)
      );
    }

    for (const [campo, valores] of Object.entries(this.filtrosColunasSelecao)) {
      if (!valores?.length) continue;
      const set = new Set(valores);
      lista = lista.filter(d => set.has(this.getValorColunaGrid(d, campo)));
    }

    this.duplicatasFiltradas = lista;
  }

  get totalValorPago(): number {
    return this.duplicatasFiltradas.reduce((s, d) => s + (d.valorPago ?? 0), 0);
  }

  get totalValorPendente(): number {
    return this.duplicatasFiltradas.reduce((s, d) => s + (d.valorPendente ?? 0), 0);
  }

  getValorColunaGrid(duplicata: DuplicataResponseDto, campo: string): string {
    switch (campo) {
      case 'numero':
        return String(duplicata.numero);
      case 'dataEmissao':
        return this.formatarData(duplicata.dataEmissao);
      case 'descricaoDespesa':
        return duplicata.descricaoDespesa?.trim() || '—';
      case 'numeroParcelas':
        return String(duplicata.numeroParcelas);
      case 'valorTotal':
        return this.formatarMoeda(duplicata.valorTotal);
      case 'valorPago':
        return this.formatarMoeda(duplicata.valorPago);
      case 'valorPendente':
        return this.formatarMoeda(duplicata.valorPendente);
      default:
        return '';
    }
  }

  getDadosParaFiltroColuna(campo: string): DuplicataResponseDto[] {
    const termoBuscaNormalizado = this.termoBusca.trim().toLowerCase();
    let lista = [...this.duplicatas];

    if (!this.exibirTitulosBaixados) {
      lista = lista.filter(d => this.possuiParcelaEmAberto(d));
    }

    if (termoBuscaNormalizado) {
      lista = lista.filter(d =>
        d.numero.toString().includes(termoBuscaNormalizado) ||
        d.dataEmissao.toLowerCase().includes(termoBuscaNormalizado) ||
        (d.descricaoDespesa?.toLowerCase().includes(termoBuscaNormalizado) ?? false)
      );
    }

    for (const [col, valores] of Object.entries(this.filtrosColunasSelecao)) {
      if (col === campo || !valores?.length) continue;
      const set = new Set(valores);
      lista = lista.filter(d => set.has(this.getValorColunaGrid(d, col)));
    }

    return lista;
  }

  getValoresDistintosColuna(campo: string): string[] {
    const set = new Set<string>();
    for (const d of this.getDadosParaFiltroColuna(campo)) {
      set.add(this.getValorColunaGrid(d, campo));
    }
    return Array.from(set).sort((a, b) => a.localeCompare(b, 'pt-BR'));
  }

  abrirFiltroColuna(campo: string, event: Event): void {
    event.stopPropagation();
    this.filtroColunaAtivo = campo;
    const all = this.getValoresDistintosColuna(campo);
    this.selecaoTemp[campo] = (this.filtrosColunasSelecao[campo]?.length
      ? [...this.filtrosColunasSelecao[campo]]
      : [...all]);
    this.opFiltroColuna.toggle(event);
  }

  isValorSelecionadoFiltroColuna(campo: string, valor: string): boolean {
    const sel = this.selecaoTemp[campo];
    if (!sel) return true;
    return sel.includes(valor);
  }

  isSelecionarTodosFiltroColuna(campo: string): boolean {
    const all = this.getValoresDistintosColuna(campo);
    const sel = this.selecaoTemp[campo] ?? [];
    return all.length > 0 && sel.length === all.length;
  }

  toggleSelecionarTodosFiltroColuna(campo: string): void {
    const all = this.getValoresDistintosColuna(campo);
    this.selecaoTemp[campo] = this.isSelecionarTodosFiltroColuna(campo) ? [] : [...all];
  }

  toggleValorFiltroColuna(campo: string, valor: string): void {
    let sel = this.selecaoTemp[campo] ?? [];
    sel = sel.includes(valor) ? sel.filter(v => v !== valor) : [...sel, valor];
    this.selecaoTemp[campo] = sel;
  }

  aplicarFiltroColuna(): void {
    if (!this.filtroColunaAtivo) return;
    const campo = this.filtroColunaAtivo;
    const all = this.getValoresDistintosColuna(campo);
    const sel = this.selecaoTemp[campo] ?? [];
    this.filtrosColunasSelecao[campo] = sel.length === all.length ? [] : [...sel];
    this.opFiltroColuna.hide();
    this.filtroColunaAtivo = null;
    this.aplicarFiltros();
  }

  cancelarFiltroColuna(): void {
    this.opFiltroColuna.hide();
    this.filtroColunaAtivo = null;
  }

  get hasFiltrosColuna(): boolean {
    return Object.values(this.filtrosColunasSelecao).some(arr => arr?.length > 0);
  }

  limparFiltrosColuna(): void {
    this.filtrosColunasSelecao = {};
    this.aplicarFiltros();
  }

  colunaComFiltroAtivo(campo: string): boolean {
    return (this.filtrosColunasSelecao[campo]?.length ?? 0) > 0;
  }

  private possuiParcelaEmAberto(duplicata: DuplicataResponseDto): boolean {
    return duplicata.parcelas?.some((parcela: ParcelaResponseDto) => isParcelaPendente(parcela.status)) ?? false;
  }

  formatarMoeda(valor: number): string {
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: 'BRL'
    }).format(valor);
  }

  formatarData(data: string): string {
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
}
