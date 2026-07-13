import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  AnotacaoGeralService,
  AnotacaoGeralResponseDto,
  CadastroAnotacaoGeralDto,
  TIPO_ANOTACAO,
  TIPO_REGRA_EMPRESA
} from '../../services/anotacao-geral.service';
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

type AbaAnotacoes = 'anotacoes' | 'regras';

@Component({
  selector: 'app-anotacoes',
  standalone: true,
  imports: [CommonModule, FormsModule, SeletorAgrupamentoGridComponent],
  templateUrl: './anotacoes.component.html',
  styleUrl: './anotacoes.component.css'
})
export class AnotacoesComponent implements OnInit {
  abaAtiva: AbaAnotacoes = 'anotacoes';
  anotacoes: AnotacaoGeralResponseDto[] = [];
  anotacoesFiltradas: AnotacaoGeralResponseDto[] = [];
  showForm = false;
  loading = false;
  error: string | null = null;
  editando = false;
  anotacaoEditando: AnotacaoGeralResponseDto | null = null;
  termoBusca = '';

  agruparPor = '';
  agruparPorOpcoes = criarOpcoesAgrupamento([
    { value: 'dataCadastro', label: 'Data Cadastro' }
  ]);
  private readonly STORAGE_KEY_AGRUPAR_POR = 'anotacoes_agrupar_por';

  novaAnotacao: CadastroAnotacaoGeralDto = {
    descricao: '',
    observacoes: '',
    link: '',
    tipo: TIPO_ANOTACAO
  };

  showConfirmModal = false;
  confirmTitle = '';
  confirmMessage = '';
  confirmCallback: (() => void) | null = null;

  showSuccessModal = false;
  successMessage = '';

  readonly TIPO_ANOTACAO = TIPO_ANOTACAO;
  readonly TIPO_REGRA_EMPRESA = TIPO_REGRA_EMPRESA;

  constructor(
    private anotacaoGeralService: AnotacaoGeralService,
    private notificacao: NotificacaoService
  ) { }

  ngOnInit() {
    this.agruparPor = carregarPreferenciaAgruparPor(this.STORAGE_KEY_AGRUPAR_POR, this.agruparPorOpcoes);
    this.carregarAnotacoes();
  }

  get isAbaRegras(): boolean {
    return this.abaAtiva === 'regras';
  }

  get tipoAbaAtual(): string {
    return this.isAbaRegras ? TIPO_REGRA_EMPRESA : TIPO_ANOTACAO;
  }

  get tituloBotaoNovo(): string {
    return this.isAbaRegras ? 'Nova Regra' : 'Nova Anotação';
  }

  get tituloFormulario(): string {
    if (this.isAbaRegras) {
      return this.editando ? 'Editar Regra da Empresa' : 'Cadastrar Regra da Empresa';
    }
    return this.editando ? 'Editar Anotação' : 'Cadastrar Nova Anotação';
  }

  get mensagemVazia(): string {
    if (this.termoBusca) {
      return this.isAbaRegras ? 'Nenhuma regra encontrada' : 'Nenhuma anotação encontrada';
    }
    return this.isAbaRegras ? 'Nenhuma regra da empresa cadastrada' : 'Nenhuma anotação cadastrada';
  }

  selecionarAba(aba: AbaAnotacoes) {
    if (this.abaAtiva === aba) return;
    this.abaAtiva = aba;
    this.termoBusca = '';
    this.fecharFormulario();
    this.aplicarFiltros();
  }

  carregarAnotacoes() {
    this.loading = true;
    this.error = null;

    this.anotacaoGeralService.listarTodasAnotacoes().subscribe({
      next: (data) => {
        this.anotacoes = data;
        this.aplicarFiltros();
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Erro ao carregar anotações. Verifique se a API está rodando.';
        this.loading = false;
        console.error(err);
      }
    });
  }

  abrirFormularioNovo() {
    this.editando = false;
    this.anotacaoEditando = null;
    this.novaAnotacao = {
      descricao: '',
      observacoes: '',
      link: '',
      tipo: this.tipoAbaAtual
    };
    this.showForm = true;
    this.error = null;
    window.scrollTo(0, 0);
  }

  abrirFormularioEdicao(anotacao: AnotacaoGeralResponseDto) {
    this.editando = true;
    this.anotacaoEditando = anotacao;
    this.novaAnotacao = {
      descricao: anotacao.descricao,
      observacoes: anotacao.observacoes || '',
      link: anotacao.link || '',
      tipo: anotacao.tipo || this.tipoAbaAtual
    };
    this.showForm = true;
    this.error = null;
    window.scrollTo(0, 0);
  }

  fecharFormulario() {
    this.showForm = false;
    this.editando = false;
    this.anotacaoEditando = null;
    this.novaAnotacao = {
      descricao: '',
      observacoes: '',
      link: '',
      tipo: this.tipoAbaAtual
    };
    this.error = null;
  }

