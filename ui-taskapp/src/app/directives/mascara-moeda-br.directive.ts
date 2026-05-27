import { Directive, ElementRef, HostListener, forwardRef } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

@Directive({
  selector: 'input[mascaraMoedaBr]',
  standalone: true,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => MascaraMoedaBrDirective),
      multi: true
    }
  ]
})
export class MascaraMoedaBrDirective implements ControlValueAccessor {
  private onChange: (valor: number | null) => void = () => { };
  private onTouched: () => void = () => { };
  private disabled = false;

  constructor(private el: ElementRef<HTMLInputElement>) { }

  writeValue(valor: number | null): void {
    this.el.nativeElement.value = this.formatar(valor);
  }

  registerOnChange(fn: (valor: number | null) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
    this.el.nativeElement.disabled = isDisabled;
  }

  @HostListener('input', ['$event'])
  onInput(event: Event): void {
    if (this.disabled) return;
    const alvo = event.target as HTMLInputElement | null;
    const valorDigitado = alvo?.value ?? '';
    const valor = this.extrairNumero(valorDigitado);
    this.el.nativeElement.value = this.formatar(valor);
    this.onChange(valor);
  }

  @HostListener('blur')
  onBlur(): void {
    this.onTouched();
  }

  private extrairNumero(texto: string): number | null {
    const apenasDigitos = (texto || '').replace(/\D/g, '');
    if (!apenasDigitos) return null;
    const centavos = Number(apenasDigitos);
    if (Number.isNaN(centavos)) return null;
    return centavos / 100;
  }

  private formatar(valor: number | null): string {
    if (valor === null || valor === undefined || Number.isNaN(valor)) return '';
    return new Intl.NumberFormat('pt-BR', {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2
    }).format(valor);
  }
}

