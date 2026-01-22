import { Routes } from '@angular/router';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./components/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: '',
    redirectTo: '/dashboard',
    pathMatch: 'full'
  },
  {
    path: 'dashboard',
    loadComponent: () => import('./components/dashboard/dashboard.component').then(m => m.DashboardComponent),
    canActivate: [authGuard]
  },
  {
    path: 'clientes',
    loadComponent: () => import('./components/clientes/clientes.component').then(m => m.ClientesComponent),
    canActivate: [authGuard]
  },
  {
    path: 'atendimentos',
    loadComponent: () => import('./components/atendimentos/atendimentos.component').then(m => m.AtendimentosComponent),
    canActivate: [authGuard]
  },
  {
    path: 'usuarios',
    loadComponent: () => import('./components/usuarios/usuarios.component').then(m => m.UsuariosComponent),
    canActivate: [authGuard]
  },
  {
    path: 'contas-pagar',
    loadComponent: () => import('./components/contas-pagar/contas-pagar.component').then(m => m.ContasPagarComponent),
    canActivate: [authGuard]
  },
  {
    path: 'contas-receber',
    loadComponent: () => import('./components/contas-receber/contas-receber.component').then(m => m.ContasReceberComponent),
    canActivate: [authGuard]
  },
  {
    path: 'anotacoes',
    loadComponent: () => import('./components/anotacoes/anotacoes.component').then(m => m.AnotacoesComponent),
    canActivate: [authGuard]
  }
];
