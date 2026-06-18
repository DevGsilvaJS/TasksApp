import { Component, ElementRef, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  CampanhaEmailStatusDto,
  DestinatarioEmailDto,
  EnvioEmailRequestDto,
  EnvioEmailService,
  RelatorioCampanhaEmailDto
} from '../../services/envio-email.service';
import { NotificacaoService } from '../../services/notificacao.service';
import { extrairMensagemErroApi } from '../../utils/erro-api.util';
import { limparHtmlCorpoEmail, limparTextoEmail } from '../../utils/email-conteudo.util';

export interface DestinatarioGridItem extends DestinatarioEmailDto {
  selecionado: boolean;
}

export interface AnexoEmail {
  arquivo: File;
  nome: string;
  tamanho: string;
}

type AbaRelatorio = 'enviados' | 'erros';

@Component({
  selector: 'app-envio-email',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './envio-email.component.html',
  styleUrl: './envio-email.component.css'
})
export class EnvioEmailComponent implements OnInit, OnDestroy {
  readonly tamanhoPagina = 15;
  private readonly intervaloPollingMs = 3000;

  @ViewChild('editorCorpo') editorCorpo?: ElementRef<HTMLDivElement>;
  @ViewChild('inputImagemCorpo') inputImagemCorpo?: ElementRef<HTMLInputElement>;
  @ViewChild('inputAnexos') inputAnexos?: ElementRef<HTMLInputElement>;

  termoPesquisa = '';
  destinatarios: DestinatarioGridItem[] = [];
  pesquisando = false;
  enviando = false;

  paginaAtual = 1;
  totalRegistros = 0;
  totalPaginas = 0;

  assunto = '';
  destinatarioManual = '';
  anexos: AnexoEmail[] = [];

  todosSelecionados = false;
  private selecoesPorId = new Map<number, DestinatarioGridItem>();

  campanhaStatus: CampanhaEmailStatusDto | null = null;
  relatorioAtual: RelatorioCampanhaEmailDto | null = null;
  exibirRelatorio = false;
  abaRelatorio: AbaRelatorio = 'enviados';
  historicoRelatorios: RelatorioCampanhaEmailDto[] = [];

  private pollingTimer?: ReturnType<typeof setInterval>;
  private campanhaMonitoradaId: number | null = null;

  constructor(
    private envioEmailService: EnvioEmailService,
    private notificacao: NotificacaoService
  ) {}

  ngOnInit(): void {
    this.carregarHistoricoRelatorios();
    this.verificarCampanhaAtiva();
  }

  ngOnDestroy(): void {
    this.pararPolling();
  }

  pesquisar(): void {
    this.paginaAtual = 1;
    this.selecoesPorId.clear();
    this.carregarPagina();
  }

  irParaPagina(pagina: number): void {
    if (pagina < 1 || pagina > this.totalPaginas || pagina === this.paginaAtual) {
      return;
    }
    this.paginaAtual = pagina;
    this.carregarPagina();
  }

  paginaAnterior(): void {
    this.irParaPagina(this.paginaAtual - 1);
  }

  proximaPagina(): void {
    this.irParaPagina(this.paginaAtual + 1);
  }

  private carregarPagina(): void {
    this.pesquisando = true;
    this.envioEmailService.pesquisarDestinatarios({
      termo: this.termoPesquisa,
      pagina: this.paginaAtual,
      tamanhoPagina: this.tamanhoPagina
    }).subscribe({
      next: (resultado) => {
        this.totalRegistros = resultado.total;
        this.totalPaginas = resultado.totalPaginas;
        this.paginaAtual = resultado.pagina;
        this.destinatarios = resultado.itens.map((item) => this.mapearDestinatario(item));
        this.atualizarSelecionarTodos();
        this.pesquisando = false;

        if (resultado.total === 0) {
          this.notificacao.info('Nenhum destinatário encontrado.');
        }
      },
      error: (err) => {
        this.pesquisando = false;
        this.notificacao.erro(extrairMensagemErroApi(err, 'Erro ao pesquisar destinatários.'));
      }
    });
  }

  alternarNaoEnviar(item: DestinatarioGridItem): void {
    const novoValor = item.naoEnviar;
    this.envioEmailService.atualizarNaoEnviar(item.id, novoValor).subscribe({
      next: (atualizado) => {
        item.naoEnviar = atualizado.naoEnviar;
        if (item.naoEnviar) {
          item.selecionado = false;
          this.selecoesPorId.delete(item.id);
        } else {
          this.sincronizarSelecao(item);
        }
        this.atualizarSelecionarTodos();
      },
      error: (err) => {
        item.naoEnviar = !novoValor;
        this.notificacao.erro(extrairMensagemErroApi(err, 'Erro ao atualizar destinatário.'));
      }
    });
  }

  podeSelecionar(item: DestinatarioGridItem): boolean {
    return !item.naoEnviar;
  }

