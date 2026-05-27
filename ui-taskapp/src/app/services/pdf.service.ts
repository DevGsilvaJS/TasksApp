import { Injectable } from '@angular/core';
import { jsPDF } from 'jspdf';
import autoTable from 'jspdf-autotable';

/**
 * Serviço para geração de PDF com tabelas usando jspdf e jspdf-autotable.
 * Uso básico:
 *   this.pdfService.gerarPdfComTabela('Título', ['Col A', 'Col B'], [['a1', 'b1'], ['a2', 'b2']], 'relatorio.pdf');
 */
@Injectable({
  providedIn: 'root'
})
export class PdfService {

  /**
   * Gera um PDF com uma tabela e faz o download.
   * @param titulo Título do documento (primeira linha)
   * @param colunas Cabeçalhos das colunas
   * @param linhas Dados: array de linhas, cada linha é um array de células
   * @param nomeArquivo Nome do arquivo para download (ex: 'relatorio.pdf')
   */
  gerarPdfComTabela(
    titulo: string,
    colunas: string[],
    linhas: (string | number)[][],
    nomeArquivo: string = 'documento.pdf'
  ): void {
    const doc = new jsPDF({ orientation: 'portrait', unit: 'mm', format: 'a4' });

    doc.setFontSize(16);
    doc.text(titulo, 14, 15);

    autoTable(doc, {
      head: [colunas],
      body: linhas,
      startY: 22,
      styles: { fontSize: 9 },
      headStyles: { fillColor: [66, 139, 202] }
    });

    doc.save(nomeArquivo);
  }

  /**
   * Retorna a instância do jsPDF e o autoTable para uso avançado.
   * Exemplo: const doc = this.pdfService.novoDocumento(); autoTable(doc, { ... });
   */
  novoDocumento(opcoes?: { orientation?: 'portrait' | 'landscape'; format?: string }): jsPDF {
    return new jsPDF({
      orientation: opcoes?.orientation ?? 'portrait',
      unit: 'mm',
      format: (opcoes?.format as 'a4') ?? 'a4'
    });
  }

  /** Acesso ao autoTable para usar com um doc existente: autoTable(doc, options) */
  get autoTable(): typeof autoTable {
    return autoTable;
  }
}
