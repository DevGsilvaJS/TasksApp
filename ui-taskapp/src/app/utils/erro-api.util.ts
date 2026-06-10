import { HttpErrorResponse } from '@angular/common/http';

type ErroApi = {
  message?: string;
  mensagem?: string;
  errors?: Record<string, string[]>;
};

function normalizarMensagemValidacao(campo: string, mensagem: string): string {
  if (!mensagem) return mensagem;
  if (
    mensagem.includes('non-empty request body') ||
    (campo === 'dto' && mensagem.includes('required'))
  ) {
    return 'Não foram enviados dados na requisição.';
  }
  return mensagem;
}

export function extrairMensagemErroApi(error: unknown, fallback = 'Ocorreu um erro inesperado.'): string {
  if (error instanceof HttpErrorResponse) {
    const corpo = error.error as ErroApi | string | null;
    if (typeof corpo === 'string' && corpo.trim()) return corpo;
    if (corpo && typeof corpo === 'object') {
      if (corpo.message) return corpo.message;
      if (corpo.mensagem) return corpo.mensagem;
      if (corpo.errors) {
        const mensagens = Object.entries(corpo.errors)
          .flatMap(([campo, msgs]) => (msgs ?? []).map(msg => normalizarMensagemValidacao(campo, msg)))
          .filter(Boolean);
        if (mensagens.length > 0) return mensagens[0];
      }
    }
    if (error.status === 0) return 'Não foi possível conectar na API.';
    return error.message || fallback;
  }
  return fallback;
}
