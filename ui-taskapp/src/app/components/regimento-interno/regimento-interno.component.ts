import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  RegimentoService,
  RegimentoResponseDto,
  RegimentoDetalheResponseDto,
  RegimentoAceiteLogResponseDto,
  CadastroRegimentoDto
} from '../../services/regimento.service';
import { AuthService } from '../../services/auth.service';
import { NotificacaoService } from '../../services/notificacao.service';
import {
  criarOpcoesAgrupamento,
  deveExibirCabecalhoGrupo,
  obterRotuloAgrupamento,
  obterValorCabecalhoGrupo,
  ordenarItensParaAgrupamento
} from '../../shared/utils/grid-agrupamento.util';
import { SeletorAgrupamentoGridComponent } from '../../shared/components/seletor-agrupamento-grid/seletor-agrupamento-grid.component';

@Component({
  selector: 'app-regimento-interno',
  standalone: true,
  imports: [CommonModule, FormsModule, SeletorAgrupamentoGridComponent],
  templateUrl: './regimento-interno.component.html',
  styleUrl: './regimento-interno.component.css'
})
export class RegimentoInternoComponent implements OnInit {
  private static readonly LOGINS_OBRIGATORIOS = ['TI.GABRIEL', 'TI.ABNER'];

  lista: RegimentoResponseDto[] = [];
  listaFiltrada: RegimentoResponseDto[] = [];
  loading = false;
  saving = false;
  error: string | null = null;
  termoBusca = '';
  agruparPor = '';

  showForm = false;
  editando = false;
  editandoId: number | null = null;
  form: CadastroRegimentoDto = { titulo: '', descricao: '', ativo: true };

  regimentoEdicao: RegimentoDetalheResponseDto | null = null;
  carregandoEdicao = false;
  observacaoAceite = '';
  processandoAceite = false;

  showModalExcluir = false;
  itemParaExcluir: RegimentoResponseDto | null = null;
  excluindo = false;

  showModalLog = false;
  itemLog: RegimentoResponseDto | null = null;
  logsAceite: RegimentoAceiteLogResponseDto[] = [];
  carregandoLog = false;

  agruparPorOpcoes = criarOpcoesAgrupamento([
    { value: 'titulo', label: 'Título' },
    { value: 'ativo', label: 'Status' },
    { value: 'situacaoAprovacao', label: 'Situação de Aprovação' }
  ]);

  constructor(
    private regimentoService: RegimentoService,
    private authService: AuthService,
    private notificacao: NotificacaoService
  ) {}

  ngOnInit(): void {
    this.carregar();
  }

  get usuarioId(): number | null {
    return this.authService.getUsuarioId();
  }

  get usuarioEhObrigatorio(): boolean {
    const login = this.authService.getUsuario()?.login?.toUpperCase();
    return !!login && RegimentoInternoComponent.LOGINS_OBRIGATORIOS.includes(login);
  }

  get listaParaTabela(): RegimentoResponseDto[] {
    return ordenarItensParaAgrupamento(this.listaFiltrada, this.agruparPor);
  }

  carregar(): void {
    this.loading = true;
    this.error = null;
    this.regimentoService.listarRegimentos().subscribe({
      next: (data) => {
        this.lista = data;
        this.filtrar();
        this.loading = false;
      },
      error: (err) => {
        this.error = err.error?.message || 'Erro ao carregar regimentos.';
        this.loading = false;
      }
    });
  }

  filtrar(): void {
    const termo = this.termoBusca.trim().toLowerCase();
    if (!termo) {
      this.listaFiltrada = [...this.lista];
      return;
    }

    this.listaFiltrada = this.lista.filter(item =>
      item.regimentoId.toString().includes(termo) ||
      item.titulo.toLowerCase().includes(termo) ||
      item.descricao.toLowerCase().includes(termo) ||
      item.situacaoAprovacao.toLowerCase().includes(termo) ||
      (item.ativo ? 'ativo' : 'inativo').includes(termo)
    );
  }

