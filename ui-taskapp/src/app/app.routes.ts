import { Routes } from '@angular/router';
import { authGuard } from './guards/auth.guard';
import { perfilGuard } from './guards/perfil.guard';

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
    canActivate: [authGuard, perfilGuard]
  },
  {
    path: 'clientes',
    loadComponent: () => import('./components/clientes/clientes.component').then(m => m.ClientesComponent),
    canActivate: [authGuard, perfilGuard]
  },
  {
    path: 'atendimentos',
    loadComponent: () => import('./components/atendimentos/atendimentos.component').then(m => m.AtendimentosComponent),
    canActivate: [authGuard, perfilGuard]
  },
  {
    path: 'usuarios',
    loadComponent: () => import('./components/usuarios/usuarios.component').then(m => m.UsuariosComponent),
    canActivate: [authGuard, perfilGuard]
  },
  {
    path: 'contas-pagar',
    loadComponent: () => import('./components/contas-pagar/contas-pagar.component').then(m => m.ContasPagarComponent),
    canActivate: [authGuard, perfilGuard]
  },
  {
    path: 'contas-receber',
    loadComponent: () => import('./components/contas-receber/contas-receber.component').then(m => m.ContasReceberComponent),
    canActivate: [authGuard, perfilGuard]
  },
  {
    path: 'anotacoes',
    loadComponent: () => import('./components/anotacoes/anotacoes.component').then(m => m.AnotacoesComponent),
    canActivate: [authGuard, perfilGuard]
  },
  {
    path: 'relatorios-gerenciais',
    loadComponent: () => import('./components/relatorios-gerenciais/relatorios-gerenciais.component').then(m => m.RelatoriosGerenciaisComponent),
    canActivate: [authGuard, perfilGuard]
  },
  {
    path: 'possiveis-clientes',
    loadComponent: () => import('./components/possiveis-clientes/possiveis-clientes.component').then(m => m.PossiveisClientesComponent),
    canActivate: [authGuard]
  },
  {
    path: 'cadastro-atendimento',
    loadComponent: () => import('./components/cadastro-atendimento/cadastro-atendimento.component').then(m => m.CadastroAtendimentoComponent),
    canActivate: [authGuard, perfilGuard]
  },
  {
    path: 'status-atendimento-comercial',
    loadComponent: () => import('./components/status-atendimento-comercial/status-atendimento-comercial.component').then(m => m.StatusAtendimentoComercialComponent),
    canActivate: [authGuard, perfilGuard]
  }
];
