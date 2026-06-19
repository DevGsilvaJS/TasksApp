import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { OverlayPanel, OverlayPanelModule } from 'primeng/overlaypanel';
import { TarefaService, TarefaResponseDto, CadastroTarefaDto, StatusTarefa, TipoAtendimento, PrioridadeTarefa, TipoContato } from '../../services/tarefa.service';
import { ClienteService, ClienteResponseDto } from '../../services/cliente.service';
import { UsuarioService, UsuarioResponseDto } from '../../services/usuario.service';
import { AnotacaoService, CadastroAnotacaoDto } from '../../services/anotacao.service';
import { AuthService } from '../../services/auth.service';
import { CadastroAtendimentoService } from '../../services/cadastro-atendimento.service';
import { forkJoin } from 'rxjs';
import { NotificacaoService } from '../../services/notificacao.service';
import { SeletorAgrupamentoGridComponent } from '../../shared/components/seletor-agrupamento-grid/seletor-agrupamento-grid.component';
import {
  carregarPreferenciaAgruparPor,
  salvarPreferenciaAgruparPor,
} from '../../shared/utils/grid-agrupamento.util';

@Component({
  selector: 'app-atendimentos',
  standalone: true,
  imports: [CommonModule, FormsModule, TableModule, OverlayPanelModule, SeletorAgrupamentoGridComponent],
  templateUrl: './atendimentos.component.html',
  styleUrl: './atendimentos.component.css'
})
export class AtendimentosComponent implements OnInit {
  // Expor os enums para uso no template
  StatusTarefa = StatusTarefa;
  TipoAtendimento = TipoAtendimento;
  PrioridadeTarefa = PrioridadeTarefa;
  TipoContato = TipoContato;

  tarefas: TarefaResponseDto[] = [];
  tarefasFiltradas: TarefaResponseDto[] = [];

  /** Dados para a tabela PrimeNG: ordenados pelo campo de agrupamento quando ativo. */
  get tarefasParaTabela(): TarefaResponseDto[] {
    if (!this.tarefasFiltradas.length) return [];
    if (!this.agruparPor) return this.tarefasFiltradas;
    const field = this.agruparPor;
    return [...this.tarefasFiltradas].sort((a, b) => {
      const va = (a as any)[field] ?? '';
      const vb = (b as any)[field] ?? '';
      return String(va).localeCompare(String(vb));
    });
  }

  /** Agrupamento por cliente para exibir quebra no grid */
  get tarefasAgrupadasPorCliente(): { clienteId: number; clienteNome: string; tarefas: TarefaResponseDto[] }[] {
    const map = new Map<number, { clienteId: number; clienteNome: string; tarefas: TarefaResponseDto[] }>();
    for (const t of this.tarefasFiltradas) {
      const key = t.clienteId;
      if (!map.has(key)) {
        map.set(key, { clienteId: t.clienteId, clienteNome: t.clienteNome, tarefas: [] });
      }
      map.get(key)!.tarefas.push(t);
    }
    return Array.from(map.values());
  }
  clientes: ClienteResponseDto[] = [];
  usuarios: UsuarioResponseDto[] = [];
  showForm = false;
  loading = false;
  error: string | null = null;
  editando = false;
  tarefaEditando: TarefaResponseDto | null = null;
  termoBusca = '';
  mostrarConcluidas = false;
  mostrarTodosUsuarios = false;
  private readonly STORAGE_KEY_MOSTRAR_CONCLUIDAS = 'atendimentos_mostrar_concluidas';
  private readonly STORAGE_KEY_MOSTRAR_TODOS_USUARIOS = 'atendimentos_mostrar_todos_usuarios';
  private readonly STORAGE_KEY_AGRUPAR_POR = 'atendimentos_agrupar_por';

  novoTarefa: CadastroTarefaDto = {
    clienteId: 0,
    usuarioId: 0,
    status: StatusTarefa.EmAberto,
    dataConclusao: undefined,
    descricao: undefined,
    titulo: undefined,
    protocolo: undefined,
    solicitante: undefined,
    celularSolicitante: undefined,
    tipoAtendimento: TipoAtendimento.Suporte,
    prioridade: PrioridadeTarefa.Baixa,
    tipoContato: TipoContato.WhatsApp,
    imagens: undefined
  };

  imagensSelecionadas: File[] = [];
  previewImagens: string[] = [];

