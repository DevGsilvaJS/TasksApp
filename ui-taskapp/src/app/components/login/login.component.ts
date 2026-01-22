import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService, LoginDto } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent implements OnInit {
  loginDto: LoginDto = {
    login: '',
    senha: ''
  };
  loading = false;
  error: string | null = null;

  constructor(
    private authService: AuthService,
    private router: Router
  ) { }

  ngOnInit() {
    // Se já estiver autenticado, redirecionar
    if (this.authService.isAuthenticated()) {
      this.router.navigate(['/dashboard']);
    }
  }

  fazerLogin() {
    if (this.loading) return;

    if (!this.loginDto.login.trim() || !this.loginDto.senha.trim()) {
      this.error = 'Por favor, preencha todos os campos.';
      return;
    }

    this.loading = true;
    this.error = null;

    this.authService.login(this.loginDto).subscribe({
      next: (response) => {
        if (response.autenticado) {
          this.router.navigate(['/dashboard']);
        } else {
          this.error = 'Falha na autenticação.';
          this.loading = false;
        }
      },
      error: (err) => {
        this.error = err.error?.message || 'Login ou senha inválidos.';
        this.loading = false;
        console.error(err);
      }
    });
  }
}
