# Continuous Integration (GitHub Actions)

В репозитории настроены GitHub Actions workflows.

## Что уже работает

### Job `validate`
Запускается на каждый push и pull request в `main`.

Проверяет:
- наличие ключевых файлов проекта
- версию Unity (`6000.3.22f1`)
- наличие важных пакетов (Input System, URP, uGUI)
- список C# скриптов

Этот job **не требует** лицензии Unity и работает сразу.

## Как включить полноценную сборку Unity

Job `build` сейчас отключён (`if: false`). Чтобы его включить:

### 1. Получите Unity License

Для Personal / Plus / Pro:

1. Установите Unity Hub и нужную версию редактора (`6000.3.22f1`).
2. Активируйте лицензию локально.
3. Найдите файл лицензии (обычно в `~/Library/Unity/Unity_lic.ulf` на macOS или `%LOCALAPPDATA%\\Unity\\Unity_lic.ulf` на Windows).
4. Скопируйте **весь** текст файла.

### 2. Добавьте Secrets в GitHub

В репозитории: **Settings → Secrets and variables → Actions → New repository secret**

Добавьте три секрета:

| Secret name       | Значение                          |
|-------------------|-----------------------------------|
| `UNITY_LICENSE`   | Полный текст файла `.ulf`         |
| `UNITY_EMAIL`     | Email от Unity-аккаунта           |
| `UNITY_PASSWORD`  | Пароль от Unity-аккаунта          |

### 3. Включите job `build`

В файле `.github/workflows/ci.yml` найдите строку:

```yaml
if: ${{ false }}  # set to true ...
```

Замените на:

```yaml
if: true
```

или просто удалите эту строку.

### 4. (Опционально) Смените платформу сборки

В параметре `targetPlatform` можно указать:
- `StandaloneWindows64`
- `StandaloneOSX`
- `StandaloneLinux64`
- `Android`
- `iOS`
- `WebGL`

Для Android/iOS потребуются дополнительные настройки (SDK, сертификаты и т.д.).

## Полезные ссылки

- [game-ci/unity-builder](https://game.ci/docs/github/builder)
- [Unity License activation](https://game.ci/docs/github/activation)