  abrirFormularioNovo(): void {
    this.editando = false;
    this.editandoId = null;
    this.regimentoEdicao = null;
    this.form = { titulo: '', descricao: '', ativo: true };
    this.observacaoAceite = '';
    this.showForm = true;
    this.error = null;
  }

  abrirEditar(item: RegimentoResponseDto): void {
    this.editando = true;
    this.editandoId = item.regimentoId;
    this.regimentoEdicao = null;
    this.observacaoAceite = '';
    this.carregandoEdicao = true;
    this.showForm = true;
    this.error = null;

    this.regimentoService.obterRegimentoPorId(item.regimentoId, this.usuarioId).subscribe({
      next: (detalhe) => {
        this.regimentoEdicao = detalhe;
        this.form = {
          titulo: detalhe.titulo,
          descricao: detalhe.descricao,
          ativo: detalhe.ativo
        };
        this.carregandoEdicao = false;
      },
      error: (err) => {
        this.error = err.error?.message || 'Erro ao carregar regimento.';
        this.carregandoEdicao = false;
        this.fecharFormulario();
      }
    });
  }

  fecharFormulario(): void {
    this.showForm = false;
    this.editando = false;
    this.editandoId = null;
    this.regimentoEdicao = null;
    this.observacaoAceite = '';
    this.error = null;
  }

  salvar(): void {
    if (!this.form.titulo?.trim()) {
      this.error = 'Título é obrigatório.';
      return;
    }
    if (!this.form.descricao?.trim()) {
      this.error = 'Descrição é obrigatória.';
      return;
    }

    this.saving = true;
    this.error = null;
    const dto: CadastroRegimentoDto = {
      titulo: this.form.titulo.trim(),
      descricao: this.form.descricao.trim(),
      ativo: this.form.ativo
    };

    const obs = this.editando && this.editandoId != null
      ? this.regimentoService.atualizarRegimento(this.editandoId, dto)
      : this.regimentoService.cadastrarRegimento(dto);

    obs.subscribe({
      next: () => {
        this.carregar();
        this.fecharFormulario();
        this.saving = false;
        this.notificacao.sucesso(this.editando ? 'Regimento atualizado com sucesso.' : 'Regimento cadastrado com sucesso.');
      },
      error: (err) => {
        this.error = err.error?.message || 'Erro ao salvar regimento.';
        this.saving = false;
      }
    });
  }

  recarregarEdicao(): void {
    if (!this.editandoId) return;

    this.carregandoEdicao = true;
    this.regimentoService.obterRegimentoPorId(this.editandoId, this.usuarioId).subscribe({
      next: (detalhe) => {
        this.regimentoEdicao = detalhe;
        this.observacaoAceite = '';
        this.carregandoEdicao = false;
        this.carregar();
      },
      error: (err) => {
        this.error = err.error?.message || 'Erro ao atualizar regimento.';
        this.carregandoEdicao = false;
      }
    });
  }

  registrarAceite(aceito: boolean): void {
    if (!this.regimentoEdicao || this.usuarioId == null) return;

    this.processandoAceite = true;
    this.error = null;

    this.regimentoService.registrarAceite(this.regimentoEdicao.regimentoId, this.usuarioId, {
      aceito,
      observacao: this.observacaoAceite.trim() || undefined
    }).subscribe({
      next: () => {
        this.processandoAceite = false;
        this.notificacao.sucesso(aceito ? 'Aceite registrado com sucesso.' : 'Recusa registrada com sucesso.');
        this.recarregarEdicao();
      },
      error: (err) => {
        this.error = err.error?.message || 'Erro ao registrar decisão.';
        this.processandoAceite = false;
      }
    });
  }

