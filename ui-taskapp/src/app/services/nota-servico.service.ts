import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface NotaServicoItemDto {
  clienteId: number;
  clienteCodigo: number;
  fantasia: string;
  diaNfServico: number;
  ano: number;
  mes: number;
  enviado: boolean;
  dataEnvio?: string;
  envioNotaServicoId?: number;
}

@Injectable({
  providedIn: 'root'
})
export class NotaServicoService {
  constructor(private api: ApiService) { }

  listarNotasDoMesAtual(): Observable<NotaServicoItemDto[]> {
    return this.api.get<NotaServicoItemDto[]>('notaservico/mes-atual');
  }

  listarNotasDoMes(ano: number, mes: number): Observable<NotaServicoItemDto[]> {
    return this.api.get<NotaServicoItemDto[]>(`notaservico/mes/${ano}/${mes}`);
  }

  marcarComoEnviado(clienteId: number, ano: number, mes: number, dataEnvio?: string): Observable<NotaServicoItemDto> {
    const body = dataEnvio ? { dataEnvio } : {};
    return this.api.patch<NotaServicoItemDto>(
      `notaservico/marcar-enviado/${clienteId}/${ano}/${mes}`,
      body
    );
  }
}
