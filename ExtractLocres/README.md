# ExtractLocres - Утиліта для витягування файлів з Unreal Engine pak архівів

Консольна утиліта для витягування файлів з .pak архівів Unreal Engine 4/5.

## Використання

### Базове використання

**Екстракція одного pak файлу:**
```bash
ExtractLocres.exe --pak "шлях/до/файлу.pak"
```

**Сканування папки і екстракція з усіх pak файлів:**
```bash
ExtractLocres.exe --scan "C:\Games\GameName\Content\Paks"
```

### Опції

| Опція | Коротка | Опис | Приклад |
|-------|---------|------|---------|
| `--pak` | `-p` | Шлях до pak файлу | `-p "Game.pak"` |
| `--scan` | `-s` | Папка для сканування pak файлів | `-s "C:\Paks"` |
| `--output` | `-o` | Папка для збереження файлів | `-o "C:\Output"` |
| `--include` | `-i` | Формати файлів для витягування | `-i .locres .uasset` |
| `--exclude-formats` | `-ef` | Формати для ігнорування | `-ef .uexp .ubulk` |
| `--exclude-folders` | `-ex` | Папки для ігнорування | `-ex "Engine/Content"` |

**Важливо:** Мають бути вказані або `--pak` або `--scan`, але не обидва одночасно.

### Приклади

**1. Витягнути тільки .locres файли з одного pak:**
```bash
ExtractLocres.exe -p "Game.pak" -o "Output" -i .locres
```

**2. Сканувати папку і витягнути всі .locres файли:**
```bash
ExtractLocres.exe -s "C:\Games\Paks" -o "Output" -i .locres
```

**3. Витягнути всі файли, окрім .uexp та Engine папки:**
```bash
ExtractLocres.exe -p "Game.pak" -o "Output" -ef .uexp .ubulk -ex "Engine/Content"
```

**4. Сканування папки з виключенням форматів і папок:**
```bash
ExtractLocres.exe -s "C:\Games\Paks" -o "Output" -ef .uexp .ubulk -ex "Engine/"
```

**5. Витягнути тільки .uasset та .umap файли:**
```bash
ExtractLocres.exe -p "Game.pak" -o "Output" -i .uasset .umap
```

**6. Сканування папки без фільтрів (витягує ВСЕ):**
```bash
ExtractLocres.exe -s "C:\Games\Paks" -o "Output"
```

**7. Сканування з виключенням Engine папки:**
```bash
ExtractLocres.exe -s "C:\Games\Paks" -o "Output" -ex "Engine/"
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
