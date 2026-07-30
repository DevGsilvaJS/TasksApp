import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DashboardService, DashboardEstatisticasDto, PeriodoFiltro, AtendimentoPorUsuarioDto, ContaAPagarDto, AtendimentoPorClienteDto, AtendimentoPorClienteMesDto, ValorPorMesPorUsuarioDto, TelemarketingContatosDto, AlertaContratoVencendoDto } from '../../services/dashboard.service';
import { NotaServicoService, NotaServicoItemDto } from '../../services/nota-servico.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent implements OnInit {
  estatisticas: DashboardEstatisticasDto | null = null;
  periodoSelecionado: PeriodoFiltro = PeriodoFiltro.Dia;
  loading = false;
  error: string | null = null;
  alertasContratosVencendo: AlertaContratoVencendoDto[] = [];
  carregandoAlertasContratosVencendo = false;
  erroAlertasContratosVencendo: string | null = null;

  /** Valores exibidos com animação de count-up (iniciam em 0 e sobem até o valor real) */
  totalAtendimentosPorUsuarioDisplay = 0;
  atendimentosPorClienteMesCountDisplay = 0;
  mediaDiariaAtendimentosDisplay = 0;
  totalContasAReceberDisplay = 0;
  totalContasRecebidasDisplay = 0;
  lucroDisplay = 0;
  totalContasAPagarDisplay = 0;
  totalContasPagasDisplay = 0;
  contatosNoDiaDisplay = 0;
  contatosSemanaAtualDisplay = 0;
  contatosMesAtualDisplay = 0;
  contatosAnoAtualDisplay = 0;
  notasPendentesDisplay = 0;
  notasTotalDisplay = 0;
  valoresPorMesNumeroDisplay = 0;

  PeriodoFiltro = PeriodoFiltro;
  
  // Modais
  showModalAtendimentosUsuario = false;
  showModalContasPagar = false;
  showModalContasPagas = false;
  showModalContasAReceber = false;
  showModalContasRecebidas = false;
  showModalAtendimentosCliente = false;
  showModalAtendimentosClienteMes = false;
  showModalValoresPorMes = false;
  showModalDRE = false;
  showModalNotasServico = false;
  showModalContratosVencendo = false;

  dadosModalNotasServico: NotaServicoItemDto[] = [];
  loadingNotasServico = false;
  mesAnoNotas = { ano: new Date().getFullYear(), mes: new Date().getMonth() + 1 };
  /** Item em edição para marcar data de envio (clienteId, ano, mes) */
  itemEmEnvio: { clienteId: number; ano: number; mes: number } | null = null;
  dataEnvioEscolhida = '';
  salvandoEnvio = false;
  
  dadosModalAtendimentosUsuario: AtendimentoPorUsuarioDto[] = [];
  dadosModalContasPagar: ContaAPagarDto[] = [];
  dadosModalContasPagas: ContaAPagarDto[] = [];
  dadosModalContasAReceber: ContaAPagarDto[] = [];
  dadosModalContasRecebidas: ContaAPagarDto[] = [];
  dadosModalAtendimentosCliente: AtendimentoPorClienteDto[] = [];
  dadosModalAtendimentosClienteMes: AtendimentoPorClienteMesDto[] = [];
  dadosModalValoresPorMes: ValorPorMesPorUsuarioDto[] = [];
  
  anoSelecionado: number = new Date().getFullYear();
  contatosTelemarketing: TelemarketingContatosDto | null = null;

  constructor(
    private dashboardService: DashboardService,
    private notaServicoService: NotaServicoService
  ) { }

  ngOnInit() {
    this.carregarEstatisticas();
    this.carregarValoresPorMes();
    this.carregarNotasServico();
    this.carregarContatosTelemarketing();
    this.carregarAlertasContratosVencendo();
  }

  carregarAlertasContratosVencendo() {
    this.carregandoAlertasContratosVencendo = true;
    this.erroAlertasContratosVencendo = null;
    this.dashboardService.obterAlertasContratosVencendo(30).subscribe({
      next: (data) => {
        this.alertasContratosVencendo = data ?? [];
        this.carregandoAlertasContratosVencendo = false;
      },
      error: () => {
        this.alertasContratosVencendo = [];
        this.erroAlertasContratosVencendo = 'Não foi possível carregar os contratos vencendo.';
        this.carregandoAlertasContratosVencendo = false;
      }
    });
  }

  abrirModalContratosVencendo() {
    this.showModalContratosVencendo = true;
  }

  fecharModalContratosVencendo() {
    this.showModalContratosVencendo = false;
  }

  carregarContatosTelemarketing() {
    this.dashboardService.obterContatosTelemarketing().subscribe({
      next: (data) => {
        this.contatosTelemarketing = data;
        const D = 600;
        this.animarValor(0, data?.contatosNoDia ?? 0, D, v => this.contatosNoDiaDisplay = v);
        this.animarValor(0, data?.contatosSemanaAtual ?? 0, D, v => this.contatosSemanaAtualDisplay = v);
        this.animarValor(0, data?.contatosMesAtual ?? 0, D, v => this.contatosMesAtualDisplay = v);
        this.animarValor(0, data?.contatosAnoAtual ?? 0, D, v => this.contatosAnoAtualDisplay = v);
      },
      error: () => {
        this.contatosTelemarketing = { contatosNoDia: 0, contatosSemanaAtual: 0, contatosMesAtual: 0, contatosAnoAtual: 0 };
      }
    });
  }

  carregarValoresPorMes() {
    this.dashboardService.obterValoresPorMesPorUsuario(this.anoSelecionado).subscribe({
      next: (data) => {
        const mesAtual = new Date().getMonth() + 1;
        this.dadosModalValoresPorMes = data.filter(item => item.mes === mesAtual);
        const total = this.dadosModalValoresPorMes.reduce((sum, item) => sum + (item.valorTotal || 0), 0);
        this.animarValor(0, total, 600, v => this.valoresPorMesNumeroDisplay = v, false);
      },
      error: (err) => {
        console.error('Erro ao carregar valores por mês:', err);
      }
    });
  }

  onAnoChange() {
    this.carregarValoresPorMes();
  }

  onPeriodoChange() {
    this.carregarEstatisticas();
  }

  carregarEstatisticas() {
    this.loading = true;
    this.error = null;
    this.zerarDisplays();

    const { dataInicio, dataFim } = this.obterDatasPeriodo();

    this.dashboardService.obterEstatisticas(dataInicio, dataFim).subscribe({
      next: (data) => {
        this.estatisticas = data;
        if (!this.estatisticas.atendimentosPorCliente) {
          this.estatisticas.atendimentosPorCliente = [];
        }
        if (!this.estatisticas.atendimentosPorClienteMes) {
          this.estatisticas.atendimentosPorClienteMes = [];
        }
        this.loading = false;
        this.animarValoresEstatisticas();
      },
      error: (err) => {
        console.error('Erro ao carregar estatísticas:', err);
        this.error = 'Erro ao carregar estatísticas';
        this.loading = false;
      }
    });
  }

  private zerarDisplays() {
    this.totalAtendimentosPorUsuarioDisplay = 0;
    this.atendimentosPorClienteMesCountDisplay = 0;
    this.mediaDiariaAtendimentosDisplay = 0;
    this.totalContasAReceberDisplay = 0;
    this.totalContasRecebidasDisplay = 0;
    this.lucroDisplay = 0;
    this.totalContasAPagarDisplay = 0;
    this.totalContasPagasDisplay = 0;
  }

  /** Animação de count-up: easeOutCubic, duração em ms. Se round=true, usa inteiros; senão usa decimais (ex.: lucro). */
  private animarValor(inicio: number, fim: number, duracaoMs: number, callback: (v: number) => void, round = true): void {
    if (inicio === fim) {
      callback(fim);
      return;
    }
    const startTime = performance.now();
    const diff = fim - inicio;
    const easeOutCubic = (t: number) => 1 - Math.pow(1 - t, 3);

    const step = (now: number) => {
      const elapsed = now - startTime;
      const progress = Math.min(elapsed / duracaoMs, 1);
      const eased = easeOutCubic(progress);
      const current = round ? Math.round(inicio + diff * eased) : inicio + diff * eased;
      callback(current);
      if (progress < 1) requestAnimationFrame(step);
      else callback(fim);
    };
    requestAnimationFrame(step);
  }

  private animarValoresEstatisticas() {
    const e = this.estatisticas!;
    const DURATION = 700;
    this.animarValor(0, e.totalAtendimentosPorUsuario ?? 0, DURATION, v => this.totalAtendimentosPorUsuarioDisplay = v);
    const totalAtendimentosCliente = (e.atendimentosPorClienteMes ?? []).reduce((sum, item) => sum + (item.quantidade ?? 0), 0);
    this.animarValor(0, totalAtendimentosCliente, DURATION, v => this.atendimentosPorClienteMesCountDisplay = v);
    this.animarValor(0, e.mediaDiariaAtendimentos ?? 0, DURATION, v => this.mediaDiariaAtendimentosDisplay = v);
    this.animarValor(0, e.valorTotalContasAReceber ?? 0, DURATION, v => this.totalContasAReceberDisplay = v, false);
    this.animarValor(0, e.valorTotalContasRecebidas ?? 0, DURATION, v => this.totalContasRecebidasDisplay = v, false);
    this.animarValor(0, e.valorTotalContasAPagar ?? 0, DURATION, v => this.totalContasAPagarDisplay = v, false);
    this.animarValor(0, e.valorTotalContasPagas ?? 0, DURATION, v => this.totalContasPagasDisplay = v, false);
    this.animarValor(0, e.lucro ?? 0, DURATION, v => this.lucroDisplay = v, false);
  }

  obterDatasPeriodo(): { dataInicio: Date; dataFim: Date } {
    const hoje = new Date();
    hoje.setHours(0, 0, 0, 0);

    switch (this.periodoSelecionado) {
      case PeriodoFiltro.Dia:
        return {
          dataInicio: new Date(hoje),
          dataFim: new Date(hoje)
        };
      
      case PeriodoFiltro.Semana:
        const inicioSemana = new Date(hoje);
        inicioSemana.setDate(hoje.getDate() - 6); // Últimos 7 dias (incluindo hoje)
        return {
          dataInicio: inicioSemana,
          dataFim: new Date(hoje)
        };
      
      case PeriodoFiltro.Mes:
        const inicioMes = new Date(hoje.getFullYear(), hoje.getMonth(), 1);
        return {
          dataInicio: inicioMes,
          dataFim: new Date(hoje)
        };
      
      default:
        return {
          dataInicio: new Date(hoje),
          dataFim: new Date(hoje)
        };
    }
  }

  abrirModalAtendimentosUsuario() {
    if (this.estatisticas) {
      this.dadosModalAtendimentosUsuario = this.estatisticas.atendimentosPorUsuario;
      this.showModalAtendimentosUsuario = true;
    }
  }

  fecharModalAtendimentosUsuario() {
    this.showModalAtendimentosUsuario = false;
  }

  abrirModalContasPagar() {
    if (this.estatisticas) {
      this.dadosModalContasPagar = this.estatisticas.contasAPagar;
      this.showModalContasPagar = true;
    }
  }

  fecharModalContasPagar() {
    this.showModalContasPagar = false;
  }

  abrirModalContasPagas() {
    if (this.estatisticas) {
      this.dadosModalContasPagas = this.estatisticas.contasPagas;
      this.showModalContasPagas = true;
    }
  }

  fecharModalContasPagas() {
    this.showModalContasPagas = false;
  }

  abrirModalContasAReceber() {
    if (this.estatisticas) {
      this.dadosModalContasAReceber = this.estatisticas.contasAReceber;
      this.showModalContasAReceber = true;
    }
  }

  fecharModalContasAReceber() {
    this.showModalContasAReceber = false;
  }

  abrirModalContasRecebidas() {
    if (this.estatisticas) {
      this.dadosModalContasRecebidas = this.estatisticas.contasRecebidas;
      this.showModalContasRecebidas = true;
    }
  }

  fecharModalContasRecebidas() {
    this.showModalContasRecebidas = false;
  }

  abrirModalAtendimentosClienteMes() {
    if (this.estatisticas) {
      this.dadosModalAtendimentosClienteMes = this.estatisticas.atendimentosPorClienteMes;
      this.showModalAtendimentosClienteMes = true;
    }
  }

  fecharModalAtendimentosClienteMes() {
    this.showModalAtendimentosClienteMes = false;
  }

  formatarPercentual(percentual: number): string {
    return percentual.toFixed(1).replace('.', ',') + '%';
  }

  abrirModalAtendimentosCliente() {
    if (this.estatisticas && this.estatisticas.atendimentosPorCliente) {
      this.dadosModalAtendimentosCliente = this.estatisticas.atendimentosPorCliente;
      this.showModalAtendimentosCliente = true;
    } else {
      this.dadosModalAtendimentosCliente = [];
      this.showModalAtendimentosCliente = true;
    }
  }

  fecharModalAtendimentosCliente() {
    this.showModalAtendimentosCliente = false;
  }

  abrirModalDRE() {
    this.showModalDRE = true;
  }

  fecharModalDRE() {
    this.showModalDRE = false;
  }

  abrirModalValoresPorMes() {
    this.showModalValoresPorMes = true;
  }

  fecharModalValoresPorMes() {
    this.showModalValoresPorMes = false;
  }

  carregarNotasServico() {
    this.loadingNotasServico = true;
    const { ano, mes } = this.mesAnoNotas;
    this.notaServicoService.listarNotasDoMes(ano, mes).subscribe({
      next: (data) => {
        this.dadosModalNotasServico = data;
        this.loadingNotasServico = false;
        const pendentes = data.filter(n => !n.enviado).length;
        const D = 500;
        this.animarValor(0, pendentes, D, v => this.notasPendentesDisplay = v);
        this.animarValor(0, data.length, D, v => this.notasTotalDisplay = v);
      },
      error: () => {
        this.loadingNotasServico = false;
      }
    });
  }

  abrirModalNotasServico() {
    this.showModalNotasServico = true;
    this.carregarNotasServico();
  }

  fecharModalNotasServico() {
    this.showModalNotasServico = false;
    this.itemEmEnvio = null;
    this.dataEnvioEscolhida = '';
  }

  quantidadeNotasPendentes(): number {
    return this.dadosModalNotasServico.filter(n => !n.enviado).length;
  }

  abrirFormEnvio(item: NotaServicoItemDto) {
    if (item.enviado) return;
    this.itemEmEnvio = { clienteId: item.clienteId, ano: item.ano, mes: item.mes };
    this.dataEnvioEscolhida = new Date().toISOString().split('T')[0];
  }

  cancelarFormEnvio() {
    this.itemEmEnvio = null;
    this.dataEnvioEscolhida = '';
  }

  isItemEmEnvio(item: NotaServicoItemDto): boolean {
    return this.itemEmEnvio !== null &&
      this.itemEmEnvio.clienteId === item.clienteId &&
      this.itemEmEnvio.ano === item.ano &&
      this.itemEmEnvio.mes === item.mes;
  }

  confirmarEnvioNota() {
    if (!this.itemEmEnvio || !this.dataEnvioEscolhida) return;
    this.salvandoEnvio = true;
    this.notaServicoService.marcarComoEnviado(
      this.itemEmEnvio.clienteId,
      this.itemEmEnvio.ano,
      this.itemEmEnvio.mes,
      this.dataEnvioEscolhida
    ).subscribe({
      next: () => {
        this.salvandoEnvio = false;
        this.itemEmEnvio = null;
        this.dataEnvioEscolhida = '';
        this.carregarNotasServico();
      },
      error: () => { this.salvandoEnvio = false; }
    });
  }

  maxDataEnvio(): string {
    return new Date().toISOString().split('T')[0];
  }

  formatarData(data: string): string {
    if (!data) {
      return '-';
    }

    const parte = data.length >= 10 ? data.substring(0, 10) : data;
    if (/^\d{4}-\d{2}-\d{2}$/.test(parte)) {
      const [ano, mes, dia] = parte.split('-');
      return `${dia}/${mes}/${ano}`;
    }

    return new Date(data).toLocaleDateString('pt-BR');
  }

  formatarMoeda(valor: number): string {
    if (valor == null || isNaN(valor)) {
      valor = 0;
    }
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: 'BRL'
    }).format(valor);
  }

  calcularTotalValores(): string {
    if (!this.dadosModalValoresPorMes || this.dadosModalValoresPorMes.length === 0) {
      return 'R$ 0,00';
    }
    const total = this.dadosModalValoresPorMes.reduce((sum, item) => sum + (item.valorTotal || 0), 0);
    return this.formatarMoeda(total);
  }

  /** Valor animado do card "Valores por Mês" */
  valoresPorMesDisplay(): string {
    return this.formatarMoeda(this.valoresPorMesNumeroDisplay);
  }

  formatarDataCurta(dataIso: string): string {
    if (!dataIso) return '-';
    const d = new Date(dataIso);
    return d.toLocaleDateString('pt-BR');
  }
}
