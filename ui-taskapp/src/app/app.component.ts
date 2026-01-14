import { Component, OnInit } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from './services/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, CommonModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit {
  title = 'TAREFAS GA';
  isAuthenticated = false;
  usuarioNome = '';

  constructor(
    private authService: AuthService,
    private router: Router
  ) { }

  ngOnInit() {
    this.authService.usuario$.subscribe(usuario => {
      this.isAuthenticated = usuario !== null;
      this.usuarioNome = usuario?.nome || '';
    });
  }

  logout() {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
