import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  PlanoContasService,
  PlanoContasResponseDto,
  CadastroPlanoContasDto
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
  selector: 'app-plano-contas',
  standalone: true,
  imports: [CommonModule, FormsModule, SeletorAgrupamentoGridComponent],
  templateUrl: './plano-contas.component.html',
  styleUrl: './plano-contas.component.css'
})
export class PlanoContasComponent implements OnInit {
  planos: PlanoContasResponseDto[] = [];
  planosFiltrados: PlanoContasResponseDto[] = [];
  loading = false;
  saving = false;
  error: string | null = null;
  termoBusca = '';
  showModal = false;
  showModalExcluir = false;
  editandoId: number | null = null;
  itemParaExcluir: PlanoContasResponseDto | null = null;
  excluindo = false;

  form: CadastroPlanoContasDto = { descricao: '' };

  agruparPor = '';
  agruparPorOpcoes = criarOpcoesAgrupamento([
    { value: 'descricao', label: 'Descrição' }
  ]);
  private readonly STORAGE_KEY_AGRUPAR_POR = 'plano_contas_agrupar_por';

  constructor(
    private planoContasService: PlanoContasService,
    private notificacao: NotificacaoService
  ) {}

  ngOnInit(): void {
    this.agruparPor = carregarPreferenciaAgruparPor(this.STORAGE_KEY_AGRUPAR_POR, this.agruparPorOpcoes);
    this.carregar();
  }

  carregar(): void {
    this.loading = true;
    this.error = null;
    this.planoContasService.listarTodosPlanosContas().subscribe({
      next: (data) => {
        this.planos = data;
        this.aplicarFiltros();
        this.loading = false;
      },
      error: (err) => {
        this.error = err.error?.message || 'Erro ao carregar plano de contas.';
        this.loading = false;
      }
    });
  }

  filtrar(): void {
    this.aplicarFiltros();
  }

  private aplicarFiltros(): void {
    const termo = this.termoBusca.trim().toLowerCase();
    if (!termo) {
      this.planosFiltrados = [...this.planos];
      return;
    }
    this.planosFiltrados = this.planos.filter(p =>
      p.descricao.toLowerCase().includes(termo)
    );
  }

  abrirNovo(): void {
    this.editandoId = null;
    this.form = { descricao: '' };
    this.showModal = true;
    this.error = null;
  }

  abrirEditar(item: PlanoContasResponseDto): void {
    this.editandoId = item.planoContasId;
    this.form = { descricao: item.descricao };
    this.showModal = true;
    this.error = null;
  }

  fecharModal(): void {
    this.showModal = false;
    this.editandoId = null;
    this.error = null;
  }

  fecharModalExcluir(): void {
    this.showModalExcluir = false;
    this.itemParaExcluir = null;
    this.error = null;
  }

  salvar(): void {
    if (!this.form.descricao?.trim()) {
      this.error = 'Descrição é obrigatória.';
      return;
    }

    this.saving = true;
    this.error = null;
    const dto: CadastroPlanoContasDto = { descricao: this.form.descricao.trim() };
    const obs = this.editandoId != null
      ? this.planoContasService.atualizarPlanoContas(this.editandoId, dto)
      : this.planoContasService.cadastrarPlanoContas(dto);

    obs.subscribe({
      next: () => {
        this.carregar();
        this.fecharModal();
        this.saving = false;
        this.notificacao.sucesso(this.editandoId != null ? 'Plano de contas atualizado com sucesso.' : 'Plano de contas cadastrado com sucesso.');
      },
      error: (err) => {
        this.error = err.error?.message || 'Erro ao salvar plano de contas.';
        this.saving = false;
      }
    });
  }

  abrirModalExcluir(item: PlanoContasResponseDto): void {
    this.itemParaExcluir = item;
    this.showModalExcluir = true;
    this.error = null;
  }

  confirmarExcluir(): void {
    if (!this.itemParaExcluir) return;
    this.excluindo = true;
    this.error = null;
    this.planoContasService.excluirPlanoContas(this.itemParaExcluir.planoContasId).subscribe({
      next: () => {
        this.carregar();
        this.fecharModalExcluir();
        this.excluindo = false;
        this.notificacao.sucesso('Plano de contas excluído com sucesso.');
      },
      error: (err) => {
        this.error = err.error?.message || 'Erro ao excluir plano de contas.';
        this.excluindo = false;
      }
    });
  }

  onAgruparPorChange(valor: string) {
    this.agruparPor = valor;
    salvarPreferenciaAgruparPor(this.STORAGE_KEY_AGRUPAR_POR, valor);
  }

  get planosParaTabela(): PlanoContasResponseDto[] {
    return ordenarItensParaAgrupamento(this.planosFiltrados, this.agruparPor);
  }

  getAgruparPorLabel(): string {
    return obterRotuloAgrupamento(this.agruparPorOpcoes, this.agruparPor);
  }

  getValorGrupoPlano(item: PlanoContasResponseDto): string {
    return obterValorCabecalhoGrupo(item as unknown as Record<string, unknown>, this.agruparPor);
  }

  exibirCabecalhoGrupoPlano(index: number): boolean {
    return deveExibirCabecalhoGrupo(
      this.planosParaTabela,
      index,
      this.agruparPor,
      (item) => this.getValorGrupoPlano(item)
    );
  }
}
