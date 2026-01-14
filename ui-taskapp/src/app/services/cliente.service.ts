import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface CadastroClienteDto {
  fantasia: string;
  docFederal?: string;
  docEstadual?: string;
  codigo: number;
  valorContrato?: number;
}

export interface ClienteResponseDto {
  clienteId: number;
  pessoaId: number;
  fantasia: string;
  docFederal?: string;
  docEstadual?: string;
  codigo: number;
  dataCadastro?: string;
  valorContrato?: number;
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
