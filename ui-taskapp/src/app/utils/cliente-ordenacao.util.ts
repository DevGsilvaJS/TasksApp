import { ClienteResponseDto } from '../services/cliente.service';

export function ordenarClientesPorCodigo(clientes: ClienteResponseDto[]): ClienteResponseDto[] {
  return [...clientes].sort((a, b) =>
    a.codigo.localeCompare(b.codigo, 'pt-BR', { numeric: true })
  );
}
