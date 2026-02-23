import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface CadastroItemDto {
  id: number;
  descricao: string;
  ativo: boolean;
}

export interface StatusAtendimentoComercialDto {
  id: number;
  numero: number;
  descricao: string;
  ativo: boolean;
}

export interface StatusAtendimentoComercialRequestDto {
  descricao: string;
  ativo: boolean;
}

export interface CadastroItemRequestDto {
  descricao: string;
  ativo: boolean;
}

const BASE = 'cadastro-atendimento';

@Injectable({ providedIn: 'root' })
export class CadastroAtendimentoService {
  constructor(private api: ApiService) {}

  listarStatus(apenasAtivos?: boolean): Observable<CadastroItemDto[]> {
    const q = apenasAtivos != null ? `?apenasAtivos=${apenasAtivos}` : '';
    return this.api.get<CadastroItemDto[]>(`${BASE}/status${q}`);
  }

  obterStatus(id: number): Observable<CadastroItemDto> {
    return this.api.get<CadastroItemDto>(`${BASE}/status/${id}`);
  }

  criarStatus(dto: CadastroItemRequestDto): Observable<CadastroItemDto> {
    return this.api.post<CadastroItemDto>(`${BASE}/status`, dto);
  }

  atualizarStatus(id: number, dto: CadastroItemRequestDto): Observable<CadastroItemDto> {
    return this.api.put<CadastroItemDto>(`${BASE}/status/${id}`, dto);
  }

  alterarAtivoStatus(id: number, ativo: boolean): Observable<void> {
    return this.api.patch<void>(`${BASE}/status/${id}/ativo`, { ativo });
  }

  listarTipoAtendimento(apenasAtivos?: boolean): Observable<CadastroItemDto[]> {
    const q = apenasAtivos != null ? `?apenasAtivos=${apenasAtivos}` : '';
    return this.api.get<CadastroItemDto[]>(`${BASE}/tipo-atendimento${q}`);
  }

  obterTipoAtendimento(id: number): Observable<CadastroItemDto> {
    return this.api.get<CadastroItemDto>(`${BASE}/tipo-atendimento/${id}`);
  }

  criarTipoAtendimento(dto: CadastroItemRequestDto): Observable<CadastroItemDto> {
    return this.api.post<CadastroItemDto>(`${BASE}/tipo-atendimento`, dto);
  }

  atualizarTipoAtendimento(id: number, dto: CadastroItemRequestDto): Observable<CadastroItemDto> {
    return this.api.put<CadastroItemDto>(`${BASE}/tipo-atendimento/${id}`, dto);
  }

  alterarAtivoTipoAtendimento(id: number, ativo: boolean): Observable<void> {
    return this.api.patch<void>(`${BASE}/tipo-atendimento/${id}/ativo`, { ativo });
  }

  listarTipoContato(apenasAtivos?: boolean): Observable<CadastroItemDto[]> {
    const q = apenasAtivos != null ? `?apenasAtivos=${apenasAtivos}` : '';
    return this.api.get<CadastroItemDto[]>(`${BASE}/tipo-contato${q}`);
  }

  obterTipoContato(id: number): Observable<CadastroItemDto> {
    return this.api.get<CadastroItemDto>(`${BASE}/tipo-contato/${id}`);
  }

  criarTipoContato(dto: CadastroItemRequestDto): Observable<CadastroItemDto> {
    return this.api.post<CadastroItemDto>(`${BASE}/tipo-contato`, dto);
  }

  atualizarTipoContato(id: number, dto: CadastroItemRequestDto): Observable<CadastroItemDto> {
    return this.api.put<CadastroItemDto>(`${BASE}/tipo-contato/${id}`, dto);
  }

  alterarAtivoTipoContato(id: number, ativo: boolean): Observable<void> {
    return this.api.patch<void>(`${BASE}/tipo-contato/${id}/ativo`, { ativo });
  }

  listarStatusAtendimentoComercial(apenasAtivos?: boolean): Observable<StatusAtendimentoComercialDto[]> {
    const q = apenasAtivos != null ? `?apenasAtivos=${apenasAtivos}` : '';
    return this.api.get<StatusAtendimentoComercialDto[]>(`${BASE}/status-atendimento-comercial${q}`);
  }

  obterStatusAtendimentoComercial(id: number): Observable<StatusAtendimentoComercialDto> {
    return this.api.get<StatusAtendimentoComercialDto>(`${BASE}/status-atendimento-comercial/${id}`);
  }

  criarStatusAtendimentoComercial(dto: StatusAtendimentoComercialRequestDto): Observable<StatusAtendimentoComercialDto> {
    return this.api.post<StatusAtendimentoComercialDto>(`${BASE}/status-atendimento-comercial`, dto);
  }

  atualizarStatusAtendimentoComercial(id: number, dto: StatusAtendimentoComercialRequestDto): Observable<StatusAtendimentoComercialDto> {
    return this.api.put<StatusAtendimentoComercialDto>(`${BASE}/status-atendimento-comercial/${id}`, dto);
  }

  excluirStatusAtendimentoComercial(id: number): Observable<void> {
    return this.api.delete<void>(`${BASE}/status-atendimento-comercial/${id}`);
  }

  alterarAtivoStatusAtendimentoComercial(id: number, ativo: boolean): Observable<void> {
    return this.api.patch<void>(`${BASE}/status-atendimento-comercial/${id}/ativo`, { ativo });
  }
}
