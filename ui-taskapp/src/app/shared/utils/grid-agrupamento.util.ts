export interface OpcaoAgrupamentoGrid {
  value: string;
  label: string;
}

export const OPCAO_AGRUPAMENTO_NENHUM: OpcaoAgrupamentoGrid = { value: '', label: 'Nenhum' };

export function criarOpcoesAgrupamento(opcoes: OpcaoAgrupamentoGrid[]): OpcaoAgrupamentoGrid[] {
  return [OPCAO_AGRUPAMENTO_NENHUM, ...opcoes];
}

export function ordenarItensParaAgrupamento<T>(itens: T[], campo: string): T[] {
  if (!campo) {
    return itens;
  }

  return [...itens].sort((a, b) => {
    const va = (a as Record<string, unknown>)[campo] ?? '';
    const vb = (b as Record<string, unknown>)[campo] ?? '';
    return String(va).localeCompare(String(vb), 'pt-BR', { numeric: true });
  });
}

export function obterRotuloAgrupamento(opcoes: OpcaoAgrupamentoGrid[], campo: string): string {
  const opcao = opcoes.find(o => o.value === campo);
  return opcao?.label ?? (campo || 'Nenhum');
}

export function obterValorCabecalhoGrupo(
  item: Record<string, unknown>,
  campo: string,
  formatar?: (item: Record<string, unknown>, campo: string) => string
): string {
  if (!campo) {
    return '';
  }

  if (formatar) {
    return formatar(item, campo);
  }

  const valor = item[campo];
  if (valor == null || valor === '') {
    return '—';
  }

  return String(valor);
}

export function deveExibirCabecalhoGrupo<T>(
  itens: T[],
  index: number,
  campo: string,
  obterValor: (item: T) => string
): boolean {
  if (!campo) {
    return false;
  }

  if (index === 0) {
    return true;
  }

  return obterValor(itens[index]) !== obterValor(itens[index - 1]);
}

export function carregarPreferenciaAgruparPor(
  storageKey: string,
  opcoes: OpcaoAgrupamentoGrid[],
  valorPadrao = ''
): string {
  try {
    const stored = sessionStorage.getItem(storageKey);
    if (stored !== null && opcoes.some(o => o.value === stored)) {
      return stored;
    }
  } catch {
    // sessionStorage indisponível
  }
  return valorPadrao;
}

export function salvarPreferenciaAgruparPor(storageKey: string, valor: string): void {
  try {
    sessionStorage.setItem(storageKey, valor);
  } catch {
    // sessionStorage indisponível
  }
}
