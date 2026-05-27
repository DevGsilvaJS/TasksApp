import { HttpInterceptorFn } from '@angular/common/http';

type JsonLike =
  | null
  | string
  | number
  | boolean
  | JsonLike[]
  | { [key: string]: JsonLike };

function normalizarParaMaiusculo(valor: string): string {
  return valor.trim().toUpperCase();
}

function transformarBody(body: unknown): unknown {
  if (body == null) return body;

  if (typeof body === 'string') return normalizarParaMaiusculo(body);

  if (body instanceof FormData) {
    const novo = new FormData();
    body.forEach((v: FormDataEntryValue, k: string) => {
      if (typeof v === 'string') {
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

export const maiusculoInterceptor: HttpInterceptorFn = (req, next) => {
  const bodyTransformado = transformarBody(req.body);
  if (bodyTransformado === req.body) return next(req);
  return next(req.clone({ body: bodyTransformado }));
};

