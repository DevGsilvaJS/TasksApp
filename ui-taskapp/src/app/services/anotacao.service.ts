import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface CadastroAnotacaoDto {
  tarefaId: number;
  usuarioId: number;
  descricao: string;
}

export interface AnotacaoResponseDto {
  anotacaoId: number;
  tarefaId: number;
  usuarioId: number;
  usuarioNome: string;
  descricao: string;
  dataCadastro?: string;
  descricaoFormatada: string;
}

@Injectable({
  providedIn: 'root'
})
export class AnotacaoService {
  constructor(private api: ApiService) { }

  cadastrarAnotacao(dto: CadastroAnotacaoDto): Observable<AnotacaoResponseDto> {
    return this.api.post<AnotacaoResponseDto>('anotacao', dto);
  }

  obterAnotacoesPorTarefa(tarefaId: number): Observable<AnotacaoResponseDto[]> {
    return this.api.get<AnotacaoResponseDto[]>(`anotacao/tarefa/${tarefaId}`);
  }

  excluirAnotacao(id: number): Observable<void> {
    return this.api.delete<void>(`anotacao/${id}`);
  }
}
