import { HttpInterceptorFn } from '@angular/common/http';

type JsonLike =
  | null
  | string
  | number
  | boolean
  | JsonLike[]
  | { [key: string]: JsonLike };

const CAMPOS_SEM_MAIUSCULO = new Set(['assunto', 'corpoHtml']);

function normalizarParaMaiusculo(valor: string): string {
  return valor.trim().toUpperCase();
}

function transformarBody(body: unknown): unknown {
  if (body == null) return body;

  if (typeof body === 'string') return normalizarParaMaiusculo(body);

  if (body instanceof FormData) {
    const novo = new FormData();
    body.forEach((v: FormDataEntryValue, k: string) => {
      if (typeof v === 'string' && CAMPOS_SEM_MAIUSCULO.has(k)) {
        novo.append(k, v);
      } else if (typeof v === 'string') {
        novo.append(k, normalizarParaMaiusculo(v));
      } else {
        novo.append(k, v);
      }
    });
    return novo;
  }

  if (Array.isArray(body)) {
    return body.map((x: unknown) => transformarBody(x));
  }

  if (typeof body === 'object') {
    const obj = body as Record<string, unknown>;
    const saida: Record<string, unknown> = {};
    for (const [k, v] of Object.entries(obj)) {
      saida[k] = transformarBody(v);
    }
    return saida as JsonLike;
  }

  return body;
}

function deveIgnorarMaiusculo(url: string, body: unknown): boolean {
  if (/email-envio/i.test(url)) {
    return true;
  }

  if (body instanceof FormData) {
    let envioEmail = false;
    body.forEach((_, chave) => {
      if (chave === 'corpoHtml' || chave === 'assunto') {
        envioEmail = true;
      }
    });
    return envioEmail;
  }

  return false;
}

export const maiusculoInterceptor: HttpInterceptorFn = (req, next) => {
  if (deveIgnorarMaiusculo(req.url, req.body)) {
    return next(req);
  }

  const bodyTransformado = transformarBody(req.body);
  if (bodyTransformado === req.body) return next(req);
  return next(req.clone({ body: bodyTransformado }));
};

