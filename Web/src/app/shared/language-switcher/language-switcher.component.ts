import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { Lang, LanguageService } from '../../core/i18n/language.service';

@Component({
  selector: 'app-language-switcher',
  standalone: true,
  imports: [CommonModule, TranslateModule],
  template: `
    <div class="btn-group btn-group-sm" role="group">
      <button type="button" class="btn"
              [class.btn-primary]="lang.current() === 'uk'"
              [class.btn-outline-secondary]="lang.current() !== 'uk'"
              (click)="set('uk')">UA</button>
      <button type="button" class="btn"
              [class.btn-primary]="lang.current() === 'en'"
              [class.btn-outline-secondary]="lang.current() !== 'en'"
              (click)="set('en')">EN</button>
    </div>
  `
})
export class LanguageSwitcherComponent {
  lang = inject(LanguageService);
  set(l: Lang): void { this.lang.switch(l); }
}
