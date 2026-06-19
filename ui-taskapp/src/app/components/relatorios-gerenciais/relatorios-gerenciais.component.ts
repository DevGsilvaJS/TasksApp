import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  RelatorioGerencialService,
  RelatorioGerencialResponseDto,
  TIPOS_RELATORIO,
  TipoRelatorioGerencial
} from '../../services/relatorio-gerencial.service';
import { extrairMensagemErroApi } from '../../utils/erro-api.util';
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
import { RelatorioGerencialLinhaDto } from '../../services/relatorio-gerencial.service';

@Component({
  selector: 'app-relatorios-gerenciais',
  standalone: true,
  imports: [CommonModule, FormsModule, SeletorAgrupamentoGridComponent],
  templateUrl: './relatorios-gerenciais.component.html',
  styleUrl: './relatorios-gerenciais.component.css'
})
export class RelatoriosGerenciaisComponent implements OnInit {
  readonly tiposRelatorio = TIPOS_RELATORIO;

  dataInicial = this.primeiroDiaDoMes();
  dataFinal = this.hojeIso();
  tipoRelatorio: TipoRelatorioGerencial = 'contas-a-receber';
  loading = false;
  error: string | null = null;
  relatorio: RelatorioGerencialResponseDto | null = null;

  agruparPor = '';
  agruparPorOpcoes = criarOpcoesAgrupamento([
    { value: 'clienteNome', label: 'Cliente' },
    { value: 'status', label: 'Status' },
    { value: 'descricaoDespesa', label: 'Descrição' },
    { value: 'numeroDuplicata', label: 'Duplicata' }
  ]);
  private readonly STORAGE_KEY_AGRUPAR_POR = 'relatorios_gerenciais_agrupar_por';

  constructor(private relatorioService: RelatorioGerencialService) { }

  ngOnInit(): void {
    this.agruparPor = carregarPreferenciaAgruparPor(this.STORAGE_KEY_AGRUPAR_POR, this.agruparPorOpcoes);
  }

  pesquisar(): void {
    if (!this.dataInicial || !this.dataFinal) {
      this.error = 'Informe a data inicial e a data final.';
      return;
    }

    if (this.dataFinal < this.dataInicial) {
      this.error = 'A data final deve ser maior ou igual à data inicial.';
      return;
    }

    this.loading = true;
    this.error = null;
    this.relatorio = null;

    this.relatorioService.obterRelatorio(this.dataInicial, this.dataFinal, this.tipoRelatorio).subscribe({
      next: (data) => {
        this.relatorio = {
          ...data,
          itens: data.itens ?? []
        };
        this.loading = false;
      },
      error: (err) => {
        this.error = extrairMensagemErroApi(err, 'Erro ao gerar relatório. Verifique se a API está rodando.');
        this.loading = false;
        console.error(err);
      }
    });
  }

  get exibirColunaCliente(): boolean {
    return this.tipoRelatorio === 'contas-a-receber' || this.tipoRelatorio === 'contas-recebidas';
  }

  get exibirColunaDataPagamento(): boolean {
    return this.tipoRelatorio === 'contas-pagas' || this.tipoRelatorio === 'contas-recebidas';
  }

  get colSpanRelatorio(): number {
    let colunas = 9;
    if (this.exibirColunaCliente) colunas++;
    if (this.exibirColunaDataPagamento) colunas++;
    return colunas;
  }

  onAgruparPorChange(valor: string) {
    this.agruparPor = valor;
    salvarPreferenciaAgruparPor(this.STORAGE_KEY_AGRUPAR_POR, valor);
  }

  get itensRelatorioParaTabela(): RelatorioGerencialLinhaDto[] {
    if (!this.relatorio?.itens) return [];
    return ordenarItensParaAgrupamento(this.relatorio.itens, this.agruparPor);
  }

  getAgruparPorLabel(): string {
    return obterRotuloAgrupamento(this.agruparPorOpcoes, this.agruparPor);
  }

  getValorGrupoRelatorio(item: RelatorioGerencialLinhaDto): string {
    return obterValorCabecalhoGrupo(item as unknown as Record<string, unknown>, this.agruparPor);
  }

  exibirCabecalhoGrupoRelatorio(index: number): boolean {
    return deveExibirCabecalhoGrupo(
      this.itensRelatorioParaTabela,
      index,
      this.agruparPor,
      (item) => this.getValorGrupoRelatorio(item)
    );
  }

  formatarMoeda(valor: number): string {
    return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(valor ?? 0);
  }

  formatarData(data: string | undefined): string {
    if (!data) return '-';
    return new Date(data).toLocaleDateString('pt-BR');
  }

  private hojeIso(): string {
    return this.formatarDataLocal(new Date());
  }

  private primeiroDiaDoMes(): string {
    const hoje = new Date();
    return this.formatarDataLocal(new Date(hoje.getFullYear(), hoje.getMonth(), 1));
  }

  private formatarDataLocal(data: Date): string {
    const y = data.getFullYear();
    const m = String(data.getMonth() + 1).padStart(2, '0');
    const d = String(data.getDate()).padStart(2, '0');
    return `${y}-${m}-${d}`;
  }
}
