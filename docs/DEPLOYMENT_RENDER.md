# Развёртывание FinanceTrack на платформе Render через Docker

В этом руководстве подробно описан процесс развёртывания приложения **FinanceTrack** в облачном сервисе [Render](https://render.com) с использованием **Docker** и **SQLite**.

---

## 🏗️ 1. Архитектура контейнера

Приложение упаковано с помощью многоэтапного Dockerfile:
- **Build Stage**: Использует официальный образ `mcr.microsoft.com/dotnet/sdk:10.0`.
  - Сначала копируются файлы `.sln` и `.csproj` для эффективного кэширования слоёв Docker при `dotnet restore`.
  - Выполняется компиляция решения в конфигурации `Release`.
  - Выполняется публикация проекта `FinanceTrack.Web` без тестовых зависимостей.
- **Runtime Stage**: Использует оптимизированный образ `mcr.microsoft.com/dotnet/aspnet:10.0`.
  - Приложение слушает порт `10000` (стандартный для Render Web Services) либо динамический порт из переменной окружения `PORT`.
  - Настроена обработка заголовков обратного прокси (`ForwardedHeaders`) для корректной работы HTTPS и WebSocket соединений Blazor Interactive Server.
  - База данных SQLite сохраняется в выделенном каталоге `/app/data/`.

---

## 🗄️ 2. Работа с базой данных SQLite на Render

### Автоматическое создание и миграции
При старте контейнера в коде `Program.cs` вызывается метод:
```csharp
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FinanceTrackDbContext>();
    db.Database.Migrate();
}
```
Это гарантирует, что:
1. Если файла базы данных нет, SQLite создаст его автоматически.
2. Применяются все миграции Entity Framework Core.
3. Автоматически создаются 13 категорий по умолчанию (8 категорий расходов и 5 категорий доходов).
4. Локальный файл базы данных с компьютера разработчика **не требуется**.

### Особенности файловой системы Render

#### Бесплатный тариф (Free Tier — Ephemeral Filesystem)
- На бесплатном тарифе Render файловая система контейнера является временной (ephemeral).
- При каждом новом деплое или после "засыпания" сервиса из-за неактивности база данных инициализируется заново со стандартными категориями.
- **Для защиты и демонстрации проекта этого тарифа достаточно.**

#### Тариф с постоянным диском (Render Persistent Disk)
Если требуется постоянное сохранение данных между перезапусками и деплоями:
1. В настройках сервиса Render (или в `render.yaml`) подключите **Disk**:
   - **Name**: `financetrack-data`
   - **Mount Path**: `/app/data`
   - **Size**: `1 GB`
2. Переменная подключения:
   - Значение по умолчанию: `ConnectionStrings__DefaultConnection="Data Source=/app/data/FinanceTrack.db"`.
   - Файл SQLite будет храниться на постоянном диске и никогда не удалится.

---

## 🚀 3. Пошаговая инструкция по публикации на Render

### Способ А: Через панель управления Render (Вручную)

1. **Загрузите репозиторий на GitHub/GitLab**:
   Убедитесь, что в репозиторий отправлены:
   - `Dockerfile`
   - `.dockerignore`
   - Все исходные коды проекта.

2. **Создайте Web Service на Render**:
   - Перейдите в [Render Dashboard](https://dashboard.render.com).
   - Нажмите **New +** &rarr; **Web Service**.
   - Выберите **Build and deploy from a Git repository** и укажите репозиторий `FinanceTrack`.

3. **Настройте параметры Web Service**:
   - **Name**: `financetrack` (или любое желаемое имя)
   - **Region**: `Frankfurt (EU Central)` или ближайший к вам
   - **Branch**: `main`
   - **Root Directory**: оставьте пустым (корень репозитория)
   - **Runtime**: **Docker**
   - **Dockerfile Path**: `./Dockerfile`
   - **Docker Context**: `.`
   - **Instance Type**: **Free**

4. **Проверьте переменные окружения (Environment Variables)**:
   При необходимости можно задать или переопределить:
   - `ASPNETCORE_ENVIRONMENT` = `Production`
   - `ConnectionStrings__DefaultConnection` = `Data Source=/app/data/FinanceTrack.db`

5. **Запустите развёртывание**:
   - Нажмите **Create Web Service**.
   - Render запустит сборку Docker-образа (`docker build`) и затем запустит контейнер.
   - В логах вы увидите:
     ```
     info: Microsoft.EntityFrameworkCore.Migrations[20402]
           Applying migration '20261117095658_InitialCreate'.
     info: Microsoft.Hosting.Lifetime[14]
           Now listening on: http://0.0.0.0:10000
     ```

6. **Проверка работоспособности**:
   - Перейдите по сгенерированному адресу (например, `https://financetrack.onrender.com`).
   - Откроются страница дашборда и интерактивные разделы транзакций, категорий, бюджетов и отчётов.

---

### Способ Б: Через Render Blueprint (`render.yaml`)

В корне проекта уже подготовлен файл `render.yaml`.
1. В Render Dashboard нажмите **New +** &rarr; **Blueprint**.
2. Выберите репозиторий с проектом.
3. Render автоматически считает параметры из `render.yaml` и настроит сервис.

---

## ⚡ 4. Особенности Blazor Server и WebSocket на Render

- **Поддержка WebSocket**: Render полностью поддерживает постоянные WebSocket соединения, необходимые для Blazor Interactive Server.
- **SSL/TLS Termination**: Render обрабатывает HTTPS на уровне своего обратного прокси. В приложении настроен middleware `app.UseForwardedHeaders()`, благодаря чему Blazor корректно определяет защищённое соединение.
- **Auto-Sleep (Холодный старт)**: На бесплатном тарифе Render сервис засыпает после 15 минут неактивности. Первый запрос после пробуждения может занимать около 30–50 секунд, после чего приложение работает быстро и плавно.

---

## 🧪 5. Проверка Docker локально (опционально)

Если на вашем компьютере установлен Docker, вы можете протестировать сборку и запуск локально:

```bash
# Сборка образа из корня репозитория
docker build -t financetrack:latest .

# Запуск контейнера с пробросом порта 10000 на локальный 5000
docker run -d -p 5000:10000 --name financetrack-app financetrack:latest

# Откройте в браузере: http://localhost:5000
```
