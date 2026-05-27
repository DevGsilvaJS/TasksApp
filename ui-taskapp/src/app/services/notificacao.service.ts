import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export type TipoNotificacao = 'sucesso' | 'erro' | 'aviso' | 'info';

export type NotificacaoUi = {
  tipo: TipoNotificacao;
  titulo: string;
  mensagem: string;
  confirmacao: boolean;
  textoOk: string;
  textoConfirmar: string;
  textoCancelar: string;
  _resolverConfirmacao?: (ok: boolean) => void;
};

@Injectable({
  providedIn: 'root'
})
export class NotificacaoService {
  private fila: NotificacaoUi[] = [];
  private atualSubject = new BehaviorSubject<NotificacaoUi | null>(null);
  notificacaoAtual$ = this.atualSubject.asObservable();

  sucesso(mensagem: string, titulo = 'Sucesso'): void {
    this.enfileirar({
      tipo: 'sucesso',
      titulo,
      mensagem,
      confirmacao: false,
      textoOk: 'OK',
      textoConfirmar: 'Confirmar',
      textoCancelar: 'Cancelar'
    });
  }

  erro(mensagem: string, titulo = 'Erro'): void {
    this.enfileirar({
      tipo: 'erro',
      titulo,
      mensagem,
      confirmacao: false,
      textoOk: 'OK',
      textoConfirmar: 'Confirmar',
      textoCancelar: 'Cancelar'
    });
  }

  aviso(mensagem: string, titulo = 'Atenção'): void {
    this.enfileirar({
      tipo: 'aviso',
      titulo,
      mensagem,
      confirmacao: false,
      textoOk: 'OK',
      textoConfirmar: 'Confirmar',
      textoCancelar: 'Cancelar'
    });
  }

  info(mensagem: string, titulo = 'Informação'): void {
    this.enfileirar({
      tipo: 'info',
      titulo,
      mensagem,
      confirmacao: false,
      textoOk: 'OK',
      textoConfirmar: 'Confirmar',
      textoCancelar: 'Cancelar'
    });
  }

  async confirmar(titulo: string, mensagem: string, textoConfirmar = 'Confirmar', textoCancelar = 'Cancelar'): Promise<boolean> {
    return await new Promise<boolean>((resolve) => {
      this.enfileirar({
        tipo: 'aviso',
        titulo,
        mensagem,
        confirmacao: true,
        textoOk: 'OK',
        textoConfirmar,
        textoCancelar,
        _resolverConfirmacao: resolve
      });
    });
  }

  responderConfirmacao(ok: boolean): void {
    const atual = this.atualSubject.value;
    if (!atual) return;
    if (atual.confirmacao && atual._resolverConfirmacao) {
      atual._resolverConfirmacao(ok);
    }
    this.fecharAtual();
  }

  fecharAtual(): void {
    this.atualSubject.next(null);
    this.exibirProximo();
  }

  private enfileirar(notificacao: NotificacaoUi): void {
    if (this.atualSubject.value == null) {
      this.atualSubject.next(notificacao);
      return;
    }
    this.fila.push(notificacao);
  }

  private exibirProximo(): void {
    if (this.atualSubject.value != null) return;
    const proximo = this.fila.shift() ?? null;
    this.atualSubject.next(proximo);
  }
}

