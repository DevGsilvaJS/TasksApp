import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  RelatorioGerencialService,
  RelatorioGerencialResponseDto,
  TIPOS_RELATORIO,
  TipoRelatorioGerencial
} from '../../services/relatorio-gerencial.service';
import { extrairMensagemErroApi } from '../../utils/erro-api.util';

@Component({
  selector: 'app-relatorios-gerenciais',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './relatorios-gerenciais.component.html',
  styleUrl: './relatorios-gerenciais.component.css'
})
export class RelatoriosGerenciaisComponent {
  readonly tiposRelatorio = TIPOS_RELATORIO;

  dataInicial = this.primeiroDiaDoMes();
  dataFinal = this.hojeIso();
  tipoRelatorio: TipoRelatorioGerencial = 'contas-a-receber';
  loading = false;
  error: string | null = null;
  relatorio: RelatorioGerencialResponseDto | null = null;

  constructor(private relatorioService: RelatorioGerencialService) { }

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
