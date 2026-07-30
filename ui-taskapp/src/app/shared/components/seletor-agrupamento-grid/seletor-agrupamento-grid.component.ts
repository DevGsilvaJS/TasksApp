import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { OpcaoAgrupamentoGrid } from '../../utils/grid-agrupamento.util';

@Component({
  selector: 'app-seletor-agrupamento-grid',
  standalone: true,
  imports: [CommonModule, FormsModule],
  styles: [`
    :host {
      display: inline-flex;
      align-items: center;
      vertical-align: middle;
      line-height: 1;
    }
    .grid-group-row {
      display: inline-flex;
      align-items: center;
      margin: 0;
      padding: 0;
      gap: 0.5rem;
    }
    .grid-group-select {
      box-sizing: border-box;
      height: 2.375rem;
      padding: 0 0.75rem;
      border: 1px solid #d1d5db;
      border-radius: 0.5rem;
      font-size: 0.875rem;
      background: white;
      color: #374151;
      min-width: 140px;
      line-height: 1.25;
      margin: 0;
    }
    .grid-group-select:focus {
      outline: none;
      border-color: #3b82f6;
      box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
    }
  `],
  template: `
    <div class="grid-group-row">
      <select
        class="grid-group-select"
        [ngModel]="agruparPor"
        (ngModelChange)="agruparPorChange.emit($event)"
        title="Agrupar por coluna"
        aria-label="Agrupar por coluna">
        @for (opt of opcoes; track opt.value) {
          <option [value]="opt.value">{{ opt.label }}</option>
        }
      </select>
      <ng-content></ng-content>
    </div>
  `
})
export class SeletorAgrupamentoGridComponent {
  @Input() agruparPor = '';
  @Input() opcoes: OpcaoAgrupamentoGrid[] = [];
  @Output() agruparPorChange = new EventEmitter<string>();
}
