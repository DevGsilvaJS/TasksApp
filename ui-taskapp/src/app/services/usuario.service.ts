import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export const PERFIL_COMERCIAL = 1;
export const PERFIL_ADMINISTRADOR = 2;

export const PERFIS_OPCOES: { value: number; label: string }[] = [
  { value: PERFIL_COMERCIAL, label: 'Comercial' },
  { value: PERFIL_ADMINISTRADOR, label: 'Administrador' }
];

export interface CadastroUsuarioDto {
  nome: string;
  sobrenome?: string;
  docFederal?: string;
  docEstadual?: string;
  login: string;
  senha: string;
  perfil: number;
}

export interface AtualizarUsuarioDto {
  nome: string;
  sobrenome?: string;
  docFederal?: string;
  docEstadual?: string;
  login: string;
  senha?: string;
  perfil: number;
}

export interface UsuarioResponseDto {
  usuarioId: number;
  pessoaId: number;
  nome: string;
  sobrenome?: string;
  docFederal?: string;
  docEstadual?: string;
  login: string;
  perfil: number;
}

@Injectable({
  providedIn: 'root'
})
export class UsuarioService {
  constructor(private api: ApiService) { }

  cadastrarUsuario(dto: CadastroUsuarioDto): Observable<UsuarioResponseDto> {
    return this.api.post<UsuarioResponseDto>('usuario', dto);
  }

  atualizarUsuario(id: number, dto: AtualizarUsuarioDto): Observable<UsuarioResponseDto> {
    return this.api.put<UsuarioResponseDto>(`usuario/${id}`, dto);
  }

  obterUsuarioPorId(id: number): Observable<UsuarioResponseDto> {
    return this.api.get<UsuarioResponseDto>(`usuario/${id}`);
  }

  obterUsuarioPorLogin(login: string): Observable<UsuarioResponseDto> {
    return this.api.get<UsuarioResponseDto>(`usuario/login/${encodeURIComponent(login)}`);
  }

  listarTodosUsuarios(): Observable<UsuarioResponseDto[]> {
    return this.api.get<UsuarioResponseDto[]>('usuario');
  }
}
