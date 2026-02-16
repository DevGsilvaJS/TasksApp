import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { NotaServicoItemDto } from './nota-servico.service';

export interface PendenciasAlertasDto {
  notasServicoPendentesMes: NotaServicoItemDto[];
  dasPendentesOuAtrasadas: unknown[];
}

@Injectable({
  providedIn: 'root'
})
export class AlertasService {
  constructor(private api: ApiService) { }

  obterPendencias(diasParaAlertaNota: number = 30): Observable<PendenciasAlertasDto> {
    return this.api.get<PendenciasAlertasDto>(`alertas/pendencias?diasParaAlertaNota=${diasParaAlertaNota}`);
  }
}
