import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TarefaService, TarefaResponseDto, CadastroTarefaDto, StatusTarefa } from '../../services/tarefa.service';
import { ClienteService, ClienteResponseDto } from '../../services/cliente.service';
import { UsuarioService, UsuarioResponseDto } from '../../services/usuario.service';
import { AnotacaoService, CadastroAnotacaoDto } from '../../services/anotacao.service';

@Component({
  selector: 'app-atendimentos',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './atendimentos.component.html',
  styleUrl: './atendimentos.component.css'
})
export class AtendimentosComponent implements OnInit {
  // Expor o enum para uso no template
  StatusTarefa = StatusTarefa;
  
  tarefas: TarefaResponseDto[] = [];
  tarefasFiltradas: TarefaResponseDto[] = [];
  clientes: ClienteResponseDto[] = [];
  usuarios: UsuarioResponseDto[] = [];
  showForm = false;
  loading = false;
  error: string | null = null;
  editando = false;
  tarefaEditando: TarefaResponseDto | null = null;
  termoBusca = '';

  novoTarefa: CadastroTarefaDto = {
    clienteId: 0,
    usuarioId: 0,
    status: StatusTarefa.EmAberto,
    dataConclusao: undefined,
    descricao: undefined,
    titulo: undefined,
    protocolo: undefined,
    solicitante: undefined,
    imagens: undefined
  };

  imagensSelecionadas: File[] = [];
  previewImagens: string[] = [];

  novaAnotacao: string = '';
  tarefaSelecionada: TarefaResponseDto | null = null;
  showAnotacoes = false;
  showImagens = false;
  tarefaImagens: TarefaResponseDto | null = null;
  imagemAtualIndex = 0;

  statusOptions = [
    { value: StatusTarefa.EmAberto, label: 'Em Aberto' },
    { value: StatusTarefa.Concluida, label: 'Concluída' },
    { value: StatusTarefa.Cancelada, label: 'Cancelada' }
  ];

  constructor(
    private tarefaService: TarefaService,
    private clienteService: ClienteService,
    private usuarioService: UsuarioService,
    private anotacaoService: AnotacaoService
  ) { }

  ngOnInit() {
    this.carregarTarefas();
    this.carregarClientes();
    this.carregarUsuarios();
  }

  carregarTarefas() {
    this.loading = true;
    this.error = null;
    this.tarefaService.listarTodasTarefas().subscribe({
      next: (data) => {
        this.tarefas = data;
        this.tarefasFiltradas = data;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Erro ao carregar tarefas. Verifique se a API está rodando.';
        this.loading = false;
        console.error(err);
      }
    });
  }

  carregarClientes() {
    this.clienteService.listarTodosClientes().subscribe({
      next: (data) => {
        this.clientes = data;
        console.log('Clientes carregados:', this.clientes);
        if (this.clientes.length === 0 && !this.error) {
          this.error = 'Nenhum cliente cadastrado. Cadastre um cliente primeiro.';
        }
      },
      error: (err) => {
        console.error('Erro ao carregar clientes:', err);
        if (!this.error) {
          this.error = 'Erro ao carregar clientes. Verifique se a API está rodando.';
        }
      }
    });
  }

  carregarUsuarios() {
    this.usuarioService.listarTodosUsuarios().subscribe({
      next: (data) => {
        this.usuarios = data;
        console.log('Usuários carregados:', this.usuarios);
        if (this.usuarios.length === 0 && !this.error) {
          this.error = 'Nenhum usuário cadastrado. Cadastre um usuário primeiro.';
        }
      },
      error: (err) => {
        console.error('Erro ao carregar usuários:', err);
        if (!this.error) {
          this.error = 'Erro ao carregar usuários. Verifique se a API está rodando.';
        }
      }
    });
  }

