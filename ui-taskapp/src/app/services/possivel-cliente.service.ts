import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface PossivelClienteResponseDto {
  pocId: number;
  pocCodigo: string;
  pocLoja?: string;
  pocStatus?: string;
  pocFantasia?: string;
  pocDdd?: string;
  pocCnpj?: string;
  pocRazaoSocial?: string;
  pocEmailComercial?: string;
  pocCelDdd?: string;
  pocCelular?: string;
  pocDataImportacao?: string;
  pocStatusAtendimento?: number;
  pocMotivoPerda?: string;
  pocDataStatusAtendimento?: string;
}

export interface AtualizarStatusAtendimentoDto {
  statusAtendimento: number;
  motivoPerda?: string;
}

export const STATUS_ATENDIMENTO_OPCOES: { valor: number; label: string; descricao: string }[] = [
  { valor: 1, label: 'Não Iniciado', descricao: 'Cliente ainda não contatado.' },
  { valor: 2, label: 'Tentativa de Contato', descricao: 'Ligação realizada, mas não atendeu. WhatsApp enviado e ainda não respondeu.' },
  { valor: 3, label: 'Contato Realizado', descricao: 'Cliente atendeu ligação ou respondeu WhatsApp. Houve conversa inicial.' },
  { valor: 4, label: 'Em Diagnóstico', descricao: 'Cliente demonstrou interesse. Está entendendo a necessidade.' },
  { valor: 5, label: 'Proposta Enviada', descricao: 'Proposta formal enviada. Aguardando retorno.' },
  { valor: 6, label: 'Em Negociação', descricao: 'Cliente pediu ajuste, está avaliando valores ou comparando com concorrente.' },
  { valor: 7, label: 'Follow-up', descricao: 'Já foi cobrado. Sem resposta após proposta.' },
  { valor: 8, label: 'Perdido', descricao: 'Sem interesse, sem orçamento, já tem fornecedor ou momento inadequado.' },
  { valor: 9, label: 'Fechado / Ganho', descricao: 'Cliente aprovou. Contrato fechado.' }
];

@Injectable({
  providedIn: 'root'
})
export class PossivelClienteService {
  constructor(private api: ApiService) { }

  listarTodos(): Observable<PossivelClienteResponseDto[]> {
    return this.api.get<PossivelClienteResponseDto[]>('possivelcliente');
  }

  obterPorId(id: number): Observable<PossivelClienteResponseDto> {
    return this.api.get<PossivelClienteResponseDto>(`possivelcliente/${id}`);
  }

  atualizarStatusAtendimento(id: number, dto: AtualizarStatusAtendimentoDto): Observable<PossivelClienteResponseDto> {
    return this.api.patch<PossivelClienteResponseDto>(`possivelcliente/${id}/status`, dto);
  }

  listarAnotacoes(pocId: number): Observable<PossivelClienteAnotacaoResponseDto[]> {
    return this.api.get<PossivelClienteAnotacaoResponseDto[]>(`possivelcliente/${pocId}/anotacoes`);
  }

  adicionarAnotacao(pocId: number, dto: CadastroPossivelClienteAnotacaoDto): Observable<PossivelClienteAnotacaoResponseDto> {
    return this.api.post<PossivelClienteAnotacaoResponseDto>(`possivelcliente/${pocId}/anotacoes`, dto);
  }

  atualizarAnotacao(pocId: number, anotacaoId: number, dto: { descricao: string }): Observable<PossivelClienteAnotacaoResponseDto> {
    return this.api.put<PossivelClienteAnotacaoResponseDto>(`possivelcliente/${pocId}/anotacoes/${anotacaoId}`, dto);
  }
}

export interface PossivelClienteAnotacaoResponseDto {
  pcaId: number;
  pocId: number;
  usuId: number;
  usuarioNome?: string;
  descricao?: string;
  dataCadastro: string;
}

export interface CadastroPossivelClienteAnotacaoDto {
  descricao?: string;
  usuarioId: number;
}
