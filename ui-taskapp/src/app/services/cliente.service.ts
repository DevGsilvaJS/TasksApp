import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export enum StatusCliente {
  Ativo = 1,
  Inativo = 2,
  Suspenso = 3
}

export interface CadastroClienteDto {
  fantasia: string;
  docFederal?: string;
  docEstadual?: string;
  codigo: number;
  usuarioId: number;
  valorContrato?: number;
  dataFinalContrato?: string;
  diaPagamento?: number;
  status?: StatusCliente;
}

export interface ClienteResponseDto {
  clienteId: number;
  pessoaId: number;
  fantasia: string;
  docFederal?: string;
  docEstadual?: string;
  codigo: number;
  usuarioId?: number;
  usuarioNome?: string;
  dataCadastro?: string;
  valorContrato?: number;
  dataFinalContrato?: string;
  diaPagamento?: number;
  status?: StatusCliente;
  statusDescricao?: string;
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
    return this.api.get<ClienteResponseDto[]>('cliente');
  }

  atualizarCliente(id: number, dto: CadastroClienteDto): Observable<ClienteResponseDto> {
    return this.api.put<ClienteResponseDto>(`cliente/${id}`, dto);
  }

  excluirCliente(id: number): Observable<void> {
    return this.api.delete<void>(`cliente/${id}`);
  }
}
