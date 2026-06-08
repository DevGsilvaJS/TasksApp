export function isParcelaPendente(status: string | null | undefined): boolean {
  return status?.toLowerCase() === 'pendente';
}

export function isParcelaPaga(status: string | null | undefined): boolean {
  return status?.toLowerCase() === 'paga';
}

export function labelStatusParcela(status: string | null | undefined): string {
  if (isParcelaPaga(status)) return 'Paga';
  if (isParcelaPendente(status)) return 'Pendente';
  if (status?.toLowerCase() === 'cancelada') return 'Cancelada';
  return status ?? '—';
}
