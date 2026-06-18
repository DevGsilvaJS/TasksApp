export function limparTextoEmail(texto: string): string {
  return texto
    .replace(/&nbsp;/gi, ' ')
    .replace(/\u00A0/g, ' ')
    .trim();
}

export function limparHtmlCorpoEmail(html: string): string {
  return html
    .replace(/&nbsp;/gi, ' ')
    .replace(/\u00A0/g, ' ');
}
