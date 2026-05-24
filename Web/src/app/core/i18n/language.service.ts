import { Injectable, inject, signal } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';

const LANG_KEY = 'gs_lang';
export type Lang = 'uk' | 'en';

@Injectable({ providedIn: 'root' })
export class LanguageService {
  private translate = inject(TranslateService);
  readonly current = signal<Lang>('uk');

  init(): void {
    const stored = (localStorage.getItem(LANG_KEY) as Lang) || 'uk';
    this.translate.addLangs(['uk', 'en']);
    this.translate.setDefaultLang('uk');
    this.translate.use(stored);
    this.current.set(stored);
    document.documentElement.lang = stored;
    document.documentElement.dir = 'ltr';
  }

  switch(lang: Lang): void {
    this.translate.use(lang);
    localStorage.setItem(LANG_KEY, lang);
    this.current.set(lang);
    document.documentElement.lang = lang;
  }

  /** locale-aware string compare for sorting */
  collator(): Intl.Collator {
    return new Intl.Collator(this.current(), { sensitivity: 'base' });
  }
}
