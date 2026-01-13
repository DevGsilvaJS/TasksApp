import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface CadastroUsuarioDto {
  nome: string;
  sobrenome?: string;
  docFederal?: string;
  docEstadual?: string;
  login: string;
  senha: string;
}

export interface UsuarioResponseDto {
  usuarioId: number;
  pessoaId: number;
  nome: string;
  sobrenome?: string;
  docFederal?: string;
  docEstadual?: string;
  login: string;
}

@Injectable({
  providedIn: 'root'
})
export class UsuarioService {
  constructor(private api: ApiService) { }

  cadastrarUsuario(dto: CadastroUsuarioDto): Observable<UsuarioResponseDto> {
    return this.api.post<UsuarioResponseDto>('usuario', dto);
  }

  obterUsuarioPorId(id: number): Observable<UsuarioResponseDto> {
    return this.api.get<UsuarioResponseDto>(`usuario/${id}`);
  }

  obterUsuarioPorLogin(login: string): Observable<UsuarioResponseDto> {
    return this.api.get<UsuarioResponseDto>(`usuario/login/${login}`);
  }

  listarTodosUsuarios(): Observable<UsuarioResponseDto[]> {
    return this.api.get<UsuarioResponseDto[]>('usuario');
  }
}
