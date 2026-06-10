import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export type TipoRelatorioGerencial =
  | 'contas-a-receber'
  | 'contas-recebidas'
  | 'contas-pagas'
  | 'contas-a-pagar';

export const TIPOS_RELATORIO: { value: TipoRelatorioGerencial; label: string }[] = [
  { value: 'contas-a-receber', label: 'Relatório de contas a receber' },
  { value: 'contas-recebidas', label: 'Relatório de contas recebidas' },
  { value: 'contas-pagas', label: 'Relatório de contas pagas' },
  { value: 'contas-a-pagar', label: 'Relatório de contas a pagar' }
];

export interface RelatorioGerencialLinhaDto {
  parcelaId: number;
  duplicataId: number;
  numeroDuplicata: number;
  numeroParcela: number;
  descricaoDespesa?: string;
  clienteNome?: string;
  dataEmissao: string;
  dataVencimento: string;
  dataPagamento?: string;
  valor: number;
  multa: number;
  juros: number;
  valorTotal: number;
  status: string;
}

export interface RelatorioGerencialResponseDto {
  tipoRelatorio: string;
  tituloRelatorio: string;
  dataInicio: string;
  dataFim: string;
  itens: RelatorioGerencialLinhaDto[];
  totalValor: number;
  totalRegistros: number;
}

@Injectable({
  providedIn: 'root'
})
export class RelatorioGerencialService {
  constructor(private api: ApiService) { }

  obterRelatorio(dataInicio: string, dataFim: string, tipo: TipoRelatorioGerencial): Observable<RelatorioGerencialResponseDto> {
    const params = `dataInicio=${encodeURIComponent(dataInicio)}&dataFim=${encodeURIComponent(dataFim)}&tipo=${encodeURIComponent(tipo)}`;
    return this.api.get<RelatorioGerencialResponseDto>(`relatorio-gerencial?${params}`);
  }
}
