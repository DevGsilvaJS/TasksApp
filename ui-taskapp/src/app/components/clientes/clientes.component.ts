import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ClienteService, ClienteResponseDto, CadastroClienteDto, StatusCliente, ClienteContratoValorDto } from '../../services/cliente.service';
import { UsuarioService, UsuarioResponseDto } from '../../services/usuario.service';
import { MascaraMoedaBrDirective } from '../../directives/mascara-moeda-br.directive';
import { NotificacaoService } from '../../services/notificacao.service';
import { extrairMensagemErroApi } from '../../utils/erro-api.util';
import {
  criarOpcoesAgrupamento,
  deveExibirCabecalhoGrupo,
  obterRotuloAgrupamento,
  obterValorCabecalhoGrupo,
  ordenarItensParaAgrupamento
} from '../../shared/utils/grid-agrupamento.util';
import { SeletorAgrupamentoGridComponent } from '../../shared/components/seletor-agrupamento-grid/seletor-agrupamento-grid.component';

@Component({
  selector: 'app-clientes',
  standalone: true,
  imports: [CommonModule, FormsModule, MascaraMoedaBrDirective, SeletorAgrupamentoGridComponent],
  templateUrl: './clientes.component.html',
  styleUrl: './clientes.component.css'
})
export class ClientesComponent implements OnInit {
  clientes: ClienteResponseDto[] = [];
  clientesFiltrados: ClienteResponseDto[] = [];
  usuarios: UsuarioResponseDto[] = [];
  showForm = false;
  loading = false;
  error: string | null = null;
  editando = false;
  clienteEditando: ClienteResponseDto | null = null;
  termoBusca = '';
  emailCliente = '';
  exibirInativos = false;
  contratosAlterados = false;

  StatusCliente = StatusCliente;

  novoCliente: CadastroClienteDto = {
    fantasia: '',
    docFederal: '',
    docEstadual: '',
    codigo: '',
    usuarioId: 0,
    status: StatusCliente.Ativo,
    contratos: []
  };

  statusOptions = [
    { value: StatusCliente.Ativo, label: 'Ativo', icon: '✓' },
    { value: StatusCliente.Inativo, label: 'Inativo', icon: '✗' },
    { value: StatusCliente.Suspenso, label: 'Suspenso', icon: '⚠' }
  ];

  agruparPor = '';
  agruparPorOpcoes = criarOpcoesAgrupamento([
    { value: 'codigo', label: 'Código' },
    { value: 'fantasia', label: 'Fantasia' },
    { value: 'usuarioNome', label: 'Usuário' },
    { value: 'status', label: 'Status' }
  ]);

  constructor(
    private clienteService: ClienteService,
    private usuarioService: UsuarioService,
    private notificacao: NotificacaoService
  ) { }

  ngOnInit() {
    this.carregarClientes();
    this.carregarUsuarios();
  }

  carregarUsuarios() {
    this.usuarioService.listarTodosUsuarios().subscribe({
      next: (data) => {
        this.usuarios = data;
      },
      error: (err) => {
        console.error('Erro ao carregar usuários:', err);
      }
    });
  }

