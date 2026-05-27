import { ApplicationConfig } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideAnimations } from '@angular/platform-browser/animations';

import { routes } from './app.routes';
import { maiusculoInterceptor } from './interceptors/maiusculo.interceptor';
import { notificacaoHttpInterceptor } from './interceptors/notificacao-http.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideHttpClient(withInterceptors([maiusculoInterceptor, notificacaoHttpInterceptor])),
    provideAnimations()
  ]
};
