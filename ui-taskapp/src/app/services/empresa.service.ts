import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface CadastroEmpresaDto {
  cnpj: string;
  razaoSocial: string;
  fantasia: string;
}

export interface EmpresaResponseDto {
  empresaId: number;
  cnpj: string;
  razaoSocial: string;
  fantasia: string;
}

@Injectable({ providedIn: 'root' })
export class EmpresaService {
  constructor(private api: ApiService) {}

  cadastrarEmpresa(dto: CadastroEmpresaDto): Observable<EmpresaResponseDto> {
    return this.api.post<EmpresaResponseDto>('empresa', dto);
  }

  obterEmpresaPorId(id: number): Observable<EmpresaResponseDto> {
    return this.api.get<EmpresaResponseDto>(`empresa/${id}`);
  }

  listarTodasEmpresas(): Observable<EmpresaResponseDto[]> {
    return this.api.get<EmpresaResponseDto[]>('empresa');
  }

  atualizarEmpresa(id: number, dto: CadastroEmpresaDto): Observable<EmpresaResponseDto> {
    return this.api.put<EmpresaResponseDto>(`empresa/${id}`, dto);
  }

  excluirEmpresa(id: number): Observable<void> {
    return this.api.delete<void>(`empresa/${id}`);
  }
}
