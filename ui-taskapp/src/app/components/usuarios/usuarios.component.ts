import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UsuarioService, UsuarioResponseDto, CadastroUsuarioDto } from '../../services/usuario.service';

@Component({
  selector: 'app-usuarios',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './usuarios.component.html',
  styleUrl: './usuarios.component.css'
})
export class UsuariosComponent implements OnInit {
  usuarios: UsuarioResponseDto[] = [];
  showForm = false;
  loading = false;
  error: string | null = null;

  novoUsuario: CadastroUsuarioDto = {
    nome: '',
    sobrenome: '',
    docFederal: '',
    docEstadual: '',
    login: '',
    senha: ''
  };

  constructor(private usuarioService: UsuarioService) { }

  ngOnInit() {
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
    this.showForm = true;
    this.novoUsuario = {
      nome: '',
      sobrenome: '',
      docFederal: '',
      docEstadual: '',
      login: '',
      senha: ''
    };
  }

  fecharFormulario() {
    this.showForm = false;
    this.error = null;
  }

  cadastrarUsuario() {
    if (!this.novoUsuario.nome || !this.novoUsuario.login || !this.novoUsuario.senha) {
      this.error = 'Preencha todos os campos obrigatórios (Nome, Login e Senha)';
      return;
    }

    this.loading = true;
    this.error = null;

    this.usuarioService.cadastrarUsuario(this.novoUsuario).subscribe({
      next: () => {
        this.carregarUsuarios();
        this.fecharFormulario();
        this.loading = false;
      },
      error: (err) => {
        this.error = err.error?.message || 'Erro ao cadastrar usuário';
        this.loading = false;
      }
    });
  }
}
