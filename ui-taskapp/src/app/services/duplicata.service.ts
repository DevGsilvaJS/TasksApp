import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface CadastroParcelaDto {
  numeroParcela: number;
  valor: number;
  vencimento: string;
  multa?: number;
  juros?: number;
}

export interface CadastroDuplicataDto {
  numero: number;
  dataEmissao: string;
  numeroParcelas: number;
  valorTotal: number;
  multa?: number;
  juros?: number;
  descricaoDespesa?: string;
  tipo?: string; // CP = Contas a Pagar, CR = Contas a Receber
  dataPrimeiroVencimento?: string;
  parcelas?: CadastroParcelaDto[];
}

export interface ParcelaResponseDto {
  parcelaId: number;
  duplicataId: number;
  numeroParcela: number;
  valor: number;
  multa: number;
  juros: number;
  valorTotal: number;
  vencimento: string;
  status: string;
  dataPagamento?: string;
}

export interface DuplicataResponseDto {
  duplicataId: number;
  numero: number;
  dataEmissao: string;
  numeroParcelas: number;
  descricaoDespesa?: string;
  tipo?: string; // CP = Contas a Pagar, CR = Contas a Receber
  parcelas: ParcelaResponseDto[];
  valorTotal: number;
  valorPago: number;
  valorPendente: number;
}

@Injectable({
  providedIn: 'root'
})
export class DuplicataService {
  constructor(private api: ApiService) { }

  cadastrarDuplicata(dto: CadastroDuplicataDto): Observable<DuplicataResponseDto> {
    return this.api.post<DuplicataResponseDto>('duplicata', dto);
  }

  obterDuplicataPorId(id: number): Observable<DuplicataResponseDto> {
    return this.api.get<DuplicataResponseDto>(`duplicata/${id}`);
  }

  listarTodasDuplicatas(): Observable<DuplicataResponseDto[]> {
    return this.api.get<DuplicataResponseDto[]>('duplicata');
  }

  atualizarDuplicata(id: number, dto: CadastroDuplicataDto): Observable<DuplicataResponseDto> {
    return this.api.put<DuplicataResponseDto>(`duplicata/${id}`, dto);
  }

  excluirDuplicata(id: number): Observable<void> {
    return this.api.delete<void>(`duplicata/${id}`);
  }

  baixarParcela(parcelaId: number): Observable<ParcelaResponseDto> {
    return this.api.post<ParcelaResponseDto>(`duplicata/parcelas/${parcelaId}/baixar`, {});
  }

  reativarParcela(parcelaId: number): Observable<ParcelaResponseDto> {
    return this.api.post<ParcelaResponseDto>(`duplicata/parcelas/${parcelaId}/reativar`, {});
  }

  listarDuplicatasPorTipo(tipo: string): Observable<DuplicataResponseDto[]> {
    return this.api.get<DuplicataResponseDto[]>(`duplicata/tipo/${tipo}`);
  }

  obterProximoNumero(tipo: string): Observable<number> {
    return this.api.get<number>(`duplicata/proximo-numero/${tipo}`);
  }
}
