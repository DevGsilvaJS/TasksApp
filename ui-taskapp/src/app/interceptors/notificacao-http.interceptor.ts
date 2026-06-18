import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { NotificacaoService } from '../services/notificacao.service';
import { extrairMensagemErroApi } from '../utils/erro-api.util';

export const notificacaoHttpInterceptor: HttpInterceptorFn = (req, next) => {
  const notificacao = inject(NotificacaoService);

  return next(req).pipe(
    catchError((err: unknown) => {
      if (err instanceof HttpErrorResponse) {
        const consultaCampanhaAtiva =
          err.status === 404 && req.url.includes('email-envio/campanhas/ativa');
        if (consultaCampanhaAtiva) {
          return throwError(() => err);
        }

        const mensagem = extrairMensagemErroApi(err);
        if (err.status === 400) {
          notificacao.aviso(mensagem);
        } else {
          notificacao.erro(mensagem);
        }
      }
      return throwError(() => err);
    })
  );
};
