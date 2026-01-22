import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DuplicataService, DuplicataResponseDto, CadastroDuplicataDto, ParcelaResponseDto, CadastroParcelaDto } from '../../services/duplicata.service';

@Component({
  selector: 'app-contas-receber',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './contas-receber.component.html',
  styleUrl: './contas-receber.component.css'
})
export class ContasReceberComponent implements OnInit {
  duplicatas: DuplicataResponseDto[] = [];
  duplicatasFiltradas: DuplicataResponseDto[] = [];
  showForm = false;
  showParcelas = false;
  loading = false;
  error: string | null = null;
  editando = false;
  duplicataEditando: DuplicataResponseDto | null = null;
  duplicataSelecionada: DuplicataResponseDto | null = null;
  termoBusca = '';
  gerarParcelasManual = false;
  parcelasManuais: CadastroParcelaDto[] = [];
  
  // Modal de confirmação
  showConfirmModal = false;
  confirmTitle = '';
  confirmMessage = '';
  confirmCallback: (() => void) | null = null;
  
  // Modal de sucesso
  showSuccessModal = false;
  successMessage = '';

  novaDuplicata: CadastroDuplicataDto = {
    numero: 0,
    dataEmissao: new Date().toISOString().split('T')[0],
    numeroParcelas: 1,
    valorTotal: 0,
    multa: 0,
    juros: 0,
    descricaoDespesa: undefined,
    tipo: 'CR',
    dataPrimeiroVencimento: new Date().toISOString().split('T')[0]
  };

  constructor(private duplicataService: DuplicataService) { }

  ngOnInit() {
    this.carregarDuplicatas();
  }

  carregarDuplicatas() {
    this.loading = true;
    this.error = null;
    this.duplicataService.listarDuplicatasPorTipo('CR').subscribe({
      next: (data) => {
        this.duplicatas = data;
        this.duplicatasFiltradas = data;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Erro ao carregar contas a receber. Verifique se a API está rodando.';
        this.loading = false;
        console.error(err);
      }
    });
  }

  abrirFormularioNovo() {
    this.editando = false;
    this.duplicataEditando = null;
    this.gerarParcelasManual = false;
    this.parcelasManuais = [];
    
    // Buscar próximo número automaticamente
    this.duplicataService.obterProximoNumero('CR').subscribe({
      next: (proximoNumero) => {
        this.novaDuplicata = {
          numero: proximoNumero,
          dataEmissao: new Date().toISOString().split('T')[0],
          numeroParcelas: 1,
          valorTotal: 0,
          multa: 0,
          juros: 0,
          descricaoDespesa: undefined,
          tipo: 'CR',
          dataPrimeiroVencimento: new Date().toISOString().split('T')[0]
        };
        this.showForm = true;
        this.error = null;
        window.scrollTo(0, 0);
      },
      error: (err) => {
        this.error = 'Erro ao obter próximo número. Tente novamente.';
        console.error(err);
      }
    });
  }

  abrirFormularioEdicao(duplicata: DuplicataResponseDto) {
    this.editando = true;
    this.duplicataEditando = duplicata;
    this.novaDuplicata = {
      numero: duplicata.numero,
      dataEmissao: duplicata.dataEmissao.split('T')[0],
      numeroParcelas: duplicata.numeroParcelas,
      valorTotal: duplicata.valorTotal,
      multa: duplicata.parcelas[0]?.multa || 0,
      juros: duplicata.parcelas[0]?.juros || 0,
      descricaoDespesa: duplicata.descricaoDespesa,
      tipo: duplicata.tipo || 'CR',
      dataPrimeiroVencimento: duplicata.parcelas[0]?.vencimento.split('T')[0] || new Date().toISOString().split('T')[0]
    };
    this.showForm = true;
    this.error = null;
    window.scrollTo(0, 0);
  }

  fecharFormulario() {
    this.showForm = false;
    this.editando = false;
    this.duplicataEditando = null;
    this.gerarParcelasManual = false;
    this.parcelasManuais = [];
    this.error = null;
  }

