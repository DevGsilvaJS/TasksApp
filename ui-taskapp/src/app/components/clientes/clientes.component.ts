import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ClienteService, ClienteResponseDto, CadastroClienteDto } from '../../services/cliente.service';

@Component({
  selector: 'app-clientes',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './clientes.component.html',
  styleUrl: './clientes.component.css'
})
export class ClientesComponent implements OnInit {
  clientes: ClienteResponseDto[] = [];
  clientesFiltrados: ClienteResponseDto[] = [];
  showForm = false;
  loading = false;
  error: string | null = null;
  editando = false;
  clienteEditando: ClienteResponseDto | null = null;
  termoBusca = '';

  novoCliente: CadastroClienteDto = {
    fantasia: '',
    docFederal: '',
    docEstadual: '',
    codigo: 0
  };

  constructor(private clienteService: ClienteService) { }

  ngOnInit() {
    this.carregarClientes();
  }

  carregarClientes() {
    this.loading = true;
    this.error = null;
    this.clienteService.listarTodosClientes().subscribe({
      next: (data) => {
        this.clientes = data;
        this.clientesFiltrados = data;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Erro ao carregar clientes. Verifique se a API está rodando.';
        this.loading = false;
        console.error(err);
      }
    });
  }

  filtrarClientes() {
    if (!this.termoBusca.trim()) {
      this.clientesFiltrados = this.clientes;
      return;
    }

    const termo = this.termoBusca.toLowerCase();
    this.clientesFiltrados = this.clientes.filter(c =>
      c.fantasia.toLowerCase().includes(termo) ||
      c.docFederal?.toLowerCase().includes(termo) ||
      c.codigo.toString().includes(termo)
    );
  }

  abrirFormularioNovo() {
    this.editando = false;
    this.clienteEditando = null;
    this.showForm = true;
    this.novoCliente = {
      fantasia: '',
      docFederal: '',
      docEstadual: '',
      codigo: 0
    };
    this.error = null;
  }

  abrirFormularioEdicao(cliente: ClienteResponseDto) {
    this.editando = true;
    this.clienteEditando = cliente;
    this.showForm = true;
    this.novoCliente = {
      fantasia: cliente.fantasia,
      docFederal: cliente.docFederal || '',
      docEstadual: cliente.docEstadual || '',
      codigo: cliente.codigo,
      valorContrato: cliente.valorContrato
    };
    this.error = null;
  }

  fecharFormulario() {
    this.showForm = false;
    this.editando = false;
    this.clienteEditando = null;
    this.error = null;
  }

  salvarCliente() {
    if (!this.novoCliente.fantasia || !this.novoCliente.codigo) {
      this.error = 'Preencha todos os campos obrigatórios (Fantasia e Código)';
      return;
    }

    this.loading = true;
    this.error = null;

    const operacao = this.editando && this.clienteEditando
      ? this.clienteService.atualizarCliente(this.clienteEditando.clienteId, this.novoCliente)
      : this.clienteService.cadastrarCliente(this.novoCliente);

    operacao.subscribe({
      next: () => {
        this.carregarClientes();
        this.fecharFormulario();
        this.loading = false;
      },
      error: (err) => {
        this.error = err.error?.message || 'Erro ao salvar cliente';
        this.loading = false;
      }
    });
  }

  excluirCliente(cliente: ClienteResponseDto) {
    if (!confirm(`Deseja realmente excluir o cliente ${cliente.fantasia}?`)) {
      return;
    }

    this.loading = true;
    this.error = null;

    this.clienteService.excluirCliente(cliente.clienteId).subscribe({
      next: () => {
        this.carregarClientes();
        this.loading = false;
      },
      error: (err) => {
        this.error = err.error?.message || 'Erro ao excluir cliente';
        this.loading = false;
      }
    });
  }

  formatarData(data?: string): string {
    if (!data) return '-';
    return new Date(data).toLocaleDateString('pt-BR');
  }

  formatarMoeda(valor?: number): string {
    if (valor === null || valor === undefined) return '-';
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: 'BRL'
    }).format(valor);
  }

  aplicarMascaraCNPJ(event: Event): void {
    const input = event.target as HTMLInputElement;
    let valor = input.value.replace(/\D/g, '');
    
    if (valor.length <= 11) {
      // CPF: 000.000.000-00
      valor = valor.replace(/(\d{3})(\d)/, '$1.$2');
      valor = valor.replace(/(\d{3})(\d)/, '$1.$2');
      valor = valor.replace(/(\d{3})(\d{1,2})$/, '$1-$2');
    } else {
      // CNPJ: 00.000.000/0000-00
      valor = valor.replace(/^(\d{2})(\d)/, '$1.$2');
      valor = valor.replace(/^(\d{2})\.(\d{3})(\d)/, '$1.$2.$3');
      valor = valor.replace(/\.(\d{3})(\d)/, '.$1/$2');
      valor = valor.replace(/(\d{4})(\d)/, '$1-$2');
    }
    
    input.value = valor;
    this.novoCliente.docFederal = valor;
  }

  formatarCNPJParaExibicao(cnpj?: string): string {
    if (!cnpj) return '-';
    // Se já está formatado, retorna como está
    if (cnpj.includes('.') || cnpj.includes('/') || cnpj.includes('-')) {
      return cnpj;
    }
    // Remove formatação existente e reaplica
    let valor = cnpj.replace(/\D/g, '');
    
    if (valor.length <= 11) {
      // CPF: 000.000.000-00
      valor = valor.replace(/(\d{3})(\d)/, '$1.$2');
      valor = valor.replace(/(\d{3})(\d)/, '$1.$2');
      valor = valor.replace(/(\d{3})(\d{1,2})$/, '$1-$2');
    } else {
      // CNPJ: 00.000.000/0000-00
      valor = valor.replace(/^(\d{2})(\d)/, '$1.$2');
      valor = valor.replace(/^(\d{2})\.(\d{3})(\d)/, '$1.$2.$3');
      valor = valor.replace(/\.(\d{3})(\d)/, '.$1/$2');
      valor = valor.replace(/(\d{4})(\d)/, '$1-$2');
    }
    
    return valor;
  }

  calcularTotalValorContrato(): number {
    return this.clientesFiltrados
      .filter(c => c.valorContrato !== null && c.valorContrato !== undefined)
      .reduce((total, c) => total + (c.valorContrato || 0), 0);
  }
}