  novaAnotacao: string = '';
  tarefaSelecionada: TarefaResponseDto | null = null;
  showAnotacoes = false;
  showImagens = false;
  tarefaImagens: TarefaResponseDto | null = null;
  imagemAtualIndex = 0;

  /** Opções do "Agrupar por" (substitui o "Drag here to set row groups" do Enterprise) */
  agruparPorOpcoes: { value: string; label: string }[] = [
    { value: '', label: 'Nenhum' },
    { value: 'clienteNome', label: 'Cliente' },
    { value: 'usuarioNome', label: 'Usuário' },
    { value: 'tipoAtendimentoDescricao', label: 'Tipo' }
  ];
  agruparPor = '';

  /** Colunas que têm filtro multi-select (ícone funil). */
  readonly colunasFiltravelis: { campo: string; label: string }[] = [
    { campo: 'numero', label: 'Nº' },
    { campo: 'protocolo', label: 'Protocolo' },
    { campo: 'titulo', label: 'Título' },
    { campo: 'clienteNome', label: 'Cliente' },
    { campo: 'solicitante', label: 'Solicitante' },
    { campo: 'tipoAtendimentoDescricao', label: 'Tipo' },
    { campo: 'prioridadeDescricao', label: 'Prioridade' },
    { campo: 'usuarioNome', label: 'Usuário' }
  ];

  /** Filtros por coluna: valores selecionados (vazio = sem filtro). */
  filtrosColunasSelecao: Record<string, string[]> = {};
  /** Coluna cujo painel de filtro está aberto. */
  filtroColunaAtivo: string | null = null;
  /** Seleção temporária no painel (antes de OK). */
  selecaoTemp: Record<string, string[]> = {};

  @ViewChild('opFiltroColuna') opFiltroColuna!: OverlayPanel;

  /** Dados já filtrados pela busca global e por outras colunas, sem o filtro da coluna dada. */
  getDadosParaFiltroColuna(campo: string): TarefaResponseDto[] {
    let lista = [...this.tarefas];
    if (this.termoBusca.trim()) {
      const termo = this.termoBusca.toLowerCase();
      lista = lista.filter(t =>
        t.clienteNome.toLowerCase().includes(termo) ||
        t.usuarioNome.toLowerCase().includes(termo) ||
        (t.statusDescricao && t.statusDescricao.toLowerCase().includes(termo)) ||
        t.tarefaId.toString().includes(termo) ||
        (t.titulo && t.titulo.toLowerCase().includes(termo)) ||
        (t.protocolo && t.protocolo.toLowerCase().includes(termo)) ||
        (t.solicitante && t.solicitante.toLowerCase().includes(termo))
      );
    }
    for (const [col, valores] of Object.entries(this.filtrosColunasSelecao)) {
      if (col === campo || !valores?.length) continue;
      const set = new Set(valores);
      lista = lista.filter(t => set.has(String((t as any)[col] ?? '')));
    }
    return lista;
  }

  getValoresDistintosColuna(campo: string): string[] {
    const dados = this.getDadosParaFiltroColuna(campo);
    const set = new Set<string>();
    for (const t of dados) {
      const v = (t as any)[campo];
      set.add(v == null ? '' : String(v));
    }
    return Array.from(set).sort((a, b) => a.localeCompare(b));
  }

  abrirFiltroColuna(campo: string, event: Event): void {
    event.stopPropagation();
    this.filtroColunaAtivo = campo;
    const all = this.getValoresDistintosColuna(campo);
    this.selecaoTemp[campo] = (this.filtrosColunasSelecao[campo]?.length ? [...this.filtrosColunasSelecao[campo]] : [...all]) as string[];
    this.opFiltroColuna.toggle(event);
  }

  isValorSelecionadoFiltroColuna(campo: string, valor: string): boolean {
    const sel = this.selecaoTemp[campo];
    if (!sel) return true;
    return sel.includes(valor);
  }

  isSelecionarTodosFiltroColuna(campo: string): boolean {
    const all = this.getValoresDistintosColuna(campo);
    const sel = this.selecaoTemp[campo] ?? [];
    return all.length > 0 && sel.length === all.length;
  }

  toggleSelecionarTodosFiltroColuna(campo: string): void {
    const all = this.getValoresDistintosColuna(campo);
    if (this.isSelecionarTodosFiltroColuna(campo)) {
      this.selecaoTemp[campo] = [];
    } else {
      this.selecaoTemp[campo] = [...all];
    }
  }

