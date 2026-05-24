import { ApplicationConfig, LOCALE_ID, provideAppInitializer, inject, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter, withComponentInputBinding } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideTranslateService } from '@ngx-translate/core';
import { provideTranslateHttpLoader } from '@ngx-translate/http-loader';

import { routes } from './app.routes';
import { jwtInterceptor } from './core/auth/jwt.interceptor';
import { LanguageService } from './core/i18n/language.service';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes, withComponentInputBinding()),
    provideHttpClient(withInterceptors([jwtInterceptor])),
    provideTranslateService({
      fallbackLang: 'uk'
    }),
    provideTranslateHttpLoader({ prefix: 'assets/i18n/', suffix: '.json' }),
    { provide: LOCALE_ID, useValue: 'uk-UA' },
    provideAppInitializer(() => {
      const lang = inject(LanguageService);
      lang.init();
    })
  ]
};