  filtrarTarefas() {
    if (!this.termoBusca.trim()) {
      this.tarefasFiltradas = this.tarefas;
      return;
    }

    const termo = this.termoBusca.toLowerCase();
    this.tarefasFiltradas = this.tarefas.filter(t =>
      t.clienteNome.toLowerCase().includes(termo) ||
      t.usuarioNome.toLowerCase().includes(termo) ||
      t.statusDescricao.toLowerCase().includes(termo) ||
      t.tarefaId.toString().includes(termo) ||
      (t.titulo && t.titulo.toLowerCase().includes(termo)) ||
      (t.protocolo && t.protocolo.toLowerCase().includes(termo)) ||
      (t.solicitante && t.solicitante.toLowerCase().includes(termo))
    );
  }

  abrirFormularioNovo() {
    this.editando = false;
    this.tarefaEditando = null;
    this.showForm = true;
    this.novoTarefa = {
      clienteId: 0,
      usuarioId: 0,
      status: StatusTarefa.EmAberto,
      dataConclusao: undefined,
      descricao: undefined,
      titulo: '',
      protocolo: '',
      solicitante: '',
      imagens: undefined
    };
    this.imagensSelecionadas = [];
    this.previewImagens = [];
    this.novaAnotacao = '';
    this.error = null;
    
    // Scroll para o topo do modal após um pequeno delay para garantir que o DOM foi renderizado
    setTimeout(() => {
      const modalContent = document.querySelector('.modal-content');
      if (modalContent) {
        modalContent.scrollTop = 0;
      }
    }, 100);
  }

  abrirFormularioEdicao(tarefa: TarefaResponseDto) {
    this.editando = true;
    this.tarefaEditando = tarefa;
    this.showForm = true;
    this.novoTarefa = {
      clienteId: tarefa.clienteId,
      usuarioId: tarefa.usuarioId,
      status: tarefa.status,
      dataConclusao: tarefa.dataConclusao,
      descricao: undefined,
      titulo: tarefa.titulo,
      protocolo: tarefa.protocolo,
      solicitante: tarefa.solicitante,
      imagens: undefined
    };
    this.imagensSelecionadas = [];
    this.previewImagens = [];
    this.novaAnotacao = '';
    this.error = null;
    
    // Carregar anotações da tarefa
    this.anotacaoService.obterAnotacoesPorTarefa(tarefa.tarefaId).subscribe({
      next: (anotacoes) => {
        if (this.tarefaEditando) {
          this.tarefaEditando.anotacoes = anotacoes || [];
        }
      },
      error: (err) => {
        console.error('Erro ao carregar anotações:', err);
      }
    });
  }

  abrirAnotacoes(tarefa: TarefaResponseDto) {
    this.tarefaSelecionada = tarefa;
    this.showAnotacoes = true;
    this.novaAnotacao = '';
    this.error = null;
    
    // Sempre carregar anotações atualizadas
    this.anotacaoService.obterAnotacoesPorTarefa(tarefa.tarefaId).subscribe({
      next: (anotacoes) => {
        if (this.tarefaSelecionada) {
          this.tarefaSelecionada.anotacoes = anotacoes || [];
        }
      },
      error: (err) => {
        console.error('Erro ao carregar anotações:', err);
      }
    });
  }

  fecharAnotacoes() {
    this.showAnotacoes = false;
    this.tarefaSelecionada = null;
    this.novaAnotacao = '';
    this.error = null;
  }

  abrirImagens(tarefa: TarefaResponseDto) {
    this.tarefaImagens = tarefa;
    this.imagemAtualIndex = 0;
    this.showImagens = true;
  }

  fecharImagens() {
    this.showImagens = false;
    this.tarefaImagens = null;
    this.imagemAtualIndex = 0;
  }

  proximaImagem() {
    if (this.tarefaImagens && this.tarefaImagens.imagens && this.imagemAtualIndex < this.tarefaImagens.imagens.length - 1) {
      this.imagemAtualIndex++;
    }
  }

  imagemAnterior() {
    if (this.imagemAtualIndex > 0) {
      this.imagemAtualIndex--;
    }
  }