  toggleValorFiltroColuna(campo: string, valor: string): void {
    let sel = this.selecaoTemp[campo] ?? [];
    if (sel.includes(valor)) {
      sel = sel.filter(v => v !== valor);
    } else {
      sel = [...sel, valor];
    }
    this.selecaoTemp[campo] = sel;
  }

  aplicarFiltroColuna(): void {
    if (!this.filtroColunaAtivo) return;
    const campo = this.filtroColunaAtivo;
    const all = this.getValoresDistintosColuna(campo);
    const sel = this.selecaoTemp[campo] ?? [];
    this.filtrosColunasSelecao[campo] = sel.length === all.length ? [] : [...sel];
    this.opFiltroColuna.hide();
    this.filtroColunaAtivo = null;
    this.aplicarFiltros();
  }

  cancelarFiltroColuna(): void {
    this.opFiltroColuna.hide();
    this.filtroColunaAtivo = null;
  }

  get hasFiltrosColuna(): boolean {
    return Object.values(this.filtrosColunasSelecao).some(arr => arr?.length > 0);
  }

  limparFiltrosColuna(): void {
    this.filtrosColunasSelecao = {};
    this.aplicarFiltros();
  }

  getAgruparPorLabel(): string {
    const opt = this.agruparPorOpcoes.find(o => o.value === this.agruparPor);
    return opt ? opt.label : this.agruparPor || 'Nenhum';
  }

  /** Valor exibido no cabeçalho do grupo (PrimeNG pode passar objeto ou string). */
  getGroupHeaderValue(rowData: any): string {
    if (rowData == null) return '';
    if (typeof rowData === 'string') return rowData;
    if (typeof rowData === 'object' && this.agruparPor && rowData[this.agruparPor] != null) {
      return String(rowData[this.agruparPor]);
    }
    return String(rowData);
  }

  /** Classes da linha por status e prioridade (PrimeNG Table). */
  getRowClass(row: TarefaResponseDto): string {
    if (!row) return '';
    const statusClass = this.obterClasseLinhaStatus(row) || '';
    const prioridadeClass = this.obterClasseLinhaPrioridade(row);
    return [statusClass, prioridadeClass].filter(Boolean).join(' ');
  }

  /** Classe da linha por prioridade (Alta, Média, Baixa) para cor de linha. */
  obterClasseLinhaPrioridade(tarefa: TarefaResponseDto | null | undefined): string {
    if (!tarefa?.prioridadeDescricao) return '';
    const d = (tarefa.prioridadeDescricao || '').toLowerCase().normalize('NFD').replace(/[\u0300-\u036f]/g, '');
    if (d === 'alta') return 'row-prioridade-alta';
    if (d === 'media') return 'row-prioridade-media';
    if (d === 'baixa') return 'row-prioridade-baixa';
    return '';
  }

  /** Listas vindas da API (cadastro-atendimento); apenas ativos. */
  statusOptions: { value: number; label: string }[] = [];
  tipoAtendimentoOptions: { value: number; label: string }[] = [];
  tipoContatoOptions: { value: number; label: string }[] = [];

  prioridadeOptions = [
    { value: PrioridadeTarefa.Baixa, label: 'Baixa' },
    { value: PrioridadeTarefa.Media, label: 'Média' },
    { value: PrioridadeTarefa.Alta, label: 'Alta' }
  ];

  constructor(
    private tarefaService: TarefaService,
    private clienteService: ClienteService,
    private usuarioService: UsuarioService,
    private anotacaoService: AnotacaoService,
    private authService: AuthService,
    private cadastroAtendimentoService: CadastroAtendimentoService,
    private notificacao: NotificacaoService
  ) { }

  ngOnInit() {
    this.carregarPreferenciaMostrarConcluidas();
    this.carregarPreferenciaMostrarTodosUsuarios();
    this.carregarPreferenciaAgruparPor();
    this.carregarCadastrosAtendimento();
    this.carregarTarefas();
    this.carregarClientes();
    this.carregarUsuarios();
  }

