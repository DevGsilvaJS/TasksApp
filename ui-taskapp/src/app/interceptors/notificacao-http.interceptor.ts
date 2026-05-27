import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { NotificacaoService } from '../services/notificacao.service';

type ErroApi = {
  message?: string;
  mensagem?: string;
  errors?: Record<string, string[]>;
};

function extrairMensagemErro(error: unknown): string {
  if (error instanceof HttpErrorResponse) {
    const corpo = error.error as ErroApi | string | null;
    if (typeof corpo === 'string' && corpo.trim()) return corpo;
    if (corpo && typeof corpo === 'object') {
      if (corpo.message) return corpo.message;
      if (corpo.mensagem) return corpo.mensagem;
      if (corpo.errors) {
        const primeira = Object.values(corpo.errors).flat().filter(Boolean)[0];
        if (primeira) return primeira;
      }
    }
    if (error.status === 0) return 'Não foi possível conectar na API.';
    return error.message || 'Ocorreu um erro inesperado.';
  }
  return 'Ocorreu um erro inesperado.';
}

export const notificacaoHttpInterceptor: HttpInterceptorFn = (req, next) => {
  const notificacao = inject(NotificacaoService);

  return next(req).pipe(
    catchError((err: unknown) => {
      // Evita alertar ruído do login em rotas que já tratam no componente, mantendo simples:
      const mensagem = extrairMensagemErro(err);
      if (err instanceof HttpErrorResponse && err.status === 400) {
        // Erro de validação/regra de negócio: tratar como aviso (não como erro crítico).
        notificacao.aviso(mensagem);
      } else {
        notificacao.erro(mensagem);
      }
      return throwError(() => err);
    })
  );
};

