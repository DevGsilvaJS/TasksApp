import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { OverlayPanel, OverlayPanelModule } from 'primeng/overlaypanel';
import { PossivelClienteService, PossivelClienteResponseDto, STATUS_ATENDIMENTO_OPCOES, AtualizarStatusAtendimentoDto, PossivelClienteAnotacaoResponseDto, CadastroPossivelClienteAnotacaoDto } from '../../services/possivel-cliente.service';

/** Grupo por cliente (código): uma linha por cliente, com lojas expansíveis */
export interface ClienteGrupo {
  codigo: string;
  fantasiaPrincipal: string;
  telefonePrincipal: PossivelClienteResponseDto | null;
  statusPrincipal: number | null;
  statusLabel: string;
  lojas: PossivelClienteResponseDto[];
}
import { AuthService } from '../../services/auth.service';
import { CadastroAtendimentoService } from '../../services/cadastro-atendimento.service';

@Component({
  selector: 'app-possiveis-clientes',
  standalone: true,
  imports: [CommonModule, FormsModule, TableModule, OverlayPanelModule],
  templateUrl: './possiveis-clientes.component.html',
  styleUrl: './possiveis-clientes.component.css'
})
export class PossiveisClientesComponent implements OnInit {
  lista: PossivelClienteResponseDto[] = [];
  listaFiltrada: PossivelClienteResponseDto[] = [];
  loading = false;
  error: string | null = null;
  termoBusca = '';

  /** Exibir apenas registros que tenham telefone/WhatsApp */
  apenasComTelefone = false;
  /** true = uma linha por cliente (agrupado por código); false = todas as lojas */
  agruparPorCliente = true;
  /** 'tabela' ou 'cards' */
  viewLayout: 'tabela' | 'cards' = 'tabela';
  /** Códigos expandidos na tabela (quando agruparPorCliente) */
  expandidos = new Set<string>();

  itemSelecionado: PossivelClienteResponseDto | null = null;
  showModal = false;
  itemParaStatus: PossivelClienteResponseDto | null = null;
  showModalStatus = false;
  statusAtendimentoSelecionado = 1;
  motivoPerda = '';
  savingStatus = false;
  /** Opções de status (carregadas da API; fallback para lista fixa) */
  statusOpcoes: { valor: number; label: string }[] = [];

  /** Modal de atendimento (WhatsApp + anotações + status) */
  itemParaAtendimento: PossivelClienteResponseDto | null = null;
  showModalAtendimento = false;
  anotacoesAtendimento: PossivelClienteAnotacaoResponseDto[] = [];
  novaAnotacaoAtendimento = '';
  loadingAnotacao = false;
  savingAtendimento = false;
  /** Edição de anotação no histórico */
  anotacaoEditandoId: number | null = null;
  textoEditando = '';
  savingEdicaoAnotacao = false;

  /** Colunas com filtro multi-select (ícone funil). */
  readonly colunasFiltravelis: { campo: string; label: string }[] = [
    { campo: 'pocCodigo', label: 'Código' },
    { campo: 'pocLoja', label: 'Loja' },
    { campo: 'pocFantasia', label: 'Fantasia' },
    { campo: 'statusAtendimentoLabel', label: 'Status Atend.' }
  ];
  filtrosColunasSelecao: Record<string, string[]> = {};
  filtroColunaAtivo: string | null = null;
  selecaoTemp: Record<string, string[]> = {};
  @ViewChild('opFiltroColuna') opFiltroColuna!: OverlayPanel;

  constructor(
    private possivelClienteService: PossivelClienteService,
    private authService: AuthService,
    private cadastroAtendimentoService: CadastroAtendimentoService
  ) { }

  ngOnInit() {
    this.carregar();
    this.cadastroAtendimentoService.listarStatusAtendimentoComercial(true).subscribe({
      next: (data) => {
        this.statusOpcoes = data.map(s => ({ valor: s.numero, label: s.descricao }));
      },
      error: () => {
        this.statusOpcoes = STATUS_ATENDIMENTO_OPCOES.map(o => ({ valor: o.valor, label: o.label }));
      }
    });
  }

  carregar() {
    this.loading = true;
    this.error = null;
    this.possivelClienteService.listarTodos().subscribe({
      next: (data) => {
        this.lista = data;
        this.aplicarFiltros();
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Erro ao carregar possíveis clientes. Verifique se a API está rodando.';
        this.loading = false;
        console.error(err);
      }
    });
  }

