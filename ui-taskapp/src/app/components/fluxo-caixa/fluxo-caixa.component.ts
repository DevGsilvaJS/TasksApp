import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  FluxoCaixaService,
  FluxoCaixaResponseDto,
  FluxoCaixaCentroCustoDto,
  FluxoCaixaPlanoContasDto,
  FluxoCaixaMesDto
} from '../../services/fluxo-caixa.service';

@Component({
  selector: 'app-fluxo-caixa',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './fluxo-caixa.component.html',
  styleUrl: './fluxo-caixa.component.css'
})
export class FluxoCaixaComponent implements OnInit {
  ano = new Date().getFullYear();
  anosDisponiveis: number[] = [];
  dados: FluxoCaixaResponseDto | null = null;
  loading = false;
  error: string | null = null;
  empresaIdFiltro: number | null = null;

  readonly mesesCabecalho = ['Jan', 'Fev', 'Mar', 'Abr', 'Mai', 'Jun', 'Jul', 'Ago', 'Set', 'Out', 'Nov', 'Dez'];

  constructor(private fluxoCaixaService: FluxoCaixaService) {}

  ngOnInit(): void {
    const atual = new Date().getFullYear();
    this.anosDisponiveis = Array.from({ length: 6 }, (_, i) => atual - i);
    this.carregar();
  }

  carregar(): void {
    this.loading = true;
    this.error = null;
    this.fluxoCaixaService.obterFluxoCaixa(this.ano).subscribe({
      next: (data) => {
        this.dados = data;
        if (
          this.empresaIdFiltro != null &&
          !data.centros.some(c => c.empresaId === this.empresaIdFiltro)
        ) {
          this.empresaIdFiltro = null;
        }
        this.loading = false;
      },
      error: (err) => {
        this.error = err.error?.message || 'Erro ao carregar fluxo de caixa.';
        this.loading = false;
      }
    });
  }

  onAnoChange(): void {
    this.carregar();
  }

  get empresasDisponiveis(): { empresaId: number; empresaFantasia: string }[] {
    if (!this.dados?.centros.length) {
      return [];
    }

    return [...this.dados.centros]
      .sort((a, b) => a.empresaFantasia.localeCompare(b.empresaFantasia, 'pt-BR', { numeric: true }));
  }

  get centrosExibicao(): FluxoCaixaCentroCustoDto[] {
    if (!this.dados) {
      return [];
    }

    if (this.empresaIdFiltro == null) {
      return this.dados.centros;
    }

    return this.dados.centros.filter(c => c.empresaId === this.empresaIdFiltro);
  }

  get totalReceitasExibicao(): number {
    return this.centrosExibicao.reduce((total, centro) => total + centro.totalReceitas, 0);
  }

  get totalDespesasExibicao(): number {
    return this.centrosExibicao.reduce((total, centro) => total + centro.totalDespesas, 0);
  }

  get saldoAnoExibicao(): number {
    return this.totalReceitasExibicao - this.totalDespesasExibicao;
  }

  get totaisMensaisExibicao(): FluxoCaixaMesDto[] {
    if (!this.dados) {
      return [];
    }

    if (this.empresaIdFiltro == null) {
      return this.dados.totaisMensais;
    }

    return this.mesesCabecalho.map((nomeMes, index) => {
      const mes = index + 1;
      let receitas = 0;
      let despesas = 0;

      for (const centro of this.centrosExibicao) {
        const dadosMes = centro.meses[index];
        receitas += dadosMes?.receitas ?? 0;
        despesas += dadosMes?.despesas ?? 0;
      }

      return {
        mes,
        nomeMes,
        receitas,
        despesas,
        saldo: receitas - despesas
      };
    });
  }

  obterMesPlano(plano: FluxoCaixaPlanoContasDto, mes: number): FluxoCaixaMesDto {
    return plano.meses[mes - 1];
  }

  obterTotalMes(mes: number): FluxoCaixaMesDto | undefined {
    return this.totaisMensaisExibicao.find(m => m.mes === mes);
  }

  formatarMoeda(valor: number): string {
    if (valor == null || Number.isNaN(valor)) return '-';
    return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(valor);
  }

  formatarCnpj(cnpj?: string): string {
    if (!cnpj) return '';
    let v = cnpj.replace(/\D/g, '');
    v = v.replace(/^(\d{2})(\d)/, '$1.$2');
    v = v.replace(/^(\d{2})\.(\d{3})(\d)/, '$1.$2.$3');
    v = v.replace(/\.(\d{3})(\d)/, '.$1/$2');
    v = v.replace(/(\d{4})(\d)/, '$1-$2');
    return v;
  }
}