  private carregarCadastrosAtendimento() {
    forkJoin({
      status: this.cadastroAtendimentoService.listarStatus(true),
      tipoAtendimento: this.cadastroAtendimentoService.listarTipoAtendimento(true),
      tipoContato: this.cadastroAtendimentoService.listarTipoContato(true)
    }).subscribe({
      next: (res) => {
        this.statusOptions = res.status.map(x => ({ value: x.id, label: x.descricao }));
        this.tipoAtendimentoOptions = res.tipoAtendimento.map(x => ({ value: x.id, label: x.descricao }));
        this.tipoContatoOptions = res.tipoContato.map(x => ({ value: x.id, label: x.descricao }));
        this.sincronizarValoresPadraoFormulario();
      },
      error: (err) => {
        console.error('Erro ao carregar cadastros de atendimento (status/tipo/contato):', err);
      }
    });
  }

  private carregarPreferenciaMostrarConcluidas() {
    try {
      const stored = sessionStorage.getItem(this.STORAGE_KEY_MOSTRAR_CONCLUIDAS);
      if (stored !== null) {
        this.mostrarConcluidas = JSON.parse(stored) === true;
      }
    } catch (error) {
      console.error('Erro ao carregar preferência de mostrar concluídas:', error);
      this.mostrarConcluidas = false;
    }
  }

  private salvarPreferenciaMostrarConcluidas() {
    try {
      sessionStorage.setItem(this.STORAGE_KEY_MOSTRAR_CONCLUIDAS, JSON.stringify(this.mostrarConcluidas));
    } catch (error) {
      console.error('Erro ao salvar preferência de mostrar concluídas:', error);
    }
  }

  private carregarPreferenciaMostrarTodosUsuarios() {
    try {
      const stored = sessionStorage.getItem(this.STORAGE_KEY_MOSTRAR_TODOS_USUARIOS);
      if (stored !== null) {
        this.mostrarTodosUsuarios = JSON.parse(stored) === true;
      }
    } catch (error) {
      console.error('Erro ao carregar preferência de todos usuários:', error);
      this.mostrarTodosUsuarios = false;
    }
  }

  private salvarPreferenciaMostrarTodosUsuarios() {
    try {
      sessionStorage.setItem(this.STORAGE_KEY_MOSTRAR_TODOS_USUARIOS, JSON.stringify(this.mostrarTodosUsuarios));
    } catch (error) {
      console.error('Erro ao salvar preferência de todos usuários:', error);
    }
  }

  private carregarPreferenciaAgruparPor() {
    this.agruparPor = carregarPreferenciaAgruparPor(this.STORAGE_KEY_AGRUPAR_POR, this.agruparPorOpcoes);
  }

  onAgruparPorChange(valor: string) {
    this.agruparPor = valor;
    salvarPreferenciaAgruparPor(this.STORAGE_KEY_AGRUPAR_POR, valor);
  }

  onMostrarConcluidasChange() {
    this.salvarPreferenciaMostrarConcluidas();
    this.carregarTarefas();
  }

  onMostrarTodosUsuariosChange() {
    this.salvarPreferenciaMostrarTodosUsuarios();
    this.carregarTarefas();
  }

  carregarTarefas() {
    this.loading = true;
    this.error = null;
    // 1) Padrão: apenas atendimentos do usuário logado (meu usuid)
    // 2) Se "Todos usuários" marcado: não envia usuarioId → API retorna de todos
    // 3) Se "Concluídas" marcado: incluirConcluidas true → API inclui concluídas
    const usuarioId = this.mostrarTodosUsuarios ? undefined : (this.authService.getUsuarioId() ?? undefined);
    this.tarefaService.listarTarefas({
      usuarioId,
      incluirConcluidas: this.mostrarConcluidas
    }).subscribe({
      next: (data) => {
        this.tarefas = data;
        this.aplicarFiltros();
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Erro ao carregar tarefas. Verifique se a API está rodando.';
        this.loading = false;
        console.error(err);
      }
    });
  }

  carregarClientes() {
    this.clienteService.listarTodosClientes().subscribe({
      next: (data) => {
        this.clientes = data;
        console.log('Clientes carregados:', this.clientes);
        if (this.clientes.length === 0 && !this.error) {
          this.error = 'Nenhum cliente cadastrado. Cadastre um cliente primeiro.';
        }
      },
      error: (err) => {
        console.error('Erro ao carregar clientes:', err);
        if (!this.error) {
          this.error = 'Erro ao carregar clientes. Verifique se a API está rodando.';
        }
      }
    });
  }

