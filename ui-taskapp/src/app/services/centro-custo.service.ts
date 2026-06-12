import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface CadastroCentroCustoDto {
  empresaId: number;
}

export interface CentroCustoResponseDto {
  centroCustoId: number;
  empresaId: number;
  empresaFantasia?: string;
  empresaCnpj?: string;
}

@Injectable({ providedIn: 'root' })
export class CentroCustoService {
  constructor(private api: ApiService) {}

  cadastrarCentroCusto(dto: CadastroCentroCustoDto): Observable<CentroCustoResponseDto> {
    return this.api.post<CentroCustoResponseDto>('centrocusto', dto);
  }

  obterCentroCustoPorId(id: number): Observable<CentroCustoResponseDto> {
    return this.api.get<CentroCustoResponseDto>(`centrocusto/${id}`);
  }

  listarTodosCentrosCusto(): Observable<CentroCustoResponseDto[]> {
    return this.api.get<CentroCustoResponseDto[]>('centrocusto');
  }

  atualizarCentroCusto(id: number, dto: CadastroCentroCustoDto): Observable<CentroCustoResponseDto> {
    return this.api.put<CentroCustoResponseDto>(`centrocusto/${id}`, dto);
  }

  excluirCentroCusto(id: number): Observable<void> {
    return this.api.delete<void>(`centrocusto/${id}`);
  }
}