  carregarClientes() {
    this.loading = true;
    this.error = null;
    this.clienteService.listarTodosClientes().subscribe({
      next: (data) => {
        this.clientes = data;
        this.aplicarFiltros();
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
    this.aplicarFiltros();
  }

  onToggleExibirInativos() {
    this.aplicarFiltros();
  }

  private aplicarFiltros() {
    const termo = this.termoBusca.trim().toLowerCase();
    let lista = [...this.clientes];

    if (!this.exibirInativos) {
      lista = lista.filter(c => c.status !== StatusCliente.Inativo);
    }

    if (termo) {
      lista = lista.filter(c =>
        c.fantasia.toLowerCase().includes(termo) ||
        c.docFederal?.toLowerCase().includes(termo) ||
        c.codigo.toString().includes(termo) ||
        c.emails?.some(e => e.toLowerCase().includes(termo))
      );
    }

    this.clientesFiltrados = lista;
  }

  abrirFormularioNovo() {
    this.editando = false;
    this.clienteEditando = null;
    this.showForm = true;
    this.emailCliente = '';
    this.novoCliente = {
      fantasia: '',
      docFederal: '',
      docEstadual: '',
      codigo: '',
      usuarioId: 0,
      status: StatusCliente.Ativo,
      contratos: []
    };
    this.contratosAlterados = true;
    this.error = null;
  }

  abrirFormularioEdicao(cliente: ClienteResponseDto) {
    this.editando = true;
    this.clienteEditando = cliente;
    this.showForm = true;
    
    // Formatar datas para o input date (YYYY-MM-DD)
    let dataFinalContratoFormatada: string | undefined = undefined;
    if (cliente.vigenciaFim) {
      const data = new Date(cliente.vigenciaFim);
      if (!isNaN(data.getTime())) {
        dataFinalContratoFormatada = data.toISOString().split('T')[0];
      }
    }
    this.emailCliente = cliente.emails?.[0] ?? '';
    this.novoCliente = {
      fantasia: cliente.fantasia,
      docFederal: cliente.docFederal || '',
      docEstadual: cliente.docEstadual || '',
      codigo: cliente.codigo,
      usuarioId: cliente.usuarioId || 0,
      valorContrato: cliente.valorContratoVigente ?? undefined,
      dataFinalContrato: dataFinalContratoFormatada,
      diaPagamento: cliente.diaPagamento,
      diaNfServico: cliente.diaNfServico ?? undefined,
      emails: cliente.emails?.length ? [...cliente.emails] : undefined,
      status: Number(cliente.status ?? StatusCliente.Ativo) as StatusCliente,
      contratos: cliente.contratos?.length
        ? cliente.contratos.map(c => ({
            valorMensal: c.valorMensal,
            dataInicio: (c.dataInicio || '').split('T')[0],
            dataFim: c.dataFim ? c.dataFim.split('T')[0] : undefined
          }))
        : []
    };
    this.contratosAlterados = false;
    this.error = null;
  }

  adicionarContrato() {
    if (!this.novoCliente.contratos) this.novoCliente.contratos = [];
    const hoje = new Date().toISOString().split('T')[0];
    const contrato: ClienteContratoValorDto = {
      valorMensal: this.novoCliente.valorContrato || 0,
      dataInicio: hoje,
      dataFim: this.novoCliente.dataFinalContrato || undefined
    };
    this.novoCliente.contratos = [...this.novoCliente.contratos, contrato];
    this.contratosAlterados = true;
  }

  removerContrato(index: number) {
    const lista = this.novoCliente.contratos ?? [];
    this.novoCliente.contratos = lista.filter((_, i) => i !== index);
    this.contratosAlterados = true;
  }

  onContratoAlterado(): void {
    this.contratosAlterados = true;
  }

  fecharFormulario() {
    this.showForm = false;
    this.editando = false;
    this.clienteEditando = null;
    this.error = null;
  }

  salvarCliente() {
    const codigoLimpo = (this.novoCliente.codigo || '').toString().trim();
    if (!this.novoCliente.fantasia || !codigoLimpo || !this.novoCliente.usuarioId || this.novoCliente.usuarioId === 0) {
      this.error = 'Preencha todos os campos obrigatórios (Fantasia, Código e Usuário)';
      this.notificacao.aviso(this.error);
      return;
    }
    if (!/^[0-9]+$/.test(codigoLimpo)) {
      this.error = 'O código do cliente deve conter apenas números (ex.: 04146).';
      this.notificacao.aviso(this.error);
      return;
    }
    this.novoCliente.codigo = codigoLimpo;

    if (this.contratosAlterados) {
      const erroVigencia = this.validarVigenciasContratos(this.novoCliente.contratos ?? []);
      if (erroVigencia) {
        this.error = erroVigencia;
        this.notificacao.aviso(this.error);
        return;
      }
    } else {
      // Não mexeu em contratos: não envia nem valida vigências.
      this.novoCliente.contratos = undefined;
    }

    this.novoCliente.emails = this.emailCliente?.trim() ? [this.emailCliente.trim()] : [];
    if (this.novoCliente.diaNfServico != null && (this.novoCliente.diaNfServico < 1 || this.novoCliente.diaNfServico > 31)) {
      this.novoCliente.diaNfServico = undefined;
    }

    this.novoCliente.usuarioId = Number(this.novoCliente.usuarioId);
    this.novoCliente.status = Number(this.novoCliente.status) as StatusCliente;

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
        this.notificacao.sucesso(this.editando ? 'Cliente atualizado com sucesso.' : 'Cliente cadastrado com sucesso.');
      },
      error: (err) => {
        this.error = extrairMensagemErroApi(err, 'Erro ao salvar cliente');
        this.loading = false;
      }
    });
  }