  carregarUsuarios() {
    this.usuarioService.listarTodosUsuarios().subscribe({
      next: (data) => {
        this.usuarios = data;
        console.log('Usuários carregados:', this.usuarios);
        if (this.usuarios.length === 0 && !this.error) {
          this.error = 'Nenhum usuário cadastrado. Cadastre um usuário primeiro.';
        }
      },
      error: (err) => {
        console.error('Erro ao carregar usuários:', err);
        if (!this.error) {
          this.error = 'Erro ao carregar usuários. Verifique se a API está rodando.';
        }
      }
    });
  }

  aplicarFiltros() {
    let tarefasFiltradas = [...this.tarefas];

    // Filtrar por termo de busca global
    if (this.termoBusca.trim()) {
      const termo = this.termoBusca.toLowerCase();
      tarefasFiltradas = tarefasFiltradas.filter(t =>
        t.clienteNome.toLowerCase().includes(termo) ||
        t.usuarioNome.toLowerCase().includes(termo) ||
        t.statusDescricao.toLowerCase().includes(termo) ||
        t.tarefaId.toString().includes(termo) ||
        (t.titulo && t.titulo.toLowerCase().includes(termo)) ||
        (t.protocolo && t.protocolo.toLowerCase().includes(termo)) ||
        (t.solicitante && t.solicitante.toLowerCase().includes(termo))
      );
    }

    // Filtros por coluna (multi-select)
    for (const [campo, valores] of Object.entries(this.filtrosColunasSelecao)) {
      if (!valores?.length) continue;
      const set = new Set(valores);
      tarefasFiltradas = tarefasFiltradas.filter(t => set.has(String((t as any)[campo] ?? '')));
    }

    this.tarefasFiltradas = tarefasFiltradas;
  }

  filtrarTarefas() {
    this.aplicarFiltros();
  }

  abrirFormularioNovo() {
    this.editando = false;
    this.tarefaEditando = null;
    this.showForm = true;
    this.novoTarefa = {
      clienteId: 0,
      usuarioId: 0,
      status: this.obterStatusPadrao(),
      dataConclusao: undefined,
      descricao: undefined,
      titulo: '',
      protocolo: '',
      solicitante: '',
      celularSolicitante: '',
      tipoAtendimento: TipoAtendimento.Suporte,
      prioridade: PrioridadeTarefa.Baixa,
      tipoContato: TipoContato.WhatsApp,
      imagens: undefined
    };
    this.imagensSelecionadas = [];
    this.previewImagens = [];
    this.novaAnotacao = '';
    this.error = null;
    this.sincronizarValoresPadraoFormulario();

    // Scroll para o topo do modal após um pequeno delay para garantir que o DOM foi renderizado
    setTimeout(() => {
      const modalContent = document.querySelector('.modal-content');
      if (modalContent) {
        modalContent.scrollTop = 0;
      }
    }, 100);
  }

  abrirFormularioEdicao(tarefa: TarefaResponseDto) {
    this.editando = true;
    this.tarefaEditando = tarefa;
    this.showForm = true;
    this.novoTarefa = {
      clienteId: tarefa.clienteId,
      usuarioId: tarefa.usuarioId,
      status: tarefa.status,
      dataConclusao: tarefa.dataConclusao,
      descricao: undefined,
      titulo: tarefa.titulo,
      protocolo: tarefa.protocolo,
      solicitante: tarefa.solicitante,
      celularSolicitante: tarefa.celularSolicitante,
      tipoAtendimento: tarefa.tipoAtendimento,
      prioridade: tarefa.prioridade || PrioridadeTarefa.Media,
      tipoContato: tarefa.tipoContato,
      imagens: undefined
    };
    this.imagensSelecionadas = [];
    this.previewImagens = [];
    this.novaAnotacao = '';
    this.error = null;
    this.sincronizarValoresPadraoFormulario();

    // Carregar anotações da tarefa
    this.anotacaoService.obterAnotacoesPorTarefa(tarefa.tarefaId).subscribe({
      next: (anotacoes) => {
        if (this.tarefaEditando) {
          this.tarefaEditando.anotacoes = anotacoes || [];
        }
      },
      error: (err) => {
        console.error('Erro ao carregar anotações:', err);
      }
    });
  }

  abrirAnotacoes(tarefa: TarefaResponseDto) {
    this.tarefaSelecionada = tarefa;
    this.showAnotacoes = true;
    this.novaAnotacao = '';
    this.error = null;

    // Sempre carregar anotações atualizadas
    this.anotacaoService.obterAnotacoesPorTarefa(tarefa.tarefaId).subscribe({
      next: (anotacoes) => {
        if (this.tarefaSelecionada) {
          this.tarefaSelecionada.anotacoes = anotacoes || [];
        }
      },
      error: (err) => {
        console.error('Erro ao carregar anotações:', err);
      }
    });
  }

