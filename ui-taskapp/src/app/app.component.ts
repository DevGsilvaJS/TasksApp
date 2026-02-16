import { Component, OnInit } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { forkJoin } from 'rxjs';
import { AuthService } from './services/auth.service';
import { AlertasService } from './services/alertas.service';
import { PendenciasAlertasDto } from './services/alertas.service';
import { TarefaService, TarefaResponseDto, TipoAtendimento } from './services/tarefa.service';

export type AlertaItem =
  | { tipo: 'pendencias'; data: PendenciasAlertasDto }
  | { tipo: 'reunioes'; data: TarefaResponseDto[] };

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, CommonModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit {
  title = 'TAREFAS GA';
  isAuthenticated = false;
  usuarioNome = '';
  /** Fila de alertas: exibe um pop-up por vez (primeiro pendencias, depois reuniões). */
  filaAlertas: AlertaItem[] = [];
  currentAlertaIndex = 0;
  private popupAlertasMostrado = false;

  constructor(
    private authService: AuthService,
    private router: Router,
    private alertasService: AlertasService,
    private tarefaService: TarefaService
  ) { }

  get alertaAtual(): AlertaItem | null {
    return this.filaAlertas[this.currentAlertaIndex] ?? null;
  }

  get showPopupAlertas(): boolean {
    return this.filaAlertas.length > 0 && this.alertaAtual !== null;
  }

  ngOnInit() {
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
    const reunioes$ = usuarioId != null
      ? this.tarefaService.listarTarefas({ usuarioId, incluirConcluidas: false })
      : null;

    if (reunioes$) {
      forkJoin({ pendencias: pendencias$, reunioes: reunioes$ }).subscribe({
        next: ({ pendencias, reunioes }) => {
          const fila: AlertaItem[] = [];
          if (pendencias.notasServicoPendentesMes?.length > 0 || pendencias.dasPendentesOuAtrasadas?.length > 0) {
            fila.push({ tipo: 'pendencias', data: pendencias });
          }
          const reunioesFiltradas = reunioes.filter(t => t.tipoAtendimento === TipoAtendimento.Reuniao);
          if (reunioesFiltradas.length > 0) {
            fila.push({ tipo: 'reunioes', data: reunioesFiltradas });
          }
          this.filaAlertas = fila;
          this.currentAlertaIndex = 0;
        }
      });
    } else {
      pendencias$.subscribe({
        next: (data) => {
          if (data.notasServicoPendentesMes?.length > 0 || data.dasPendentesOuAtrasadas?.length > 0) {
            this.filaAlertas = [{ tipo: 'pendencias', data }];
            this.currentAlertaIndex = 0;
          }
        }
      });
    }
  }

  avancarAlerta() {
    if (this.currentAlertaIndex + 1 < this.filaAlertas.length) {
      this.currentAlertaIndex++;
    } else {
      this.filaAlertas = [];
      this.currentAlertaIndex = 0;
    }
  }

  formatarData(data: string): string {
    return new Date(data).toLocaleDateString('pt-BR');
  }

  logout() {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
