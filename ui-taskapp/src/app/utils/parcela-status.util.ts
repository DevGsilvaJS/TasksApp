export function isParcelaPendente(status: string | null | undefined): boolean {
  return status?.toLowerCase() === 'pendente';
}

export function isParcelaPaga(status: string | null | undefined): boolean {
  return status?.toLowerCase() === 'paga';
}

export function isParcelaCancelada(status: string | null | undefined): boolean {
  return status?.toLowerCase() === 'cancelada';
}

export function isParcelaCongeladaPorCliente(
  status: string | null | undefined,
  congeladaPorCliente?: boolean
): boolean {
  return isParcelaCancelada(status) && congeladaPorCliente === true;
}

export function labelStatusParcela(
  status: string | null | undefined,
  congeladaPorCliente?: boolean
): string {
  if (isParcelaPaga(status)) return 'Recebida';
  if (isParcelaPendente(status)) return 'Pendente';
  if (isParcelaCongeladaPorCliente(status, congeladaPorCliente)) return 'Congelada';
  if (isParcelaCancelada(status)) return 'Inativa';
  return status ?? '—';
}
