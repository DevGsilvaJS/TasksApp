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
      codigo: cliente.codigo
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
}