  toggleGerarParcelasManual() {
    // O valor já foi atualizado pelo ngModel
    if (this.gerarParcelasManual) {
      // Ativando geração manual - gerar parcelas
      this.gerarParcelas();
    } else {
      // Desativando geração manual - limpar parcelas
      this.parcelasManuais = [];
      this.novaDuplicata.parcelas = undefined;
    }
  }

  gerarParcelas() {
    const numParcelas = this.novaDuplicata.numeroParcelas || 1;
    const valorPorParcela = this.novaDuplicata.valorTotal;
    this.parcelasManuais = [];

    for (let i = 1; i <= numParcelas; i++) {
      const dataVencimento = this.novaDuplicata.dataPrimeiroVencimento 
        ? new Date(this.novaDuplicata.dataPrimeiroVencimento)
        : new Date();
      
      // Se não for a primeira parcela, adiciona meses
      if (i > 1 && this.novaDuplicata.dataPrimeiroVencimento) {
        dataVencimento.setMonth(dataVencimento.getMonth() + (i - 1));
      }

      this.parcelasManuais.push({
        numeroParcela: i,
        valor: valorPorParcela,
        vencimento: dataVencimento.toISOString().split('T')[0],
        multa: this.novaDuplicata.multa || 0,
        juros: this.novaDuplicata.juros || 0
      });
    }
  }

  atualizarNumeroParcelas() {
    if (this.gerarParcelasManual) {
      this.gerarParcelas();
    }
  }

  atualizarValorTotal() {
    if (this.gerarParcelasManual && this.parcelasManuais.length > 0) {
      const valorPorParcela = this.novaDuplicata.valorTotal;
      this.parcelasManuais.forEach(p => p.valor = valorPorParcela);
    }
  }

  atualizarMultaJuros() {
    if (this.gerarParcelasManual && this.parcelasManuais.length > 0) {
      this.parcelasManuais.forEach(p => {
        if (this.novaDuplicata.multa !== undefined) {
          p.multa = this.novaDuplicata.multa;
        }
        if (this.novaDuplicata.juros !== undefined) {
          p.juros = this.novaDuplicata.juros;
        }
      });
    }
  }

  salvarDuplicata() {
    if (this.loading) return;

    // Validar parcelas manuais se estiver gerando manualmente
    if (this.gerarParcelasManual) {
      if (this.parcelasManuais.length !== this.novaDuplicata.numeroParcelas) {
        this.error = 'Número de parcelas não corresponde ao número informado.';
        return;
      }
      
      const todasTemData = this.parcelasManuais.every(p => p.vencimento);
      if (!todasTemData) {
        this.error = 'Todas as parcelas devem ter data de vencimento preenchida.';
        return;
      }

      this.novaDuplicata.parcelas = this.parcelasManuais;
      this.novaDuplicata.dataPrimeiroVencimento = undefined;
    } else {
      this.novaDuplicata.parcelas = undefined;
    }

    // Para novo cadastro, enviar número como 0 para gerar automaticamente
    if (!this.editando) {
      this.novaDuplicata.numero = 0;
    }

    this.loading = true;
    this.error = null;

    const operacao = this.editando && this.duplicataEditando
      ? this.duplicataService.atualizarDuplicata(this.duplicataEditando.duplicataId, this.novaDuplicata)
      : this.duplicataService.cadastrarDuplicata(this.novaDuplicata);

    operacao.subscribe({
      next: () => {
        this.carregarDuplicatas();
        this.fecharFormulario();
        this.loading = false;
        this.successMessage = this.editando ? 'Duplicata atualizada com sucesso!' : 'Duplicata cadastrada com sucesso!';
        this.showSuccessModal = true;
        setTimeout(() => {
          this.fecharSuccessModal();
        }, 3000);
      },
      error: (err) => {
        this.error = err.error?.message || 'Erro ao salvar conta a receber.';
        this.loading = false;
        console.error(err);
      }
    });
  }

