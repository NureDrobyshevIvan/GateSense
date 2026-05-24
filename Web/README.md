# GateSense Web

Angular 21 front-end для системи GateSense. Лабораторна робота 3.

## Стек

- Angular 21 (standalone components, signals, нова `@if`/`@for` синтаксис)
- ng-bootstrap + Bootstrap 5
- ngx-translate (i18n: uk / en)
- Lazy-loaded routes

Backend: `https://gatesense-production.up.railway.app/`

## Запуск

```powershell
cd c:\Study\3_1\ATARK\GateSense\Web
npm install
npm start
```

Відкривається на `http://localhost:4200`.
Backend на Railway вже задеплоєний і CORS налаштований для `localhost:4200`.

## Структура

```
src/app/
├── app.ts, app.config.ts, app.routes.ts
├── core/
│   ├── api/         REST-обгортки (GaragesApi, GateApi, AdminApi, BackupApi, ...)
│   ├── auth/        AuthService, TokenStorage, jwtInterceptor, authGuard/adminGuard
│   ├── i18n/        LanguageService (uk/en)
│   └── models/      DTO інтерфейси
├── features/
│   ├── auth/        Login, Register
│   ├── user/        UserLayout + garages/events/access
│   └── admin/       AdminLayout + users/garages/backup
└── shared/
    └── language-switcher/
src/assets/i18n/
├── uk.json, en.json
```

## Маршрути

- `/login`, `/register` — public
- `/app/garages` (authGuard) — список своїх гаражів
- `/app/garages/:id` — деталі (open/close, сенсори, alerts)
- `/app/garages/:id/events` — журнал подій
- `/app/garages/:id/access` — керування family/guest доступом
- `/admin/users` (adminGuard) — список усіх юзерів + assign/remove admin role + delete
- `/admin/garages` (adminGuard) — список усіх гаражів
- `/admin/backup` (adminGuard) — export/import JSON