  /** Valor exibido da coluna (para status atendimento usa label). */
  getValorColuna(item: PossivelClienteResponseDto, campo: string): string {
    if (campo === 'statusAtendimentoLabel') return this.labelStatusAtendimento(item);
    const v = (item as any)[campo];
    return v == null ? '' : String(v);
  }

  getDadosParaFiltroColuna(campo: string): PossivelClienteResponseDto[] {
    let dados = [...this.lista];
    if (this.termoBusca.trim()) {
      const termo = this.termoBusca.toLowerCase();
      dados = dados.filter(p =>
        (p.pocCodigo && p.pocCodigo.toLowerCase().includes(termo)) ||
        (p.pocLoja && p.pocLoja.toLowerCase().includes(termo)) ||
        (p.pocFantasia && p.pocFantasia.toLowerCase().includes(termo)) ||
        (p.pocRazaoSocial && p.pocRazaoSocial.toLowerCase().includes(termo)) ||
        (p.pocCnpj && p.pocCnpj.replace(/\D/g, '').includes(termo.replace(/\D/g, ''))) ||
        (p.pocEmailComercial && p.pocEmailComercial.toLowerCase().includes(termo)) ||
        (p.pocCelular && p.pocCelular.includes(termo))
      );
    }
    for (const [col, valores] of Object.entries(this.filtrosColunasSelecao)) {
      if (col === campo || !valores?.length) continue;
      const set = new Set(valores);
      dados = dados.filter(p => set.has(this.getValorColuna(p, col)));
    }
    return dados;
  }

