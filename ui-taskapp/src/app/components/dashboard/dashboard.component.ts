import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DashboardService, DashboardEstatisticasDto, PeriodoFiltro, AtendimentoPorUsuarioDto, ContaAPagarDto, AtendimentoPorClienteDto, AtendimentoPorClienteMesDto, ValorPorMesPorUsuarioDto, TelemarketingContatosDto } from '../../services/dashboard.service';
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
  
  PeriodoFiltro = PeriodoFiltro;
  
  // Modais
  showModalAtendimentosUsuario = false;
  showModalContasPagar = false;
  showModalContasPagas = false;
  showModalContasPagarPagas = false;
  showModalContasReceberRecebidas = false;
  showModalAtendimentosCliente = false;
  showModalAtendimentosClienteMes = false;
  showModalValoresPorMes = false;
  showModalDRE = false;
  showModalNotasServico = false;

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
  abaContasSelecionada: 'pagar' | 'pagas' = 'pagar';
  abaContasReceberSelecionada: 'receber' | 'recebidas' = 'receber';

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
  }

  carregarContatosTelemarketing() {
    this.dashboardService.obterContatosTelemarketing().subscribe({
      next: (data) => { this.contatosTelemarketing = data; },
      error: () => { this.contatosTelemarketing = { contatosNoDia: 0, contatosSemanaAtual: 0, contatosMesAtual: 0, contatosAnoAtual: 0 }; }
    });
  }

  carregarValoresPorMes() {
    this.dashboardService.obterValoresPorMesPorUsuario(this.anoSelecionado).subscribe({
      next: (data) => {
        // Filtrar apenas o mês atual
        const mesAtual = new Date().getMonth() + 1; // getMonth() retorna 0-11, então +1
        this.dadosModalValoresPorMes = data.filter(item => item.mes === mesAtual);
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

    const { dataInicio, dataFim } = this.obterDatasPeriodo();

    this.dashboardService.obterEstatisticas(dataInicio, dataFim).subscribe({
      next: (data) => {
        this.estatisticas = data;
        // Garantir que as listas existam
        if (!this.estatisticas.atendimentosPorCliente) {
          this.estatisticas.atendimentosPorCliente = [];
        }
        if (!this.estatisticas.atendimentosPorClienteMes) {
          this.estatisticas.atendimentosPorClienteMes = [];
        }
        this.loading = false;
      },
      error: (err) => {
        console.error('Erro ao carregar estatísticas:', err);
        this.error = 'Erro ao carregar estatísticas';
        this.loading = false;
      }
    });
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

  abrirModalContasPagarPagas() {
    if (this.estatisticas) {
      this.dadosModalContasPagar = this.estatisticas.contasAPagar;
      this.dadosModalContasPagas = this.estatisticas.contasPagas;
      this.showModalContasPagarPagas = true;
    }
  }

  fecharModalContasPagarPagas() {
    this.showModalContasPagarPagas = false;
  }

  abrirModalContasReceberRecebidas() {
    if (this.estatisticas) {
      this.dadosModalContasAReceber = this.estatisticas.contasAReceber;
      this.dadosModalContasRecebidas = this.estatisticas.contasRecebidas;
      this.showModalContasReceberRecebidas = true;
    }
  }

  fecharModalContasReceberRecebidas() {
    this.showModalContasReceberRecebidas = false;
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
    const date = new Date(data);
    return date.toLocaleDateString('pt-BR');
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
}