  fecharAnotacoes() {
    this.showAnotacoes = false;
    this.tarefaSelecionada = null;
    this.novaAnotacao = '';
    this.error = null;
  }

  abrirImagens(tarefa: TarefaResponseDto) {
    this.tarefaImagens = tarefa;
    this.imagemAtualIndex = 0;
    this.showImagens = true;
  }

  fecharImagens() {
    this.showImagens = false;
    this.tarefaImagens = null;
    this.imagemAtualIndex = 0;
  }

  proximaImagem() {
    if (this.tarefaImagens && this.tarefaImagens.imagens && this.imagemAtualIndex < this.tarefaImagens.imagens.length - 1) {
      this.imagemAtualIndex++;
    }
  }

  imagemAnterior() {
    if (this.imagemAtualIndex > 0) {
      this.imagemAtualIndex--;
    }
  }

  inserirAnotacao() {
    if (!this.novaAnotacao.trim()) {
      this.error = 'Digite uma descrição para a anotação';
      this.notificacao.aviso(this.error);
      return;
    }

    if (!this.tarefaSelecionada) {
      return;
    }

    this.loading = true;
    this.error = null;

    // Criar anotação (a data/hora será adicionada no backend)
    const dto: CadastroAnotacaoDto = {
      tarefaId: this.tarefaSelecionada.tarefaId,
      usuarioId: this.tarefaSelecionada.usuarioId,
      descricao: this.novaAnotacao.trim()
    };

    this.anotacaoService.cadastrarAnotacao(dto).subscribe({
      next: (anotacao) => {
        // tarefaSelecionada é a mesma referência do item em this.tarefas; atualizar só aqui evita duplicata
        if (this.tarefaSelecionada) {
          if (!this.tarefaSelecionada.anotacoes) {
            this.tarefaSelecionada.anotacoes = [];
          }
          this.tarefaSelecionada.anotacoes.unshift(anotacao);
        }
        this.novaAnotacao = '';
        this.loading = false;
        this.notificacao.sucesso('Anotação adicionada com sucesso.');
      },
      error: (err) => {
        console.error('Erro ao salvar anotação:', err);
        this.error = err.error?.message || 'Erro ao salvar anotação';
        this.loading = false;
      }
    });
  }

  fecharFormulario() {
    this.showForm = false;
    this.editando = false;
    this.tarefaEditando = null;
    this.error = null;
  }

  fecharModalErro() {
    this.error = null;
  }

  onStatusChange(novoStatus: number): void {
    const statusNormalizado = this.normalizarCampoNumerico(novoStatus, this.obterStatusPadrao());
    this.novoTarefa.status = statusNormalizado;

    if (statusNormalizado === StatusTarefa.Concluida && !this.novoTarefa.dataConclusao) {
      this.novoTarefa.dataConclusao = new Date().toISOString().split('T')[0];
    } else if (statusNormalizado !== StatusTarefa.Concluida) {
      this.novoTarefa.dataConclusao = undefined;
    }
  }

  private obterStatusPadrao(): number {
    const emAberto = this.statusOptions.find(s => s.label.toUpperCase().includes('ABERTO'));
    return emAberto?.value ?? StatusTarefa.EmAberto;
  }

  private normalizarCampoNumerico(valor: unknown, padrao: number): number {
    const numero = Number(valor);
    return Number.isFinite(numero) ? numero : padrao;
  }

  private sincronizarValoresPadraoFormulario(): void {
    if (!this.showForm) return;

    const statusAtual = this.normalizarCampoNumerico(this.novoTarefa.status, NaN);
    if (!Number.isFinite(statusAtual) || !this.statusOptions.some(s => s.value === statusAtual)) {
      this.novoTarefa.status = this.obterStatusPadrao();
    }
  }