  alternarSelecionarTodos(): void {
    this.destinatarios.forEach((item) => {
      if (!this.podeSelecionar(item)) {
        return;
      }
      item.selecionado = this.todosSelecionados;
      this.sincronizarSelecao(item);
    });
  }

  atualizarSelecionarTodos(): void {
    const elegiveis = this.destinatarios.filter((item) => this.podeSelecionar(item));
    if (elegiveis.length === 0) {
      this.todosSelecionados = false;
      return;
    }
    this.todosSelecionados = elegiveis.every((item) => item.selecionado);
  }

  aoAlterarSelecao(item: DestinatarioGridItem): void {
    this.sincronizarSelecao(item);
    this.atualizarSelecionarTodos();
  }

  get destinatariosSelecionados(): DestinatarioGridItem[] {
    return Array.from(this.selecoesPorId.values()).filter(
      (item) => item.selecionado && this.podeSelecionar(item)
    );
  }

  get quantidadeSelecionados(): number {
    return this.destinatariosSelecionados.length;
  }

  get inicioRegistro(): number {
    if (this.totalRegistros === 0) return 0;
    return (this.paginaAtual - 1) * this.tamanhoPagina + 1;
  }

  get fimRegistro(): number {
    if (this.totalRegistros === 0) return 0;
    return Math.min(this.paginaAtual * this.tamanhoPagina, this.totalRegistros);
  }

  get campanhaEmAndamento(): boolean {
    if (!this.campanhaStatus) return false;
    return this.campanhaStatus.status === 'Fila' || this.campanhaStatus.status === 'Processando';
  }

  get percentualCampanha(): number {
    if (!this.campanhaStatus || this.campanhaStatus.totalItens === 0) return 0;
    const processados = this.campanhaStatus.enviados + this.campanhaStatus.erros;
    return Math.round((processados / this.campanhaStatus.totalItens) * 100);
  }

  get usandoDestinatarioManual(): boolean {
    return this.destinatarioManual.trim().length > 0;
  }

  aplicarFormato(comando: string): void {
    document.execCommand(comando, false);
    this.editorCorpo?.nativeElement.focus();
  }

  abrirSeletorImagemCorpo(): void {
    this.inputImagemCorpo?.nativeElement.click();
  }

  inserirImagemNoCorpo(event: Event): void {
    const input = event.target as HTMLInputElement;
    const arquivo = input.files?.[0];
    if (!arquivo || !arquivo.type.startsWith('image/')) {
      this.notificacao.aviso('Selecione um arquivo de imagem válido.');
      input.value = '';
      return;
    }

    const leitor = new FileReader();
    leitor.onload = () => {
      const editor = this.editorCorpo?.nativeElement;
      if (!editor) return;

      editor.focus();
      const imagemHtml = `<img src="${leitor.result}" alt="${arquivo.name}" style="max-width:100%;height:auto;" />`;
      document.execCommand('insertHTML', false, imagemHtml);
      input.value = '';
    };
    leitor.readAsDataURL(arquivo);
  }

  abrirSeletorAnexos(): void {
    this.inputAnexos?.nativeElement.click();
  }

  adicionarAnexos(event: Event): void {
    const input = event.target as HTMLInputElement;
    const arquivos = input.files;
    if (!arquivos?.length) return;

    Array.from(arquivos).forEach((arquivo) => {
      const jaExiste = this.anexos.some(
        (anexo) => anexo.nome === arquivo.name && anexo.arquivo.size === arquivo.size
      );
      if (!jaExiste) {
        this.anexos.push({
          arquivo,
          nome: arquivo.name,
          tamanho: this.formatarTamanhoArquivo(arquivo.size)
        });
      }
    });

    input.value = '';
  }

  removerAnexo(indice: number): void {
    this.anexos.splice(indice, 1);
  }

  abrirRelatorio(relatorio: RelatorioCampanhaEmailDto): void {
    this.relatorioAtual = relatorio;
    this.abaRelatorio = relatorio.erros > 0 ? 'erros' : 'enviados';
    this.exibirRelatorio = true;
  }

  fecharRelatorio(): void {
    this.exibirRelatorio = false;
  }

  async enviar(): Promise<void> {
    const assunto = limparTextoEmail(this.assunto);
    const corpoHtml = this.obterCorpoHtml();

    if (!assunto) {
      this.notificacao.aviso('Informe o assunto do e-mail.');
      return;
    }

    if (!corpoHtml || corpoHtml === '<br>') {
      this.notificacao.aviso('Informe o corpo do e-mail.');
      return;
    }

    const destinatariosEnvio = this.obterDestinatariosEnvio();
    if (destinatariosEnvio.length === 0) {
      this.notificacao.aviso(
        this.usandoDestinatarioManual
          ? 'Informe ao menos um e-mail válido no campo de destinatário.'
          : 'Selecione ao menos um destinatário na grade ou informe um e-mail manualmente.'
      );
      return;
    }

    if (this.campanhaEmAndamento) {
      this.notificacao.aviso('Já existe uma campanha em andamento. Aguarde a conclusão.');
      return;
    }

    const confirmado = await this.notificacao.confirmar(
      'Confirmar envio',
      `Deseja enviar o e-mail para ${destinatariosEnvio.length} destinatário(s)?`
    );
    if (!confirmado) return;

    const dto: EnvioEmailRequestDto = {
      assunto,
      corpoHtml,
      destinatarios: destinatariosEnvio,
      anexos: this.anexos.map((anexo) => anexo.arquivo)
    };

    this.enviando = true;
    this.envioEmailService.enviar(dto).subscribe({
      next: (resposta) => {
        this.enviando = false;
        this.notificacao.sucesso(resposta.mensagem);
        this.limparComposicao();
        this.iniciarMonitoramentoCampanha(resposta.campanhaId);
      },
      error: (err) => {
        this.enviando = false;
        this.notificacao.erro(extrairMensagemErroApi(err, 'Erro ao enviar e-mail.'));
      }
    });
  }

