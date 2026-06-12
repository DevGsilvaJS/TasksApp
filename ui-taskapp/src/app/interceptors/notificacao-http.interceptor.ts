import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { NotificacaoService } from '../services/notificacao.service';
import { extrairMensagemErroApi } from '../utils/erro-api.util';

export const notificacaoHttpInterceptor: HttpInterceptorFn = (req, next) => {
  const notificacao = inject(NotificacaoService);

  return next(req).pipe(
    catchError((err: unknown) => {
      const mensagem = extrairMensagemErroApi(err);
      if (err instanceof HttpErrorResponse && err.status === 400) {
        notificacao.aviso(mensagem);
      } else {
        notificacao.erro(mensagem);
      }
      return throwError(() => err);
    })
  );
};
