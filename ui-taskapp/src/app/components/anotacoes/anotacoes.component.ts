import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AnotacaoGeralService, AnotacaoGeralResponseDto, CadastroAnotacaoGeralDto } from '../../services/anotacao-geral.service';

@Component({
  selector: 'app-anotacoes',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './anotacoes.component.html',
  styleUrl: './anotacoes.component.css'
})
export class AnotacoesComponent implements OnInit {
  anotacoes: AnotacaoGeralResponseDto[] = [];
  anotacoesFiltradas: AnotacaoGeralResponseDto[] = [];
  showForm = false;
  loading = false;
  error: string | null = null;
  editando = false;
  anotacaoEditando: AnotacaoGeralResponseDto | null = null;
  termoBusca = '';

  novaAnotacao: CadastroAnotacaoGeralDto = {
    descricao: '',
    link: ''
  };

  // Modais
  showConfirmModal = false;
  confirmTitle = '';
  confirmMessage = '';
  confirmCallback: (() => void) | null = null;

  showSuccessModal = false;
  successMessage = '';

  constructor(private anotacaoGeralService: AnotacaoGeralService) { }

  ngOnInit() {
    this.carregarAnotacoes();
  }

  carregarAnotacoes() {
    this.loading = true;
    this.error = null;

    this.anotacaoGeralService.listarTodasAnotacoes().subscribe({
      next: (data) => {
        this.anotacoes = data;
        this.anotacoesFiltradas = data;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Erro ao carregar anotações. Verifique se a API está rodando.';
        this.loading = false;
        console.error(err);
      }
    });
  }

  abrirFormularioNovo() {
    this.editando = false;
    this.anotacaoEditando = null;
    this.novaAnotacao = {
      descricao: '',
      link: ''
    };
    this.showForm = true;
    this.error = null;
    window.scrollTo(0, 0);
  }

  abrirFormularioEdicao(anotacao: AnotacaoGeralResponseDto) {
    this.editando = true;
    this.anotacaoEditando = anotacao;
    this.novaAnotacao = {
      descricao: anotacao.descricao,
      link: anotacao.link || ''
    };
    this.showForm = true;
    this.error = null;
    window.scrollTo(0, 0);
  }

  fecharFormulario() {
    this.showForm = false;
    this.editando = false;
    this.anotacaoEditando = null;
    this.novaAnotacao = {
      descricao: '',
      link: ''
    };
    this.error = null;
  }

  salvarAnotacao() {
    if (this.loading) return;

    if (!this.novaAnotacao.descricao.trim()) {
      this.error = 'A descrição é obrigatória.';
      return;
    }

    this.loading = true;
    this.error = null;

    const operacao = this.editando && this.anotacaoEditando
      ? this.anotacaoGeralService.atualizarAnotacao(this.anotacaoEditando.anotacaoId, this.novaAnotacao)
      : this.anotacaoGeralService.cadastrarAnotacao(this.novaAnotacao);

    operacao.subscribe({
      next: () => {
        this.carregarAnotacoes();
        this.fecharFormulario();
        this.loading = false;
        this.successMessage = this.editando ? 'Anotação atualizada com sucesso!' : 'Anotação cadastrada com sucesso!';
        this.showSuccessModal = true;
        setTimeout(() => {
          this.fecharSuccessModal();
        }, 3000);
      },
      error: (err) => {
        this.error = err.error?.message || 'Erro ao salvar anotação.';
        this.loading = false;
        console.error(err);
      }
    });
  }

  excluirAnotacao(anotacao: AnotacaoGeralResponseDto) {
    this.confirmTitle = 'Confirmar Exclusão';
    this.confirmMessage = `Tem certeza que deseja excluir esta anotação?`;
    this.confirmCallback = () => {
      this.loading = true;
      this.anotacaoGeralService.excluirAnotacao(anotacao.anotacaoId).subscribe({
        next: () => {
          this.carregarAnotacoes();
          this.loading = false;
        },
        error: (err) => {
          this.error = err.error?.message || 'Erro ao excluir anotação.';
          this.loading = false;
          console.error(err);
        }
      });
    };
    this.showConfirmModal = true;
  }

  filtrarAnotacoes() {
    if (!this.termoBusca.trim()) {
      this.anotacoesFiltradas = this.anotacoes;
      return;
    }

    const termo = this.termoBusca.toLowerCase();
    this.anotacoesFiltradas = this.anotacoes.filter(a =>
      a.descricao.toLowerCase().includes(termo) ||
      (a.link && a.link.toLowerCase().includes(termo))
    );
  }

  formatarData(data?: string): string {
    if (!data) return '-';
    const date = new Date(data);
    return date.toLocaleDateString('pt-BR');
  }

  abrirLink(link?: string) {
    if (link) {
      // Verificar se o link já tem http:// ou https://
      const url = link.startsWith('http://') || link.startsWith('https://') 
        ? link 
        : `https://${link}`;
      window.open(url, '_blank');
    }
  }

  confirmarAcao() {
    if (this.confirmCallback) {
      this.confirmCallback();
      this.confirmCallback = null;
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
}
