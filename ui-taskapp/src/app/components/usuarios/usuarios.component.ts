import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UsuarioService, UsuarioResponseDto, CadastroUsuarioDto, AtualizarUsuarioDto, PERFIS_OPCOES, PERFIL_COMERCIAL } from '../../services/usuario.service';
import { NotificacaoService } from '../../services/notificacao.service';
import {
  criarOpcoesAgrupamento,
  carregarPreferenciaAgruparPor,
  salvarPreferenciaAgruparPor,
  deveExibirCabecalhoGrupo,
  obterRotuloAgrupamento,
  obterValorCabecalhoGrupo,
  ordenarItensParaAgrupamento
} from '../../shared/utils/grid-agrupamento.util';
import { SeletorAgrupamentoGridComponent } from '../../shared/components/seletor-agrupamento-grid/seletor-agrupamento-grid.component';

@Component({
  selector: 'app-usuarios',
  standalone: true,
  imports: [CommonModule, FormsModule, SeletorAgrupamentoGridComponent],
  templateUrl: './usuarios.component.html',
  styleUrl: './usuarios.component.css'
})
export class UsuariosComponent implements OnInit {
  usuarios: UsuarioResponseDto[] = [];
  showForm = false;
  editando = false;
  usuarioEditando: UsuarioResponseDto | null = null;
  loading = false;
  error: string | null = null;

  readonly perfisOpcoes = PERFIS_OPCOES;

  agruparPor = '';
  agruparPorOpcoes = criarOpcoesAgrupamento([
    { value: 'perfil', label: 'Perfil' },
    { value: 'nome', label: 'Nome' }
  ]);
  private readonly STORAGE_KEY_AGRUPAR_POR = 'usuarios_agrupar_por';

  novoUsuario: CadastroUsuarioDto = {
    nome: '',
    sobrenome: '',
    docFederal: '',
    docEstadual: '',
    login: '',
    senha: '',
    perfil: PERFIL_COMERCIAL
  };

  senhaEdicao = '';

  constructor(
    private usuarioService: UsuarioService,
    private notificacao: NotificacaoService
  ) { }

  ngOnInit() {
    this.agruparPor = carregarPreferenciaAgruparPor(this.STORAGE_KEY_AGRUPAR_POR, this.agruparPorOpcoes);
    this.carregarUsuarios();
  }

  carregarUsuarios() {
    this.loading = true;
    this.error = null;
    this.usuarioService.listarTodosUsuarios().subscribe({
      next: (data) => {
        this.usuarios = data;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Erro ao carregar usuários. Verifique se a API está rodando.';
        this.loading = false;
        console.error(err);
      }
    });
  }

  abrirFormulario() {
    this.editando = false;
    this.usuarioEditando = null;
    this.showForm = true;
    this.novoUsuario = {
      nome: '',
      sobrenome: '',
      docFederal: '',
      docEstadual: '',
      login: '',
      senha: '',
      perfil: PERFIL_COMERCIAL
    };
    this.senhaEdicao = '';
  }

  abrirEdicao(usuario: UsuarioResponseDto) {
    this.editando = true;
    this.usuarioEditando = usuario;
    this.showForm = true;
    this.novoUsuario = {
      nome: usuario.nome,
      sobrenome: usuario.sobrenome ?? '',
      docFederal: usuario.docFederal ?? '',
      docEstadual: usuario.docEstadual ?? '',
      login: usuario.login,
      senha: '',
      perfil: usuario.perfil
    };
    this.senhaEdicao = '';
  }

  fecharFormulario() {
    this.showForm = false;
    this.editando = false;
    this.usuarioEditando = null;
    this.error = null;
  }

  get tituloModal(): string {
    return this.editando ? 'Editar Usuário' : 'Cadastrar Novo Usuário';
  }

  labelPerfil(perfil: number): string {
    return PERFIS_OPCOES.find(p => p.value === perfil)?.label ?? '—';
  }

  salvar() {
    if (!this.novoUsuario.nome?.trim() || !this.novoUsuario.login?.trim()) {
      this.error = 'Preencha Nome e Login.';
      this.notificacao.aviso(this.error);
      return;
    }
    if (!this.editando && !this.novoUsuario.senha) {
      this.error = 'Senha é obrigatória para novo usuário.';
      this.notificacao.aviso(this.error);
      return;
    }

    this.loading = true;
    this.error = null;

    if (this.editando && this.usuarioEditando) {
      const dto: AtualizarUsuarioDto = {
        nome: this.novoUsuario.nome.trim(),
        sobrenome: this.novoUsuario.sobrenome?.trim() || undefined,
        docFederal: this.novoUsuario.docFederal?.trim() || undefined,
        docEstadual: this.novoUsuario.docEstadual?.trim() || undefined,
        login: this.novoUsuario.login.trim(),
        senha: this.senhaEdicao?.trim() || undefined,
        perfil: this.novoUsuario.perfil
      };
      this.usuarioService.atualizarUsuario(this.usuarioEditando.usuarioId, dto).subscribe({
        next: () => {
          this.carregarUsuarios();
          this.fecharFormulario();
          this.loading = false;
          this.notificacao.sucesso('Usuário atualizado com sucesso.');
        },
        error: (err) => {
          this.error = err.error?.message || 'Erro ao atualizar usuário.';
          this.loading = false;
        }
      });
    } else {
      this.usuarioService.cadastrarUsuario(this.novoUsuario).subscribe({
        next: () => {
          this.carregarUsuarios();
          this.fecharFormulario();
          this.loading = false;
          this.notificacao.sucesso('Usuário cadastrado com sucesso.');
        },
        error: (err) => {
          this.error = err.error?.message || 'Erro ao cadastrar usuário.';
          this.loading = false;
        }
      });
    }
  }

  onAgruparPorChange(valor: string) {
    this.agruparPor = valor;
    salvarPreferenciaAgruparPor(this.STORAGE_KEY_AGRUPAR_POR, valor);
  }

  get usuariosParaTabela(): UsuarioResponseDto[] {
    return ordenarItensParaAgrupamento(this.usuarios, this.agruparPor);
  }

  getAgruparPorLabel(): string {
    return obterRotuloAgrupamento(this.agruparPorOpcoes, this.agruparPor);
  }

  getValorGrupoUsuario(usuario: UsuarioResponseDto): string {
    if (this.agruparPor === 'perfil') {
      const perfil = this.perfisOpcoes.find(p => p.value === usuario.perfil);
      return perfil?.label ?? '—';
    }

    return obterValorCabecalhoGrupo(usuario as unknown as Record<string, unknown>, this.agruparPor);
  }

  exibirCabecalhoGrupoUsuario(index: number): boolean {
    return deveExibirCabecalhoGrupo(
      this.usuariosParaTabela,
      index,
      this.agruparPor,
      (usuario) => this.getValorGrupoUsuario(usuario)
    );
  }
}
