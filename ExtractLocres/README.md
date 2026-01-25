# ExtractLocres - Утиліта для витягування файлів з Unreal Engine pak архівів

Консольна утиліта для витягування файлів з .pak архівів Unreal Engine 4/5.

## Використання

### Базове використання

```bash
ExtractLocres.exe --pak "шлях/до/файлу.pak"
```

### Опції

| Опція | Коротка | Опис | Приклад |
|-------|---------|------|---------|
| `--pak` | `-p` | Шлях до pak файлу (обов'язково) | `-p "Game.pak"` |
| `--output` | `-o` | Папка для збереження файлів | `-o "C:\Output"` |
| `--include` | `-i` | Формати файлів для витягування | `-i .locres .uasset` |
| `--exclude-formats` | `-ef` | Формати для ігнорування | `-ef .uexp .ubulk` |
| `--exclude-folders` | `-ex` | Папки для ігнорування | `-ex "Engine/Content"` |

### Приклади

**1. Витягнути тільки .locres файли:**
```bash
ExtractLocres.exe -p "Game.pak" -o "Output" -i .locres
```

**2. Витягнути всі файли, окрім .uexp та Engine папки:**
```bash
ExtractLocres.exe -p "Game.pak" -o "Output" -ef .uexp .ubulk -ex "Engine/Content"
```

**3. Витягнути .uasset та .umap файли:**
```bash
ExtractLocres.exe -p "Game.pak" -i .uasset .umap
```

**4. Витягнути все з гри, окрім певних папок:**
```bash
ExtractLocres.exe -p "Game.pak" -ex "Engine/Plugins" "Engine/Content/EditorResources"
```

**5. Витягнути лише TheObserver контент (без Engine):**
```bash
ExtractLocres.exe -p "TheObserver-WindowsNoEditor.pak" -ex "Engine/"
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
