import { Component, OnInit } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive, Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { AuthService } from './services/auth.service';
import { AlertasService } from './services/alertas.service';
import { PendenciasAlertasDto } from './services/alertas.service';
import { TarefaService, TarefaResponseDto, TipoAtendimento } from './services/tarefa.service';
import { DashboardService, AlertaContratoVencendoDto } from './services/dashboard.service';
import { NotificacaoCentralComponent } from './components/notificacao-central/notificacao-central.component';

export type AlertaItem =
  | { tipo: 'pendencias'; data: PendenciasAlertasDto }
  | { tipo: 'reunioes'; data: TarefaResponseDto[] }
  | { tipo: 'contrato-vencendo'; data: AlertaContratoVencendoDto };

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, CommonModule, FormsModule, NotificacaoCentralComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit {
  title = 'GA CONSULTORIA';
  isAuthenticated = false;
  usuarioNome = '';
  filaAlertas: AlertaItem[] = [];
  currentAlertaIndex = 0;
  private popupAlertasMostrado = false;
  cienteAlertaAtual = false;
  menuComercialAberto = false;
  comercialAtivo = false;
  menuFinanceiroAberto = false;
  financeiroAtivo = false;
  menuFiscalAberto = false;
  fiscalAtivo = false;
  menuUtilitariosAberto = false;
  utilitariosAtivo = false;
  private readonly rotasComercial = [
    '/clientes',
    '/atendimentos',
    '/anotacoes',
    '/possiveis-clientes',
    '/envio-email'
  ];
  private readonly rotasFinanceiro = ['/contas-pagar', '/contas-receber', '/relatorios-gerenciais'];
  private readonly rotasFiscal = ['/empresas', '/plano-contas', '/fluxo-caixa'];
  private readonly rotasUtilitarios = [
    '/usuarios',
    '/cadastro-atendimento',
    '/status-atendimento-comercial'
  ];
  private readonly CHAVE_CIENCIA_ALERTAS = 'alertas_ciencia';
  private readonly CHAVE_CIENCIA_CONTRATOS_LEGADO = 'contratos_vencendo_ciencia';

  constructor(
    private authService: AuthService,
    private router: Router,
    private alertasService: AlertasService,
    private tarefaService: TarefaService,
    private dashboardService: DashboardService
  ) { }

  get alertaAtual(): AlertaItem | null {
    return this.filaAlertas[this.currentAlertaIndex] ?? null;
  }

  get showPopupAlertas(): boolean {
    return this.filaAlertas.length > 0 && this.alertaAtual !== null;
  }

  get isAdmin(): boolean {
    return this.authService.isAdministrador();
  }

  ngOnInit() {
    this.atualizarMenusLaterais();
    this.router.events.pipe(filter(e => e instanceof NavigationEnd)).subscribe(() => {
      this.atualizarMenusLaterais();
    });

    this.authService.usuario$.subscribe(usuario => {
      this.isAuthenticated = usuario !== null;
      this.usuarioNome = usuario?.nome || '';
      if (this.isAuthenticated && !this.popupAlertasMostrado) {
        this.popupAlertasMostrado = true;
        this.carregarFilaAlertas();
      }
    });
  }

  private carregarFilaAlertas() {
    const usuarioId = this.authService.getUsuarioId();
    const pendencias$ = this.alertasService.obterPendencias(30);
    const contratos$ = this.dashboardService.obterAlertasContratosVencendo(30);
    const reunioes$ = usuarioId != null
      ? this.tarefaService.listarTarefas({ usuarioId, incluirConcluidas: false })
      : null;

    if (reunioes$) {
      forkJoin({ pendencias: pendencias$, contratos: contratos$, reunioes: reunioes$ }).subscribe({
        next: ({ pendencias, contratos, reunioes }) => {
          const fila: AlertaItem[] = [];
          const cientes = this.lerCienciasAlertas();

          if (((pendencias.notasServicoPendentesMes?.length ?? 0) > 0 || (pendencias.dasPendentesOuAtrasadas?.length ?? 0) > 0) &&
            !cientes.has(this.chaveCienciaPendencias())) {
            fila.push({ tipo: 'pendencias', data: pendencias });
          }

          (contratos ?? [])
            .filter(c => !cientes.has(this.chaveCienciaContrato(c)))
            .forEach(c => fila.push({ tipo: 'contrato-vencendo', data: c }));

          const reunioesFiltradas = reunioes.filter(t => t.tipoAtendimento === TipoAtendimento.Reuniao);
          if (reunioesFiltradas.length > 0 && !cientes.has(this.chaveCienciaReunioes(reunioesFiltradas))) {
            fila.push({ tipo: 'reunioes', data: reunioesFiltradas });
          }

          this.filaAlertas = fila;
          this.currentAlertaIndex = 0;
          this.cienteAlertaAtual = false;
        }
      });
    } else {
      forkJoin({ pendencias: pendencias$, contratos: contratos$ }).subscribe({
        next: ({ pendencias, contratos }) => {
          const fila: AlertaItem[] = [];
          const cientes = this.lerCienciasAlertas();

          if (((pendencias.notasServicoPendentesMes?.length ?? 0) > 0 || (pendencias.dasPendentesOuAtrasadas?.length ?? 0) > 0) &&
            !cientes.has(this.chaveCienciaPendencias())) {
            fila.push({ tipo: 'pendencias', data: pendencias });
          }

          (contratos ?? [])
            .filter(c => !cientes.has(this.chaveCienciaContrato(c)))
            .forEach(c => fila.push({ tipo: 'contrato-vencendo', data: c }));

          this.filaAlertas = fila;
          this.currentAlertaIndex = 0;
          this.cienteAlertaAtual = false;
        }
      });
    }
  }

  onCliqueOverlayAlerta() {
    // Padronização: só avança após marcar ciência.
  }

  onCliqueFecharAlerta() {
    // Padronização: só avança após marcar ciência.
  }

  avancarAlerta() {
    this.cienteAlertaAtual = false;
    if (this.currentAlertaIndex + 1 < this.filaAlertas.length) {
      this.currentAlertaIndex++;
    } else {
      this.filaAlertas = [];
      this.currentAlertaIndex = 0;
    }
  }

  fecharAlertaPendencias(): void {
    if (this.alertaAtual?.tipo !== 'pendencias') return;
    this.avancarAlerta();
  }

  confirmarPendenciasEAvancar(): void {
    if (this.alertaAtual?.tipo !== 'pendencias') return;
    if (this.cienteAlertaAtual) {
      this.marcarCiencia(this.chaveCienciaPendencias());
    }
    this.avancarAlerta();
  }

  confirmarCienciaEAvancar() {
    if (!this.alertaAtual) return;
    if (!this.cienteAlertaAtual) return;

    if (this.alertaAtual.tipo === 'contrato-vencendo') {
      this.marcarCiencia(this.chaveCienciaContrato(this.alertaAtual.data));
    }
    if (this.alertaAtual.tipo === 'reunioes') {
      this.marcarCiencia(this.chaveCienciaReunioes(this.alertaAtual.data));
    }

    this.avancarAlerta();
  }

  private marcarCiencia(chave: string): void {
    const lista = this.lerListaCienciasAlertas();
    if (!lista.includes(chave)) {
      lista.push(chave);
      localStorage.setItem(this.CHAVE_CIENCIA_ALERTAS, JSON.stringify(lista));
    }
  }

  private lerCienciasAlertas(): Set<string> {
    const atual = this.lerListaCienciasAlertas();
    const legado = this.lerListaCienciasContratosLegado();
    return new Set([...atual, ...legado]);
  }

  private lerListaCienciasAlertas(): string[] {
    try {
      const raw = localStorage.getItem(this.CHAVE_CIENCIA_ALERTAS);
      if (!raw) return [];
      const parsed = JSON.parse(raw) as unknown;
      if (Array.isArray(parsed) && parsed.every(x => typeof x === 'string')) return parsed;
      return [];
    } catch {
      return [];
    }
  }

  private lerListaCienciasContratosLegado(): string[] {
    try {
      const raw = localStorage.getItem(this.CHAVE_CIENCIA_CONTRATOS_LEGADO);
      if (!raw) return [];
      const parsed = JSON.parse(raw) as unknown;
      if (Array.isArray(parsed) && parsed.every(x => typeof x === 'string')) return parsed;
      return [];
    } catch {
      return [];
    }
  }

  private chaveCienciaPendencias(): string {
    const agora = new Date();
    const ano = agora.getFullYear();
    const mes = (agora.getMonth() + 1).toString().padStart(2, '0');
    return `pendencias|${ano}-${mes}`;
  }

  private chaveCienciaReunioes(reunioes: TarefaResponseDto[]): string {
    const ids = (reunioes ?? [])
      .map(r => r.tarefaId)
      .filter(id => typeof id === 'number')
      .sort((a, b) => a - b)
      .join('-');
    return `reunioes|${ids}`;
  }

  private chaveCienciaContrato(alerta: AlertaContratoVencendoDto): string {
    const data = (alerta.dataFimVigencia ?? '').toString();
    return `${alerta.clienteId}|${data}`;
  }

  formatarData(data: string): string {
    return new Date(data).toLocaleDateString('pt-BR');
  }

  logout() {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  alternarMenuComercial(): void {
    this.menuComercialAberto = !this.menuComercialAberto;
  }

  alternarMenuFinanceiro(): void {
    this.menuFinanceiroAberto = !this.menuFinanceiroAberto;
  }

  alternarMenuFiscal(): void {
    this.menuFiscalAberto = !this.menuFiscalAberto;
  }

  alternarMenuUtilitarios(): void {
    this.menuUtilitariosAberto = !this.menuUtilitariosAberto;
  }

  private atualizarMenusLaterais(): void {
    const url = this.router.url.split('?')[0];
    this.comercialAtivo = this.rotasComercial.some(rota => url === rota || url.startsWith(`${rota}/`));
    this.financeiroAtivo = this.rotasFinanceiro.some(rota => url === rota || url.startsWith(`${rota}/`));
    this.fiscalAtivo = this.rotasFiscal.some(rota => url === rota || url.startsWith(`${rota}/`));
    this.utilitariosAtivo = this.rotasUtilitarios.some(rota => url === rota || url.startsWith(`${rota}/`));

    if (this.comercialAtivo) {
      this.menuComercialAberto = true;
    }
    if (this.financeiroAtivo) {
      this.menuFinanceiroAberto = true;
    }
    if (this.fiscalAtivo) {
      this.menuFiscalAberto = true;
    }
    if (this.utilitariosAtivo) {
      this.menuUtilitariosAberto = true;
    }
  }
}
