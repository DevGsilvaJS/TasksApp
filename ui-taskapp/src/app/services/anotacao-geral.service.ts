import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface CadastroAnotacaoGeralDto {
  descricao: string;
  link?: string;
}

export interface AnotacaoGeralResponseDto {
  anotacaoId: number;
  descricao: string;
  link?: string;
  dataCadastro?: string;
}

@Injectable({
  providedIn: 'root'
})
export class AnotacaoGeralService {
  constructor(private api: ApiService) { }

  listarTodasAnotacoes(): Observable<AnotacaoGeralResponseDto[]> {
    return this.api.get<AnotacaoGeralResponseDto[]>('anotacao-geral');
  }

  obterAnotacaoPorId(id: number): Observable<AnotacaoGeralResponseDto> {
    return this.api.get<AnotacaoGeralResponseDto>(`anotacao-geral/${id}`);
  }

  cadastrarAnotacao(dto: CadastroAnotacaoGeralDto): Observable<AnotacaoGeralResponseDto> {
    return this.api.post<AnotacaoGeralResponseDto>('anotacao-geral', dto);
  }

  atualizarAnotacao(id: number, dto: CadastroAnotacaoGeralDto): Observable<AnotacaoGeralResponseDto> {
    return this.api.put<AnotacaoGeralResponseDto>(`anotacao-geral/${id}`, dto);
  }

  excluirAnotacao(id: number): Observable<void> {
    return this.api.delete<void>(`anotacao-geral/${id}`);
  }
}