  inserirAnotacao() {
    if (!this.novaAnotacao.trim()) {
      this.error = 'Digite uma descrição para a anotação';
      return;
    }

    if (!this.tarefaSelecionada) {
      return;
    }

    this.loading = true;
    this.error = null;

    // Criar anotação (a data/hora será adicionada no backend)
    const dto: CadastroAnotacaoDto = {
      tarefaId: this.tarefaSelecionada.tarefaId,
      usuarioId: this.tarefaSelecionada.usuarioId,
      descricao: this.novaAnotacao.trim()
    };

    this.anotacaoService.cadastrarAnotacao(dto).subscribe({
      next: (anotacao) => {
        // Adicionar anotação à lista
        if (this.tarefaSelecionada) {
          if (!this.tarefaSelecionada.anotacoes) {
            this.tarefaSelecionada.anotacoes = [];
          }
          this.tarefaSelecionada.anotacoes.unshift(anotacao);
          
          // Atualizar também na lista de tarefas
          const tarefaNaLista = this.tarefas.find(t => t.tarefaId === this.tarefaSelecionada!.tarefaId);
          if (tarefaNaLista) {
            if (!tarefaNaLista.anotacoes) {
              tarefaNaLista.anotacoes = [];
            }
            tarefaNaLista.anotacoes.unshift(anotacao);
          }
        }
        
        // Limpar campo
        this.novaAnotacao = '';
        this.loading = false;
      },
      error: (err) => {
        console.error('Erro ao salvar anotação:', err);
        this.error = err.error?.message || 'Erro ao salvar anotação';
        this.loading = false;
      }
    });
  }

  fecharFormulario() {
    this.showForm = false;
    this.editando = false;
    this.tarefaEditando = null;
    this.error = null;
  }

  onStatusChange(event: any) {
    const novoStatus = Number(event.target.value) as StatusTarefa;
    this.novoTarefa.status = novoStatus;
    
    // Se o status for alterado para "Concluída", preencher a data de conclusão com a data atual
    if (novoStatus === StatusTarefa.Concluida && !this.novoTarefa.dataConclusao) {
      const hoje = new Date();
      this.novoTarefa.dataConclusao = hoje.toISOString().split('T')[0];
    } else if (novoStatus !== StatusTarefa.Concluida) {
      // Se mudar para outro status que não seja "Concluída", limpar a data de conclusão
      this.novoTarefa.dataConclusao = undefined;
    }
  }

