import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface CadastroRegimentoDto {
  titulo: string;
  descricao: string;
  ativo: boolean;
}

export interface RegimentoResponseDto {
  regimentoId: number;
  titulo: string;
  descricao: string;
  ativo: boolean;
  situacaoAprovacao: string;
  quantidadeAceites: number;
  possuiAceites: boolean;
}

export interface RegimentoAceiteResponseDto {
  aceiteId: number;
  usuarioId: number;
  usuarioNome: string;
  aceito: boolean;
  situacao: string;
  observacao?: string;
  dataAceite?: string;
}

export interface RegimentoDetalheResponseDto extends RegimentoResponseDto {
  meuAceiteAtual?: RegimentoAceiteResponseDto | null;
  aceites: RegimentoAceiteResponseDto[];
}

export interface CadastroRegimentoAceiteDto {
  aceito: boolean;
  observacao?: string;
}

export interface RegimentoAceiteLogResponseDto {
  logId: number;
  usuarioId: number;
  usuarioNome: string;
  acao: string;
  observacao?: string;
  data: string;
}

@Injectable({ providedIn: 'root' })
export class RegimentoService {
  constructor(private api: ApiService) {}

  cadastrarRegimento(dto: CadastroRegimentoDto): Observable<RegimentoResponseDto> {
    return this.api.post<RegimentoResponseDto>('regimento', dto);
  }

  obterRegimentoPorId(id: number, usuarioId?: number | null): Observable<RegimentoDetalheResponseDto> {
    const query = usuarioId != null ? `?usuarioId=${usuarioId}` : '';
    return this.api.get<RegimentoDetalheResponseDto>(`regimento/${id}${query}`);
  }

  listarRegimentos(): Observable<RegimentoResponseDto[]> {
    return this.api.get<RegimentoResponseDto[]>('regimento');
  }

  atualizarRegimento(id: number, dto: CadastroRegimentoDto): Observable<RegimentoResponseDto> {
    return this.api.put<RegimentoResponseDto>(`regimento/${id}`, dto);
  }

  excluirRegimento(id: number): Observable<void> {
    return this.api.delete<void>(`regimento/${id}`);
  }

  registrarAceite(id: number, usuarioId: number, dto: CadastroRegimentoAceiteDto): Observable<RegimentoAceiteResponseDto> {
    return this.api.post<RegimentoAceiteResponseDto>(`regimento/${id}/aceite?usuarioId=${usuarioId}`, dto);
  }

  desfazerAceite(aceiteId: number, usuarioId: number): Observable<void> {
    return this.api.delete<void>(`regimento/aceite/${aceiteId}?usuarioId=${usuarioId}`);
  }

  listarLogAceites(regimentoId: number): Observable<RegimentoAceiteLogResponseDto[]> {
    return this.api.get<RegimentoAceiteLogResponseDto[]>(`regimento/${regimentoId}/log`);
  }
}
