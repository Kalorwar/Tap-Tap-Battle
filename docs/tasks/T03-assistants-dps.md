# T03: Ассистенты (DPS-помощники)

## Цель
Система наёмных помощников: покупка/прокачка за монеты, суммарный DPS автоматически наносит урон текущему врагу раз в секунду.

## Контекст
- Проект: `F:/Unity/Tap-Tap Battle` (Unity 6, 2D URP, Zenject, C# events). R3 удалён — ТОЛЬКО обычные C# `event`.
- Текущий враг: `EnemySpawnedSignal` (Events/EnemySpawnedSignal.cs, УЖЕ существует): `event Action<EnemySpawnedInfo>`; `EnemySpawnedInfo{Enemy Enemy; bool IsBoss}` — эмитится при каждом спавне. Смерть врага: `EnemyDiedEvent` (event Action). `Enemy`: `TakeDamage(float)`, `IsAlive`, `MaxHealth`.
- Валюта: `ICurrencyController` (HasEnough/TryRemoveCurrency, `CurrencyType.SoftCurrency`).
- Округление: `MathfRound.Round(float)` (Assets/_Project/Scripts/Core/MathfRound.cs).
- Уровень игрока: `LevelCountService.LevelCount` (Assets/_Project/Scripts/Core/LevelCountService.cs — НЕ менять). Побеждённые боссы: событие `BossDefeatedSignal` (Events/BossDefeatedSignal.cs, УЖЕ существует): `event Action<int>` (1-based индекс).
- Паттерн событий: static class + `event Action`/`Action<T>` + Emit — см. Events/TapEvent.cs.
- Формулы ГДД: стоимость = BaseCost * 1.07^Level (Level = число купленных уровней, 0 на старте); DPS = BaseDps * 1.05^(Level-1) при Level >= 1, при Level 0 помощник не наносит урон.

## Что сделать
1. `Assets/_Project/Scripts/Assistants/AssistantConfig.cs` — ScriptableObject (`[CreateAssetMenu(menuName = "Assistants/AssistantConfig")]`): поля `string Id`, `string DisplayName`, `float BaseDps`, `float BaseCost`, `float CostGrowth = 1.07f`, `float DpsGrowth = 1.05f`, `int UnlockLevel`, `int UnlockBossIndex` (0 = условие не используется). .asset-файлы НЕ создавать — только класс.
2. `Assets/_Project/Scripts/Assistants/AssistantService.cs` — Zenject plain C# сервис (конструктор: `IReadOnlyList<AssistantConfig> configs`, `ICurrencyController currency`, `LevelCountService levelCount`):
   - состояние: `int GetLevel(string id)`, сохранение PlayerPrefs `Assistants.{Id}.Level`;
   - `bool IsUnlocked(config)`: UnlockLevel > 0 → `levelCount.LevelCount >= UnlockLevel`; UnlockBossIndex > 0 → `_bossesDefeated >= UnlockBossIndex` (счётчик из `BossDefeatedSignal` подпиской, хранить в памяти, restored нет — ок); оба 0 → открыт;
   - `float GetDps(config)`, `float GetCost(config)` (стоимость следующего уровня), `float TotalDps` (сумма по всем), `bool TryBuy(config)`;
   - покупки/изменения → `AssistantChangedSignal.Emit()` (новый Events/AssistantChangedSignal.cs, `event Action`).
3. `Assets/_Project/Scripts/Assistants/DpsTicker.cs` — MonoBehaviour: `[Inject] Construct(AssistantService)`; копит время в `Update`, раз в 1 секунду наносит `MathfRound.Round(AssistantService.TotalDps)` текущему врагу. Текущий враг: подписка на `EnemySpawnedSignal` (OnEnable/OnDisable), очистка по `EnemyDiedEvent`; перед ударом проверять `enemy != null && enemy.IsAlive`. Если TotalDps == 0 — не бить.
4. Дефолтные конфиги (данные для .asset, которые создаст интегратор — просто перечисли их в отчёте в виде таблицы): CatMage (DPS 1, Cost 50, сразу), Elf (DPS 5, Cost 500, уровень 10), Dragon (DPS 25, Cost 5000, уровень 25), Pirat (DPS 125, Cost 50000, босс #3), Spirit (DPS 625, Cost 500000, босс #5).

## Ограничения
- Файлы: только новые Assistants/AssistantConfig.cs, Assistants/AssistantService.cs, Assistants/DpsTicker.cs, Events/AssistantChangedSignal.cs. НИЧЕГО другое не трогать (особенно LevelInstaller.cs, LevelCountService.cs, EnemySpawner.cs, DamageController.cs).
- ЗАПРЕЩЕНО: R3/UniRx/UniTask; git commit; mcp__UnityMCP__*; запуск Unity; сцены/префабы/.asset; создавать ScriptableObject-ассеты.
- Стиль: 4 пробела, `_camelCase`, readonly, namespace `_Project.Scripts.Assistants`, комментарии по-английски минимально.

## Критерии готовности
- [ ] CatMage Level 0: DPS 0, Cost 50; после покупки: DPS 1, Cost 53.5
- [ ] CatMage Level 5: DPS = 1 * 1.05^4 ≈ 1.216
- [ ] Elf закрыт при LevelCount < 10, открыт при >= 10; Pirat закрыт, пока BossDefeatedSignal не пришёл 3 раза
- [ ] DpsTicker: при TotalDps=2.5 и живом враге — раз в секунду TakeDamage(2.5); по смерти врага не бьёт (нет NRE)
- [ ] Покупка при нехватке монет не проходит; уровни сохраняются в PlayerPrefs

## Проверка
Не опирайся на скриншоты — проверяй кодом и числами. Застрял >20 мин или 2 гипотезы не подтвердились — остановись и отчитайся.

## Отчёт в конце
Файлы; таблица конфигов для .asset; строки биндингов Zenject для LevelInstaller (включая список конфигов); что не вышло.
