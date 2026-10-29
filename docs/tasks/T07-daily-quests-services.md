# T07: Ежедневные награды + ежедневные задания (сервисы, без UI)

## Цель
Два Zenject-сервиса по GDD: 7-дневный цикл ежедневных наград со сбросом серии и 3 случайных ежедневных задания со сбросом каждые 24ч. Только логика + persistence.

## Контекст (прочитай!)
- `Assets/_Project/Scripts/CurrencyController/ICurrencyController.cs`, `CurrencyType` (Soft/Hard)
- Паттерн сигналов: `Assets/_Project/Scripts/Events/BossDefeatedSignal.cs`
- Существующие события: `EnemyKilledSignal(EnemyKilledInfo{IsBoss,Level})`, `BossDefeatedSignal(int)`, `TapEvent`, `AssistantChangedSignal`
- PlayerPrefs-ключи: `Daily.*`, `Quests.*`
- Время: `DateTime.UtcNow`; persistence как long `ToBinary()`

## Что сделать

### 1. Assets/_Project/Scripts/Core/DailyRewardService.cs
Календарь по GDD (7-дневный цикл):
| День | Награда |
|---|---|
| 1 | 50 монет |
| 2 | 100 монет |
| 3 | 5 алмазов |
| 4 | 500 монет |
| 5 | 10 алмазов |
| 6 | 1000 монет |
| 7 | 25 алмазов (скин — пост-MVP, не реализуем) |
- Persistence: `Daily.LastClaimUtc` (long), `Daily.Streak` (int, 0..6 — сколько дней подряд уже забрано)
- Правила: клейм доступен (`CanClaim`), если с прошлого клейма прошло ≥ 20ч. Если прошло > 48ч — серия сброшена (`Streak=0`, день снова 1). День к клейму = Streak+1 (по модулю цикла: после 7-го дня цикл начинается заново с 1-го, streak продолжает расти до сброса — т.е. `DayForClaim = Streak % 7 + 1`)
- `void Claim()`: если CanClaim — начислить награду дня (AddAmount), Streak++, LastClaimUtc=now, Emit `DailyRewardClaimedSignal(DailyRewardInfo)`, затем Emit `DailyChangedSignal`
- `DailyRewardInfo{int Day; CurrencyType RewardType; float RewardAmount; int Streak}` — в Events/
- `DescribeDay(int day1to7)` → string для UI ("50 монет" / "5 алмазов")
- Свойства: `bool CanClaim`, `int CurrentStreak`, `int NextDay` (1..7)
- Конструктор сразу Emit `DailyChangedSignal` (стартовое состояние для UI)

### 2. Assets/_Project/Scripts/Core/DailyQuestService.cs
Пул из 4 по GDD:
| Id | Описание | Цель | Трекинг | Награда |
|---|---|---|---|---|
| kill20 | Убей 20 врагов | 20 | EnemyKilledSignal | 200 монет |
| boss2 | Победи 2 боссов | 2 | BossDefeatedSignal | 500 монет + 3 алмаза |
| tap1000 | Сделай 1000 тапов | 1000 | TapEvent | 300 монет |
| upgrade5 | Прокачай помощника 5 раз | 5 | AssistantChangedSignal (каждая покупка уровня ассистента; покупка урона тапа НЕ считается) | 400 монет |
- Окно 24ч: `Quests.WindowStartUtc`; при конструкторе если окно истекло (>24ч) или пусто — сгенерировать 3 случайных РАЗНЫХ квеста из пула (UnityEngine.Random), WindowStartUtc=now, прогресс 0, выдать их через Emit
- Persistence: `Quests.ActiveIds` (string, join ";" например), `Quests.Progress.{id}` (float/int), `Quests.Claimed.{id}` (bool), `Quests.WindowStartUtc`
- `DailyQuest{string Id; string Description; float Goal; float Progress; bool Completed; bool Claimed; CurrencyType RewardType; float RewardAmount; int BonusDiamonds}` (BonusDiamonds=3 только у boss2, иначе 0; поле нужно для UI)
- `IReadOnlyList<DailyQuest> Quests` — активная тройка
- Трекинг: прогресс ++ до Goal, при достижении Completed=true + Emit `QuestsChangedSignal`; заокlicmileнные claim'ит UI через `bool TryClaim(DailyQuest)` → AddAmount (монеты + алмазы если BonusDiamonds>0), Claimed=true, Emit `QuestsChangedSignal`
- `bool WindowActive` для UI (сколько осталось — верни `double SecondsRemaining`)

### 3. Events: `DailyRewardClaimedSignal(Action<DailyRewardInfo>)`, `DailyChangedSignal(Action)`, `QuestsChangedSignal(Action)` — файлы в `Assets/_Project/Scripts/Events/`, паттерн проекта.

## Ограничения
- ТОЛЬКО перечисленные новые файлы. Не трогать LevelInstaller/сцену/чужое. Не git commit, не Unity, не mcp__UnityMCP__*.
- Стиль: 4 пробела, `_camelCase`, namespace `_Project.Scripts.Core` / `_Project.Scripts.Events`, комментарии англ. минимально.

## Критерии
- [ ] Смоук-числа в отчёте: свежий игрок → CanClaim=true (клейма не было), Claim() день 1 → 50 монет, streak=1; эмуляция -25ч → CanClaim=true; -50ч → streak сброшен, день снова 1
- [ ] 7-й день: streak=6 → Day=7 → 25 алмазов → streak=7 → следующий день 1 (50 монет)
- [ ] Квесты: окно новое → 3 разных из 4; kill20 прогресс 20/20 → Completed; TryClaim до Completion=false; после — монеты начислены
- [ ] Перезапуск сессии: активные квесты и прогресс восстановлены из PlayerPrefs; окно не истекло → НЕ перегенерированы
- [ ] boss2: только BossDefeatedSignal (не каждый босс-уровень EnemyKilledSignal с IsBoss) — сверься, какой сигнал точнее соответствует «победе над боссом», и используй BossDefeatedSignal

## Отчёт
Файлы; точные сигнатуры API; прогон сценариев с числами; что не вышло.
