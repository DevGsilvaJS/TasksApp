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

  obterMes(centro: FluxoCaixaCentroCustoDto, mes: number): FluxoCaixaMesDto {
    return centro.meses[mes - 1];
  }

  obterMesPlano(plano: FluxoCaixaPlanoContasDto, mes: number): FluxoCaixaMesDto {
    return plano.meses[mes - 1];
  }

  obterTotalMes(mes: number): FluxoCaixaMesDto | undefined {
    return this.dados?.totaisMensais.find(m => m.mes === mes);
  }

  formatarMoeda(valor: number): string {
    if (!valor) return '-';
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