  salvarTarefa(form?: NgForm) {
    if (form?.form) form.form.markAllAsTouched();

    const usuarioIdLogado = this.authService.getUsuarioId();
    if (!usuarioIdLogado) {
      this.error = 'Usuário não autenticado. Faça login novamente.';
      this.notificacao.erro(this.error);
      return;
    }

    const titulo = (this.novoTarefa.titulo ?? '').toString().trim();
    const solicitante = (this.novoTarefa.solicitante ?? '').toString().trim();
    const clienteId = Number(this.novoTarefa.clienteId);
    if (!titulo || !solicitante || !clienteId || clienteId === 0) {
      return;
    }

    this.loading = true;
    this.error = null;

    const statusEnvio = this.normalizarCampoNumerico(this.novoTarefa.status, this.obterStatusPadrao());

    if (statusEnvio === StatusTarefa.Concluida && !this.novoTarefa.dataConclusao) {
      this.novoTarefa.dataConclusao = new Date().toISOString().split('T')[0];
    }

    // Na edição, usar o usuarioId original; na criação, usar o usuário logado (já validado acima)
    const usuarioIdFinal: number = this.editando && this.tarefaEditando
      ? this.tarefaEditando.usuarioId
      : usuarioIdLogado!;

    // Preparar dados para envio
    const dadosEnvio: CadastroTarefaDto = {
      clienteId: clienteId,
      usuarioId: usuarioIdFinal,
      status: statusEnvio,
      dataConclusao: statusEnvio === StatusTarefa.Concluida
        ? (this.novoTarefa.dataConclusao || new Date().toISOString().split('T')[0])
        : undefined,
      descricao: this.novoTarefa.descricao ?? undefined,
      titulo: this.novoTarefa.titulo ? this.novoTarefa.titulo.toUpperCase() : undefined,
      protocolo: this.novoTarefa.protocolo ? this.novoTarefa.protocolo.toUpperCase() : undefined,
      solicitante: this.novoTarefa.solicitante ? this.novoTarefa.solicitante.toUpperCase() : undefined,
      celularSolicitante: this.novoTarefa.celularSolicitante?.trim() || undefined,
      tipoAtendimento: this.novoTarefa.tipoAtendimento,
      prioridade: this.novoTarefa.prioridade || PrioridadeTarefa.Media,
      tipoContato: this.novoTarefa.tipoContato,
      imagens: this.imagensSelecionadas.length > 0 ? this.imagensSelecionadas : undefined
    };

    console.log('Dados enviados:', dadosEnvio);

    const operacao = this.editando && this.tarefaEditando
      ? this.tarefaService.atualizarTarefa(this.tarefaEditando.tarefaId, dadosEnvio)
      : this.tarefaService.cadastrarTarefa(dadosEnvio);

    operacao.subscribe({
      next: (result) => {
        console.log('Tarefa salva com sucesso:', result);
        // Recarregar tarefas para atualizar anotações
        this.carregarTarefas();
        // Se estava editando, atualizar a tarefa editada com as anotações
        if (this.editando && this.tarefaEditando) {
          this.tarefaEditando.anotacoes = result.anotacoes || [];
          this.tarefaEditando.imagens = result.imagens || [];
        }
        this.fecharFormulario();
        this.loading = false;
        this.notificacao.sucesso(this.editando ? 'Atendimento atualizado com sucesso.' : 'Atendimento cadastrado com sucesso.');
      },
      error: (err) => {
        console.error('Erro completo ao salvar tarefa:', err);
        console.error('Status:', err.status);
        console.error('Mensagem:', err.message);
        console.error('Error body:', err.error);
        this.error = err.error?.message || err.message || 'Erro ao salvar tarefa. Verifique se a API está rodando e se há clientes e usuários cadastrados.';
        this.loading = false;
      }
    });
  }

  excluirTarefa(tarefa: TarefaResponseDto) {
    this.confirmarExclusaoTarefa(tarefa);
  }

  private async confirmarExclusaoTarefa(tarefa: TarefaResponseDto): Promise<void> {
    const ok = await this.notificacao.confirmar(
      'Confirmar exclusão',
      `Deseja realmente excluir o atendimento #${tarefa.tarefaId}?`,
      'Excluir',
      'Cancelar'
    );
    if (!ok) return;

    this.loading = true;
    this.error = null;

    this.tarefaService.excluirTarefa(tarefa.tarefaId).subscribe({
      next: () => {
        this.carregarTarefas();
        this.loading = false;
        this.notificacao.sucesso('Atendimento excluído com sucesso.');
      },
      error: (err) => {
        this.error = err.error?.message || 'Erro ao excluir tarefa';
        this.loading = false;
      }
    });
  }

