import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  CadastroAtendimentoService,
  StatusAtendimentoComercialDto,
  StatusAtendimentoComercialRequestDto
} from '../../services/cadastro-atendimento.service';

@Component({
  selector: 'app-status-atendimento-comercial',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './status-atendimento-comercial.component.html',
  styleUrl: './status-atendimento-comercial.component.css'
})
export class StatusAtendimentoComercialComponent implements OnInit {
  lista: StatusAtendimentoComercialDto[] = [];
  loading = false;
  error: string | null = null;
  saving = false;

  showModal = false;
  editandoId: number | null = null;
  form: StatusAtendimentoComercialRequestDto = { descricao: '', ativo: true };

  showModalExcluir = false;
  itemParaExcluir: StatusAtendimentoComercialDto | null = null;
  excluindo = false;

  constructor(private service: CadastroAtendimentoService) {}

  ngOnInit() {
    this.carregar();
  }

  carregar() {
    this.loading = true;
    this.error = null;
    this.service.listarStatusAtendimentoComercial().subscribe({
      next: (data) => {
        this.lista = data;
        this.loading = false;
      },
      error: (err) => {
        this.error = err.error?.message || 'Erro ao carregar status de atendimento comercial.';
        this.loading = false;
      }
    });
  }

  abrirNovo() {
    this.editandoId = null;
    this.form = { descricao: '', ativo: true };
    this.showModal = true;
    this.error = null;
  }

  abrirEditar(item: StatusAtendimentoComercialDto) {
    this.editandoId = item.id;
    this.form = { descricao: item.descricao, ativo: item.ativo };
    this.showModal = true;
    this.error = null;
  }

  fecharModal() {
    this.showModal = false;
    this.editandoId = null;
    this.error = null;
  }

  fecharModalErro() {
    this.error = null;
  }

  salvar() {
    if (!this.form.descricao?.trim()) {
      this.error = 'Descrição é obrigatória.';
      return;
    }
    this.saving = true;
    this.error = null;
    const dto = { descricao: this.form.descricao.trim(), ativo: this.form.ativo };
    const obs = this.editandoId != null
      ? this.service.atualizarStatusAtendimentoComercial(this.editandoId, dto)
      : this.service.criarStatusAtendimentoComercial(dto);
    obs.subscribe({
      next: () => {
        this.carregar();
        this.fecharModal();
        this.saving = false;
      },
      error: (err) => {
        this.error = err.error?.message || 'Erro ao salvar.';
        this.saving = false;
      }
    });
  }

  alterarAtivo(item: StatusAtendimentoComercialDto) {
    this.service.alterarAtivoStatusAtendimentoComercial(item.id, !item.ativo).subscribe({
      next: () => this.carregar(),
      error: (err) => {
        this.error = err.error?.message || 'Erro ao alterar status.';
      }
    });
  }

  abrirModalExcluir(item: StatusAtendimentoComercialDto) {
    this.itemParaExcluir = item;
    this.showModalExcluir = true;
    this.error = null;
  }

  fecharModalExcluir() {
    this.showModalExcluir = false;
    this.itemParaExcluir = null;
    this.error = null;
  }

  confirmarExcluir() {
    if (!this.itemParaExcluir) return;
    this.excluindo = true;
    this.error = null;
    this.service.excluirStatusAtendimentoComercial(this.itemParaExcluir.id).subscribe({
      next: () => {
        this.carregar();
        this.fecharModalExcluir();
        this.excluindo = false;
      },
      error: (err) => {
        this.error = err.error?.message || 'Erro ao excluir.';
        this.excluindo = false;
      }
    });
  }
}
