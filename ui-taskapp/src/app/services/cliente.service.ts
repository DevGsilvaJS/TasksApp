import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiService } from './api.service';
import { ordenarClientesPorCodigo } from '../utils/cliente-ordenacao.util';

export enum StatusCliente {
  Ativo = 1,
  Inativo = 2,
  Suspenso = 3
}

export interface CadastroClienteDto {
  fantasia: string;
  docFederal?: string;
  docEstadual?: string;
  codigo: string;
  usuarioId: number;
  valorContrato?: number;
  dataFinalContrato?: string;
  diaPagamento?: number;
  /** Dia do mês (1-31) da NF de serviço. */
  diaNfServico?: number;
  emails?: string[];
  status?: StatusCliente;
  contratos?: ClienteContratoValorDto[];
}

export interface ClienteContratoValorDto {
  valorMensal: number;
  dataInicio: string;
  dataFim?: string;
}

export interface ClienteResponseDto {
  clienteId: number;
  pessoaId: number;
  fantasia: string;
  docFederal?: string;
  docEstadual?: string;
  codigo: string;
  usuarioId?: number;
  usuarioNome?: string;
  dataCadastro?: string;
  valorContrato?: number;
  dataFinalContrato?: string;
  valorContratoVigente?: number;
  vigenciaInicio?: string;
  vigenciaFim?: string;
  diaPagamento?: number;
  /** Dia do mês (1-31) da NF de serviço. */
  diaNfServico?: number;
  emails?: string[];
  status?: StatusCliente;
  statusDescricao?: string;
  contratos?: ClienteContratoValorResponseDto[];
}

export interface ClienteContratoValorResponseDto {
  contratoId: number;
  valorMensal: number;
  dataInicio: string;
  dataFim?: string;
}

@Injectable({
  providedIn: 'root'
})
export class ClienteService {
  constructor(private api: ApiService) { }

  cadastrarCliente(dto: CadastroClienteDto): Observable<ClienteResponseDto> {
    return this.api.post<ClienteResponseDto>('cliente', dto);
  }

  obterClientePorId(id: number): Observable<ClienteResponseDto> {
    return this.api.get<ClienteResponseDto>(`cliente/${id}`);
  }

  listarTodosClientes(): Observable<ClienteResponseDto[]> {
    return this.api.get<ClienteResponseDto[]>('cliente').pipe(
      map(ordenarClientesPorCodigo)
    );
  }

  atualizarCliente(id: number, dto: CadastroClienteDto): Observable<ClienteResponseDto> {
    return this.api.put<ClienteResponseDto>(`cliente/${id}`, dto);
  }

  excluirCliente(id: number): Observable<void> {
    return this.api.delete<void>(`cliente/${id}`);
  }
}
