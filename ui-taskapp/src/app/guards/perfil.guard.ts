import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';

/**
 * Redireciona perfil Comercial para /possiveis-clientes ao tentar acessar outras rotas.
 * Usar nas rotas que só Administrador pode acessar.
 */
export const perfilGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAdministrador()) return true;
  if (state.url.includes('possiveis-clientes') || state.url.includes('envio-email')) return true;

  router.navigate(['/possiveis-clientes']);
  return false;
};