  excluirDuplicata(duplicata: DuplicataResponseDto) {
    this.confirmTitle = 'Confirmar Exclusão';
    this.confirmMessage = `Tem certeza que deseja excluir a duplicata #${duplicata.numero}?`;
    this.confirmCallback = () => {
      this.loading = true;
      this.error = null;

      this.duplicataService.excluirDuplicata(duplicata.duplicataId).subscribe({
        next: () => {
          this.carregarDuplicatas();
          this.loading = false;
        },
        error: (err) => {
          this.error = err.error?.message || 'Erro ao excluir conta a receber.';
          this.loading = false;
          console.error(err);
        }
      });
    };
    this.showConfirmModal = true;
  }

  baixarParcela(parcela: ParcelaResponseDto) {
    this.confirmTitle = 'Confirmar Recebimento';
    this.confirmMessage = `Deseja receber a parcela ${parcela.numeroParcela}?`;
    this.confirmCallback = () => {
      this.loading = true;
      this.error = null;

      this.duplicataService.baixarParcela(parcela.parcelaId).subscribe({
        next: () => {
          this.carregarDuplicatas();
          if (this.duplicataSelecionada) {
            const duplicataAtualizada = this.duplicatas.find(d => d.duplicataId === this.duplicataSelecionada!.duplicataId);
            if (duplicataAtualizada) {
              this.duplicataSelecionada = duplicataAtualizada;
            }
          }
          this.loading = false;
        },
        error: (err) => {
          this.error = err.error?.message || 'Erro ao receber parcela.';
          this.loading = false;
          console.error(err);
        }
      });
    };
    this.showConfirmModal = true;
  }

  reativarParcela(parcela: ParcelaResponseDto) {
    this.confirmTitle = 'Confirmar Reativação';
    this.confirmMessage = `Deseja reativar a parcela ${parcela.numeroParcela}? A parcela voltará para o status "Pendente".`;
    this.confirmCallback = () => {
      this.loading = true;
      this.error = null;

      this.duplicataService.reativarParcela(parcela.parcelaId).subscribe({
        next: () => {
          this.carregarDuplicatas();
          if (this.duplicataSelecionada) {
            const duplicataAtualizada = this.duplicatas.find(d => d.duplicataId === this.duplicataSelecionada!.duplicataId);
            if (duplicataAtualizada) {
              this.duplicataSelecionada = duplicataAtualizada;
            }
          }
          this.loading = false;
        },
        error: (err) => {
          this.error = err.error?.message || 'Erro ao reativar parcela.';
          this.loading = false;
          console.error(err);
        }
      });
    };
    this.showConfirmModal = true;
  }

  confirmarAcao() {
    if (this.confirmCallback) {
      this.confirmCallback();
    }
    this.fecharConfirmModal();
  }

  fecharConfirmModal() {
    this.showConfirmModal = false;
    this.confirmTitle = '';
    this.confirmMessage = '';
    this.confirmCallback = null;
  }

  fecharSuccessModal() {
    this.showSuccessModal = false;
    this.successMessage = '';
  }

  abrirParcelas(duplicata: DuplicataResponseDto) {
    this.duplicataSelecionada = duplicata;
    this.showParcelas = true;
  }

  fecharParcelas() {
    this.showParcelas = false;
    this.duplicataSelecionada = null;
  }

  filtrarDuplicatas() {
    if (!this.termoBusca.trim()) {
      this.duplicatasFiltradas = this.duplicatas;
      return;
    }

    const termo = this.termoBusca.toLowerCase();
    this.duplicatasFiltradas = this.duplicatas.filter(d =>
      d.numero.toString().includes(termo) ||
      d.dataEmissao.toLowerCase().includes(termo)
    );
  }

  formatarMoeda(valor: number): string {
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: 'BRL'
    }).format(valor);
  }

  formatarData(data: string): string {
    return new Date(data).toLocaleDateString('pt-BR');
  }

  getStatusClass(status: string): string {
    switch (status.toLowerCase()) {
      case 'paga':
        return 'status-paga';
      case 'pendente':
        return 'status-pendente';
      case 'cancelada':
        return 'status-cancelada';
      default:
        return '';
    }
  }
}
