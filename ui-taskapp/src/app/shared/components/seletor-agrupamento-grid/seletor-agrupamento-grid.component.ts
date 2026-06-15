import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { OpcaoAgrupamentoGrid } from '../../utils/grid-agrupamento.util';

@Component({
  selector: 'app-seletor-agrupamento-grid',
  standalone: true,
  imports: [CommonModule, FormsModule],
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
