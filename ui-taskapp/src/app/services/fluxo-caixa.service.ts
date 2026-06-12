import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface FluxoCaixaMesDto {
  mes: number;
  nomeMes: string;
  receitas: number;
  despesas: number;
  saldo: number;
}

export interface FluxoCaixaPlanoContasDto {
  planoContasId: number;
  descricao: string;
  meses: FluxoCaixaMesDto[];
  totalReceitas: number;
  totalDespesas: number;
  saldo: number;
}

export interface FluxoCaixaCentroCustoDto {
  empresaId: number;
  empresaFantasia: string;
  empresaCnpj?: string;
  meses: FluxoCaixaMesDto[];
  planosContas: FluxoCaixaPlanoContasDto[];
  totalReceitas: number;
  totalDespesas: number;
  saldo: number;
}

export interface FluxoCaixaResponseDto {
  ano: number;
  centros: FluxoCaixaCentroCustoDto[];
  totaisMensais: FluxoCaixaMesDto[];
  totalReceitasAno: number;
  totalDespesasAno: number;
  saldoAno: number;
}

@Injectable({ providedIn: 'root' })
export class FluxoCaixaService {
  constructor(private api: ApiService) {}

  obterFluxoCaixa(ano?: number): Observable<FluxoCaixaResponseDto> {
    const query = ano != null ? `?ano=${ano}` : '';
    return this.api.get<FluxoCaixaResponseDto>(`fluxocaixa${query}`);
  }
}
