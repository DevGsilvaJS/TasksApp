import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  CadastroAtendimentoService,
  CadastroItemDto,
  CadastroItemRequestDto
} from '../../services/cadastro-atendimento.service';

type TipoCadastro = 'status' | 'tipoAtendimento' | 'tipoContato';

@Component({
  selector: 'app-cadastro-atendimento',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './cadastro-atendimento.component.html',
  styleUrl: './cadastro-atendimento.component.css'
})
export class CadastroAtendimentoComponent implements OnInit {
  statusList: CadastroItemDto[] = [];
  tipoAtendimentoList: CadastroItemDto[] = [];
  tipoContatoList: CadastroItemDto[] = [];

  loading = false;
  error: string | null = null;
  saving = false;

  showModal = false;
  modalTipo: TipoCadastro | null = null;
  editandoId: number | null = null;
  form: CadastroItemRequestDto = { descricao: '', ativo: true };

  constructor(private service: CadastroAtendimentoService) {}

  ngOnInit() {
    this.carregarTudo();
  }

  carregarTudo() {
    this.loading = true;
    this.error = null;
    this.service.listarStatus().subscribe({
      next: (data) => { this.statusList = data; },
      error: (err) => { this.trataErro(err, 'status'); }
    });
    this.service.listarTipoAtendimento().subscribe({
      next: (data) => { this.tipoAtendimentoList = data; },
      error: (err) => { this.trataErro(err, 'tipo atendimento'); }
    });
    this.service.listarTipoContato().subscribe({
      next: (data) => {
        this.tipoContatoList = data;
        this.loading = false;
      },
      error: (err) => { this.trataErro(err, 'tipo contato'); }
    });
  }

  private trataErro(err: any, contexto: string) {
    this.error = err.error?.message || `Erro ao carregar ${contexto}.`;
    this.loading = false;
  }

  abrirNovo(tipo: TipoCadastro) {
    this.modalTipo = tipo;
    this.editandoId = null;
    this.form = { descricao: '', ativo: true };
    this.showModal = true;
    this.error = null;
  }

  abrirEditar(tipo: TipoCadastro, item: CadastroItemDto) {
    this.modalTipo = tipo;
    this.editandoId = item.id;
    this.form = { descricao: item.descricao, ativo: item.ativo };
    this.showModal = true;
    this.error = null;
  }

  fecharModal() {
    this.showModal = false;
    this.modalTipo = null;
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
    if (!this.modalTipo) return;
    this.saving = true;
    this.error = null;
    const dto: CadastroItemRequestDto = { descricao: this.form.descricao.trim(), ativo: this.form.ativo };

    const obs = this.editandoId != null
      ? this.atualizar(this.modalTipo, this.editandoId, dto)
      : this.criar(this.modalTipo, dto);

    obs.subscribe({
      next: () => {
        this.carregarTudo();
        this.fecharModal();
        this.saving = false;
      },
      error: (err) => {
        this.error = err.error?.message || 'Erro ao salvar.';
        this.saving = false;
      }
    });
  }

  private criar(tipo: TipoCadastro, dto: CadastroItemRequestDto) {
    if (tipo === 'status') return this.service.criarStatus(dto);
    if (tipo === 'tipoAtendimento') return this.service.criarTipoAtendimento(dto);
    return this.service.criarTipoContato(dto);
  }

  private atualizar(tipo: TipoCadastro, id: number, dto: CadastroItemRequestDto) {
    if (tipo === 'status') return this.service.atualizarStatus(id, dto);
    if (tipo === 'tipoAtendimento') return this.service.atualizarTipoAtendimento(id, dto);
    return this.service.atualizarTipoContato(id, dto);
  }

  alterarAtivo(tipo: TipoCadastro, item: CadastroItemDto) {
    const novoAtivo = !item.ativo;
    const obs = tipo === 'status'
      ? this.service.alterarAtivoStatus(item.id, novoAtivo)
      : tipo === 'tipoAtendimento'
        ? this.service.alterarAtivoTipoAtendimento(item.id, novoAtivo)
        : this.service.alterarAtivoTipoContato(item.id, novoAtivo);
    obs.subscribe({
      next: () => this.carregarTudo(),
      error: (err) => {
        this.error = err.error?.message || 'Erro ao alterar.';
      }
    });
  }

  tituloModal(): string {
    if (!this.modalTipo) return '';
    const nome = this.modalTipo === 'status' ? 'Status' : this.modalTipo === 'tipoAtendimento' ? 'Tipo de Atendimento' : 'Tipo de Contato';
    return this.editandoId != null ? `Editar ${nome}` : `Novo ${nome}`;
  }
}
