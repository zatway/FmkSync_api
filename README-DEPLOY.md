# komSync — развёртывание

## Требования

- .NET 9 SDK
- Node.js 20+ (для UI)
- PostgreSQL 14+

## База данных

1. Создайте БД (например `komSync`).
2. Укажите строку подключения в `WebApi/appsettings.json` → `ConnectionStrings:DefaultConnection`.
3. Примените миграции из каталога `Infrastructure/Migrations`:

```bash
cd KomSync/WebApi
dotnet ef database update --project ../Infrastructure/Infrastructure.csproj
```

## API (KomSync/WebApi)

Переменные и секции:

- `ConnectionStrings:DefaultConnection` — PostgreSQL.
- `JwtSettings:Secret` — секрет JWT (≥ 32 символов).
- `SmtpEmail` — SMTP для писем (сброс пароля, напоминания, заявки). При `Enabled: false` письма пишутся в лог (`LoggingEmailSender`).
- `PasswordReset:FrontendBaseUrl` — URL фронтенда для ссылки «сброс пароля» (например `http://localhost:5173`).
- `DeadlineReminders` — фоновые напоминания о дедлайнах (`Enabled`, `IntervalHours`, `OffsetsDays`).
- `SeedAdmin` — опциональное создание администратора при старте (только Development).

Запуск:

```bash
cd KomSync/WebApi
dotnet run
```

По умолчанию Kestrel слушает URL из `launchSettings.json` / переменных окружения.

## UI (KomSync_Ui)

В `src/env.ts` задайте `VITE_API_BASE_URL` (базовый URL API, включая `/api/v1`).

```bash
cd KomSync_Ui
npm ci
npm run build
npm run preview
```

## Нагрузочное тестирование (приёмка)

Для отчёта по п. приёмки можно использовать [k6](https://k6.io/) или [NBomber](https://nbomber.com/): 100+ параллельных запросов к типовым GET (`/api/v1/projects`, `/api/v1/search?q=test`) с заголовком `Authorization: Bearer …`.

## Docker Compose (backend + db)

В директории `KomSync/WebApi`:

- **`docker-compose.yml`** — **деплой на Linux**: контейнер `backend` с `network_mode: host` (исходящий трафик и SMTP как у хоста). Строка подключения к БД: `Host=127.0.0.1`, порт совпадает с проброшенным `POSTGRES_PORT` сервиса `db`. Kestrel слушает `BACKEND_PORT` на хосте: заданы и `ASPNETCORE_HTTP_PORTS`, и `ASPNETCORE_URLS` (в образах `aspnet` по умолчанию порт **8080**, без override API остаётся на 8080). Секция `ports` у backend не используется.
- **`docker-compose.local.yml`** — **локальный Docker** (Docker Desktop и т.п.): обычная bridge-сеть, `Host=db`, проброс `${BACKEND_PORT}:8080`, как раньше.

Разработка **без** Docker API: `dotnet run` и Postgres на машине — по-прежнему без изменений.

### Переменные окружения

Скопируйте шаблон и заполните значения:

```bash
cd KomSync/WebApi
cp .env.example .env
```

Основные переменные (для backend):

- `POSTGRES_DB`, `POSTGRES_USER`, `POSTGRES_PASSWORD`, `POSTGRES_PORT`
- `BACKEND_PORT` (на деплое — порт API на хосте; в `docker-compose.local.yml` — проброс хоста на контейнер 8080)
- `ASPNETCORE_ENVIRONMENT`
- `JWT_SECRET`
- `PASSWORD_RESET_FRONTEND_BASE_URL`, `PASSWORD_RESET_TOKEN_LIFETIME_HOURS`
- `DEADLINE_REMINDERS_ENABLED`, `DEADLINE_REMINDERS_INTERVAL_HOURS`, `DEADLINE_REMINDERS_OFFSET_0..3`
- `SMTP_ENABLED`, `SMTP_HOST`, `SMTP_PORT`, `SMTP_USE_SSL`, `SMTP_USERNAME`, `SMTP_PASSWORD`, `SMTP_FROM_EMAIL`, `SMTP_FROM_NAME`
- `SEED_ADMIN_ENABLED`, `SEED_ADMIN_EMAIL`, `SEED_ADMIN_PASSWORD`, `SEED_ADMIN_FULLNAME`, `SEED_ADMIN_DEPARTMENT`, `SEED_ADMIN_POSITION`

### Запуск (деплой Linux)

```bash
cd KomSync/WebApi
docker compose up -d --build
```

После старта на сервере API доступен на `http://<хост>:${BACKEND_PORT}` (например `5237`). PostgreSQL на хосте: `127.0.0.1:${POSTGRES_PORT}`.

### Запуск (локально в Docker)

```bash
cd KomSync/WebApi
docker compose -f docker-compose.local.yml up -d --build
```

- Backend: `http://localhost:${BACKEND_PORT}`
- PostgreSQL: `localhost:${POSTGRES_PORT}`

### SMTP timeout и `listening on ... 8080`

Если в логах **`Now listening on: http://[::]:8080`** и почта падает по таймауту:

1. Убедитесь, что на сервере запускается **`docker compose up`** с файлом **`docker-compose.yml`**, а не **`docker-compose.local.yml`** (bridge — исходящий SMTP из контейнера часто недоступен, как с хоста).
2. После правок compose пересоберите образ: `docker compose up -d --build`.
3. В логах при деплое должно быть прослушивание порта **`${BACKEND_PORT}`** (например `5237`), не `8080`.

Фронтенд поднимается отдельно из репозитория `KomSync_Ui` своим `docker-compose.yml`. Для контейнера Nginx укажите `BACKEND_UPSTREAM=http://host.docker.internal:${BACKEND_PORT}` (в compose уже добавлен `extra_hosts: host-gateway`), чтобы проксировать на API на хосте при деплое с `network_mode: host`.
