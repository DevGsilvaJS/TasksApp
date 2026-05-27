import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { NotificacaoService, NotificacaoUi } from '../../services/notificacao.service';

@Component({
  selector: 'app-notificacao-central',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './notificacao-central.component.html',
  styleUrl: './notificacao-central.component.css'
})
export class NotificacaoCentralComponent {
  notificacaoAtual: NotificacaoUi | null = null;

  constructor(private notificacao: NotificacaoService) {
    this.notificacao.notificacaoAtual$.subscribe(n => this.notificacaoAtual = n);
  }

  confirmar(): void {
    if (!this.notificacaoAtual) return;
    this.notificacao.responderConfirmacao(true);
  }

  cancelar(): void {
    if (!this.notificacaoAtual) return;
    this.notificacao.responderConfirmacao(false);
  }

  fechar(): void {
    if (!this.notificacaoAtual) return;
    this.notificacao.fecharAtual();
  }

  get classeTipo(): string {
    return this.notificacaoAtual ? `tipo-${this.notificacaoAtual.tipo}` : '';
  }
}

