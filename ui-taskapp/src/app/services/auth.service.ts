import { Injectable } from '@angular/core';
import { Observable, BehaviorSubject, of } from 'rxjs';
import { tap, catchError } from 'rxjs/operators';
import { ApiService } from './api.service';

export interface LoginDto {
  login: string;
  senha: string;
}

export interface LoginResponseDto {
  usuarioId: number;
  login: string;
  nome: string;
  autenticado: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly STORAGE_KEY = 'usuario_autenticado';
  private usuarioSubject = new BehaviorSubject<LoginResponseDto | null>(this.getUsuarioFromStorage());
  public usuario$ = this.usuarioSubject.asObservable();

  constructor(private api: ApiService) {
    // Verificar se há usuário no sessionStorage ao inicializar
    const usuario = this.getUsuarioFromStorage();
    if (usuario) {
      this.usuarioSubject.next(usuario);
    }
  }

  login(dto: LoginDto): Observable<LoginResponseDto> {
    return this.api.post<LoginResponseDto>('auth/login', dto).pipe(
      tap(response => {
        if (response.autenticado) {
          this.salvarUsuario(response);
          this.usuarioSubject.next(response);
        }
      }),
      catchError(error => {
        console.error('Erro ao fazer login:', error);
        throw error;
      })
    );
  }

  logout(): void {
    sessionStorage.removeItem(this.STORAGE_KEY);
    this.usuarioSubject.next(null);
  }

  isAuthenticated(): boolean {
    const usuario = this.getUsuarioFromStorage();
    return usuario !== null && usuario.autenticado;
  }

  getUsuario(): LoginResponseDto | null {
    return this.usuarioSubject.value;
  }

  getUsuarioId(): number | null {
    const usuario = this.getUsuario();
    return usuario ? usuario.usuarioId : null;
  }

  private salvarUsuario(usuario: LoginResponseDto): void {
    sessionStorage.setItem(this.STORAGE_KEY, JSON.stringify(usuario));
  }

  private getUsuarioFromStorage(): LoginResponseDto | null {
    try {
      const stored = sessionStorage.getItem(this.STORAGE_KEY);
      if (stored) {
        const usuario = JSON.parse(stored) as LoginResponseDto;
        return usuario.autenticado ? usuario : null;
      }
    } catch (error) {
      console.error('Erro ao ler usuário do sessionStorage:', error);
    }
    return null;
  }
}
