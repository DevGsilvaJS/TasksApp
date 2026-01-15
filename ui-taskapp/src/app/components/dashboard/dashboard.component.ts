import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DashboardService, DashboardEstatisticasDto, PeriodoFiltro, AtendimentoPorUsuarioDto, ContaAPagarDto, AtendimentoPorClienteDto, ValorPorMesPorUsuarioDto } from '../../services/dashboard.service';

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
  showModalAtendimentosCliente = false;
  showModalValoresPorMes = false;
  
  dadosModalAtendimentosUsuario: AtendimentoPorUsuarioDto[] = [];
  dadosModalContasPagar: ContaAPagarDto[] = [];
  dadosModalContasPagas: ContaAPagarDto[] = [];
  dadosModalAtendimentosCliente: AtendimentoPorClienteDto[] = [];
  dadosModalValoresPorMes: ValorPorMesPorUsuarioDto[] = [];
  
  anoSelecionado: number = new Date().getFullYear();
  abaContasSelecionada: 'pagar' | 'pagas' = 'pagar';

  constructor(private dashboardService: DashboardService) { }

  ngOnInit() {
    this.carregarEstatisticas();
    this.carregarValoresPorMes();
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

  abrirModalAtendimentosCliente() {
    if (this.estatisticas) {
      this.dadosModalAtendimentosCliente = this.estatisticas.atendimentosPorCliente;
      this.showModalAtendimentosCliente = true;
    }
  }

  fecharModalAtendimentosCliente() {
    this.showModalAtendimentosCliente = false;
  }

  abrirModalValoresPorMes() {
    this.showModalValoresPorMes = true;
  }

  fecharModalValoresPorMes() {
    this.showModalValoresPorMes = false;
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
    const total = this.dadosModalValoresPorMes.reduce((sum, item) => sum + item.valorTotal, 0);
    return this.formatarMoeda(total);
  }
}
