import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface CadastroPlanoContasDto {
  descricao: string;
}

export interface PlanoContasResponseDto {
  planoContasId: number;
  descricao: string;
}

export const PLANO_RECEITA_CONSULTORIA = 'RECEITA DE CONSULTORIA';

export function ehPlanoReceitaConsultoria(plano: Pick<PlanoContasResponseDto, 'descricao'>): boolean {
  return plano.descricao?.trim().toUpperCase() === PLANO_RECEITA_CONSULTORIA;
}

export function filtrarPlanosParaContasPagar(planos: PlanoContasResponseDto[]): PlanoContasResponseDto[] {
  return planos.filter(p => !ehPlanoReceitaConsultoria(p));
}

export function planoContasIdPermitidoEmContasPagar(
  planoContasId: number | undefined,
  planos: PlanoContasResponseDto[]
): number | undefined {
  if (!planoContasId) {
    return undefined;
  }
  const plano = planos.find(p => p.planoContasId === planoContasId);
  if (!plano || ehPlanoReceitaConsultoria(plano)) {
    return undefined;
  }
  return planoContasId;
}

@Injectable({ providedIn: 'root' })
export class PlanoContasService {
  constructor(private api: ApiService) {}

  cadastrarPlanoContas(dto: CadastroPlanoContasDto): Observable<PlanoContasResponseDto> {
    return this.api.post<PlanoContasResponseDto>('planocontas', dto);
  }

  obterPlanoContasPorId(id: number): Observable<PlanoContasResponseDto> {
    return this.api.get<PlanoContasResponseDto>(`planocontas/${id}`);
  }

  listarTodosPlanosContas(): Observable<PlanoContasResponseDto[]> {
    return this.api.get<PlanoContasResponseDto[]>('planocontas');
  }

  atualizarPlanoContas(id: number, dto: CadastroPlanoContasDto): Observable<PlanoContasResponseDto> {
    return this.api.put<PlanoContasResponseDto>(`planocontas/${id}`, dto);
  }

  excluirPlanoContas(id: number): Observable<void> {
    return this.api.delete<void>(`planocontas/${id}`);
  }
}