  desfazerMeuAceite(): void {
    if (!this.regimentoEdicao?.meuAceiteAtual || this.usuarioId == null) return;

    this.processandoAceite = true;
    this.error = null;

    this.regimentoService.desfazerAceite(this.regimentoEdicao.meuAceiteAtual.aceiteId, this.usuarioId).subscribe({
      next: () => {
        this.processandoAceite = false;
        this.notificacao.sucesso('Decisão desfeita com sucesso.');
        this.recarregarEdicao();
      },
      error: (err) => {
        this.error = err.error?.message || 'Erro ao desfazer decisão.';
        this.processandoAceite = false;
      }
    });
  }

  abrirModalExcluir(item: RegimentoResponseDto): void {
    if (item.possuiAceites) {
      this.error = 'Não é possível excluir regimento com aceites ou recusas registrados.';
      return;
    }
    this.itemParaExcluir = item;
    this.showModalExcluir = true;
    this.error = null;
  }

  fecharModalExcluir(): void {
    this.showModalExcluir = false;
    this.itemParaExcluir = null;
  }

  abrirModalLog(item: RegimentoResponseDto): void {
    this.itemLog = item;
    this.logsAceite = [];
    this.showModalLog = true;
    this.carregandoLog = true;
    this.error = null;

    this.regimentoService.listarLogAceites(item.regimentoId).subscribe({
      next: (logs) => {
        this.logsAceite = logs;
        this.carregandoLog = false;
      },
      error: (err) => {
        this.error = err.error?.message || 'Erro ao carregar log de aprovações.';
        this.carregandoLog = false;
        this.fecharModalLog();
      }
    });
  }

  fecharModalLog(): void {
    this.showModalLog = false;
    this.itemLog = null;
    this.logsAceite = [];
    this.carregandoLog = false;
  }

  getClasseAcaoLog(acao: string): string {
    if (acao.startsWith('Desfazimento')) return 'acao-desfazimento';
    if (acao === 'Aceite') return 'situacao-aceito';
    return 'situacao-recusado';
  }

  confirmarExcluir(): void {
    if (!this.itemParaExcluir) return;

    this.excluindo = true;
    this.error = null;
    this.regimentoService.excluirRegimento(this.itemParaExcluir.regimentoId).subscribe({
      next: () => {
        this.carregar();
        this.fecharModalExcluir();
        this.excluindo = false;
        this.notificacao.sucesso('Regimento excluído com sucesso.');
      },
      error: (err) => {
        this.error = err.error?.message || 'Erro ao excluir regimento.';
        this.excluindo = false;
      }
    });
  }

  fecharModalErro(): void {
    this.error = null;
  }

  getAgruparPorLabel(): string {
    return obterRotuloAgrupamento(this.agruparPorOpcoes, this.agruparPor);
  }

  getValorGrupo(item: RegimentoResponseDto): string {
    if (this.agruparPor === 'ativo') {
      return item.ativo ? 'Ativo' : 'Inativo';
    }
    return obterValorCabecalhoGrupo(item as unknown as Record<string, unknown>, this.agruparPor);
  }

  exibirCabecalhoGrupo(index: number): boolean {
    return deveExibirCabecalhoGrupo(
      this.listaParaTabela,
      index,
      this.agruparPor,
      (item) => this.getValorGrupo(item)
    );
  }

  getClasseSituacao(situacao: string): string {
    switch (situacao) {
      case 'Aprovado':
        return 'situacao-aprovado';
      case 'Parcialmente Aprovado':
        return 'situacao-parcial';
      default:
        return 'situacao-reprovado';
    }
  }

  getClasseSituacaoAceite(situacao: string): string {
    return situacao === 'Aceito' ? 'situacao-aceito' : 'situacao-recusado';
  }

  formatarData(data?: string): string {
    if (!data) return '-';
    return new Date(data).toLocaleString('pt-BR');
  }

  podeExcluir(item: RegimentoResponseDto): boolean {
    return !item.possuiAceites;
  }
}
