# T06: Оффлайн-прогресс + достижения (сервисы, без UI)

## Цель
Два Zenject-сервиса: расчёт оффлайн-заработка за время отсутствия и система достижений по GDD. Только логика + persistence, без UI.

## Контекст (прочитай эти файлы для точных сигнатур!)
- `Assets/_Project/Scripts/CurrencyController/ICurrencyController.cs` — валюта (`AddAmount`, `GetAmount`, событие OnCurrencyChange)
- `Assets/_Project/Scripts/Assistants/AssistantService.cs` — `TotalDps`, `CurrentDamage`? нет — тап-урон в `Upgrades/TapDamageUpgradeService.cs` (`CurrentDamage`)
- Сигналы-паттерн: `Assets/_Project/Scripts/Events/BossDefeatedSignal.cs` (static class + event + Emit)
- Существующие события: `EnemyKilledSignal(EnemyKilledInfo{IsBoss,Level})`, `BossDefeatedSignal(int)`, `TapEvent`, `UpgradeChangedSignal`; `LevelCountService` (`BossesDefeated`)
- PlayerPrefs-стиль: ключи вида `Meta.*`, `Achievements.*`
- Округление: `MathfRound.Round(float)`

## Что сделать

### 1. Assets/_Project/Scripts/Core/OfflineProgressService.cs
- Zenject-сервис (как `RewardService`: без MonoBehaviour, `[Inject] Construct(...)`)
- Параметры (константы с [SerializeField]-подобными полями не нужны — const): `CapSeconds = 7200` (2ч), `Efficiency = 0.5f`, `MinAwaySeconds = 60`
- Конструктор: читает `Meta.LastSeenUtc` (long, `DateTime.UtcNow.ToBinary()`), если ключа нет — просто сохранить now и PendingCoins=0. Иначе: away = now-last, если away ≥ MinAway и `AssistantService.TotalDps > 0` → `PendingCoins = MathfRound.Round(TotalDps * min(away, CapSeconds) * Efficiency)`; AwaySeconds = away (без кэпа — для UI «вас не было N ч»)
- `void Claim()` — если PendingCoins > 0: AddAmount(Soft, PendingCoins), PendingCoins=0, Emit `OfflineProgressSignal` (пустой, как триггер «обновлено»)
- `void TouchLastSeen()` — перезаписывает `Meta.LastSeenUtc` текущим временем; вызывать из `TickHeartbeat()`? — нет, просто публичный метод; сохранение метки на выходе — задача интегратора
- Если PendingCoins == 0 после конструктора — не эмитить ничего

### 2. Assets/_Project/Scripts/Events/OfflineProgressSignal.cs
- По паттерну проекта: `public static event Action OnOfflineProgress;` + `Emit()`

### 3. Assets/_Project/Scripts/Achievements/AchievementService.cs
Ачивки по GDD (id, имя, цель, прогресс, награда алмазами):
| Id | DisplayName | Goal | ProgressSource | Reward 💎 |
|---|---|---|---|---|
| novice | Новичок | 100 убийств | суммарный счётчик EnemyKilledSignal | 10 |
| finisher | Добиватель | 10 000 убийств | тот же счётчик | 50 |
| slayer | Slayer | 50 боссов | BossDefeatedSignal | 100 |
| millionaire | Миллионер | 1 000 000 монет (баланс ≥) | подписка OnCurrencyChange + init-проверка | 25 |
| tapmaster | Безрукий | 1 000 000 урона тапами | TapEvent → += TapDamageUpgradeService.CurrentDamage | 50 |
- Счётчики персистить: `Achievements.TotalKills`, `Achievements.TotalBosses`, `Achievements.TotalTapDamage` (long, double → string для tap damage)
- Разблок: при достижении цели — ОДИН раз: `Achievements.Unlocked.{id}=1`, AddAmount(Hard, reward), Emit `AchievementUnlockedSignal(AchievementInfo)`
- `IReadOnlyList<AchievementEntry> Entries` — для UI; `AchievementEntry{string Id; string DisplayName; string GoalText; float Goal; float Progress; bool IsUnlocked; int DiamondReward}` (GoalText типа "Убей 100 врагов")
- Unlock-проверку гонять: на каждом обновлении счётчика + при конструкторе (для millionaire и для случая, если порог достигнут между сессиями)
- Подписки: сервис не MonoBehaviour — подписки в конструкторе, отписка не нужна (живёт вечно, static events)

### 4. Assets/_Project/Scripts/Achievements/AchievementInfo.cs + Assets/_Project/Scripts/Events/AchievementUnlockedSignal.cs + Assets/_Project/Scripts/Events/AchievementsChangedSignal.cs
- `AchievementInfo{string Id; string DisplayName; int DiamondReward}`
- `AchievementUnlockedSignal`: `event Action<AchievementInfo>` + Emit
- `AchievementsChangedSignal`: `event Action` + Emit (прогресс обновился — UI перерисуется)

## Ограничения
- ТОЛЬКО перечисленные новые файлы. Не трогать LevelInstaller (интегратор забиндит сам), сцену, чужие файлы. Не git commit, не Unity, не mcp__UnityMCP__*.
- Стиль: 4 пробела, `_camelCase`, комментарии англ. минимально, namespace `_Project.Scripts.Core` / `_Project.Scripts.Achievements` / `_Project.Scripts.Events`.

## Критерии
- [ ] Смоук-сценарий в отчёте: сессия → выход (LastSeen=t0) → «время идёт» (эмулируй установкой LastSeen вручную) → конструктор с TotalDps=10, away=3ч → PendingCoins = 10*7200*0.5 = 36000; away=30 мин → 10*1800*0.5=9000
- [ ] Ачивка novice: 100× EnemyKilledSignal → Unlocked + AddAmount(10 💎), повторные киллы ничего не выдают
- [ ] Millionaire: баланс 1M → unlock; перезапуск (сессия 2) — не выдаёт повторно
- [ ] Tapmaster: 100000 тапов по 10 урона = 1M → unlock
- [ ] Все счётчики восстанавливаются из PlayerPrefs между сессиями

## Отчёт
Файлы; API-сводка (точные сигнатуры); мысленный прогон сценариев с числами; что не вышло.
