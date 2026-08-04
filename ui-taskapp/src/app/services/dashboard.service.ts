import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface DashboardEstatisticasDto {
  totalAtendimentosPorUsuario: number;
  mediaDiariaAtendimentos: number;
  mediaDiariaPorOperador: number;
  diasUteisMesAtual: number;
  mediasPorOperador: MediaPorOperadorDto[];
  totalContasAPagar: number;
  valorTotalContasAPagar: number;
  totalAtendimentosPorCliente: number;
  totalContasPagas: number;
  valorTotalContasPagas: number;
  totalContasAReceber: number;
  valorTotalContasAReceber: number;
  totalContasRecebidas: number;
  valorTotalContasRecebidas: number;
  lucro: number;
  atendimentosPorUsuario: AtendimentoPorUsuarioDto[];
  contasAPagar: ContaAPagarDto[];
  contasPagas: ContaAPagarDto[];
  contasAReceber: ContaAPagarDto[];
  contasRecebidas: ContaAPagarDto[];
  atendimentosPorCliente: AtendimentoPorClienteDto[];
  atendimentosPorClienteMes: AtendimentoPorClienteMesDto[];
}

export interface MediaPorOperadorDto {
  usuarioId: number;
  usuarioNome: string;
  quantidade: number;
  mediaDiaria: number;
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
  titulo?: string;
  dataCadastro?: string;
  clienteId: number;
  clienteCodigo: string;
  clienteNome: string;
}

export interface ContaAPagarDto {
  parcelaId: number;
  duplicataId: number;
  numeroDuplicata: string;
  descricaoDespesa?: string;
  dataVencimento: string;
  dataPagamento?: string;
  valor: number;
  paga: boolean;
  clienteNome?: string;
  centroCustoDescricao?: string;
  planoContasDescricao?: string;
}

export interface AtendimentoPorClienteDto {
  clienteId: number;
  clienteNome: string;
  quantidade: number;
}

export interface AtendimentoPorClienteMesDto {
  clienteId: number;
  clienteNome: string;
  quantidade: number;
  percentual: number;
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
  clienteCodigo: string;
  clienteNome: string;
  valorContrato: number;
}

export interface TelemarketingContatosDto {
  contatosNoDia: number;
  contatosSemanaAtual: number;
  contatosMesAtual: number;
  contatosAnoAtual: number;
}

export interface AlertaContratoVencendoDto {
  clienteId: number;
  clienteCodigo: string;
  clienteNome: string;
  dataFimVigencia: string;
  diasParaVencer: number;
  valorMensalVigente: number;
}

export enum PeriodoFiltro {
  Dia = 'dia',
  Semana = 'semana',
  Mes = 'mes',
  MesAnterior = 'mes-anterior'
}

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  constructor(private api: ApiService) { }

  obterContatosTelemarketing(): Observable<TelemarketingContatosDto> {
    return this.api.get<TelemarketingContatosDto>('dashboard/contatos-telemarketing');
  }

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

  obterAlertasContratosVencendo(diasAntecedencia = 30): Observable<AlertaContratoVencendoDto[]> {
    return this.api.get<AlertaContratoVencendoDto[]>(`dashboard/alertas-contratos-vencendo?diasAntecedencia=${diasAntecedencia}`);
  }
}