  alterarStatus(tarefa: TarefaResponseDto, novoStatus: number) {
    this.loading = true;
    this.error = null;

    this.tarefaService.alterarStatusTarefa(tarefa.tarefaId, novoStatus).subscribe({
      next: () => {
        this.carregarTarefas();
        this.loading = false;
        this.notificacao.sucesso('Status alterado com sucesso.');
      },
      error: (err) => {
        this.error = err.error?.message || 'Erro ao alterar status';
        this.loading = false;
      }
    });
  }

  formatarData(data?: string): string {
    if (!data) return '-';
    return new Date(data).toLocaleDateString('pt-BR');
  }

  formatarDataHoraAnotacao(data?: string): string {
    if (!data) return '';
    const date = new Date(data);
    const dia = date.getDate().toString().padStart(2, '0');
    const mes = (date.getMonth() + 1).toString().padStart(2, '0');
    const ano = date.getFullYear();
    const horas = date.getHours().toString().padStart(2, '0');
    const minutos = date.getMinutes().toString().padStart(2, '0');
    return `${dia}/${mes}/${ano} - ${horas}:${minutos}`;
  }

  obterClasseStatus(status: number): string {
    switch (status) {
      case StatusTarefa.EmAberto:
        return 'status-aberto';
      case StatusTarefa.Concluida:
        return 'status-concluida';
      case StatusTarefa.Cancelada:
        return 'status-cancelada';
      case StatusTarefa.Reativada:
        return 'status-reativada';
      case StatusTarefa.AguardandoCliente:
        return 'status-aguardando';
      default:
        return '';
    }
  }

  obterClassePrioridade(prioridadeDescricao?: string): string {
    if (!prioridadeDescricao) return 'prioridade-media';

    const descricaoNormalizada = prioridadeDescricao.toLowerCase()
      .normalize('NFD')
      .replace(/[\u0300-\u036f]/g, ''); // Remove acentos

    return `prioridade-${descricaoNormalizada}`;
  }

  /** Classe da linha inteira por status (para AG Grid getRowClass). */
  obterClasseLinhaStatus(tarefa: TarefaResponseDto | null | undefined): string {
    if (!tarefa) return 'row-status-aberto';
    const pelaDescricao = (desc: string): string => {
      const d = (desc || '').toLowerCase().normalize('NFD').replace(/[\u0300-\u036f]/g, '');
      if (d.includes('aberto')) return 'row-status-aberto';
      if (d.includes('concluida')) return 'row-status-concluida';
      if (d.includes('cancelada')) return 'row-status-cancelada';
      if (d.includes('reativada')) return 'row-status-reativada';
      if (d.includes('aguardando')) return 'row-status-aguardando';
      return 'row-status-aberto';
    };
    const classe = this.obterClasseStatus(tarefa.status);
    if (classe) return 'row-' + classe;
    return pelaDescricao(tarefa.statusDescricao || '');
  }

  onImagensSelecionadas(event: any) {
    const files = event.target.files;
    if (files && files.length > 0) {
      this.imagensSelecionadas = Array.from(files);
      this.previewImagens = [];

      this.imagensSelecionadas.forEach((file: File) => {
        const reader = new FileReader();
        reader.onload = (e: any) => {
          this.previewImagens.push(e.target.result);
        };
        reader.readAsDataURL(file);
      });
    }
  }

  removerImagem(index: number) {
    this.imagensSelecionadas.splice(index, 1);
    this.previewImagens.splice(index, 1);
  }

  onErroImagem(event: Event) {
    const target = event.target as HTMLImageElement;
    if (target) {
      target.style.display = 'none';
    }
  }

  aplicarMascaraCelular(event: Event): void {
    const input = event.target as HTMLInputElement;
    let valor = input.value.replace(/\D/g, '');

    // Limitar a 11 dígitos (DDD + 9 dígitos)
    if (valor.length > 11) {
      valor = valor.substring(0, 11);
    }

    // Aplicar máscara: (11) 98327-0236
    if (valor.length > 0) {
      if (valor.length <= 2) {
        valor = `(${valor}`;
      } else if (valor.length <= 7) {
        valor = `(${valor.substring(0, 2)}) ${valor.substring(2)}`;
      } else {
        valor = `(${valor.substring(0, 2)}) ${valor.substring(2, 7)}-${valor.substring(7)}`;
      }
    }

    input.value = valor;
    this.novoTarefa.celularSolicitante = valor;
  }
}
