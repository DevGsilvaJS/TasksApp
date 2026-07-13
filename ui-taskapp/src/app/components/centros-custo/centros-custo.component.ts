import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  CentroCustoService,
  CentroCustoResponseDto,
  CadastroCentroCustoDto
} from '../../services/centro-custo.service';
import { EmpresaService, EmpresaResponseDto } from '../../services/empresa.service';
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
  selector: 'app-centros-custo',
  standalone: true,
  imports: [CommonModule, FormsModule, SeletorAgrupamentoGridComponent],
  templateUrl: './centros-custo.component.html',
  styleUrl: './centros-custo.component.css'
})
export class CentrosCustoComponent implements OnInit {
  centros: CentroCustoResponseDto[] = [];
  empresas: EmpresaResponseDto[] = [];
  loading = false;
  saving = false;
  error: string | null = null;
  showModal = false;
  showModalExcluir = false;
  editandoId: number | null = null;
  itemParaExcluir: CentroCustoResponseDto | null = null;
  excluindo = false;

  form: CadastroCentroCustoDto = { empresaId: 0 };

  agruparPor = '';
  agruparPorOpcoes = criarOpcoesAgrupamento([
    { value: 'empresaFantasia', label: 'Empresa' }
  ]);
  private readonly STORAGE_KEY_AGRUPAR_POR = 'centros_custo_agrupar_por';

  constructor(
    private centroCustoService: CentroCustoService,
    private empresaService: EmpresaService,
    private notificacao: NotificacaoService
  ) {}

  ngOnInit(): void {
    this.agruparPor = carregarPreferenciaAgruparPor(this.STORAGE_KEY_AGRUPAR_POR, this.agruparPorOpcoes);
    this.carregarEmpresas();
    this.carregar();
  }

  carregarEmpresas(): void {
    this.empresaService.listarTodasEmpresas().subscribe({
      next: (data) => { this.empresas = data; },
      error: (err) => console.error('Erro ao carregar empresas:', err)
    });
  }

  carregar(): void {
    this.loading = true;
    this.error = null;
    this.centroCustoService.listarTodosCentrosCusto().subscribe({
      next: (data) => {
        this.centros = data;
        this.loading = false;
      },
      error: (err) => {
        this.error = err.error?.message || 'Erro ao carregar centros de custo.';
        this.loading = false;
      }
    });
  }

  abrirNovo(): void {
    this.editandoId = null;
    this.form = { empresaId: this.empresas[0]?.empresaId ?? 0 };
    this.showModal = true;
    this.error = null;
  }

  abrirEditar(item: CentroCustoResponseDto): void {
    this.editandoId = item.centroCustoId;
    this.form = { empresaId: item.empresaId };
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
    if (!this.form.empresaId || this.form.empresaId === 0) {
      this.error = 'Selecione uma empresa.';
      return;
    }

    this.saving = true;
    this.error = null;
    const obs = this.editandoId != null
      ? this.centroCustoService.atualizarCentroCusto(this.editandoId, this.form)
      : this.centroCustoService.cadastrarCentroCusto(this.form);

    obs.subscribe({
      next: () => {
        this.carregar();
        this.fecharModal();
        this.saving = false;
        this.notificacao.sucesso(this.editandoId != null ? 'Centro de custo atualizado com sucesso.' : 'Centro de custo cadastrado com sucesso.');
      },
      error: (err) => {
        this.error = err.error?.message || 'Erro ao salvar centro de custo.';
        this.saving = false;
      }
    });
  }

  abrirModalExcluir(item: CentroCustoResponseDto): void {
    this.itemParaExcluir = item;
    this.showModalExcluir = true;
    this.error = null;
  }

  confirmarExcluir(): void {
    if (!this.itemParaExcluir) return;
    this.excluindo = true;
    this.error = null;
    this.centroCustoService.excluirCentroCusto(this.itemParaExcluir.centroCustoId).subscribe({
      next: () => {
        this.carregar();
        this.fecharModalExcluir();
        this.excluindo = false;
        this.notificacao.sucesso('Centro de custo excluído com sucesso.');
      },
      error: (err) => {
        this.error = err.error?.message || 'Erro ao excluir centro de custo.';
        this.excluindo = false;
      }
    });
  }

  formatarCnpj(cnpj?: string): string {
    if (!cnpj) return '-';
    let valor = cnpj.replace(/\D/g, '');
    valor = valor.replace(/^(\d{2})(\d)/, '$1.$2');
    valor = valor.replace(/^(\d{2})\.(\d{3})(\d)/, '$1.$2.$3');
    valor = valor.replace(/\.(\d{3})(\d)/, '.$1/$2');
    valor = valor.replace(/(\d{4})(\d)/, '$1-$2');
    return valor;
  }

  onAgruparPorChange(valor: string) {
    this.agruparPor = valor;
    salvarPreferenciaAgruparPor(this.STORAGE_KEY_AGRUPAR_POR, valor);
  }

  get centrosParaTabela(): CentroCustoResponseDto[] {
    return ordenarItensParaAgrupamento(this.centros, this.agruparPor);
  }

  getAgruparPorLabel(): string {
    return obterRotuloAgrupamento(this.agruparPorOpcoes, this.agruparPor);
  }

  getValorGrupoCentro(item: CentroCustoResponseDto): string {
    return obterValorCabecalhoGrupo(item as unknown as Record<string, unknown>, this.agruparPor);
  }

  exibirCabecalhoGrupoCentro(index: number): boolean {
    return deveExibirCabecalhoGrupo(
      this.centrosParaTabela,
      index,
      this.agruparPor,
      (item) => this.getValorGrupoCentro(item)
    );
  }
}
