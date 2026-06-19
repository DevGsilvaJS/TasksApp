export function isParcelaPendente(status: string | null | undefined): boolean {
  return status?.toLowerCase() === 'pendente';
}

export function isParcelaPaga(status: string | null | undefined): boolean {
  return status?.toLowerCase() === 'paga';
}

export function isParcelaCancelada(status: string | null | undefined): boolean {
  return status?.toLowerCase() === 'cancelada';
}

export function labelStatusParcela(status: string | null | undefined): string {
  if (isParcelaPaga(status)) return 'Recebida';
  if (isParcelaPendente(status)) return 'Pendente';
  if (isParcelaCancelada(status)) return 'Inativa';
  return status ?? '—';
}