  private verificarCampanhaAtiva(): void {
    this.envioEmailService.obterCampanhaAtiva().subscribe({
      next: (status) => {
        if (status) {
          this.iniciarMonitoramentoCampanha(status.id, status);
        }
      }
    });
  }

  private iniciarMonitoramentoCampanha(id: number, statusInicial?: CampanhaEmailStatusDto): void {
    this.campanhaMonitoradaId = id;
    if (statusInicial) {
      this.campanhaStatus = statusInicial;
    }
    this.pararPolling();
    this.atualizarStatusCampanha();
    this.pollingTimer = setInterval(() => this.atualizarStatusCampanha(), this.intervaloPollingMs);
  }

  private atualizarStatusCampanha(): void {
    if (this.campanhaMonitoradaId == null) return;

    this.envioEmailService.obterStatusCampanha(this.campanhaMonitoradaId).subscribe({
      next: (status) => {
        this.campanhaStatus = status;
        if (status.status === 'Concluida') {
          this.pararPolling();
          this.carregarRelatorioFinal(status.id);
        }
      },
      error: () => this.pararPolling()
    });
  }

  private carregarRelatorioFinal(campanhaId: number): void {
    this.envioEmailService.obterRelatorio(campanhaId).subscribe({
      next: (relatorio) => {
        this.relatorioAtual = relatorio;
        this.abaRelatorio = relatorio.erros > 0 ? 'erros' : 'enviados';
        this.exibirRelatorio = true;
        this.carregarHistoricoRelatorios();
        this.notificacao.sucesso(
          `Campanha concluída: ${relatorio.enviados} enviado(s), ${relatorio.erros} erro(s).`
        );
      },
      error: (err) => {
        this.notificacao.erro(extrairMensagemErroApi(err, 'Erro ao carregar relatório da campanha.'));
      }
    });
  }

  private carregarHistoricoRelatorios(): void {
    this.envioEmailService.listarRelatorios().subscribe({
      next: (relatorios) => {
        this.historicoRelatorios = relatorios;
      },
      error: () => undefined
    });
  }

  private pararPolling(): void {
    if (this.pollingTimer) {
      clearInterval(this.pollingTimer);
      this.pollingTimer = undefined;
    }
  }

  private mapearDestinatario(item: DestinatarioEmailDto): DestinatarioGridItem {
    const existente = this.selecoesPorId.get(item.id);
    if (existente) {
      return {
        ...item,
        selecionado: existente.selecionado && !item.naoEnviar
      };
    }
    return { ...item, selecionado: false };
  }

  private sincronizarSelecao(item: DestinatarioGridItem): void {
    if (item.selecionado && this.podeSelecionar(item)) {
      this.selecoesPorId.set(item.id, { ...item });
      return;
    }
    this.selecoesPorId.delete(item.id);
  }

  private obterDestinatariosEnvio(): string[] {
    const manual = this.destinatarioManual.trim();
    if (manual) {
      return this.extrairEmailsValidos(manual);
    }
    return this.destinatariosSelecionados.map((item) => item.email);
  }

  private extrairEmailsValidos(texto: string): string[] {
    const partes = texto.split(/[;,\n]+/);
    const emails: string[] = [];

    partes.forEach((parte) => {
      const email = parte.trim().toLowerCase();
      if (!email || !this.emailValido(email)) {
        return;
      }
      if (!emails.includes(email)) {
        emails.push(email);
      }
    });

    return emails;
  }

  private emailValido(email: string): boolean {
    return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
  }

  private obterCorpoHtml(): string {
    const html = this.editorCorpo?.nativeElement.innerHTML.trim() ?? '';
    return limparHtmlCorpoEmail(html);
  }

  private limparComposicao(): void {
    this.assunto = '';
    this.destinatarioManual = '';
    if (this.editorCorpo) {
      this.editorCorpo.nativeElement.innerHTML = '';
    }
    this.anexos = [];
    this.selecoesPorId.clear();
    this.destinatarios.forEach((item) => {
      item.selecionado = false;
    });
    this.todosSelecionados = false;
  }

  private formatarTamanhoArquivo(bytes: number): string {
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
  }
}
