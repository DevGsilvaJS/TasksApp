import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface DashboardEstatisticasDto {
  totalAtendimentosPorUsuario: number;
  totalContasAPagar: number;
  totalAtendimentosPorCliente: number;
  totalContasPagas: number;
  valorTotalContasPagas: number;
  atendimentosPorUsuario: AtendimentoPorUsuarioDto[];
  contasAPagar: ContaAPagarDto[];
  contasPagas: ContaAPagarDto[];
  atendimentosPorCliente: AtendimentoPorClienteDto[];
}

export interface AtendimentoPorUsuarioDto {
  usuarioId: number;
  usuarioNome: string;
  quantidade: number;
  detalhes: DetalheAtendimentoDto[];
}

export interface DetalheAtendimentoDto {
  tarefaId: number;
  numero?: number;
  clienteId: number;
  clienteCodigo: number;
  clienteNome: string;
}

export interface ContaAPagarDto {
  parcelaId: number;
  duplicataId: number;
  numeroDuplicata: string;
  dataVencimento: string;
  dataPagamento?: string;
  valor: number;
  paga: boolean;
}

export interface AtendimentoPorClienteDto {
  clienteId: number;
  clienteNome: string;
  quantidade: number;
}

export interface ValorPorMesPorUsuarioDto {
  usuarioId: number;
  usuarioNome: string;
  ano: number;
  mes: number;
  mesNome: string;
  valorTotal: number;
  quantidadeContratos: number;
  contratos: ContratoDetalheDto[];
}

export interface ContratoDetalheDto {
  clienteId: number;
  clienteCodigo: number;
  clienteNome: string;
  valorContrato: number;
}

export enum PeriodoFiltro {
  Dia = 'dia',
  Semana = 'semana',
  Mes = 'mes'
}

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  constructor(private api: ApiService) { }

  obterEstatisticas(dataInicio?: Date, dataFim?: Date): Observable<DashboardEstatisticasDto> {
    let url = 'dashboard';
    const params: string[] = [];

    if (dataInicio) {
      params.push(`dataInicio=${dataInicio.toISOString().split('T')[0]}`);
    }
    if (dataFim) {
      params.push(`dataFim=${dataFim.toISOString().split('T')[0]}`);
    }

    if (params.length > 0) {
      url += '?' + params.join('&');
    }

    return this.api.get<DashboardEstatisticasDto>(url);
  }

  obterValoresPorMesPorUsuario(ano?: number): Observable<ValorPorMesPorUsuarioDto[]> {
    let url = 'dashboard/valores-por-mes-usuario';
    if (ano) {
      url += `?ano=${ano}`;
    }
    return this.api.get<ValorPorMesPorUsuarioDto[]>(url);
  }
}