  salvarAnotacao() {
    if (this.loading) return;

    if (!this.novaAnotacao.descricao.trim()) {
      this.error = 'A descrição é obrigatória.';
      this.notificacao.aviso(this.error);
      return;
    }

    this.novaAnotacao.tipo = this.tipoAbaAtual;
    if (this.isAbaRegras) {
      this.novaAnotacao.link = undefined;
    } else {
      this.novaAnotacao.observacoes = undefined;
    }

    this.loading = true;
    this.error = null;

    const operacao = this.editando && this.anotacaoEditando
      ? this.anotacaoGeralService.atualizarAnotacao(this.anotacaoEditando.anotacaoId, this.novaAnotacao)
      : this.anotacaoGeralService.cadastrarAnotacao(this.novaAnotacao);

    operacao.subscribe({
      next: () => {
        this.carregarAnotacoes();
        this.fecharFormulario();
        this.loading = false;
        const msg = this.isAbaRegras
          ? (this.editando ? 'Regra atualizada com sucesso.' : 'Regra cadastrada com sucesso.')
          : (this.editando ? 'Anotação atualizada com sucesso.' : 'Anotação cadastrada com sucesso.');
        this.notificacao.sucesso(msg);
      },
      error: (err) => {
        this.error = err.error?.message || 'Erro ao salvar.';
        this.loading = false;
        console.error(err);
      }
    });
  }

  excluirAnotacao(anotacao: AnotacaoGeralResponseDto) {
    this.confirmTitle = 'Confirmar Exclusão';
    this.confirmMessage = this.isAbaRegras
      ? 'Tem certeza que deseja excluir esta regra da empresa?'
      : 'Tem certeza que deseja excluir esta anotação?';
    this.confirmCallback = () => {
      this.loading = true;
      this.anotacaoGeralService.excluirAnotacao(anotacao.anotacaoId).subscribe({
        next: () => {
          this.carregarAnotacoes();
          this.loading = false;
          this.notificacao.sucesso(this.isAbaRegras ? 'Regra excluída com sucesso.' : 'Anotação excluída com sucesso.');
        },
        error: (err) => {
          this.error = err.error?.message || 'Erro ao excluir.';
          this.loading = false;
          console.error(err);
        }
      });
    };
    this.showConfirmModal = true;
  }

  filtrarAnotacoes() {
    this.aplicarFiltros();
  }

  private aplicarFiltros(): void {
    const tipo = this.tipoAbaAtual;
    let lista = this.anotacoes.filter(a => (a.tipo || TIPO_ANOTACAO) === tipo);

    const termo = this.termoBusca.trim().toLowerCase();
    if (termo) {
      lista = lista.filter(a =>
        a.descricao.toLowerCase().includes(termo) ||
        (a.observacoes?.toLowerCase().includes(termo) ?? false) ||
        (a.link?.toLowerCase().includes(termo) ?? false)
      );
    }

    this.anotacoesFiltradas = lista;
  }

  formatarData(data?: string): string {
    if (!data) return '-';
    const parte = data.length >= 10 ? data.substring(0, 10) : data;
    if (/^\d{4}-\d{2}-\d{2}$/.test(parte)) {
      const [ano, mes, dia] = parte.split('-');
      return `${dia}/${mes}/${ano}`;
    }
    return new Date(data).toLocaleDateString('pt-BR');
  }

  abrirLink(link?: string) {
    if (link) {
      const url = link.startsWith('http://') || link.startsWith('https://')
        ? link
        : `https://${link}`;
      window.open(url, '_blank');
    }
  }

  confirmarAcao() {
    if (this.confirmCallback) {
      this.confirmCallback();
      this.confirmCallback = null;
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

  onAgruparPorChange(valor: string) {
    this.agruparPor = valor;
    salvarPreferenciaAgruparPor(this.STORAGE_KEY_AGRUPAR_POR, valor);
  }

  get anotacoesParaTabela(): AnotacaoGeralResponseDto[] {
    return ordenarItensParaAgrupamento(this.anotacoesFiltradas, this.agruparPor);
  }

  getAgruparPorLabel(): string {
    return obterRotuloAgrupamento(this.agruparPorOpcoes, this.agruparPor);
  }

  getValorGrupoAnotacao(anotacao: AnotacaoGeralResponseDto): string {
    if (this.agruparPor === 'dataCadastro') {
      return this.formatarData(anotacao.dataCadastro);
    }

    return obterValorCabecalhoGrupo(anotacao as unknown as Record<string, unknown>, this.agruparPor);
  }

  exibirCabecalhoGrupoAnotacao(index: number): boolean {
    return deveExibirCabecalhoGrupo(
      this.anotacoesParaTabela,
      index,
      this.agruparPor,
      (anotacao) => this.getValorGrupoAnotacao(anotacao)
    );
  }
}