  salvarTarefa() {
    console.log('Salvando tarefa:', this.novoTarefa);
    
    // Validar se cliente e usuário foram selecionados (não pode ser 0)
    const clienteId = Number(this.novoTarefa.clienteId);

    if (!clienteId || clienteId === 0) {
      this.error = 'Selecione um cliente';
      return;
    }

    // Usar o primeiro usuário da lista por padrão (depois será o usuário da sessão)
    let usuarioId = 0;
    if (this.usuarios && this.usuarios.length > 0) {
      usuarioId = this.usuarios[0].usuarioId;
    } else {
      this.error = 'Nenhum usuário disponível. Cadastre um usuário primeiro.';
      return;
    }

    this.loading = true;
    this.error = null;

    // Se o status for "Concluída" e não tiver data de conclusão, preencher com a data atual
    if (this.novoTarefa.status === StatusTarefa.Concluida && !this.novoTarefa.dataConclusao) {
      const hoje = new Date();
      this.novoTarefa.dataConclusao = hoje.toISOString().split('T')[0];
    }

    // Na edição, usar o usuarioId original; na criação, usar o primeiro usuário
    const usuarioIdFinal = this.editando && this.tarefaEditando 
      ? this.tarefaEditando.usuarioId 
      : usuarioId;

    // Preparar dados para envio
    const dadosEnvio: CadastroTarefaDto = {
      clienteId: clienteId,
      usuarioId: usuarioIdFinal,
      status: Number(this.novoTarefa.status) as StatusTarefa,
      dataConclusao: this.novoTarefa.status === StatusTarefa.Concluida 
        ? (this.novoTarefa.dataConclusao || new Date().toISOString().split('T')[0])
        : undefined,
      descricao: this.novoTarefa.descricao || undefined,
      titulo: this.novoTarefa.titulo || undefined,
      protocolo: this.novoTarefa.protocolo || undefined,
      solicitante: this.novoTarefa.solicitante || undefined,
      imagens: this.imagensSelecionadas.length > 0 ? this.imagensSelecionadas : undefined
    };

    console.log('Dados enviados:', dadosEnvio);

    const operacao = this.editando && this.tarefaEditando
      ? this.tarefaService.atualizarTarefa(this.tarefaEditando.tarefaId, dadosEnvio)
      : this.tarefaService.cadastrarTarefa(dadosEnvio);

    operacao.subscribe({
      next: (result) => {
        console.log('Tarefa salva com sucesso:', result);
        // Recarregar tarefas para atualizar anotações
        this.carregarTarefas();
        // Se estava editando, atualizar a tarefa editada com as anotações
        if (this.editando && this.tarefaEditando) {
          this.tarefaEditando.anotacoes = result.anotacoes || [];
          this.tarefaEditando.imagens = result.imagens || [];
        }
        this.fecharFormulario();
        this.loading = false;
      },
      error: (err) => {
        console.error('Erro completo ao salvar tarefa:', err);
        console.error('Status:', err.status);
        console.error('Mensagem:', err.message);
        console.error('Error body:', err.error);
        this.error = err.error?.message || err.message || 'Erro ao salvar tarefa. Verifique se a API está rodando e se há clientes e usuários cadastrados.';
        this.loading = false;
      }
    });
  }

  excluirTarefa(tarefa: TarefaResponseDto) {
    if (!confirm(`Deseja realmente excluir a tarefa #${tarefa.tarefaId}?`)) {
      return;
    }

    this.loading = true;
    this.error = null;

    this.tarefaService.excluirTarefa(tarefa.tarefaId).subscribe({
      next: () => {
        this.carregarTarefas();
        this.loading = false;
      },
      error: (err) => {
        this.error = err.error?.message || 'Erro ao excluir tarefa';
        this.loading = false;
      }
    });
  }

  alterarStatus(tarefa: TarefaResponseDto, novoStatus: StatusTarefa) {
    this.loading = true;
    this.error = null;

    this.tarefaService.alterarStatusTarefa(tarefa.tarefaId, novoStatus).subscribe({
      next: () => {
        this.carregarTarefas();
        this.loading = false;
      },
      error: (err) => {
        this.error = err.error?.message || 'Erro ao alterar status';
        this.loading = false;
      }
    });
  }

  formatarData(data?: string): string {
    if (!data) return '-';
    return new Date(data).toLocaleDateString('pt-BR');
  }

  obterClasseStatus(status: StatusTarefa): string {
    switch (status) {
      case StatusTarefa.EmAberto:
        return 'status-aberto';
      case StatusTarefa.Concluida:
        return 'status-concluida';
      case StatusTarefa.Cancelada:
        return 'status-cancelada';
      default:
        return '';
    }
  }

  onImagensSelecionadas(event: any) {
    const files = event.target.files;
    if (files && files.length > 0) {
      this.imagensSelecionadas = Array.from(files);
      this.previewImagens = [];
      
      this.imagensSelecionadas.forEach((file: File) => {
        const reader = new FileReader();
        reader.onload = (e: any) => {
          this.previewImagens.push(e.target.result);
        };
        reader.readAsDataURL(file);
      });
    }
  }

  removerImagem(index: number) {
    this.imagensSelecionadas.splice(index, 1);
    this.previewImagens.splice(index, 1);
  }

  onErroImagem(event: Event) {
    const target = event.target as HTMLImageElement;
    if (target) {
      target.style.display = 'none';
    }
  }
}