  getValoresDistintosColuna(campo: string): string[] {
    const dados = this.getDadosParaFiltroColuna(campo);
    const set = new Set<string>();
    for (const p of dados) {
      set.add(this.getValorColuna(p, campo));
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

  aplicarFiltros() {
    let dados = [...this.lista];
    if (this.termoBusca.trim()) {
      const termo = this.termoBusca.toLowerCase();
      dados = dados.filter(p =>
        (p.pocCodigo && p.pocCodigo.toLowerCase().includes(termo)) ||
        (p.pocLoja && p.pocLoja.toLowerCase().includes(termo)) ||
        (p.pocFantasia && p.pocFantasia.toLowerCase().includes(termo)) ||
        (p.pocRazaoSocial && p.pocRazaoSocial.toLowerCase().includes(termo)) ||
        (p.pocCnpj && p.pocCnpj.replace(/\D/g, '').includes(termo.replace(/\D/g, ''))) ||
        (p.pocEmailComercial && p.pocEmailComercial.toLowerCase().includes(termo)) ||
        (p.pocCelular && p.pocCelular.includes(termo))
      );
    }
    for (const [campo, valores] of Object.entries(this.filtrosColunasSelecao)) {
      if (!valores?.length) continue;
      const set = new Set(valores);
      dados = dados.filter(p => set.has(this.getValorColuna(p, campo)));
    }
    this.listaFiltrada = dados;
  }

  /** Lista filtrada + opcionalmente apenas com telefone, ordenada: status (não atendido primeiro) e depois cliente */
  get listaOrdenada(): PossivelClienteResponseDto[] {
    let dados = [...this.listaFiltrada];
    if (this.apenasComTelefone) {
      dados = dados.filter(p => this.temWhatsApp(p));
    }
    const statusOrd = (p: PossivelClienteResponseDto) => p.pocStatusAtendimento ?? 99;
    const nomeOrd = (p: PossivelClienteResponseDto) => (p.pocFantasia ?? p.pocCodigo ?? '').toLowerCase();
    dados.sort((a, b) => {
      const sa = statusOrd(a);
      const sb = statusOrd(b);
      if (sa !== sb) return sa - sb;
      return nomeOrd(a).localeCompare(nomeOrd(b));
    });
    return dados;
  }

  /** Agrupa por PocCodigo para exibir uma linha por cliente */
  get listaAgrupada(): ClienteGrupo[] {
    const ordenada = this.listaOrdenada;
    const map = new Map<string, PossivelClienteResponseDto[]>();
    for (const p of ordenada) {
      const cod = p.pocCodigo ?? '';
      if (!map.has(cod)) map.set(cod, []);
      map.get(cod)!.push(p);
    }
    const grupos: ClienteGrupo[] = [];
    for (const [codigo, lojas] of map) {
      const comTelefone = lojas.find(p => this.temWhatsApp(p)) ?? lojas[0];
      const statusMin = Math.min(...lojas.map(p => p.pocStatusAtendimento ?? 99));
      const statusPrincipal = statusMin === 99 ? null : statusMin;
      const primeiro = lojas[0];
      grupos.push({
        codigo,
        fantasiaPrincipal: primeiro?.pocFantasia ?? primeiro?.pocCodigo ?? codigo,
        telefonePrincipal: this.temWhatsApp(comTelefone) ? comTelefone : null,
        statusPrincipal,
        statusLabel: statusPrincipal != null ? this.labelStatusAtendimento(lojas.find(p => (p.pocStatusAtendimento ?? 99) === statusMin)!) : '—',
        lojas
      });
    }
    grupos.sort((a, b) => {
      const sa = a.statusPrincipal ?? 99;
      const sb = b.statusPrincipal ?? 99;
      if (sa !== sb) return sa - sb;
      return (a.fantasiaPrincipal ?? a.codigo).toLowerCase().localeCompare((b.fantasiaPrincipal ?? b.codigo).toLowerCase());
    });
    return grupos;
  }

  toggleExpandir(codigo: string) {
    if (this.expandidos.has(codigo)) this.expandidos.delete(codigo);
    else this.expandidos.add(codigo);
    this.expandidos = new Set(this.expandidos);
  }

  estaExpandido(codigo: string): boolean {
    return this.expandidos.has(codigo);
  }

  filtrar() {
    this.aplicarFiltros();
  }

  abrirCadastro(item: PossivelClienteResponseDto) {
    this.itemSelecionado = item;
    this.showModal = true;
  }

  fecharModal() {
    this.showModal = false;
    this.itemSelecionado = null;
  }

  fecharModalErro() {
    this.error = null;
  }

  /** Link para abrir WhatsApp com o número do cliente (55 + DDD + número). */
  linkWhatsApp(item: PossivelClienteResponseDto): string {
    const ddd = (item.pocCelDdd ?? item.pocDdd ?? '').replace(/\D/g, '');
    let cel = (item.pocCelular ?? '').replace(/\D/g, '');
    if (!cel && ddd.length > 2) cel = ddd.slice(2);
    const ddd2 = ddd.slice(0, 2);
    const numero = (ddd2 + cel).replace(/\D/g, '');
    if (numero.length < 10) return '#';
    return `https://wa.me/55${numero}`;
  }

  temWhatsApp(item: PossivelClienteResponseDto): boolean {
    return this.linkWhatsApp(item) !== '#';
  }

  abrirModalStatus(item: PossivelClienteResponseDto) {
    this.abrirModalAtendimento(item);
  }

  abrirModalAtendimento(item: PossivelClienteResponseDto) {
    this.itemParaAtendimento = item;
    this.statusAtendimentoSelecionado = item.pocStatusAtendimento ?? 1;
    this.motivoPerda = item.pocMotivoPerda ?? '';
    this.novaAnotacaoAtendimento = '';
    this.anotacaoEditandoId = null;
    this.textoEditando = '';
    this.showModalAtendimento = true;
    this.anotacoesAtendimento = [];
    this.possivelClienteService.listarAnotacoes(item.pocId).subscribe({
      next: (data) => { this.anotacoesAtendimento = data; },
      error: () => { this.anotacoesAtendimento = []; }
    });
  }

  fecharModalAtendimento() {
    this.showModalAtendimento = false;
    this.itemParaAtendimento = null;
    this.novaAnotacaoAtendimento = '';
    this.anotacoesAtendimento = [];
    this.anotacaoEditandoId = null;
    this.textoEditando = '';
  }

  iniciarEdicaoAnotacao(a: PossivelClienteAnotacaoResponseDto) {
    this.anotacaoEditandoId = a.pcaId;
    this.textoEditando = a.descricao ?? '';
  }

  cancelarEdicaoAnotacao() {
    this.anotacaoEditandoId = null;
    this.textoEditando = '';
  }

  salvarEdicaoAnotacao() {
    if (!this.itemParaAtendimento || this.anotacaoEditandoId == null || !this.textoEditando.trim()) return;
    this.savingEdicaoAnotacao = true;
    this.error = null;
    this.possivelClienteService.atualizarAnotacao(this.itemParaAtendimento.pocId, this.anotacaoEditandoId, { descricao: this.textoEditando.trim() }).subscribe({
      next: (atualizada) => {
        const idx = this.anotacoesAtendimento.findIndex(x => x.pcaId === atualizada.pcaId);
        if (idx >= 0) this.anotacoesAtendimento[idx] = atualizada;
        this.anotacaoEditandoId = null;
        this.textoEditando = '';
        this.savingEdicaoAnotacao = false;
      },
      error: (err) => {
        this.error = err.error?.message || 'Erro ao atualizar anotação.';
        this.savingEdicaoAnotacao = false;
      }
    });
  }

  fecharModalStatus() {
    this.showModalStatus = false;
    this.itemParaStatus = null;
    this.motivoPerda = '';
  }

  /** Anotação e status obrigatórios para poder salvar. */
  podeSalvarAtendimento(): boolean {
    return !!(
      this.itemParaAtendimento &&
      this.novaAnotacaoAtendimento?.trim() &&
      this.statusAtendimentoSelecionado != null
    );
  }

  /** Salva anotação e status em sequência (ambos obrigatórios). */
  salvarAnotacaoEStatus() {
    if (!this.podeSalvarAtendimento() || !this.itemParaAtendimento) return;
    const usuarioId = this.authService.getUsuarioId();
    if (!usuarioId) {
      this.error = 'Usuário não autenticado.';
      return;
    }
    this.savingAtendimento = true;
    this.error = null;
    const descricao = this.novaAnotacaoAtendimento.trim();
    const dtoStatus: AtualizarStatusAtendimentoDto = {
      statusAtendimento: Number(this.statusAtendimentoSelecionado),
      motivoPerda: this.statusAtendimentoSelecionado === 8 ? (this.motivoPerda?.trim() || undefined) : undefined
    };
    this.possivelClienteService.adicionarAnotacao(this.itemParaAtendimento.pocId, { descricao, usuarioId }).subscribe({
      next: (nova) => {
        this.anotacoesAtendimento = [nova, ...this.anotacoesAtendimento];
        this.novaAnotacaoAtendimento = '';
        this.possivelClienteService.atualizarStatusAtendimento(this.itemParaAtendimento!.pocId, dtoStatus).subscribe({
          next: (atualizado) => {
            const idx = this.lista.findIndex(p => p.pocId === atualizado.pocId);
            if (idx >= 0) this.lista[idx] = atualizado;
            if (this.itemParaAtendimento?.pocId === atualizado.pocId) {
              this.itemParaAtendimento = atualizado;
            }
            this.aplicarFiltros();
            this.savingAtendimento = false;
          },
          error: (err) => {
            this.error = err.error?.message || 'Erro ao salvar status.';
            this.savingAtendimento = false;
          }
        });
      },
      error: (err) => {
        this.error = err.error?.message || 'Erro ao salvar anotação.';
        this.savingAtendimento = false;
      }
    });
  }

  inserirAnotacaoAtendimento() {
    if (!this.itemParaAtendimento || !this.novaAnotacaoAtendimento.trim()) return;
    const usuarioId = this.authService.getUsuarioId();
    if (!usuarioId) {
      this.error = 'Usuário não autenticado.';
      return;
    }
    this.loadingAnotacao = true;
    this.possivelClienteService.adicionarAnotacao(this.itemParaAtendimento.pocId, {
      descricao: this.novaAnotacaoAtendimento.trim(),
      usuarioId
    }).subscribe({
      next: (nova) => {
        this.anotacoesAtendimento = [nova, ...this.anotacoesAtendimento];
        this.novaAnotacaoAtendimento = '';
        this.loadingAnotacao = false;
      },
      error: (err) => {
        this.error = err.error?.message || 'Erro ao salvar anotação.';
        this.loadingAnotacao = false;
      }
    });
  }

  salvarStatusAtendimento() {
    if (!this.itemParaAtendimento) return;
    this.savingStatus = true;
    this.error = null;
    const dto: AtualizarStatusAtendimentoDto = {
      statusAtendimento: Number(this.statusAtendimentoSelecionado),
      motivoPerda: this.statusAtendimentoSelecionado === 8 ? (this.motivoPerda?.trim() || undefined) : undefined
    };
    this.possivelClienteService.atualizarStatusAtendimento(this.itemParaAtendimento.pocId, dto).subscribe({
      next: (atualizado) => {
        const idx = this.lista.findIndex(p => p.pocId === atualizado.pocId);
        if (idx >= 0) this.lista[idx] = atualizado;
        if (this.itemParaAtendimento?.pocId === atualizado.pocId) {
          this.itemParaAtendimento = atualizado;
        }
        this.aplicarFiltros();
        this.savingStatus = false;
      },
      error: (err) => {
        this.error = err.error?.message || 'Erro ao salvar status.';
        this.savingStatus = false;
      }
    });
  }

  formatarDataHora(data?: string): string {
    if (!data) return '-';
    const d = new Date(data);
    return d.toLocaleString('pt-BR', { dateStyle: 'short', timeStyle: 'short' });
  }

  salvarStatus() {
    if (!this.itemParaStatus) return;
    this.savingStatus = true;
    this.error = null;
    const dto: AtualizarStatusAtendimentoDto = {
      statusAtendimento: Number(this.statusAtendimentoSelecionado),
      motivoPerda: this.statusAtendimentoSelecionado === 8 ? (this.motivoPerda?.trim() || undefined) : undefined
    };
    this.possivelClienteService.atualizarStatusAtendimento(this.itemParaStatus.pocId, dto).subscribe({
      next: (atualizado) => {
        const idx = this.lista.findIndex(p => p.pocId === atualizado.pocId);
        if (idx >= 0) this.lista[idx] = atualizado;
        this.filtrar();
        this.fecharModalStatus();
        this.savingStatus = false;
      },
      error: (err) => {
        this.error = err.error?.message || 'Erro ao salvar status.';
        this.savingStatus = false;
      }
    });
  }

  labelStatusAtendimento(item: PossivelClienteResponseDto): string {
    const n = item.pocStatusAtendimento;
    if (n == null) return '—';
    const list = this.statusOpcoes.length > 0 ? this.statusOpcoes : STATUS_ATENDIMENTO_OPCOES.map(o => ({ valor: o.valor, label: o.label }));
    const op = list.find(o => o.valor === n);
    return op ? `${n}. ${op.label}` : String(n);
  }

  formatarData(data?: string): string {
    if (!data) return '-';
    return new Date(data).toLocaleDateString('pt-BR');
  }

  valorOuTraco(valor?: string | null): string {
    return (valor != null && valor !== '') ? valor : '-';
  }

  /** Máscara CPF (11 dígitos) XXX.XXX.XXX-XX ou CNPJ (14 dígitos) XX.XXX.XXX/XXXX-XX. */
  formatarCnpj(valor?: string | null): string {
    if (valor == null || valor === '') return '-';
    const nums = valor.replace(/\D/g, '');
    if (nums.length === 0) return '-';
    if (nums.length <= 11) {
      const a = nums.slice(0, 3);
      const b = nums.slice(3, 6);
      const c = nums.slice(6, 9);
      const d = nums.slice(9, 11);
      if (nums.length <= 3) return a;
      if (nums.length <= 6) return `${a}.${b}`;
      if (nums.length <= 9) return `${a}.${b}.${c}`;
      return `${a}.${b}.${c}-${d}`;
    }
    if (nums.length >= 14) {
      return nums.slice(0, 14).replace(/^(\d{2})(\d{3})(\d{3})(\d{4})(\d{2})$/, '$1.$2.$3/$4-$5');
    }
    return nums;
  }

  /** Celular com DDD: (XX) XXXXX-XXXX ou (XX) XXXX-XXXX. Usa pocCelDdd + pocCelular (ou pocDdd se celular vazio). */
  formatarCelular(item: PossivelClienteResponseDto): string {
    const dddRaw = (item.pocCelDdd ?? item.pocDdd ?? '').replace(/\D/g, '');
    let celRaw = (item.pocCelular ?? '').replace(/\D/g, '');
    if (!celRaw && dddRaw.length > 2) {
      celRaw = dddRaw.slice(2);
    }
    const ddd2 = dddRaw.slice(0, 2);
    if (!ddd2 && !celRaw) return '-';
    if (celRaw.length >= 9) {
      return `(${ddd2}) ${celRaw.slice(0, 5)}-${celRaw.slice(5, 9)}`;
    }
    if (celRaw.length >= 8) {
      return `(${ddd2}) ${celRaw.slice(0, 4)}-${celRaw.slice(4, 8)}`;
    }
    return `(${ddd2}) ${celRaw}`.trim() || '-';
  }
}
