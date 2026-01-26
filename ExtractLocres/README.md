# ExtractLocres - Утиліта для витягування файлів з Unreal Engine pak архівів

Консольна утиліта для витягування файлів з .pak архівів Unreal Engine 4/5.

## Використання

### Базове використання

**Екстракція одного pak файлу:**
```bash
ExtractLocres.exe --pak "шлях/до/файлу.pak"
```

**Список усіх pak файлів в папці (без екстракції):**
```bash
ExtractLocres.exe --scan "C:\Games\GameName\Content\Paks"
```

**Сканування папки і екстракція з усіх pak файлів:**
```bash
ExtractLocres.exe --scan "C:\Games\GameName\Content\Paks" --include .locres
```

### Опції

| Опція | Коротка | Опис | Приклад |
|-------|---------|------|---------|
| `--pak` | `-p` | Шлях до pak файлу | `-p "Game.pak"` |
| `--scan` | `-s` | Папка для сканування pak файлів | `-s "C:\Paks"` |
| `--scan-pak` | `-sp` | Сканувати один pak файл (без екстракції) | `-sp "Game.pak"` |
| `--output` | `-o` | Папка для збереження файлів | `-o "C:\Output"` |
| `--include` | `-i` | Формати файлів для витягування | `-i .locres .uasset` |
| `--exclude-formats` | `-ef` | Формати для ігнорування | `-ef .uexp .ubulk` |
| `--exclude-folders` | `-ex` | Папки для ігнорування | `-ex "Engine/Content"` |

**Важливо:** 
- Коли вказано `--scan` БЕЗ фільтрів - програма просто показує список pak файлів
- Коли вказано `--scan` З фільтрами (`--include`, `--exclude-formats`, `--exclude-folders`) - програма витягує файли
- Мають бути вказані або `--pak` або `--scan`, але не обидва одночасно

### Приклади

**1. Витягнути тільки .locres файли з одного pak:**
```bash
ExtractLocres.exe -p "Game.pak" -o "Output" -i .locres
```

**2. Сканувати папку і витягнути всі .locres файли:**
```bash
ExtractLocres.exe -s "C:\Games\Paks" -o "Output" -i .locres
```

**3. Показати список усіх pak файлів в папці:**
```bash
ExtractLocres.exe -s "C:\Games\Paks"
```

**4. Витягнути всі файли, окрім .uexp та Engine папки:****
```bash
ExtractLocres.exe -p "Game.pak" -o "Output" -ef .uexp .ubulk -ex "Engine/Content"
```

**4. Сканування папки з виключенням форматів і папок:**
```bash
ExtractLocres.exe -s "C:\Games\Paks" -o "Output" -ef .uexp .ubulk -ex "Engine/"
```

**5. Сканування папки з виключенням форматів і папок:**
```bash
ExtractLocres.exe -s "C:\Games\Paks" -o "Output" -ef .uexp .ubulk -ex "Engine/"
```

**6. Витягнути тільки .uasset та .umap файли:**
```bash
ExtractLocres.exe -p "Game.pak" -o "Output" -i .uasset .umap
```

**7. Сканування папки без фільтрів (витягує ВСЕ):**
```bash
ExtractLocres.exe -s "C:\Games\Paks" -o "Output"
```

**8. Сканування з виключенням Engine папки:**
```bash
ExtractLocres.exe -s "C:\Games\Paks" -o "Output" -ex "Engine/"
```

**9. Сканування одного pak файлу (без екстракції):**
```bash
ExtractLocres.exe --scan-pak "Game.pak"
```

## Особливості

- ✅ Автоматичне завантаження та ініціалізація Zlib для декомпресії
- ✅ Підтримка фільтрації за форматами файлів
- ✅ Підтримка виключення папок
- ✅ Збереження структури каталогів
- ✅ Прогрес виконання для великих pak файлів
- ✅ Обробка помилок з продовженням роботи

## Системні вимоги

- Windows 10/11 x64
- Не потребує встановлення .NET Runtime (автономний виконуваний файл)

## Розробка

Проект створено на базі [CUE4Parse](https://github.com/FabianFG/CUE4Parse) для роботи з Unreal Engine pak архівами.

### Збірка з вихідного коду

```bash
dotnet publish -c Release -o publish
```

## Ліцензія

Використовує бібліотеки з відповідними ліцензіями:
- CUE4Parse - Apache License 2.0
- Zlib-ng - Zlib License
