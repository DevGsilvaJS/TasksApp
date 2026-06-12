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
