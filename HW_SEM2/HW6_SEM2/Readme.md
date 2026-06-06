Запуск проекта

```bash
dotnet restore
dotnet run
```

При запуске проект сам создаст базу данных и применит миграцию.

Файл базы данных будет создан автоматически:

```text
homework.db
```

Проверка

После команды `dotnet run` в терминале появится адрес приложения.
Дальше нужно открыть этот адрес вместе с путём контроллера.

### Получить все товары

```text
/api/products
```

Пример:

```text
http://localhost:5000/api/products
```

### Получить один товар по id

```text
/api/products/1
```

Пример:

```text
http://localhost:5000/api/products/1
```

### Получить все категории

```text
/api/categories
```

Пример:

```text
http://localhost:5000/api/categories
```

### Получить одну категорию по id

```text
/api/categories/1
```

Пример:

```text
http://localhost:5000/api/categories/1
```

В браузере должен появиться JSON с данными из базы данных.

Например для товаров:

```json
[
  {
    "id": 1,
    "name": "Ноутбук",
    "price": 75000,
    "categoryId": 1,
    "categoryName": "Электроника"
  }
]
```

## Если нужно пересоздать базу данных

Удалить файл:

```text
homework.db
```

Потом снова запустить проект:

```bash
dotnet run
```

База данных создастся заново автоматически.
