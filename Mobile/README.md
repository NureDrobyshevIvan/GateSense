# GateSense Mobile

Android-клієнт для системи GateSense. Лабораторна робота 2 — Мобільний застосунок.

## Стек

- Kotlin 2.2.10
- Jetpack Compose (Material 3)
- Architecture: MVVM + Repository, ручний DI через `ServiceLocator`
- Networking: Retrofit 2 + OkHttp + kotlinx.serialization
- Local storage: DataStore Preferences (для JWT)
- Navigation: Navigation Compose

Backend: `https://gatesense-production.up.railway.app/`

## Структура пакетів

```
com.example.gatesensemobile/
├── data/
│   ├── api/         (Retrofit ApiService, DTO, ApiClient)
│   ├── auth/        (TokenStore, AuthInterceptor)
│   └── repository/  (Auth/Garage/Gate/Sensor/Access/Log Repository)
├── di/              (ServiceLocator)
├── ui/
│   ├── auth/        (LoginScreen, RegisterScreen, AuthViewModel)
│   ├── garages/     (GaragesScreen, GarageDetailScreen + VMs)
│   ├── access/      (AccessScreen + ViewModel)
│   ├── events/      (EventLogScreen + ViewModel)
│   ├── nav/         (GateSenseNavGraph + Routes)
│   └── theme/
├── GateSenseApp.kt  (Application — ініціалізує ServiceLocator)
└── MainActivity.kt
```

## Реалізовані екрани та фічі

| Екран | Що робить | API |
|---|---|---|
| LoginScreen | Вхід за логіном і паролем | `POST /Auth/login` |
| RegisterScreen | Реєстрація | `POST /Auth/register` |
| GaragesScreen | Список гаражів, створення, видалення, logout | `GET/POST/DELETE /garages` |
| GarageDetailScreen | Open/Close воріт, стан, останні показники сенсорів, банер тривоги | `GET /gate/state`, `POST /gate/open\|close`, `GET /sensors/latest`, `GET /sensors/alerts` |
| AccessScreen | Управління доступами: список членів, видача family/guest, відкликання | `GET /access/garages/{id}/members`, `POST /access/family\|guest`, `DELETE /access/{id}` |
| EventLogScreen | Журнал подій воріт | `GET /garages/{id}/logs/gate` |

## Запуск

1. Відкрити папку `Mobile/` як проєкт у Android Studio (Hedgehog+).
2. Sync Gradle.
3. Run на емуляторі/девайсі (minSdk 24, targetSdk 36).
4. Зареєструватись або залогінитись (бекенд уже задеплоєний на Railway).

## Документація / діаграми

У `docs/` є чотири PlantUML файли — рендерити можна через [PlantUML web server](https://www.plantuml.com/plantuml/uml/) або плагін IntelliJ:

- `use-case-diagram.puml` — Use Case Diagram (обов'язкова)
- `component-diagram.puml` — Component Diagram (обов'язкова)
- `activity-diagram-open-gate.puml` — Activity Diagram (циклу відкриття воріт)
- `state-diagram-auth.puml` — State Diagram (стани авторизації)
