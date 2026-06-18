import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface DestinatarioEmailDto {
  id: number;
  email: string;
  naoEnviar: boolean;
}

export interface DestinatariosEmailPaginadoDto {
  itens: DestinatarioEmailDto[];
  total: number;
  pagina: number;
  tamanhoPagina: number;
  totalPaginas: number;
}

export interface PesquisaDestinatariosParams {
  termo?: string;
  pagina?: number;
  tamanhoPagina?: number;
}

export interface EnfileirarCampanhaEmailDto {
  campanhaId: number;
  totalDestinatarios: number;
  mensagem: string;
}

export interface CampanhaEmailStatusDto {
  id: number;
  status: string;
  assunto: string;
  totalItens: number;
  enviados: number;
  erros: number;
  pendentes: number;
  dataCriacao: string;
  pausaAte?: string;
}

export interface RelatorioItemEmailDto {
  email: string;
  remetenteEmail?: string;
  dataEnvio?: string;
  mensagemErro?: string;
}

export interface RelatorioCampanhaEmailDto {
  id: number;
  assunto: string;
  status: string;
  dataCriacao: string;
  dataConclusao?: string;
  totalItens: number;
  enviados: number;
  erros: number;
  itensEnviados: RelatorioItemEmailDto[];
  itensComErro: RelatorioItemEmailDto[];
}

export interface EnvioEmailRequestDto {
  assunto: string;
  corpoHtml: string;
  destinatarios: string[];
  anexos?: File[];
}

@Injectable({
  providedIn: 'root'
})
export class EnvioEmailService {
  constructor(private api: ApiService) {}

  pesquisarDestinatarios(params: PesquisaDestinatariosParams = {}): Observable<DestinatariosEmailPaginadoDto> {
    const query = new URLSearchParams();
    if (params.termo?.trim()) {
      query.set('termo', params.termo.trim());
    }
    query.set('pagina', String(params.pagina ?? 1));
    query.set('tamanhoPagina', String(params.tamanhoPagina ?? 15));
    return this.api.get<DestinatariosEmailPaginadoDto>(`email-envio/destinatarios?${query.toString()}`);
  }

  atualizarNaoEnviar(id: number, naoEnviar: boolean): Observable<DestinatarioEmailDto> {
    return this.api.patch<DestinatarioEmailDto>(`email-envio/destinatarios/${id}/nao-enviar`, { naoEnviar });
  }

  enviar(dto: EnvioEmailRequestDto): Observable<EnfileirarCampanhaEmailDto> {
    const formData = new FormData();
    formData.append('assunto', dto.assunto);
    formData.append('corpoHtml', dto.corpoHtml);
    dto.destinatarios.forEach((email) => formData.append('destinatarios', email));
    dto.anexos?.forEach((arquivo) => formData.append('anexos', arquivo, arquivo.name));
    return this.api.postFormData<EnfileirarCampanhaEmailDto>('email-envio/enviar', formData);
  }

  obterCampanhaAtiva(): Observable<CampanhaEmailStatusDto | null> {
    return this.api.get<CampanhaEmailStatusDto | null>('email-envio/campanhas/ativa');
  }

  obterStatusCampanha(id: number): Observable<CampanhaEmailStatusDto> {
    return this.api.get<CampanhaEmailStatusDto>(`email-envio/campanhas/${id}`);
  }

  obterRelatorio(id: number): Observable<RelatorioCampanhaEmailDto> {
    return this.api.get<RelatorioCampanhaEmailDto>(`email-envio/campanhas/${id}/relatorio`);
  }

  listarRelatorios(): Observable<RelatorioCampanhaEmailDto[]> {
    return this.api.get<RelatorioCampanhaEmailDto[]>('email-envio/campanhas/relatorios');
  }
}
