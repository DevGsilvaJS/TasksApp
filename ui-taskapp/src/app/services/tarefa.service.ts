import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export enum StatusTarefa {
  EmAberto = 1,
  Concluida = 2,
  Cancelada = 3
}

export interface CadastroTarefaDto {
  clienteId: number;
  usuarioId: number;
  status: StatusTarefa;
  dataConclusao?: string;
  descricao?: string;
  titulo?: string;
  protocolo?: string;
  solicitante?: string;
  imagens?: File[];
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

export interface ImagemResponseDto {
  imagemId: number;
  tarefaId: number;
  urlImagem: string;
  dataArquivo?: string;
}

export interface TarefaResponseDto {
  tarefaId: number;
  clienteId: number;
  clienteNome: string;
  usuarioId: number;
  usuarioNome: string;
  dataCadastro?: string;
  dataConclusao?: string;
  status: StatusTarefa;
  statusDescricao: string;
  titulo?: string;
  protocolo?: string;
  solicitante?: string;
  anotacoes: AnotacaoResponseDto[];
  imagens: ImagemResponseDto[];
}

@Injectable({
  providedIn: 'root'
})
export class TarefaService {
  constructor(private api: ApiService) { }

  cadastrarTarefa(dto: CadastroTarefaDto): Observable<TarefaResponseDto> {
    const formData = this.criarFormData(dto);
    return this.api.postFormData<TarefaResponseDto>('tarefa', formData);
  }

  obterTarefaPorId(id: number): Observable<TarefaResponseDto> {
    return this.api.get<TarefaResponseDto>(`tarefa/${id}`);
  }

  listarTodasTarefas(): Observable<TarefaResponseDto[]> {
    return this.api.get<TarefaResponseDto[]>('tarefa');
  }

  atualizarTarefa(id: number, dto: CadastroTarefaDto): Observable<TarefaResponseDto> {
    const formData = this.criarFormData(dto);
    return this.api.putFormData<TarefaResponseDto>(`tarefa/${id}`, formData);
  }

  private criarFormData(dto: CadastroTarefaDto): FormData {
    const formData = new FormData();
    formData.append('clienteId', dto.clienteId.toString());
    formData.append('usuarioId', dto.usuarioId.toString());
    formData.append('status', dto.status.toString());
    
    if (dto.dataConclusao) {
      formData.append('dataConclusao', dto.dataConclusao);
    }
    if (dto.descricao) {
      formData.append('descricao', dto.descricao);
    }
    if (dto.titulo) {
      formData.append('titulo', dto.titulo);
    }
    if (dto.protocolo) {
      formData.append('protocolo', dto.protocolo);
    }
    if (dto.solicitante) {
      formData.append('solicitante', dto.solicitante);
    }
    
    if (dto.imagens && dto.imagens.length > 0) {
      dto.imagens.forEach((imagem, index) => {
        formData.append(`imagens`, imagem);
      });
    }
    
    return formData;
  }

  excluirTarefa(id: number): Observable<void> {
    return this.api.delete<void>(`tarefa/${id}`);
  }

  alterarStatusTarefa(id: number, status: StatusTarefa): Observable<TarefaResponseDto> {
    return this.api.patch<TarefaResponseDto>(`tarefa/${id}/status`, { status });
  }
}
