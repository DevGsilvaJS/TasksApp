import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  EmpresaService,
  EmpresaResponseDto,
  CadastroEmpresaDto
} from '../../services/empresa.service';
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
  selector: 'app-empresas',
  standalone: true,
  imports: [CommonModule, FormsModule, SeletorAgrupamentoGridComponent],
  templateUrl: './empresas.component.html',
  styleUrl: './empresas.component.css'
})
export class EmpresasComponent implements OnInit {
  empresas: EmpresaResponseDto[] = [];
  empresasFiltradas: EmpresaResponseDto[] = [];
  loading = false;
  saving = false;
  error: string | null = null;
  termoBusca = '';
  showModal = false;
  showModalExcluir = false;
  editandoId: number | null = null;
  itemParaExcluir: EmpresaResponseDto | null = null;
  excluindo = false;

  form: CadastroEmpresaDto = {
    cnpj: '',
    razaoSocial: '',
    fantasia: ''
  };

  agruparPor = '';
  agruparPorOpcoes = criarOpcoesAgrupamento([
    { value: 'fantasia', label: 'Fantasia' },
    { value: 'razaoSocial', label: 'Razão Social' }
  ]);
  private readonly STORAGE_KEY_AGRUPAR_POR = 'empresas_agrupar_por';

  constructor(
    private empresaService: EmpresaService,
    private notificacao: NotificacaoService
  ) {}

  ngOnInit(): void {
    this.agruparPor = carregarPreferenciaAgruparPor(this.STORAGE_KEY_AGRUPAR_POR, this.agruparPorOpcoes);
    this.carregar();
  }

  carregar(): void {
    this.loading = true;
    this.error = null;
    this.empresaService.listarTodasEmpresas().subscribe({
      next: (data) => {
        this.empresas = data;
        this.aplicarFiltros();
        this.loading = false;
      },
      error: (err) => {
        this.error = err.error?.message || 'Erro ao carregar empresas.';
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
      this.empresasFiltradas = [...this.empresas];
      return;
    }
    this.empresasFiltradas = this.empresas.filter(e =>
      e.fantasia.toLowerCase().includes(termo) ||
      e.razaoSocial.toLowerCase().includes(termo) ||
      e.cnpj.toLowerCase().includes(termo)
    );
  }

  abrirNovo(): void {
    this.editandoId = null;
    this.form = { cnpj: '', razaoSocial: '', fantasia: '' };
    this.showModal = true;
    this.error = null;
  }

  abrirEditar(item: EmpresaResponseDto): void {
    this.editandoId = item.empresaId;
    this.form = {
      cnpj: this.formatarCnpjParaExibicao(item.cnpj),
      razaoSocial: item.razaoSocial,
      fantasia: item.fantasia
    };
    this.showModal = true;
    this.error = null;
  }

  fecharModal(): void {
    this.showModal = false;
    this.editandoId = null;
    this.error = null;
  }

  fecharModalErro(): void {
    this.error = null;
  }

  salvar(): void {
    if (!this.form.fantasia?.trim() || !this.form.razaoSocial?.trim() || !this.form.cnpj?.trim()) {
      this.error = 'Preencha CNPJ, Razão Social e Fantasia.';
      return;
    }

    this.saving = true;
    this.error = null;
    const dto: CadastroEmpresaDto = {
      cnpj: this.form.cnpj.trim(),
      razaoSocial: this.form.razaoSocial.trim(),
      fantasia: this.form.fantasia.trim()
    };

    const obs = this.editandoId != null
      ? this.empresaService.atualizarEmpresa(this.editandoId, dto)
      : this.empresaService.cadastrarEmpresa(dto);

    obs.subscribe({
      next: () => {
        this.carregar();
        this.fecharModal();
        this.saving = false;
        this.notificacao.sucesso(this.editandoId != null ? 'Empresa atualizada com sucesso.' : 'Empresa cadastrada com sucesso.');
      },
      error: (err) => {
        this.error = err.error?.message || 'Erro ao salvar empresa.';
        this.saving = false;
      }
    });
  }

  abrirModalExcluir(item: EmpresaResponseDto): void {
    this.itemParaExcluir = item;
    this.showModalExcluir = true;
    this.error = null;
  }

  fecharModalExcluir(): void {
    this.showModalExcluir = false;
    this.itemParaExcluir = null;
    this.error = null;
  }

  confirmarExcluir(): void {
    if (!this.itemParaExcluir) return;
    this.excluindo = true;
    this.error = null;
    this.empresaService.excluirEmpresa(this.itemParaExcluir.empresaId).subscribe({
      next: () => {
        this.carregar();
        this.fecharModalExcluir();
        this.excluindo = false;
        this.notificacao.sucesso('Empresa excluída com sucesso.');
      },
      error: (err) => {
        this.error = err.error?.message || 'Erro ao excluir empresa.';
        this.excluindo = false;
      }
    });
  }

  aplicarMascaraCnpj(event: Event): void {
    const input = event.target as HTMLInputElement;
    let valor = input.value.replace(/\D/g, '');
    valor = valor.replace(/^(\d{2})(\d)/, '$1.$2');
    valor = valor.replace(/^(\d{2})\.(\d{3})(\d)/, '$1.$2.$3');
    valor = valor.replace(/\.(\d{3})(\d)/, '.$1/$2');
    valor = valor.replace(/(\d{4})(\d)/, '$1-$2');
    input.value = valor;
    this.form.cnpj = valor;
  }

  formatarCnpjParaExibicao(cnpj?: string): string {
    if (!cnpj) return '-';
    if (cnpj.includes('.') || cnpj.includes('/') || cnpj.includes('-')) return cnpj;
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

  get empresasParaTabela(): EmpresaResponseDto[] {
    return ordenarItensParaAgrupamento(this.empresasFiltradas, this.agruparPor);
  }

  getAgruparPorLabel(): string {
    return obterRotuloAgrupamento(this.agruparPorOpcoes, this.agruparPor);
  }

  getValorGrupoEmpresa(empresa: EmpresaResponseDto): string {
    return obterValorCabecalhoGrupo(empresa as unknown as Record<string, unknown>, this.agruparPor);
  }

  exibirCabecalhoGrupoEmpresa(index: number): boolean {
    return deveExibirCabecalhoGrupo(
      this.empresasParaTabela,
      index,
      this.agruparPor,
      (empresa) => this.getValorGrupoEmpresa(empresa)
    );
  }
}