  excluirCliente(cliente: ClienteResponseDto) {
    this.confirmarExclusaoCliente(cliente);
  }

  private async confirmarExclusaoCliente(cliente: ClienteResponseDto): Promise<void> {
    const ok = await this.notificacao.confirmar(
      'Confirmar exclusão',
      `Deseja realmente excluir o cliente ${cliente.fantasia}?`,
      'Excluir',
      'Cancelar'
    );
    if (!ok) return;

    this.loading = true;
    this.error = null;

    this.clienteService.excluirCliente(cliente.clienteId).subscribe({
      next: () => {
        this.carregarClientes();
        this.loading = false;
        this.notificacao.sucesso('Cliente excluído com sucesso.');
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
      .filter(c => c.valorContratoVigente !== null && c.valorContratoVigente !== undefined)
      .reduce((total, c) => total + (c.valorContratoVigente || 0), 0);
  }

  obterPrimeiroEmail(cliente: ClienteResponseDto): string {
    if (cliente.emails?.length) {
      return cliente.emails[0];
    }
    return '-';
  }

  private validarVigenciasContratos(contratos: ClienteContratoValorDto[]): string | null {
    const lista = (contratos || [])
      .filter(c => (c.valorMensal ?? 0) > 0)
      .map((c, idx) => ({
        idx: idx + 1,
        inicio: new Date(c.dataInicio),
        fim: c.dataFim ? new Date(c.dataFim) : null
      }))
      .sort((a, b) => a.inicio.getTime() - b.inicio.getTime());

    for (const c of lista) {
      if (isNaN(c.inicio.getTime())) return `Contrato inválido: data início inválida (linha ${c.idx}).`;
      if (c.fim && isNaN(c.fim.getTime())) return `Contrato inválido: data fim inválida (linha ${c.idx}).`;
      if (c.fim && c.fim.getTime() < c.inicio.getTime()) return `Contrato inválido: data fim menor que data início (linha ${c.idx}).`;
    }

    for (let i = 0; i < lista.length - 1; i++) {
      const a = lista[i];
      const b = lista[i + 1];

      // comparação inclusiva por dia (00:00)
      const fimA = a.fim ? new Date(a.fim.toISOString().split('T')[0]) : null;
      const inicioB = new Date(b.inicio.toISOString().split('T')[0]);

      if (!fimA || inicioB.getTime() <= fimA.getTime()) {
        return `Não é permitido ter 2 contratos com vigência no mesmo período. Conflito entre as linhas ${a.idx} e ${b.idx}.`;
      }
    }

    return null;
  }

  get clientesParaTabela(): ClienteResponseDto[] {
    return ordenarItensParaAgrupamento(this.clientesFiltrados, this.agruparPor);
  }

  getAgruparPorLabel(): string {
    return obterRotuloAgrupamento(this.agruparPorOpcoes, this.agruparPor);
  }

  getValorGrupoCliente(cliente: ClienteResponseDto): string {
    if (this.agruparPor === 'status') {
      const status = this.statusOptions.find(s => s.value === cliente.status);
      return status?.label ?? '—';
    }

    return obterValorCabecalhoGrupo(cliente as unknown as Record<string, unknown>, this.agruparPor);
  }

  exibirCabecalhoGrupoCliente(index: number): boolean {
    return deveExibirCabecalhoGrupo(
      this.clientesParaTabela,
      index,
      this.agruparPor,
      (cliente) => this.getValorGrupoCliente(cliente)
    );
  }
}
